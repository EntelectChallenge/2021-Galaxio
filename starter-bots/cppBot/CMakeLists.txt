### Entelect Challenge 2021
### c++ bot
### Author: fabio.loreggian@entelect.co.za

###
### DOCKER BUILD STEPS - PLEASE DON'T MODIFY STATEMENTS ENDING IN #IMPORTANT
###
cmake_minimum_required(VERSION 3.13.4) #IMPORTANT

#Use the defined environment variable to determine build process {DOCKER_BUILD=true}
if(DEFINED ENV{DOCKER_BUILD})
    set(DOCKER_BUILD $ENV{DOCKER_BUILD}) #IMPORTANT
else()
    #If you don't set the environment variable you can define it here
    set(DOCKER_BUILD true)
endif()

if(DOCKER_BUILD)
    #Build steps for docker container
    project(cppBot)

    set(CMAKE_CXX_STANDARD 17)

    #Signal R Docker include
    include(signalr_client_docker_compile.cmake) #IMPORTANT

    add_executable(cppBot main.cpp signalRHelper/SignalRHelper.cpp signalRHelper/SignalRHelper.h logger/Logger.hpp enums/ObjectTypes.h enums/PlayerActions.h models/GameObject.h models/GameState.h models/PlayerAction.h models/Position.h models/World.h service/BotService.cpp service/BotService.hpp uuid/UUID.hpp)
    target_compile_definitions(cppBot PUBLIC -DNO_SIGNALRCLIENT_EXPORTS) #IMPORTANT
else()
    #EXAMPLE LOCAL CONFIG
    #Build steps for local configuration using vcpkg
    set(CMAKE_TOOLCHAIN_FILE ${CMAKE_SOURCE_DIR}/vcpkg/scripts/buildsystems/vcpkg.cmake CACHE STRING "Vcpkg toolchain file")
    project(cppBot)

    set(CMAKE_CXX_STANDARD 17)
    find_path(SIGNALR_INCLUDE_DIR signalrclient/hub_connection.h)
    message(${SIGNALR_INCLUDE_DIR})
    include_directories(${SIGNALR_INCLUDE_DIR})

    find_library(SIGNALR_LIBRARY NAMES signalrclient PATHS {SIGNALR_INCLUDE_DIR} REQUIRED)
    link_libraries(${SIGNALR_LIBRARY})

    add_executable(cppBot main.cpp signalRHelper/SignalRHelper.cpp signalRHelper/SignalRHelper.h logger/Logger.hpp enums/ObjectTypes.h enums/PlayerActions.h models/GameObject.h models/GameState.h models/PlayerAction.h models/Position.h models/World.h service/BotService.cpp service/BotService.hpp uuid/UUID.hpp)
endif()

