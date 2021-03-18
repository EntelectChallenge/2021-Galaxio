# Game Logger

This project contains the Game Logger for the Entelect Challenge 2021

## About the Game Logger

The Game Logger is used to write logs after a match for the Game State at each tick to be used for debugging and input for the visualiser. The Game Logger connects to the Game Runner
in the same manner as a bot with the exception that it happens at the very start along with the Game Engine, together these are key components for each match.

Once the Game Logger is connected to the Game Runner it will save the current Game State in memory and write a file once the match is finished. The Game Logger 
also serves as an Exception Logger for any Game Engine related exceptions.

As soon as the match is finished, the logging process needs to complete before sending a response to the Game Runner to start the shutdown process of all the Game Components.