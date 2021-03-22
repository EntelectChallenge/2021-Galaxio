# Building a bot

This year, the way bots are built and interact with the Runner is entirely new. For more information on how the Runner works, please see [here](./game-runner/README.md)

***N.B. It is highly recommended to start with a starter bot and proceed from there, as the below steps 1 through 3 are completed for you.***

All examples will be provided in C#, assuming a runtime of .NET 5

## Preface

The bots communicate with the Runner over a WebSocket framework called SignalR. More details about SignalR can be found [here](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-5.0) 

In essence, the runner will start up, connect to its components, and then wait until the required amount of bots have connected to it before starting a match. 

Matches are run asynchronously to bot commands, meaning that the runner and engine to not wait for commands from your bot to process the next state of the world (much like a modern multiplayer game).

The runner and engine can also be configured to expect as many bots in a match as you like. More information on configuring the Runner can be found [here](./game-runner/README.md#-configuration-options), and configuration options for the Engine can be found [here](./game-engine/README.md)

## Step 1 - Implementing SignalR and getting connected to the runner.

First, find your SignalR client library. As above, these examples will be for the .NET Core Client. There are client libraries in each of the starter bots, so it's advisable to reuse that library from your chosen language.

***N.B. There are separate client libraries for hubs running .NET Core and .NET Framework. The Runner is running on .NET Core, and thus you must ensure your SignalR library is a .NET Core compatible library***

### 1.1 Get the runner's IP address from the environment

This step is needed for running your bot in our cloud infrastructure.
You need to get the environment variable called "RUNNER_IPV4", and use that to connect to the runner. 

When running a match on your local machine, you can default this value to `http://localhost`.

The runner will always use port `5000`.

Getting the Environment variable:
```cs
string runnerIp = Environment.GetEnvironmentVariable("RUNNER_IPV4");
```

Setting it to the default for local matches:
```cs
runnerIp = string.IsNullOrWhitespace(runnerIp) ? "http://localhost" : runnerIp
```

Adding the port and hub endpoint:
```cs
string finalUrl = runnerIp + ":5000/runnerhub"
```


### 1.2 Build your connection to the hub:
```cs
connection = new HubConnectionBuilder()
                .WithUrl(finalUrl)
                .Build();
```

Notes:
- At this time, AutomaticReconnect is not supported. You must maintain your connection from beginning of the game to the end. 
- The Hub will always be present on port `5000/runnerhub`, regardless of environment

### 1.3 Register the events you need to listen to

All bots must listen to the following events that are announced by the hub:
- `Registered`
- `Disconnect`
- `ReceiveGameState`

*More information on each of these events can be found [here](./game-runner/README.md#events)*

You can optionally also listen to the following events:
- `ReceivePlayerConsumed`
- `ReceiveGameComplete`

Example of registering an event listener:
```cs
connection.On<Guid>("Registered", (Guid Id) =>{ /* callback to handle the event*/);
```

### 1.4 Start your connection
Once you have all of your listeners registered to the connection, its time to kick it off!

```cs
await connection.StartAsync()
```

This will begin your connection to the Runner's SignalR Hub, and allow the runner to communicate to you as well as allow you to send actions to the Runner.

## Step 2 - Let the Runner know who you are

Once a connection is established through SignalR, your bot needs to tell the runner who it is.

When running a match in the cloud, we use a token to provide a layer of security, authorising you to play the match.
You can think of this token much like a ticket into the Cinema when going to watch a movie.

When on your local machine, you can just generate a guid to satisfy this requirement.

This is also when you will tell the Runner what you like to be called.

This Token is a GUID. More information on what that is, and example generator, [here](https://www.guidgenerator.com/)

### 2.1 Get your token from your environment

Retrieve the token:
```cs
string registrationToken = Environment.GetEnvironmentVariable("Token");
```

Set it to a default if it is on your local machine:
```cs
token = !string.IsNullOrWhiteSpace(token) ? token : Guid.NewGuid().ToString();
```

### 2.2 Register with your token and your Nickname

Great! Now we can let the runner know who we are. 

Send a message to the runner on the "Register" endpoint.
```cs
connection.SendAsync("Register", token, "LordCopyPasta");
```
*Note: Your client library might call this "Invoke" rather than "Send". There is a slight difference between them, but it has no impact on this game or the way your commands are processed*

Once this process complets, the Runner will send an event to your `Register` listener we created earlier.

In this callback, you will receive a new GUID.

*** This GUID is the GUID used to represent your bot during the match ***

*** NB: The GUID you receive back IS NOT the same as the Token. Do not try to use the token for any other purpose than registering. You won't see it again***

## Step 3 - Make some decisions and play the game!

Now that you are set up and registered, you are ready to play the game.

Once all bots have connected, a "Tick 0" Game State will be published on the `ReceiveGameState` listener.

The engine will then pause for 5 seconds before starting the game, giving you time to establish any sort of map vision you need prior to starting.

You already should have registered a listener and handler for the `ReceiveGameState` listener from Step 1.3, so let's take a look at how you can send commands

The Structure of this payload can be found [here](./game-engine/game-rules.md#game-tick-payload)

### 3.1 Send a command to the Runner

Commands can be sent by sending a `PlayerAction` message to the runner.
```cs
connection.SendAsync("SendPlayerAction", playerAction);
```
*Note: Your client library might call this "Invoke" rather than "Send". There is a slight difference between them, but it has no impact on this game or the way your commands are processed*

The structure of the `playerAction` payload can be found [here](./game-engine/game-rules.md#-command-structure)

### 3.2 Send as many as you like!

The above can be called at any time, and the engine does not wait to hear from your bot before processing.

Note that you can wait as many ticks between commands as you like, however, you may only issue one command per tick. Any commands given after the first are ignored.

## Step 4 - WIN!!!

Congratulations! You now have all the pieces in place to complete your bot.

The only thing left to do, is to implement some cutting edge AI, win the challenge, and then take over the world!