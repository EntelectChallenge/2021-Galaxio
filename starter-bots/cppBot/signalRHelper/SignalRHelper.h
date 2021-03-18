#include <iostream>
#include <future>
#include "signalrclient/hub_connection.h"
#include "signalrclient/hub_connection_builder.h"
#include "../logger/Logger.hpp"
#include "../models/PlayerAction.h"

#ifndef CPPBOT_SIGNALRHELPER_H
#define CPPBOT_SIGNALRHELPER_H


class SignalRHelper {
private:
    static std::string getRunnerEndpoint();
public:
    static signalr::hub_connection initializeHubConnect();
    static void send_register(signalr::hub_connection &connection, std::string &token, std::string &nickname);
    static void send_player_action(signalr::hub_connection &connection, PlayerAction &action);
};


#endif //CPPBOT_SIGNALRHELPER_H
