# The Game

The Game for Entelect Challenge 2021 is **Galaxio**.

In a match your **1** ship will compete against a **variable** amount of other ships in an ever decreasing circular map. The goal is to be the last ship alive.


## Contents
- [The Game](#the-game)
    - [The Map](#the-map)
        - [Visibility](#visibility)
        - [Boundary](#boundary)
    - [Objects](#objects)
        - [Food](#food)
        - [Super Food](#super-food)
        - [Wormholes](#wormholes)
        - [Gas Clouds](#gas-clouds)
        - [Asteroid Fields](#asteroid-fields)
        - [Torpedo Salvo](#torpedo-salvo)
        - [Supernova](#supernova)
        - [Teleport](#teleport)
    - [The Ship](#the-ship)
        - [Speed](#speed)
        - [Afterburner](#afterburner)
        - [Dark Matter Torpedoes](#dark-matter-torpedoes)
        - [Shield](#shield)
    - [Collisions](#collisions)
        - [Ship to ship collisions](#ship-to-ship-collisions)
        - [Food collisions](#food-collisions)
    - [Game Tick Payload](#game-tick-payload)
        - [GameObjects](#gameObjects)
    - [The Commands](#the-commands)
        - [All Commands](#all-commands)
        - [Command Structure](#command-structure)
        - [Command: FORWARD](#command:-FORWARD)
        - [Command: STOP](#command:-STOP)
        - [Command: START_AFTERBURNER](#command:-START_AFTERBURNER)
        - [Command: STOP_AFTERBURNER](#command:-STOP_AFTERBURNER)
        - [Command: FIRE_TORPEDOES](#command:-FIRE_TORPEDOES)
        - [Command: FIRE_SUPERNOVA](#command:-FIRE_SUPERNOVA)
        - [Command: DETONATE_SUPERNOVA](#command:-DETONATE_SUPERNOVA)
        - [Command: FIRE_TELEPORTER](#command:-FIRE_TELEPORTER)
        - [Command: TELEPORT](#command:-TELEPORT)
    - [Endgame](#endgame)
    - [Scoring](#scoring)
    - [The Math](#the-math)
        - [Distance Calculation](#distance-calculation)
        - [Direction Calculation](#direction-calculation)
        - [Rounding](#rounding)

## The Map

The Map is a cartesian plane stretching out in both positive and negative directions. The map will only deal with whole numbers. Your ship can only be in an integer x,y position on the map. The map center will be 0,0 and the edge will be provided as a radius, the maximum rounds of a game will be equal to the size of the radius.

The map contains the following objects:

* Players - yourself and the other opponents.
* Food - small objects that you need to collect to grow in size.
* Wormholes - two points on the map that are connected in both directions.
* Gas clouds - harmful zones that will hurt you while traversing through.
* Asteroid fields - hazardous zones that will slow you down you while traversing through.

### Visibility

The entire map will be visible at all times and will shrink as the boundary decreases.

### Boundary

The size of the map will shrink each game tick. Objects that fall outside of the map at the end of a game tick will be removed from the map. This excludes players, being outside of the map will reduce the players size by 1 each game tick.

* The map size reduction is the final action of the game tick.

## Objects

All objects are represented by a circular shape and has a centre point with X and Y coordinates and a radius that defines it size and shape.

### Food

The map will be scattered with food objects of size 3 which can be consumed by players.

* Food will not move.
* Food will be removed if it falls outside of the map.
* If a player collides with food it will consume the food in it's entirety and the player will increase by the same size as the food.

### Super Food
When food spawns on the map, it has a percentage chance to spawn as super food.

Super food increases your "absorption rate" as a config driven value currently set to 2, meaning you consume 2 times the size of other standard food you consume. This ability stays for a set number of ticks, currently 5.

The following values configure this feature:
* **ScoreRates (Item 6):** chance for super food to spawn.
* **superfoodConsumptionRate:** consumption multiplier for super food.
* **superfoodEffectDuration:** the amount of ticks that the effect lasts.
* **maxSuperfoodCount:** max amount of super food that can be spawned on a map.


**Example**:
You have bot size 10 which collides with a super food and increases by 3 to 13, in the next tick it collides with nothing. In the second tick it collides with standard food, the bot's size now increases by 6 to 19. As that was the second tick with consumption rate active the effect now expires.

### Wormholes

Wormholes exist in pairs, allowing a player's ship to enter one side and exit the other. Wormholes will grow each game tick to a set maximum size, when traversed, the wormhole will shrink by half the size of the ship traversing through it.

* Traversal will only be possible if the wormhole is larger than the ship.
* Traversing the wormhole is instantaneous, no penalties are applied to the player for using a wormhole.
* Momentum and direction is maintained through the wormhole.
* If one end of the wormhole pair falls outside of the map both will be destroyed.
* Wormhole pairings are not given but rather will need to be mapped by players through trial and error.

### Gas Clouds

Gas clouds will be scattered around the map in groupings of smaller clouds. Ships can traverse through gas clouds, however once a ship is colliding with a gas cloud it will be reduced in size by 1 each game tick. Once a ship is no longer colliding with the gas cloud the effect will be removed.

### Asteroid Fields

Asteroid fields will be scattered around the map in groupings of smaller clouds. Ships can traverse through asteroid fields, however once a ship is colliding with a asteroid cloud its speed will be reduced by a factor of 2. Once a ship is no longer colliding with the asteroid field the effect will be removed.

### Torpedo Salvo

When these appear in the map, they have been launched from another ship! Watch out for their path as collisions with them will deduct their current size from your size. They also collide with everything on the map, causing damage to anything in their path!

* They travel in a straight-line trajectory, in the given heading represented in their state
* They travel at a speed of 60
* They begin at a size of 10
* They continue on so long as they have any size left
* Size is deducted from them when they collide with anything except wormholes, as well as from whatever they collide with
    * The size they deduct from what they collide with is equal to the smaller object's size involved in the collision
    * Examples:
        * If the salvo collides with a food, and food is size 3, and the salvo is size 10, at the end of the collision the food will be size 0 and removed, and the salvo will be size 7
        * If the salvo collides with a player of size 20, and the salvo is size 10, the salvo will be size 0 and removed, and the player will be size 10. The player who fired the salvo will gain 10 size also.
        * If two salvos collide, one of size 5 and one of size 10, the one of size 5 will be size 0 and removed, and the other will be of size 5 and continue on its path.
* If they collide with a bot, they steal the size they consume and give it back to the original bot that fired them
* They will be immediately removed from the world as soon as they exit the world bounds.

**Wormhole Collisions**

* Torpedo salvos interact and traverse with wormholes the same way players do
* They will exit the wormhole at the opposite end and continue along their path
* They consume energy from the wormhole traversal, in the same manner players do

### Supernova

A supernova is an extremely powerful weapon, which only appears once per match!
A supernova pickup will spawn sometime between the first quarter and last quarter of the game.

This pickup will enable the player who has collected it to fire a supernova bomb!

The supernova bomb travels like a torpedo, but does not collide with anything while travelling.
The firing player can then send a follow up command to detonate the supernova, causing a large amount of damage to any players caught in the blast zone! Additionally, it will spawn a gas cloud in its fallout radius.

Once a player has collected the pickup, it cannot be used by any other player. No further pickups will spawn, either. Destroying a player who currently has the supernova pickup will destroy the pickup. 


### Teleport

A player can launch a teleporter in a direction on the map. The teleporter moves in that direction at speed 20 and doesn't collide with anything.

The player can then use another command to teleport to the location of their teleporter.

It gets destroyed if it reaches the end of the map.

It costs a player 20 size to launch a teleporter regardless of whether they teleport to it or not. A player starts with one teleporter and gets another every 100 ticks and can have a maximum of 10 at any point.

The following values configure this feature in the app settings file:
* **ChargeRate** 
* **StartChargeCount** 
* **MaxChargeCount** 
* **Size** 
* **Speed** 
* **Cost** 


---

## The Ship

Your bot is playing as a circular spaceship, that feeds on planetary objects and other ships to grow in size. Your ship begins with the following values:

* **Speed** - your ship will move x positions forward each game tick, where x is your speed. Your speed will start at 20 and decrease as the ship grows.
* **Size** - your ship will start with a radius of 10.
* **Heading** - your ship will move in this direction, between 0 and 359 degrees.

Your ship will not begin moving until you have given it a command to do so. Once given the first forward command it will continue moving at the ship's current speed and heading until the stop command is given. 

There is a minimum size of 5 for the ship if at any point the size is smaller than this the ship will be removed from the map and considered eliminated.

### Speed

Speed determines how far forward your ship will move each game tick. Your ship will not move when stopped or an initial FORWARD command has been issued, however your speed value will remain the same. Speed is inversely linked to size, a larger ship will move slower. Speed is determined by the following formula:
```
speedRatio = 200
speedRatio/bot.size = speed
```
With the result being rounded to ceiling and with a minimum of 1.


**Note** *the value 200 comes from the Speeds.Ratio in the appsettings.json and may change during balancing.*


### Afterburner

The afterburner will increase your ship's speed by a factor of 2. However this will also start reducing the size of your ship by 1 per tick.

* An active afterburner can cause self destruction if the ship's size becomes less than 5.

### Dark Matter Torpedoes

Your ship comes equipped with dark matter torpedos. These can be used to manipulate space around you, and lay down the charge to your enemies.

Torpedoes that hit objects in the world will destroy those objects, and when they land on your enemies, you steal matter from them proportionally to the damage done to them. 

* Your ship will receive 1 salvo charge every 10 ticks. 
* Your ship can only carry a maximum of 5 salvo charges at a tick. 
* Your ship can then trade a salvo charge for 5 size to fire a barrage of 10 torpedoes in your chosen direction.
* Firing a salvo of torpedoes while your ship is less than size 10 can cause your ship to self destruct if it becomes less than size 10. 

### Shield

For defence your ship can activate it's shield. Once enabled, it can defect torepedos, which bounce right off.

While torpedos will bouce off in the opposite direction. Your ship will be propelled in the direction the torpedo was orrinally heading.

Gas clouds and other phenomioa are not effected.

Once activated the sheild will last for the next 20 ticks and will consume a size of 20. They also require a recharge time of 20 ticks.

## Collisions

A collision will occur when two objects overlap by at least one unit of world space. This means that objects can touch each other, but the moment they overlap by a single world space unit they will be considered colliding and collision mechanics will apply.

### Ship to ship collisions

When player ships collide, the ship with the larger size will consume the smaller ship at the rate of 50% of the larger ship's size to a maximum of the smaller ship's size.

After a ship collision, both ships heading will be reversed by 180 degrees, they will be separated by 1 unit of world space and thereafter will continue to move in this new direction thereafter. This will simulate a bounce.

### Food collisions

When a ship collides with a food particle, the ship will consume the food in its entirety and will increase in size by the size of the food it just collided with.

## Game Tick Payload

All players will receive the state of the world, all game objects and all player objects at the start of each tick. The payload of each game tick will contain the following information:

```
{
  "World": {
      "CenterPoint": {
        "X": 0,
        "Y": 0
      },
      "Radius": 1000,
      "CurrentTick": 0
    },
    "GameObjects": {
      "8b77d46b-2844-48c4-a3f3-179de15776a3": [ 3, 0, 0, 2, 42, 225 ],
      "2b75d46b-2866-48f1-a4g5-179de15779o0": [ 3, 0, 0, 2, 234, -900 ],
      "9b34d46b-4844-48g2-a3b2-179de15771m8": [ 3, 0, 0, 2, -100, 189 ],
      ...
    },
    "PlayerObjects": {
      "ad672ef2-f6a7-404c-950a-a867c54c7de0": [ 10, 20, 0, 1, 42, 225, 1 ],
      "5f535caf-c3fc-4935-9d95-6e48b3680fd7": [ 20, 10, 90, 1, 234, -900, 2 ],
      "e671e725-4bbb-4d4d-ad18-f013a567dfda": [ 40, 5, 180, 1, -100, 189, 4 ],
      ...
    }
}
```

### GameObjects

The GameObjects list contains all objects on the map. The list contains of a guid for each object and the objects data.

The order of the data will not change, and is as follows:
* Size - The radius of the object
* Speed - The speed it is able to move at
* Heading - The direction it is looking towards
* GameObjectType - The type of object
    * 2: Food
    * 3: Wormhole
    * 4: Gas Cloud
    * 5: Asteroid Field
* X Position - The x position on the cartesian plane
* Y Position - The y position on the cartesian plane

### PlayerObjects

The PlayerObjects list contains all player objects on the map. The list contains of a guid for each object and the objects data.

The order of the data will not change, and is as follows:
* Size - The radius of the object
* Speed - The speed it is able to move at
* Heading - The direction it is looking towards
* GameObjectType - The type of object
    * 1: Player
* X Position - The x position on the cartesian plane
* Y Position - The y position on the cartesian plane
* Active Effects - Bitwise effects currently on affect of the bot
    * This is a cumulative bit flag, represented by:
        * 0 = No effect
        * 1 = Afterburner active
        * 2 = Asteriod Field
        * 4 = Gas cloud 
    * For example, if a ship has all three effects the active effect will be 7.

## The Commands

In each game tick each player can submit one command for their ship.

* All player commands are validated before executing any commands. Invalid commands (eg. Invalid syntax) result in the command being ignored and your ships maintaining it's current state.
* All player's commands are executed at the same time (in a single game tick), and not sequentially.

### All Commands

* FORWARD
* STOP
* START_AFTERBURNER
* STOP_AFTERBURNER

### Command Structure

Commands can be sent to the engine as often as you like, but the engine does not wait for commands from the bots and processes the game state at a set rate.

The Runner will only allow a maximum of one command per tick.

Your commands will be lodged against your bot for processing during the next tick. These actions are processed under FIFO (First in, First out), meaning your earliest sent action is processed first. However, this list of actions should only ever be one command long.

This means that your bot does not need to send a command each tick and can take as long as it wants to send each command. Feel free to run all the clever artificial intelligence you like! Just note that other bots might still be sending commands as there is no wait time.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000",//string or GUID, not required
  "action": 1,// int,
  "heading": 45 // int
}
```


### Command: FORWARD

```
FORWARD: 1, 238
```

This command will start moving your ship in the direction provided in degrees.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000",//string or GUID, not required
  "action": 1,// int,
  "heading": 45 // int
}
```

### Command: STOP

```
STOP: 2
```

This command will stop moving your ship this game tick until another movement command is issued. There is no interia, therefore your ship stops instantly.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000",//string or GUID, not required
  "action": 2,// int,
  "heading": 0 // int
}
```

### Command: START_AFTERBURNER

```
START_AFTERBURNER: 3
```

This command activates your ship's afterburner. Note, your speed is only used when you're currently moving in a direction, therefore afterburner will only have an effect when you are moving. The cost of afterburner will always be in effec, if the after burner effect is active.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000",//string or GUID, not required
  "action": 3,// int,
  "heading": 0 // int
}
```

### Command: STOP_AFTERBURNER

```
STOP_AFTERBURNER
```

This command deactivates your ship's afterburner.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000",//string or GUID, not required
  "action": 4,// int,
  "heading": 0 // int
}
```

### Command: FIRE_TORPEDOES

```
FIRE_TORPEDOES
```

This command consumes 1 salvo charge and 5 size to send out a salvo of torpedoes in the given heading

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 5, // int,
  "heading": 75 // int
}
```

***Note:** the state will reflect a single object of size 10 to represent your torpedo salvo*

### Command: FIRE_SUPERNOVA
```
FIRE_SUPERNOVA
```
This command consumes supernova pickup to send out a supernova in the given heading

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 6, // int
  "heading": 75 // int
}
```

### Command: DETONATE_SUPERNOVA
```
DETONATE_SUPERNOVA
```
This command detonates existing supernova

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 7 // int
}
```

### Command: FIRE_TELEPORTER

```
FIRE_TELEPORTER
```
This command consumes 1 teleport charge and 20 size to send out a teleporter in the given heading

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 8, // int,
  "heading": 75 // int
}
```

### Command: TELEPORT

```
TELEPORT
```
This command teleports you to your existing teleporter if it exists.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 9 // int
}
```

### Command: USE_SHIELD

```
USE_SHIELD
```
This command activates your shield to deflect incoming torpedoes, if you have one available.

Example Payload in JSON with types:
```jsonc
{
  "playerId": "00000000-0000-0000-0000-000000000000", //string or GUID, not required
  "action": 10 // int
}
```
## Endgame

The last ship alive is the winning ship and thus the winning bot.

* Should the final two ships both die at the same time, the ship with the highest score will be the winner.

## Scoring

A score will be kept for each player which will be visible once the match is over. This will only be used for tie breaking.

* Consuming another player's ship = 10 points
* Consuming food = 1 point
* Traversing a wormhole = 1 point

***Note** these values are representative only and are subject to change during balancing. Final values can be found in the appsettings.json of the game-engine folder.*

## The Math
This section is to explain the general math used for the movement and placement calculations by the engine, as well as basic functions to calculate this. These formulas have been included as functions in each starter bot.

The world uses the standard mathematical cartesian plane with the 0 degree at the rightmost axis and it positively increments counter clockwise as can be seen below:

![cartesian plane](https://github.com/Jana-Wessels/images/blob/master/cartesian_plane.png?raw=true)

### Distance Calculation
Distance is calculated using the standard pythagoras theorem, a diagram showing the values can be seen below:

![cartesian plane](https://github.com/Jana-Wessels/images/blob/master/distance_calculation.png?raw=true)

Direction does not matter in the distance calculation as we are only concerned with the distance between two objects regardless of where they are.
Where delta y and delta x:
```
ΔX = X1 - X2
ΔY = Y1 - Y2
```
It does not matter if the delta's are negative as that will be cancelled out in the formula through squaring them. The formula for the distance calculation is as follows:

```
distance = Math.Sqrt((deltaX ^ 2) + (deltaY ^ 2)) 
// or
distance = Math.Sqrt((y2 - y1)^2 + (x2 - x1)^2)
```
This formula can be used to calculate the distance between objects in the world. Note this calculation gives the distance between the centrepoints of the objects so their radius is not taken into account with this calculation.

### Direction Calculation
Direction is calculated using the arctan2 formula to get the direction between two objects. Note this formula returns a value between -180 and 180, and so an adjustment formula is explained further below.

The formula also returns the direction in radians; see the formula below to convert radians to degrees:
```
private int toDegrees(radians)
{
    return Math.Round((radians * (180 / Math.PI)), 0);
}
```
***Note** the engine only accepts degrees.*

The arctan2 calculation to find the direction from object 1 to object 2 is as follows:
```
direction_inRadians = Math.Atan2(Y2 - Y1, X2 - X1)

direction_inDegrees = toDegrees(direction_inRadians)
```
***Note** that the order of the values is important otherwise you will get the direction from object 2 to object 1 which will be inverted by 180 degrees.*

This now need to be corrected for the possible negative values, this can be done by adding 360° to the calculated degrees and modulo it by 360. This will return a value between 0° and 360° which equates to the same cardinality as the calculated value.

```
corrected_direction_inDegrees = (cartesianDegrees + 360) % 360;
 
```

**Example:
-150° will be corrected to 210° which when referring to the image of the cartesian plane above is in the same position, note the placement of a negative heading would be clockwise from 0°.*


### Rounding
Due to the fact that the engine uses integers to represent the world rounding had to be applied to certain calculations that gave decimal results. 

The game engine uses C# which uses round-to-even or banker's rounding, which means any value that is at exactly at the midpoint ( x.5 ), will be rounded to the nearest even number.
Take the following examples:

- 23.5 will become 24
- 24.5 will become 24
- 24.6 will become 25
- 25.5 will become 26

A summary of those rounding decisions can be seen below:

####Distance between objects:
Uses standard Math.Round rounding.

####Position calculation:
Uses standard Math.Round rounding

####Speed calculations:
Uses Math.Ceiling rounding as there is a minimum that it shouldn't go below.

####Size calculations:
Uses Math.Ceiling rounding as there is a minimum that it shouldn't go below.

Note when doing rounding calculations in your own bot check the rules of the specific language as some use different rounding strategies to C#. 

No language will have a advantage as only integer values are used in commands and the engine calculates all bot commands in the same way.
This section is for reference to check that your bot calculations and decision points align with how the engine works.



**NB:
The values provided within this readme are subject to change during balance phases.
Entelect will endeavour to maintain this readme file. However the most accurate values can be found in appsettings.json within the game-engine folder.**


