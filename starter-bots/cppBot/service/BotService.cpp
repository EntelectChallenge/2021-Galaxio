#include "BotService.hpp"
/*
 * Helper functions
 */

void BotService::calculateNextActionFromInput(std::promise<void> &startTask){
    std::cout << "ENTER COMMAND: (W,A,S,D) or q to quit: ";
    char command;
    std::cin >> command;
    switch (command) {
        case 'w':
        case 'W':
            playerAction.action = PlayerActions::StartAfterburner;
            break;
        case 's':
        case 'S':
            playerAction.action = PlayerActions::Stop;
            break;
        case 'a':
        case 'A':
            playerAction.action = PlayerActions::Forward;
            playerAction.heading = playerAction.heading + 5;
            if (playerAction.heading >= 360) {
                playerAction.heading -= 360;
            }
            break;
        case 'd':
        case 'D':
            playerAction.action = PlayerActions::Forward;
            playerAction.heading = playerAction.heading - 5;
            if (playerAction.heading < 0) {
                playerAction.heading += 360;
            }
            break;
        case 'r':
        case 'R':
            playerAction.action = PlayerActions::StopAfterburner;
            break;
        case 'q':
        case 'Q':
            startTask.set_value();
            break;
        default:
            std::cout << "Command not recognized - default" << std::endl;
            // Do nothing and retain current action
            break;
    }
}

void BotService::calculateNextActionFromRandom(){
    playerAction.action = PlayerActions::Forward;
    playerAction.heading = rand() % 360;
}

/**
 * Main Function to calculate the next action here
 */
void BotService::computeNextPlayerAction(std::promise<void> &startTask) {
    // Actions sent before 20 ms have passed will not be counted (to fast for the game loop)
    std::this_thread::sleep_for(std::chrono::milliseconds(20));

    // Get next move from input
    // calculateNextActionFromInput(&startTask);

    // Get next move from random
    calculateNextActionFromRandom();
}


