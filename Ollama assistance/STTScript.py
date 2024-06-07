import multiprocessing.process
import os
import numpy as np
import speech_recognition as sr
import whisper
import torch

import random
import win32pipe, win32file
import multiprocessing

from datetime import datetime, timedelta
from queue import Queue
from time import sleep
from sys import platform

class Config:
    def __init__(self, energy_threshold, record_timeout, phrase_timeout):
        self.energy_threshold = energy_threshold
        self.record_timeout = record_timeout
        self.phrase_timeout = phrase_timeout

config = Config(
    1000,   # energy_threshold
    2,      # record_timeout
    3       # phrase_timeout
    )

pipe_name = r'\\.\pipe\STT_fifo'
shutdown_flag = 'shutdown.flag'

"""def run_server():
    while True:
        random_number = str(random.randint(1, 9))
        pipe = win32pipe.CreateNamedPipe(
            pipe_name,
            win32pipe.PIPE_ACCESS_OUTBOUND,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None
        )
        win32pipe.ConnectNamedPipe(pipe, None)
        win32file.WriteFile(pipe, random_number.encode())
        win32file.CloseHandle(pipe)
        sleep(10)"""

def run_server():
    phrase_time = None

    data_queue = Queue()

    recorder = sr.Recognizer()
    recorder.energy_threshold = config.energy_threshold
    recorder.dynamic_energy_threshold = False

    source = sr.Microphone(sample_rate=16000) # dosen't work with linux, sorry linux chads.

    # options(VRAM requierment) => tiny.en(1GB) | base.en(1GB) | small.en(2GB) | medium.en(5GB) | large(10GB)
    audio_model = whisper.load_model("medium.en")

    record_timeout = config.record_timeout
    phrase_timeout = config.phrase_timeout
    
    transcription = ['']

    with source:
        recorder.adjust_for_ambient_noise(source)

    def record_callback(_, audio:sr.AudioData) -> None:
        data = audio.get_raw_data()
        data_queue.put(data)

    recorder.listen_in_background(source, record_callback, phrase_time_limit=record_timeout)

    while True:
        now = datetime.utcnow()
        if not data_queue.empty():
            phrase_complete = False
            if phrase_time and now - phrase_time > timedelta(seconds=phrase_timeout):
                phrase_complete = True
            phrase_time = now

            audio_data = b''.join(data_queue.queue)
            data_queue.queue.clear()

            audio_np = np.frombuffer(audio_data, dtype=np.int16).astype(np.float32) / 32768.0

            result = audio_model.transcribe(audio_np, fp16=torch.cuda.is_available())
            text = result['text'].strip()

            if phrase_complete:
                transcription.append(text)
            else:
                transcription[-1] = text

            os.system('cls' if os.name=='nt' else 'clear')
            for line in transcription:
                #random_number = str(random.randint(1, 9))
                pipe = win32pipe.CreateNamedPipe(
                    pipe_name,
                    win32pipe.PIPE_ACCESS_OUTBOUND,
                    win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
                    1, 65536, 65536,
                    0,
                    None
                )
                win32pipe.ConnectNamedPipe(pipe, None)
                win32file.WriteFile(pipe, line.encode())
                win32file.CloseHandle(pipe)

                # add to pipe(fifo)

            sleep(0.25)

if __name__ == "__main__":
    """mic_process = multiprocessing.Process(target=main)
    pipe_process = multiprocessing.Process(target=run_server)

    mic_process.start()
    pipe_process.start()

    if os.path.exists(shutdown_flag):
        os.remove(shutdown_flag)
        mic_process.terminate()
        pipe_process.terminate()

        mic_process.join()
        pipe_process.join()    """

    run_server()
    #run_server()