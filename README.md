# Entelect Challenge 2021 - Galaxio

The Entelect Challenge is an annual coding competition where students, professional developers, and enthusiasts develop an intelligent bot to play a game.

This year, the game is Galaxio!

We have made it as simple as possible to get started. Just follow the steps below.

## Step 1 - Download
Download the starter pack zip file and extract it to the folder you want to work in.

## Step 2 - Run
Follow the instructions in the readme files to run your first match between 4 of the provided starter bots.

## Step 3 - Upload
Sign up to the player portal [here](https://challenge.entelect.co.za/login), and follow the steps to upload your bot to take part in tournaments!

## Step 3 - Improve
Change some of the provided logic or code your own into one of the given starter bots and upload it to the player portal, and see how you stack up against the rest!

## WIN!!!
For more information, visit one of the following:

[Our website](https://challenge.entelect.co.za)

[Our forum](https://forum.entelect.co.za)

Or read the rules in the [game-rules.md](./game-engine/game-rules.md) file.

## Project Structure

In this project you will find everything we use to build a starter pack that you can use to run a bot on your local machine.  This project contains the following:

1. **engine** - The engine is responsible for enforcing the rules of the game, by applying the bot commands to the game state if they are valid.
2. **runner** - The runner is responsible for running matches between players, calling the appropriate commands as given by the bots and handing them to the engine to execute.
2. **logger** - The logger is responsible for capturing all changes to game state, as well as any exceptions, and write them out to a log file at the end of the game.
4. **starter-bots** - Starter bots with limited logic that can be used a starting point for your bot. This folder also contains a bot called `ReferenceBot`, you can use this bot to test your bot against!
5. **ec-compose** - This folder contains all files required to run this year's game in docker. Refer to the [readme in this folder](./ec-compose/README.md) on how to use it.

This project can be used to get a better understanding of the rules and to help debug your bot.

Improvements and enhancements will be made to the game engine code over time.  The game engine will also evolve during the competition after every battle, so be prepared. Any changes made to the game engine or rules will be updated here, so check in here now and then to see the latest changes and updates.

The game engine has been made available to the community for peer review and bug fixes, so if you find any bugs or have any concerns, please [e-mail us](mailto:challenge@entelect.co.za) or discuss it with us on the [Challenge forum](http://forum.entelect.co.za/), alternatively submit a pull request on Github and we will review it.

## Starter Pack
The starter pack will provide you with everything that you'll need to run your first bot and compete in this year's challenge. To get the starter pack, simply download the latest release found [here](https://github.com/EntelectChallenge/2021-Galaxio/releases/latest).

More information on how to use this starter pack is included in the readme you'll receive inside it, as well as [here](./starter-bots/README.md)

## Submission Process

This year, we have introduced a new way to automate submissions through GitHub!

For more information, sign up for the player portal [here](https://challenge.entelect.co.za/login), and follow the steps!


## Right, I'm ready. How do I build my own bot?

Great! We're super excited to see how well you do!
Please read through [here](./starter-bots/README.md) for everything you need in order to build your own bot. 