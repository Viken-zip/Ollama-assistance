import ollama
import os
import time
import win32pipe, win32file
import numpy as np
import speech_recognition as sr
import whisper
import torch
import threading
from datetime import datetime, timedelta
from queue import Queue

# pipes to send from python to C#
stt_loaded_pipe_name = r'\\.\pipe\STTLoaded_pipe'
stt_result_pipe_name = r'\\.\pipe\STTResult_pipe'
ai_answer_name = r'\\.\pipe\AIAnswer_pipe'

#pipes to get from C# to python
ai_question_pipe_name = r'\\.\pipe\AIQuestion_pipe'
stt_on_pipe_name = r'\\.\pipe\STTOn_pipe'

shutdown_flag = 'shutdown.flag'

STTOn = False

class Config:
    def __init__(self, energy_threshold, record_timeout, phrase_timeout):
        self.energy_threshold = energy_threshold
        self.record_timeout = record_timeout
        self.phrase_timeout = phrase_timeout

config = Config(
    1000,   # energy_threshold
    20,      # record_timeout
    3       # phrase_timeout
)

def createPipe(message, pipe_name):
    pipe = win32pipe.CreateNamedPipe(
            pipe_name,
            win32pipe.PIPE_ACCESS_OUTBOUND,
            win32pipe.PIPE_TYPE_MESSAGE | win32pipe.PIPE_WAIT,
            1, 65536, 65536,
            0,
            None
        )
    win32pipe.ConnectNamedPipe(pipe, None)
    win32file.WriteFile(pipe, message.encode())
    win32file.CloseHandle(pipe)

def AskOllama(question):
    messages = []
    
    messages.append({
        'role': 'user',
        'content': question,
    })
    response = ollama.chat(
        model='llama3',
        messages=messages
        )
    return response['message']['content']

def STT():
    phrase_time = None

    data_queue = Queue()

    recorder = sr.Recognizer()
    recorder.energy_threshold = config.energy_threshold
    recorder.dynamic_energy_threshold = False
    source = sr.Microphone(sample_rate=16000) # dosen't work with linux, sorry linux chads.

    # options(VRAM requierment) => tiny.en(1GB) | base.en(1GB) | small.en(2GB) | medium.en(5GB) | large(10GB)
    audio_model = whisper.load_model("base.en")

    record_timeout = config.record_timeout
    phrase_timeout = config.phrase_timeout

    transcription = ['']

    with source:
        recorder.adjust_for_ambient_noise(source)

    def record_callback(_, audio:sr.AudioData) -> None:
        data = audio.get_raw_data()
        data_queue.put(data)

    recorder.listen_in_background(source, record_callback, phrase_time_limit=record_timeout)

    while STTOn:
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
                createPipe(text, stt_result_pipe_name)
            else:
                transcription[-1] = text

            os.system('cls' if os.name == 'nt' else 'clear')
            time.sleep(0.25) # the cpu also needs to take some breaks man.

STT_Thread = threading.Thread(target=STT) #initaliziong thread for STT.

def check_pipes():
    global STTOn
    while True:
        try:
            pipe = win32file.CreateFile(
                ai_question_pipe_name,
                win32file.GENERIC_READ,
                0,
                None,
                win32file.OPEN_EXISTING,
                0,
                None
            )
            question = win32file.ReadFile(pipe, 64 * 1024)[1].decode()
            answer = AskOllama(question)
            createPipe(answer, ai_answer_name)

            win32file.CloseHandle(pipe)
        except Exception as e:
            time.sleep(0.5)
            
        try:
            pipe = win32file.CreateFile(
                stt_on_pipe_name,
                win32file.GENERIC_READ,
                0,
                None,
                win32file.OPEN_EXISTING,
                0,
                None
            )
            result = (win32file.ReadFile(pipe, 64 * 1024)[1].decode()).lower()
            if "true" in result:
                STTOn = True
                STT_Thread.start()
            elif "false" in result:
                STTOn = False
                
            win32file.CloseHandle(pipe)
        except Exception as e:
            time.sleep(0.5)
                

def start_server():
    check_pipes()

if __name__=='__main__':
    start_server()