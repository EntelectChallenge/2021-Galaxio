# Starter Bots

Entelect provides starter bots for the following languages:

* .Net Core (C#)
* Python 
* Javascript
* Java
* C++

Starter bots are the bare essentials you need to get going with very little decision making capability. They are built to be able to do the following:

- Connect to SignalR
- Register with a token and nickname to the runner
- Listen for Game State Published events
- Respond to Game State Published events with a new random action

All Starter bots additionally include the base mathematic formulas you will need for your bot to find it's way around a map during the game.

The reference bot, on the other hand, is capable of playing a game from start to finish with some cleverness built in. This is there to help contestants who want something a bit smarter to work from, and also provides a benchmark against which to run your bot.

## Bot versions
- Java = 11 
- NodeJS = 14
- .Net core = 3.9
- Python = 3.9
    - Pytorch = 1.8.0
    - Tensorflow = 2.4.1
- C++ = 9.3.0

## Supported Languages

This year, we have a new submission process described in more detail [here](../README.md##Submission-Process).
In short, we have a new Automatic process, as well as a more traditional "upload it yourself" approach.

For each of these, the following languages are supported:

- Automatic
    - Java
    - NodeJS
    - .Net core
    - Python
    - Pytorch
    - Tensorflow
    - C++

- Manual
    - Java
    - NodeJS
    - .Net core
    - Python
    - Pytorch
    - Tensorflow

## Okay, I'm really ready now! How do I build my _own_ bot?

Alright, alright. I'll give up the secret.
Please read through [this](../building-a-bot.md) for everything you need in order to build your own bot. 