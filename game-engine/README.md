# Game Engine

This project contains the Game Engine for the Entelect Challenge 2021

## Game Rules

The game for 2021 is Galaxio. Detailed game rules can be found [here](game-rules.md)

## Configuration Options

The engine will respect the following environment variables to change how the game is run:

- `BOT_COUNT`
    - This sets the expected amount of bots to connect before a game will be run

When these are not specified, the values present in `appsettings.json` will be used.

The game map is additionally dynamically generated based on the number of bots given.

The following fields in `appsettings.json` will be multiplied by the bot count given to generate the final world:

- Map Radius
- Max Rounds
- Start Radius
- Total starting food (WorldFood.StartingFoodCount)
- Gas Cloud Max Count
- Gas Cloud Modular
- Asteroid Fields Max Count
- Asteroid Fields Modular

To compute the values of these fields at runtime, each of these fields has a corresponding `fieldnameRatio` named config item in `appsettings.json`
This is ratio field is then multiplied by the bot count to generate the final world the game will start with.

