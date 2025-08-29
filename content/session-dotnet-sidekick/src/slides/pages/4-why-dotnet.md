---
layout: figure-side
figureUrl: images/dotnet.svg
---

# Why .NET?

<v-clicks>

- Enterprise-grade, Microsoft supported
- Rich ecosystem
- Extensions.AI
    - Modern
    - Vendor support:
        - OpenAI
        - Ollama
        - Groq
        - Azure

</v-clicks>

<!--

Make a few Microsoft jokes
- expand on some Microsoft points

Completions
Embeddings
- chatting with documents, etc
MCP
-->

---
layout: center
class: "text-center"
hideInToc: true
---

# Microsoft.Extensions.AI

<!--
They stay up to date with the tech and direction and you can stay up to date with them

What does it provide?
- ALOT

I can only bring up so many, so I took the entry point approach
-->

---
layout: figure-side
figureCaption: fat-free yogurt
figureUrl: images/yogurt.webp
---

# Microsoft.Extensions.AI

<v-click>
Fat-free:
</v-click>

<v-clicks>

- Major Vendor Support
    - LLMs
    - Vector Database Providers
- Capabilities
    - Completions
    - Embeddings
    - Model Context Protocol (MCP)
    - Tool calling
- Ecosystem
    - Caching
    - Rate limiting
    - Dependency injection

</v-clicks>

<!--
Examples of each of these

LLM
- as I mentioned, major vendors
Tool Calling
Caching
Rate limiting
Dependency Injection
-->

---
hideInToc: true
---

# Microsoft.Extensions.AI

[Tool calling](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#tool-calling)

```csharp {4,5,11,14|all}
using Microsoft.Extensions.AI;
using OllamaSharp;

string GetCurrentWeather() => Random.Shared.NextDouble() > 0.5 
    ? "It's sunny" : "It's raining";

IChatClient client = new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.1");

client = ChatClientBuilderChatClientExtensions
    .AsBuilder(client)
    .UseFunctionInvocation()
    .Build();

ChatOptions options = new() { Tools = [AIFunctionFactory.Create(GetCurrentWeather)] };

var response = client.GetStreamingResponseAsync("Should I wear a rain coat?", options);
await foreach (var update in response)
{
    Console.Write(update);
}
```

<!--
Explain the:
- UseFunctionInvocation() builder extension
- AIFucntionFactory.Create() method
-->

---
hideInToc: true
---

# Microsoft.Extensions.AI

[Caching](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#cache-responses)

```csharp {5,6|all}
var sampleChatClient = 
    new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.1");

IChatClient client = new ChatClientBuilder(sampleChatClient)
    .UseDistributedCache(new MemoryDistributedCache(
            Options.Create(new MemoryDistributedCacheOptions()))
    ).Build();

string[] prompts = ["What is AI?", "What is .NET?", "What is AI?"];

foreach (var prompt in prompts)
{
    await foreach (var update in client.GetStreamingResponseAsync(prompt))
    {
        Console.Write(update);
    }
    Console.WriteLine();
}
```

<!--
Explain huge impact this could have
- edge caching straight out of the box

- Powerful options
  - FusionCache
  - Redis

Or for the Microsoft shops, just stick everything in SqlServer
-->

---
hideInToc: true
---

# Microsoft.Extensions.AI

[Rate-limiting](https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#provide-options)

```csharp {2|all}
// https://learn.microsoft.com/en-us/dotnet/ai/microsoft-extensions-ai#provide-options
var client = new RateLimitingChatClient(
    new OllamaApiClient(new Uri("http://localhost:11434"), "llama3.1"),
    new ConcurrencyLimiter(new() { PermitLimit = 1, QueueLimit = int.MaxValue }));

Console.WriteLine(await client.GetResponseAsync("What color is the sky?"));
```

<!--
Concerned about costs or folks spamming your token usage? Fear not.
-->