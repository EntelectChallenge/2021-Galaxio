#include <iostream>
#include "Position.h"
#include "../enums/ObjectTypes.h"

#ifndef CPPBOT_GAMEOBJECT_H
#define CPPBOT_GAMEOBJECT_H

struct GameObject
{
    std::string id;
    int size;
    int speed;
    int currentHeading;
    Position position;
    ObjectTypes gameObjectType;
};

#endif //CPPBOT_GAMEOBJECT_H
