# Entelect Challenge 2021 Galaxio Visualiser

This visualiser is aimed at helping you, the player, to make sense of what is going on during your games and process log files.

## Usage

For all use cases, you will need two files ready:

- The game state log
- The game complete log

These files should be situated on the same path on your OS.
The game complete log should be named as `gameState_gameComplete.json`, where `gameState` is the name of your game state log you want to visualise.

### Windows

1. Unzip `Galaxio-windows.zip`
2. Run the Galaxio.exe file
    - If you get a pop up from windows defender, choose `More Info` and then `Run Anyway`

### Linux

1. Unzip `Galaxio-linux.zip`
2. Run the Galaxio.x86_64 file

### MacOS

1. Unzip `Galaxio-macos.app.zip`
2. Run the Galaxio-macos.app file

### Next Steps

1. Go to Options and follow this process when opening the visualiser for the first time:
    - Enter path to the log files
    - Select camera angle (0 by default)
    - Toggle simple mode (Simple mode will use basic shapes instead of visual effects which are not available on Linux)
    - Save
2. Go back to the Main Menu
3. Go to Load and select the desired log file from the list
    - Only log files with a corresponding GameComplete.json will appear in this list
    - Click Start, this might take a few moments to load if the log is big
4. Once in the Game view, use the following hotkeys to control the camera:
    - W - Forward
    - S - Backward
    - A - Left
    - D - Right
    - Q - Rotate counter clockwise
    - E - Rotate clockwise
    - Mouse wheel - Zoom in and out
5. In-game UI:
    - Current tick - not interactable
    - Game speed - Adjust by clicking the + and - buttons to increase and slow down the game (1 to 5, but 1.5 is the most optimal speed)
    - Players - Select a player in the list to have the camera follow it around, once the player is no 
                longer active or the Reset camera button is clicked, the camera can be moved freely
    - Start - Play log forward
    - Pause - Pause the game
    - Rewind - Play log backward
    - Reset - Restart the log
    - Main menu - Go back to the main menu to change settings, select another log or quit the visualiser