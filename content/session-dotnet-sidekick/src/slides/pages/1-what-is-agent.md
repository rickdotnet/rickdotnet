---
layout: image-right
image: images/wizard.svg
backgroundSize: contain
---

# What's an Agent?

- ???
- ???
- ???

<!--
Kind of like agile, everyone has their own definition
-->

---
layout: center
class: "text-center"
hideInToc: true
---

## a computer application designed to automate certain tasks (such as gathering information online)

*[Merriam-Webster](https://www.merriam-webster.com/dictionary/agent)*

<!--
Notes
- works well
- not AI scoped

Too broad and doesn't fit the theme.

Hugging Face is next
-->

---
layout: center
class: "text-center"
hideInToc: true
---

## an AI model capable of reasoning, planning, and interacting with its environment

*[Hugging Face](https://huggingface.co/learn/agents-course/en/unit1/what-are-agents)*

<!--

If that was too broad, this is too narrow.

I think agents are simpler than that.
-->

---
layout: center
class: "text-center"
hideInToc: true
---

## an llm with tools

*AgentCon KC 2025 - Keynote*

<!--

If that was too broad, this is too narrow.

I think agents are simpler than that.
-->

---
layout: center
class: "text-center"
hideInToc: true
---

## It’s an LLM, a loop, and enough tokens

*[Thorsten Ball](https://ampcode.com/how-to-build-an-agent)*

<!--
I recently came across an article, which I've link with his name, but I'll also link on a further slide.

And at the end of the day, he's right. An agent is what you make it. Simple or complex.
-->

---
layout: center
class: "text-center"
hideInToc: true
---

## It's whatever you want it to be

*rickdotnet*

<!--
This is the simplest way to put it.
-->

---
layout: figure-side

figureUrl: images/llm-tokens.svg
hideInToc: true
---

# What's an Agent?

<v-clicks>

- an LLM, a loop, and enough tokens
    - https://ampcode.com/how-to-build-an-agent
- aka, whatever you want it to be

</v-clicks>

<!--
Replace image with the one we use in the previous slide

- give some props to the article, it uses Go and Anthropic API/tool calling
-->

---
layout: figure
figureCaption: Agent without an LLM
figureUrl: images/agent-loop0.svg
hideInToc: true
---

# What's an Agent?

<!--
One of the bits of feedback I got from the dry run was... 

**I'm still not sure what an agent is or how
it differs from a regular application.**

This is an agent without an LLM, but it still has a loop and can do things.
- it can read files
- it can write files
- it can call APIs
-->

---
layout: figure
figureCaption: Agent with an LLM
figureUrl: images/agent-loop1.svg
hideInToc: true
---

# What's an Agent?

<!--
And here's an agent with an LLM.
- it can read files
- it can write files
- it can call APIs

The only difference is that it has an LLM in the loop.

-->

---
layout: figure
figureCaption: Home is where you make it - Old Cajun Man
figureUrl: images/agent-loop2.svg
hideInToc: true
---

# What's an Agent?

<!--
At the end of the day, how the system is composed and the functions it provides IS the agent.

The only thing stopping your agent from talking to your thermostat is you... and good judgment.

The agent is more than just the loop, there are different entry points and a lot of indirection... but the box in the middle is a pretty good way to break down what's really happening.

For the purposes of this talk, we're going to call that box, Dusty.
-->