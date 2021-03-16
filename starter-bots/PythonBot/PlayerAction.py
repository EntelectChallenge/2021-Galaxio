import json

from PlayerActions import PlayerActions

class PlayerAction:
    def __init__(self, player_id, action, heading):
        self.PlayerId = player_id
        self.Action = action
        self.Heading = heading

    def toJson(self):
        return json.dumps(self, default=lambda o: o._asdict())