package galaxio.model

data class GameComplete(
	val TotalTicks: Int,
	val Players: List<PlayerResult>,
	val WorldSeeds: List<Int>,
	val WinningBot: GameObject
)