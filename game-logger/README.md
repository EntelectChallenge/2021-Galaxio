# Game Engine

This project contains the Game Engine for the Entelect Challenge 2020

## Game Rules

The game for 2020 is Overdrive. Detailed game rules can be found [here](game-rules.md)

## Building the project
To build this project simply run `build.sh` in the project root directory.
The game-engine.jar that is needed by the game runner can be found in the project root directory.
You can inspect the state of the build in the build.log that will be created in the project root folder

## Useful commands for development
If you want to compile the project you can run ```sbt compile```.  
If you want to create a jar file you run ```sbt package```.  
If you want to build a fat jar with the dependencies inside you run ```sbt assembly```.

You can find more information on the project structure and technologies [here](technical.md) 