import ollama

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