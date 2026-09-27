A token is a small chunk of text that the model reads and writes.
About ¾ of a word (roughly 4 characters). "Hello world" is about 2–3 tokens.
context window is The maximum number of tokens in one call: system prompt + history + your message + the answer. Anything beyond that, the model can't see.
statelessness is the inability to remember everything from previous chats.It remembers nothing between calls. The app has to resend the history every time
prompt eval shows different numbers every time.The count went up when you sent the history (step 17 vs step 16). More history means more tokens on every call, which means more cost.