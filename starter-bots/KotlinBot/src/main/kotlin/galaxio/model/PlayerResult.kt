package galaxio.model

data class PlayerResult(
	val Placement: Int,
	val Seed: Int,
	val Score: Int,
	val Id: String,
	val Nickname: String,
	val MatchPoints: Int
)