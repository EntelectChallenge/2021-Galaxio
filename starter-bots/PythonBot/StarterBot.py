# -*- coding: utf-8 -*-
"""
Entelect StarterBot 2021 for Python3
"""
import logging
import os
import time
import uuid
from signalrcore.hub_connection_builder import HubConnectionBuilder

from BotService import BotService
from GameObject import GameObject
from ObjectTypes import ObjectTypes
from Position import Position

logging.basicConfig(filename='sample_python_bot.log', filemode='w', level=logging.DEBUG)
logger = logging.getLogger(__name__)

botService = BotService()
hub_connected = False

def on_register(args):
    bot = GameObject(args[0], 10, 20, Position(0, 0), ObjectTypes.Player)
    botService.set_bot(bot)
    print("Registered")


def print_message(x):
    print(x)


def set_hub_connection(connected):
    global hub_connected
    hub_connected = connected


def run_bot():
    environmentIp = os.getenv('RUNNER_IPV4', "http://localhost")

    environmentIp = environmentIp if environmentIp.startswith("http://") else "http://" + environmentIp

    url = environmentIp + ":" + "5000" + "/runnerhub"

    print(url)
    hub_connection = HubConnectionBuilder() \
        .with_url(url) \
        .configure_logging(logging.INFO) \
        .with_automatic_reconnect({
        "type": "raw",
        "keep_alive_interval": 10,
        "reconnect_interval": 5,
        "max_attempts": 5
    }).build()

    hub_connection.on_open(lambda: (print("Connection opened and handshake received, ready to send messages"),
                                    set_hub_connection(True)))
    hub_connection.on_error(lambda data: print(f"An exception was thrown closed: {data.error}"))
    hub_connection.on_close(lambda: (print("Connection closed"),
                                     set_hub_connection(False)))

    hub_connection.on("Registered", on_register)
    hub_connection.on("ReceiveGameState", botService.set_game_state)
    hub_connection.on("Disconnect", lambda data: (print("Disconnect Called"),(set_hub_connection(False))))

    hub_connection.start()
    time.sleep(1)

    token = os.getenv("REGISTRATION_TOKEN")
    token = token if token is not None else uuid.uuid4()

    print("Registering with the runner...")
    bot_nickname = "Jungle_Cobra"
    registration_args = [str(token), bot_nickname]
    hub_connection.send("Register", registration_args)

    time.sleep(5)
    while hub_connected:
        bot = botService.bot
        if (bot == None):
            continue
        botService.computeNextPlayerAction(bot.object_id)
        actionList = [botService.playerAction]

        hub_connection.send("SendPlayerAction", actionList)
        print("Send Action to Runner")

    hub_connection.stop()


if __name__ == "__main__":
    run_bot()
