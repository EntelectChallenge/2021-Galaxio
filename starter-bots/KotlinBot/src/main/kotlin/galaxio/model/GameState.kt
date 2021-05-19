package galaxio.model

class GameState(
	val world: World = World(),
	val gameObjects: List<GameObject> = emptyList(),
	val playerObjects: List<GameObject> = emptyList()
)