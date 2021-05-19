package galaxio.model

data class World(
	val centerPoint: Position = Position(),
	val radius: Int = 0,
	val currentTick: Int = 0
)