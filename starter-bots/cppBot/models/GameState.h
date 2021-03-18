#include <vector>
#include "GameObject.h"
#include "World.h"

#ifndef CPPBOT_GAMESTATE_H
#define CPPBOT_GAMESTATE_H

struct GameState
{
    World world;
    std::vector<GameObject> gameObjects;
};

#endif //CPPBOT_GAMESTATE_H
