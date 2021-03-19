# Game Runner

## Contents
- [Game Runner](#game-runner)
    - [Overview](#overview)
    - [Getting started](#getting-started)
        - [Prerequisites](#prerequisites)
        - [Installation](#installation)
            - [Windows](#windows)
            - [Linux](#linux)
        - [Usage](#usage)
            - [Windows](#windows-1)
            - [Linux](#linux-1)
    - [Additional languages](#additional-languages)
    - [Runner Events](#runner-events)
    - [Runner Actions](#runner-actions)
    - [Abuse of SignalR](#abuse-of-signalr)
    - [Configuration Options](#configuration-options)

## Overview
The game runner is responsible for facilitating a match between bots. It can be seen as a proxy that relays information between the [bots](../starter-bots/README.md) and the [game engine](../game-engine/README.md). The game engine produces state information which the game runner passes onto the bots. Once the bots have processed the state and produced a command, that command is then consumed by the game runner and passed back to the game engine, this process continues until the match ends.

The bots used in a match will start up as clients which connect to the runner. These bots will run continuously throughout the match. The .NET Core SignalR Hub as a central point for all communication between the
bots, game engine and the logger which are all SignalR clients.

The game runner is used for both local matches as well as tournament matches. **Note**: The latest release of the game runner will be used to run the matches between contestants during the tournament.

## Getting started
### Prerequisites
- .NET Core 3.1
    - [Windows](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-3.1.407-windows-x64-installer)
    - [Linux](https://docs.microsoft.com/en-us/dotnet/core/install/linux)
    - [MacOS](https://dotnet.microsoft.com/download/dotnet/thank-you/sdk-3.1.407-macos-x64-installer)
- NuGet Packages used
    - Microsoft.AspNet.WebApi.Client
    - Microsoft.AspNetCore.SignalR.Core
    - Newtonsoft.Json

### Installation
A run-all.sh is provided in the starter-pack which can be used to start up the different game components (Runner, Engine and Logger)
The following files are also provided for each game component (eg. Runner)
    - GameRunner.dll
    - GameRunner.exe
    - GameRunner.pdb

#### Windows
Simply double click on `run-all.sh` to start a new game.
Alternatively, open the Command Prompt in the game runner directory and execute the following command:
```
app /build /run-all.sh
```

#### Linux
Simply open a terminal in the game runner directory and execute the following command:
```
make
```

### Usage
The `appsettings.json` file consists of all the necessary information required to run a match. The config file is already present when the starter pack is downloaded and will have default values for each of its fields. When running a match locally the correct values have to be set in the config.

The `appsettings.json` has the following fields:
- `Logging` - This section pertains to different levels of logging used within SignalR, for more information please visit https://docs.microsoft.com/en-us/aspnet/core/signalr/diagnostics?view=aspnetcore-3.1
    - `LogLevel`
        - `Default`: `Information`
        - `Microsoft`: `Warning`
        - `Microsoft.Hosting.Lifetime`: `Information`,
        - `Microsoft.AspNetCore.SignalR`: `Information`,
        - `Microsoft.AspNetCore.Http.Connections`: `Information`

- `AllowedHosts`: `*` - This allows client connections from any host

- `RunnerConfig`
    - `ClientCount`: 16 - This is the number of bots that is expected to connect for the game
    - `MaxRounds`: 2000 - This is the max number of rounds (ticks) before the game will end
    - `ComponentTimeoutInMS`: 60000 - After the runner starts up, the Engine and Logger must connect to the before this timeout is reached or the runner will shutdown

Not all of these fields need to be changed, as stated above, the starter pack will come with default values for these fields. The starter pack will always contain the latest version of the runner, as well as the runner`s configuration.

All that needs to change for a local match are the following:
- `ClientCount`
- `MaxRounds`

Once the correct fields are set a match can be run. Once again, a run-all.sh is provided to assist.

#### Windows
Simply double click on `run-all.sh` to start up a match.
Alternatively, open the Command Prompt in the game runner directory and execute the following command:
```
app /build /run-all.sh
```

TODO - update how games can be started in Linux and MacOS

#### Linux
Simply open a terminal in the game runner directory and execute the following command:
```
make run
```

## Additional languages

This year's game runs over SignalR Core, meaning any client library that implements this can be used. If there is a library for your language, you are free to use it!

However, if one of these languages is not currently in our support list [here](../starter-bots/README.md#-supported-languages), please open an Issue on github for your language and we will guide you on the process from there.


The following five languages are known supported:
- .Net Core (C#)
- Python
- Javascript
- Java
- CPP

The starter bots for each of these languages can be found [here](../starter-bots/)

## Runner Events

These are SignalR events that your bot can subscribe to.

### Mandatory:

- "Registered"
    - This will be called once your bot has successfully registered with the runner for the match
    - This will provide you with the ID the runner will use to represent your bot in the game state during a match
- "ReceiveGameState" 
    - Once every tick a gameState will be sent to all active bots via this action
- "Disconnect"
    - This will be called once your bot has been consumed and has been informed. You are required to disconnect your signalR connection at this point.
    - All further interactions from your bot will be ignored from this point onwards.

### Optional

- "GameComplete" 
    - Once the game completes, this action will be used to notify active bots with the final gameState before disconnecting all active bots
- "PlayerConsumed"
    - This serves as a notice when a bot has been consumed, which means it will no longer be active


## Runner Actions

These are endpoints available to your bot, with which it can communicate to the runner. All of these should be implemented to have a functioning bot.

- "Register"
    - This process requires an access token and a nickname. This should be called as soon as your SignalR connection to the runner is established.
- "SendPlayerAction"
    - This allows your player to issue its next action back to the runner.


## Abuse of SignalR

Repeated abuse of your SignalR connection to the Runner will result in your bot being blacklisted.

Once blacklisted, your bot will not be run in simulation matches for a time.
After this time has elapsed, your bot will be allowed to run as normal again.

If your bot is repeatedly marked for abuse, your bot will stand to be disqualified from running in tournaments.

Abuse of your SignalR connection is defined under the following:
- Failure to disconnect in a reasonable time after the runner has informed your bot of a disconnect request
- Attempts to flood or otherwise DOS the Hub with requests
- Attempts to spoof your commands as that of another bot
- Attempts to spoof your connection as that of another bot

## Configuration options

The runner will respect the following environment variables to change how you play the game:

- `BOT_COUNT`
    - This sets the expected amount of bots to connect before a game will be run
- `COMPONENT_TIMEOUT`
    - How long should the runner wait for the logger and engine to boot up before shutting down with a failure (in milliseconds)
- `BOT_TIMEOUT`
    - How long should the runner wait for all bots to connect before shutting down with a failure (in milliseconds)

When these are not specified, the values present in `appsettings.json` will be used.
