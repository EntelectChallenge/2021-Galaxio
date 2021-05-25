package galaxio.model

import galaxio.model.enums.PlayerActions
import java.util.*

data class PlayerAction(
	val playerId: UUID,
	val action: PlayerActions,
	val heading: Int
)