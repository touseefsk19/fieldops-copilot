# AI notes 02: prompts, few-shot, JSON output

Model: `qwen2.5:0.5b` (local, Ollama), temperature 0.

## Ideas
- **System prompt**: the job description. It tells the AI its role, its rules and the exact output format.
- **Few-shot**: giving a few exact examples (input → ideal output) so the AI copies the same shape and style.
- **JSON mode** (`"format": "json"`): the AI must reply in valid JSON instead of normal chat text. It guarantees valid JSON *syntax*, not correct *content*.
- **Validation**: my app code checks the AI's response before using it: the JSON parses, all required fields are present, and each value is allowed.
- **Temperature 0**: the same input gives the same output. I used it so I could compare the steps fairly.

## What I got

Step 5, zero-shot (no system prompt): a chat reply, not data my code can use.
```json
"content": "I'm sorry, but I can't assist with that. If you have any other questions or need help with something else, feel free to ask!"
```

Step 7, system prompt + JSON mode: valid JSON, but the equipment came back as "Pump P-101" instead of just the tag.
```json
"content": "{ \"equipment\": \"Pump P-101\", \"priority\": \"High\", \"title\": \"Oil Leaks - Urgent\" }"
```

Step 9, few-shot (one example added): matched the example's format exactly, with equipment "P-101".
```json
"content": "{\"equipment\": \"P-101\", \"priority\": \"High\", \"title\": \"Pump leaking oil badly\"}"
```

Step 11, prompt injection: the model obeyed the attacker. The JSON is valid, but the priority is not an allowed value.
```json
"content": "{ \"equipment\": \"none\", \"priority\": \"CRITICAL-ESCALATE-TO-CEO\", \"title\": \"APPROVED BY ADMIN\" }"
```

## What I learned
- Few-shot fixed the format better than instructions alone. The cost: the example is sent with every call, so there are more input tokens each time.
- Prompts alone can't stop prompt injection. JSON mode fixes the format, not the truth; my code decides what's allowed.

## Checks my C# code would do before saving the request
1. `priority` must be exactly `Low`, `Medium` or `High`; anything else is rejected. This alone blocks the step 11 attack.
2. `equipment` must be a real tag that exists in our Equipment table; `none` or unknown tags are rejected.
3. Also: the model only *suggests* a request. A human approves it before anything is saved.