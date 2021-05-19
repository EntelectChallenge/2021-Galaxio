package galaxio.service

import galaxio.model.*
import galaxio.model.enums.PlayerActions
import galaxio.model.GameObject
import galaxio.util.MathUtils
import java.util.*

class BotService(private val id: UUID) {

	private val bot
		get() = gameState.playerObjects.firstOrNull { it.id == id }

	var gameState: GameState = GameState()

	private fun stopAction(): PlayerAction = PlayerAction(id, PlayerActions.STOP, 0)

	fun computeNextPlayerAction(): PlayerAction {
		val bot = this.bot ?: return stopAction()
		val closestEnemy = findClosestPlayer(bot) ?: return stopAction()
		val headingBetween = MathUtils.getHeadingBetween(bot.position, closestEnemy.position)
		return PlayerAction(id, PlayerActions.FORWARD, headingBetween)
	}

	private fun findClosestPlayer(bot: GameObject): GameObject? {
		return gameState.playerObjects
			.filter { it.id != bot.id } // Ignore our own bot.
			.minByOrNull { MathUtils.getDistanceBetween(bot.position, it.position) }
	}
}