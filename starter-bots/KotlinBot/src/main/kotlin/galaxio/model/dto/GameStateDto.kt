package galaxio.model.dto

import galaxio.model.World

data class GameStateDto(
    val world: World,
    val gameObjects: Map<String, List<Int>>,
    val playerObjects: Map<String, List<Int>>
)