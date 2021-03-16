#include "../enums/PlayerActions.h"
#include <iostream>
#include <optional>

#ifndef CPPBOT_PLAYERACTION_H
#define CPPBOT_PLAYERACTION_H

struct PlayerAction
{
    std::string playerId;
    PlayerActions action;
    int heading = 0;
};

#endif //CPPBOT_PLAYERACTION_H
