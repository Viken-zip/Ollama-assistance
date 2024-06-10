import ollama

import os
import time
import random
import win32pipe, win32file

# pipes to send from python to C#
stt_loaded_pipe_name = r'\\.\pipe\STTLoaded_pipe'
stt_result_pipe_name = r'\\.\pipe\STTResult_pipe'
ai_answer_name = r'\\.\pipe\AIAnswer_pipe'

#pipes to get from C# to python
ai_question_pipe_name = r'\\.\pipe\AIQuestion_pipe'
stt_on_pipe_name = r'\\.\pipe\STTOn_pipe'

shutdown_flag = 'shutdown.flag'

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
        model='llama3', #llama3 dolphin-mixtral dolphin-llama3 llama3-prompt1 llama3-prompt-demon1
        messages=messages
        )
    return response['message']['content']

def check_pipes():
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

def start_server():
    while True:
        if os.path.exists(shutdown_flag):
            os.remove(shutdown_flag)
            break

    #if not win32pipe.WaitNamedPipe(ai_question_pipe_name, 1000):
    #    continue
        check_pipes()
        #pipe("hello?", ai_answer_name)
        #time.sleep(0.25)

if __name__=='__main__':
    start_server()