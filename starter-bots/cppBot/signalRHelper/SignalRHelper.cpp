#include "SignalRHelper.h"

void SignalRHelper::send_register(signalr::hub_connection &connection, std::string &token, std::string &nickname) {
    std::vector<signalr::value> arr{token, nickname};
    signalr::value args(arr);

    if (connection.get_connection_state() == signalr::connection_state::connected) {
        connection.invoke("Register", args, [](const signalr::value &value, const std::exception_ptr &exception) {
            try {
                if (exception) {
                    std::rethrow_exception(exception);
                }
                if (value.is_string()) {
                    std::cout << "Received: " << value.as_string() << std::endl;
                } else {
                    std::cout << "Hub method invocation has completed" << std::endl;
                }
            }
            catch (const std::exception &e) {
                std::cout << "Error while sending data: " << e.what() << std::endl;
            }
        });
    }
}

void SignalRHelper::send_player_action(signalr::hub_connection &connection, PlayerAction &action) {
    std::map<std::string, signalr::value> map;
    map.insert(std::make_pair("PlayerId", action.playerId));
    map.insert(std::make_pair("Heading", static_cast<double>(action.heading)));
    map.insert(std::make_pair("Action", static_cast<double>(action.action)));

    std::vector<signalr::value> arr{map};

    signalr::value args(arr);
    if (connection.get_connection_state() == signalr::connection_state::connected) {
        connection.invoke("SendPlayerAction", args,
                          [](const signalr::value &value, const std::exception_ptr &exception) {
                              try {
                                  if (exception) {
                                      std::rethrow_exception(exception);
                                  }
                                  if (value.is_string()) {
                                      // std::cout << "Received: " << value.as_string() << std::endl;
                                  } else {
                                      // std::cout << "Hub method invocation has completed" << std::endl;
                                  }
                              }
                              catch (const std::exception &e) {
                                  std::cout << "Error while sending data: " << e.what() << std::endl;
                              }
                          });
    }
}

signalr::hub_connection SignalRHelper::initializeHubConnect() {
    std::string endpoint = SignalRHelper::getRunnerEndpoint();
    signalr::hub_connection connection = signalr::hub_connection_builder::create(endpoint)
            .with_logging(std::make_shared<Logger>(), signalr::trace_level::none)
            .build();
    return connection;
}

std::string SignalRHelper::getRunnerEndpoint() {
    const char *ipAddr = std::getenv("RUNNER_IPV4");
    if (ipAddr == nullptr) { // invalid to assign nullptr to std::string
        return "http://127.0.0.1:5000/runnerhub";
    } else {
        std::string endpoint;
        
        if (ipAddr.rfind("http://", 0) != 0) {
            endpoint.append("http://");
        }

        endpoint.append(ipAddr);
        endpoint.append(":5000/runnerhub");
        return endpoint;
    }
}


