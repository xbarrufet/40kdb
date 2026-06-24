## Description
The goal of this spec is to create the layout of the main app and the core function og famte data

## Layout
Add a sidebar in the left side of the screen 
Layout is
TopBar
--------
sideBar | [ BreadcrumBar
            Page Area]
initially with this 2 entries
- Home (Home icon)
- Games (Dices icon)
We kepp th Home screen with just a message

On top of the screen ther

## Games
This screen shows the main stucture of the game. There will be different games in the system, even tought we start with just "Warhammer 40K 11th edition"
The screen show a dropdown on top with the different games, one is selected we show a list of the different games faction with the number of units all of them has
If we click on a facction a new screen appears with the diffeent units and points


## Database
The structure is 
Games(gameId, Name) --> Factions(factionId, factionName, factionGroup, gameId) --> Units (unitI, unitName, points, factionId)
In the case of 40K factionGroup are "Imperium","Space Marines", "Chaos" and "Xenox"


## Backend
Add apis to
- GET  /games/ :Get the games
- GET/games/<gameId>: get the factions
- GET games/<gameId>/<factionId> Get The Units
- POST  /games/<gameName> create a new game
- PUT  /games/<gameId> changes the name of the game
- DELETE  /games/<gameId> deletes a game

- GET/games/<gameId>: get the factions
- POST /games/<gameId>:adds  the factions via JSON, it can be a single one or a group, if exists-->replace

- GET games/<gameId>/<factionId> Get The Units
- POST games/<gameId>/<factionId> Add the units via JSON, it can be a single one one or a group, if exists-->replace
- PUT games/<gameId>/<factionId> udpate name and points
- DELETE games/<gameId>/<factionId> deletes faction 




