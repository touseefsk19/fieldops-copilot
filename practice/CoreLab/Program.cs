string? maybe = Environment.GetEnvironmentVariable("NOT_SET");
Console.WriteLine(maybe?.Length ?? 0);   // ?. = safe access, ?? = default if null