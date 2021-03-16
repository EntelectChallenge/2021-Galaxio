import enum 
  
class PlayerActions(enum.Enum):
    Forward = 1
    Stop = 2
    StartAfterburner = 3
    StopAfterburner = 4

    def __str__(self):
        return '%s' % self.name