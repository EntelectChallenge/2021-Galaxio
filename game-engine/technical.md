# Game Engine

## Building the project

To build this project you will need to install SBT (1.3.8) which can be found [here]https://www.scala-sbt.org/download.html .
Afterwhich you can run the build.sh. This will output a fat game-engine.jar that can be used by the game-runner.

### Prerequisites

JDK 1.8
- [Windows](https://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html)
- [Linux](https://openjdk.java.net/install/)

### IDE Support
We recommend using IntelliJ Idea

## Language

The game engine is built in Scala.

The project consists of 2 source sets:

### Main
The language used here is Scala.
This where the logic for the game engine lives.
It is built by satifying all the contracts specified by za.co.entelect.challenge.game.contracts package.

### Test
The framework used for this is Scala-test
This is where the unit tests for the various parts of the game live. They are split into:
- commands: these are tests that ensure player commands have the intended effects
- map_objects: these are tests that ensure player interactions with the world have the intended effects

## Documentation
### Scala
* The docs can be found [here]https://www.scala-lang.org/

### Scala-test
* The docs can be found [here]http://www.scalatest.org/