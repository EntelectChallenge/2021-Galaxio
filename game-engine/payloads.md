payloads

[← back to game rules](game-rules.md "The readme file that explain the game rules")

### Json File [*example*](./example-assets/state.json "An example of the JSON state file")

#### A file called "state.json", containing the following game details:
* currentRound → *The current round number.*
* maxRounds → *The maximum number of allowed rounds for this match.*
* player → *Your player's details:*
    * id → *The Id number of your player.*
    * position → *Your player's current position:*
        * y → *The current lane your player is in.*
        * x → *The current block down the track your player is in.*
    * speed → *Your player's current speed.*
    * state → *The state your player is in after the last command.*
    * powerups → *A list of powerups your player has collected.*
    * boosting → *Shows whether or not your player is currently using a boost*
    * boostCounter → *Shows the number of rounds left for which your player will remain in boosted speed*
* opponent → *Your opponent's details:*
    * id → *The Id number of your opponent.*
    * position → *Equivalent to the player position element, describing where the opponent is in the map.*
    * speed → *Your opponent's current speed.*
* worldMap → *An Array of objects describing each block in the visible map.*

#### World Map Cell block properties

The world map is made up of an array of objects. Each object defines a block in the map. Those objects properties are defined below:

* position → *Where this block is in the map*
    * y → The block y co-ordinate, 1 <= y <= 4
    * x → The block x co-ordinate. 1 <= x <= *block with finish line*

* surfaceObject → *Defines what is on this block.*
* occupiedByPlayerId → *Corresponds to the ID of the player in this block. Is 0 if no player is on the block*

##### Surface Object Options
The surface object is defined by an enumerator. The value will be an integer in the state file. They correspond to the following:

*  EMPTY = 0
*  MUD = 1
*  OIL SPILL = 2
*  OIL ITEM = 3
*  FINISH LINE = 4
*  BOOST = 5

The effects of these are defined in the game rules.

### Console [*example*](assets/example-state/console.txt "An example of the console file")

* round → *The current game round*
* player → *All the information about the given player. The data is the same as in the JSON file.*
* opponent → *Limited info opponent your opponent (position and speed). The data is the same as in the JSON file.*
* *A map drawn characters per map cell, to describe where entities are*:
    * 1 or 2 if block is occupied by player 1 or 2
    * ░ block is empty
    * ▓ block has mud
    * » block contains a boost powerup
    * ║ block is the finish line
    *  block contains an oil barral power up Φ
    * █ block contains an oil spill obstacle