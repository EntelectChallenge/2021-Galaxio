#include <iostream>
#include "signalrclient/hub_connection.h"
#include <thread>

#ifndef CPPBOT_LOGGER_HPP
#define CPPBOT_LOGGER_HPP

class Logger : public signalr::log_writer {
public:
    void write(const std::string &entry) override {
        std::cout << entry;
    }
};

#endif //CPPBOT_LOGGER_H
