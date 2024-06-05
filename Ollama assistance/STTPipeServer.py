import os
import time
import random
import win32pipe, win32file

#fifo_path = 'STT_fifo'
pipe_name = r'\\.\pipe\STT_fifo'
shutdown_flag = 'shutdown.flag'

#if not os.path.exists(fifo_path):
#    os.mkfifo(fifo_path)

def run_server():
    while True:
        if os.path.exists(shutdown_flag):
            os.remove(shutdown_flag)
            break

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
        time.sleep(10)

if __name__=='__main__':
    run_server()