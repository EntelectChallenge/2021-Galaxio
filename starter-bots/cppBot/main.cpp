#include "signalRHelper/SignalRHelper.h"
#include "service/BotService.hpp"
#include "uuid/UUID.hpp"

BotService botService;

/**
 * Logger Functions
 */

void log_game_state() {
    GameState gameState = botService.getGameState();
    auto now = std::chrono::duration_cast<std::chrono::milliseconds>(
            std::chrono::system_clock::now().time_since_epoch()).count();
    std::cout << "Timestamp: " << now << std::endl;
    std::cout << "World: " << std::endl;
    std::cout << "Center Point: x: " << gameState.world.centerPoint.x << ", y: " << gameState.world.centerPoint.y
              << std::endl;
    std::cout << "Radius: " << gameState.world.radius << std::endl;
    std::cout << "Game Tick: " << gameState.world.currentTick << std::endl << std::endl;

    std::cout << "Game Objects: " << gameState.gameObjects.size() << std::endl << std::endl;

    GameObject bot = botService.getBot();

    std::cout << "Bot: " << bot.id << std::endl;
    std::cout << "Bot - Speed: " << bot.speed << std::endl;
    std::cout << "Bot - Heading: " << bot.currentHeading << std::endl;
    std::cout << "Bot - Size: " << bot.size << std::endl;
    std::cout << "Bot - x: " << bot.position.x << ", y: " << bot.position.y << std::endl;
    std::cout << "===================================" << std::endl << std::endl;
}

void log_player_action(PlayerAction &playerAction) {
    std::cout << "PlayerId: " << playerAction.playerId << ", action: " << static_cast<int>(playerAction.action)
              << ", heading: " << playerAction.heading << std::endl;
}

void log_connection_state(signalr::hub_connection &connection) {
    switch (connection.get_connection_state()) {
        case signalr::connection_state::connected:
            std::cout << "ConState: Connected" << std::endl;
            break;
        case signalr::connection_state::connecting:
            std::cout << "ConState: Connecting" << std::endl;
            break;
        case signalr::connection_state::disconnecting:
            std::cout << "ConState: Disconnecting" << std::endl;
            break;
        case signalr::connection_state::disconnected:
            std::cout << "ConState: Disconnected" << std::endl;
            break;

    }
}

/**
 * Handler Functions
 */

void handle_disconnect(std::promise<void> &task) {
    // Other cleanup here
    std::cout << "Disconnected" << std::endl;
    // End the connection
    task.set_value();
}

void handle_bot_registered(const std::string &id) {
    std::cout << "Registered: " << id << std::endl;

    botService.setPlayerId(const_cast<std::string &>(id));

    GameObject botGameObject;
    Position botPosition;
    botPosition.x = 0;
    botPosition.y = 0;
    botGameObject.gameObjectType = ObjectTypes::Player;
    botGameObject.size = 10;
    botGameObject.id = id;
    botGameObject.position = botPosition;
    botService.setBot(botGameObject);
}

void handle_game_state(const signalr::value &m) {
    auto map = m.as_array()[0].as_map();
    GameState gameState;

    std::map<std::string, signalr::value> worldMap = map.find("world")->second.as_map();
    std::map<std::string, signalr::value> centerPointMap = worldMap.find("centerPoint")->second.as_map();

    std::map<std::string, signalr::value> gameObjectsMap = map.find("gameObjects")->second.as_map();
    std::map<std::string, signalr::value> playerObjectsMap = map.find("playerObjects")->second.as_map();


    gameState.world.centerPoint.x = centerPointMap.find("x")->second.as_double();
    gameState.world.centerPoint.y = centerPointMap.find("y")->second.as_double();
    gameState.world.currentTick = worldMap.find("currentTick")->second.as_double();
    gameState.world.radius = worldMap.find("radius")->second.as_double();

    for (auto const&[key, val] : gameObjectsMap) {
        /*Size,
        Speed,
        CurrentHeading,
        (int) GameObjectType,
        Position.X,
        Position.Y*/

        GameObject gameObject;
        gameObject.id = key;
        gameObject.size = val.as_array()[0].as_double();
        gameObject.speed = val.as_array()[1].as_double();
        gameObject.currentHeading = val.as_array()[2].as_double();
        gameObject.gameObjectType = static_cast<ObjectTypes>( static_cast<int>(val.as_array()[3].as_double()));
        gameObject.position.x = val.as_array()[4].as_double();
        gameObject.position.y = val.as_array()[5].as_double();

        gameState.gameObjects.push_back(gameObject);
    }

    for (auto const&[key, val] : playerObjectsMap) {
        /*Size,
        Speed,
        CurrentHeading,
        (int) GameObjectType,
        Position.X,
        Position.Y*/

        GameObject gameObject;
        gameObject.id = key;
        gameObject.size = val.as_array()[0].as_double();
        gameObject.speed = val.as_array()[1].as_double();
        gameObject.currentHeading = val.as_array()[2].as_double();
        gameObject.gameObjectType = static_cast<ObjectTypes>( static_cast<int>(val.as_array()[3].as_double()));
        gameObject.position.x = val.as_array()[4].as_double();
        gameObject.position.y = val.as_array()[5].as_double();

        gameState.gameObjects.push_back(gameObject);
    }
    botService.setGameState(gameState);
    log_game_state();
}

void handle_bot_action(signalr::hub_connection &connection, std::promise<void> &startTask) {
    // Compute next action
    botService.computeNextPlayerAction(startTask);
    // Get next action
    PlayerAction nextAction = botService.getPlayerAction();
    log_player_action(nextAction);
    SignalRHelper::send_player_action(connection, nextAction);
}

/**
 * Helper functions
 */
std::string getToken() {
    const char *token = std::getenv("Token");
    if (token == nullptr) {
        return uuid::generate_uuid_v4();
    } else {
        return token;
    }
}

void run() {

    BotService bService = botService;
    // Get SignalR Hub Connection
    signalr::hub_connection connection = SignalRHelper::initializeHubConnect();

    // Start Task promise
    std::promise<void> startTask;

    // Disconnected
    connection.on("Disconnect", [&startTask](const signalr::value &m) {
        handle_disconnect(startTask);
    });

    // On Bot Register
    connection.on("Registered", [](const signalr::value &m) {
        handle_bot_registered(m.as_array()[0].as_string());
    });

    // On GameState Received
    connection.on("ReceiveGameState", [](const signalr::value &m) {
        handle_game_state(m);
    });

    // On GameComplete Received
    connection.on("ReceiveGameComplete", [](const signalr::value &m) {
        //disconnect(startTask);
    });

    // Start the main connection
    connection.start([&connection, &startTask, &bService](const std::exception_ptr &exception) {
        if (exception) {
            try {
                std::rethrow_exception(exception);
            }
            catch (const std::exception &ex) {
                std::cout << "Exception when starting connection: " << ex.what() << std::endl;
            }
            handle_disconnect(startTask);
            return;
        }
        std::cout << connection.get_connection_id() << std::endl;

        std::this_thread::sleep_for(std::chrono::milliseconds(500));

        // Get initial token
        std::string token = getToken();
        std::cout << "Initial Token: " << token << std::endl;
        std::string nickname = "cppNickName";

        // Send register to Game Runner
        SignalRHelper::send_register(connection, token, nickname);

        // Main Bot Loop
        while (connection.get_connection_state() == signalr::connection_state::connected) {
            handle_bot_action(connection, startTask);
        }
    });
    startTask.get_future().get();
}

int main() {
    run();
    return 0;
}


