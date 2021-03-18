#include "../models/PlayerAction.h"
#include "../models/GameState.h"
#include <iostream>
#include <chrono>
#include <thread>
#include <future>

#ifndef CPPBOT_BOTSERVICE_HPP
#define CPPBOT_BOTSERVICE_HPP

class BotService {
private:
    GameObject bot;
    PlayerAction playerAction;
    GameState gameState;
public:
    void setPlayerId(std::string &id){
        playerAction.playerId = id;
    }

    PlayerAction getPlayerAction() const
    {
        return playerAction;
    }

    GameState getGameState()
    {
        return gameState;
    }

    GameObject getBot() const
    {
        return bot;
    }

    void setBot(GameObject& bot)
    {
        this->bot = bot;
    }

    void updateSelfState()
    {
        for (auto& gameObject : gameState.gameObjects)
        {
            if (gameObject.id == bot.id)
            {
                bot = gameObject;
                return;
            }
        }
    }

    void setGameState(GameState& gameState)
    {
        this->gameState = gameState;
        updateSelfState();
    }



    void calculateNextActionFromInput(std::promise<void> &startTask);

    void calculateNextActionFromRandom();

    void computeNextPlayerAction(std::promise<void> &startTask);
};

#endif //CPPBOT_BOTSERVICE_HPP
