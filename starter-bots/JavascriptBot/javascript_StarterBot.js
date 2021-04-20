const signalR = require("@microsoft/signalr");
const token = process.env["REGISTRATION_TOKEN"] ?? createGuid();
let url = process.env["RUNNER_IPV4"] ?? "http://localhost";
url = url.startsWith("http://") ?  url : "http://" + url
url += ":5000/runnerhub";
let _bot = null;
let _playerAction = null;
let _gameState = null;
let _playerKeys = null;
let _gameObjectKeys = null;

let connection = new signalR.HubConnectionBuilder()
    .withUrl(url)
    .configureLogging(signalR.LogLevel.Information)
    .build();

async function start () {
    try {
        await connection.start();
        console.assert(connection.state === signalR.HubConnectionState.Connected);
        console.log("Connected to Runner");
        await connection.invoke("Register", token, "JSNickName");
    } catch (err) {
        console.assert(connection.state === signalR.HubConnectionState.Disconnected);
        onDisconnect();
    }
};

connection.on("Disconnect", (id) => onDisconnect());

connection.on("Registered", (id) => {
    console.log("Registered with the runner");
    _bot = { id: id, data: [] };
});

connection.on("ReceiveGameState", gameStateDto => {
    console.log("GameState received", Object.keys(gameStateDto));
    _gameState = gameStateDto;

    //Get the ID's of all players
    _playerKeys = Object.keys(_gameState.playerObjects);

    if (!_playerKeys.includes(_bot.id)) {
        console.warn("I am no longer in the game state, and have been consumed");
        onDisconnect();
        return;
    }

    _bot = {id: _bot.id, data :_gameState.playerObjects[_bot.id]};
    _gameObjectKeys = Object.keys(_gameState.gameObjects);

    const foodByDistance = _gameObjectKeys
        .filter(key => _gameState.gameObjects[key][3] === 2)
        .sort((a, b) => getDistanceBetween(_gameState.gameObjects[a], _bot.data) - getDistanceBetween(_gameState.gameObjects[b], _bot.data));

    const closestFood = _gameState.gameObjects[foodByDistance[0]];
    console.log("Closest food:", closestFood);
    console.log("Me: ", _bot);
    
    const heading = getHeadingTo(closestFood)
    _playerAction = {
        PlayerId: _bot.id,
        Action: 1,
        Heading: heading
    };
    console.log("Send Action", _playerAction);
    connection.invoke("SendPlayerAction", _playerAction);
});

connection.on("ReceivePlayerConsumed", () => {
    console.log("You died");
});

connection.on("ReceiveGameComplete", (winningBot) => {
    console.log("Game complete");
    onDisconnect();
});

// Start the connection.
start();

function createGuid () {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c === "x" ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function onDisconnect () {
    console.log("Disconnected");
    connection.stop().then();
}

function getDistanceBetween (go, bot) {
    const triX = Math.abs(bot[4] - go[4]);
    const triY = Math.abs(bot[5] - go[5]);
    const distance = Math.ceil(Math.sqrt(triX * triX + triY * triY));
    return distance;
}

function getHeadingTo (go) {
    const v = Math.atan2(go[5] -_bot.data[5], go[4]- _bot.data[4])
    let direction = (v * (180 / Math.PI));
    direction = (direction + 360) % 360;
    return Math.round(direction);
}
