from PlayerAction import PlayerAction
from PlayerActions import PlayerActions
import random


class BotService:
    def __init__(self, bot=None, playerAction=None, gameState=None):
        self.bot = bot
        self.playerAction = playerAction
        self.gameState = gameState

    def computeNextPlayerAction(self, id):
        self.playerAction = {"PlayerId": id, "Action": PlayerActions.Forward.value, "Heading": random.randint(0, 359)}
        return self.playerAction

    def set_bot(self, args):
        self.bot = args

    def set_game_state(self, args):
        self.gameState = args
        print("Receive GameState")

    def set_playerAction_id(self, id):
        self.playerAction.PlayerId = id
