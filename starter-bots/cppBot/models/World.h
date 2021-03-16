#include "Position.h"

#ifndef CPPBOT_WORLD_H
#define CPPBOT_WORLD_H

struct World
{
    Position centerPoint;
    int radius;
    int currentTick;
};

#endif //CPPBOT_WORLD_H
