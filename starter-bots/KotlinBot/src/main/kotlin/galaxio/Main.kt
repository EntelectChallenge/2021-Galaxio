package galaxio

import com.microsoft.signalr.HubConnectionBuilder
import com.microsoft.signalr.HubConnectionState
import galaxio.model.GameObject
import galaxio.model.GameState
import galaxio.model.dto.GameStateDto
import galaxio.service.BotService
import java.util.*
import java.util.concurrent.TimeUnit

object Main {
	// NOTE: You can change the nickname of your bot here.
	private const val botNickname = "KotlinBot"

	private fun getRunnerUrl(): String {
		var ip = System.getenv("RUNNER_IPV4")
		if (ip == null || ip.isBlank()) {
			ip = "localhost"
		}
		if (!ip.startsWith("http://")) {
			ip = "http://$ip"
		}
		return "$ip:5000/runnerhub"
	}

	@JvmStatic
	fun main(args: Array<String>) {
		val token = System.getenv("REGISTRATION_TOKEN") ?: UUID.randomUUID().toString()
		val url = getRunnerUrl()
		var moveComputed = true

		HubConnectionBuilder.create(url).build().use { hubConnection ->
			var service: BotService? = null
			var shouldQuit = false

			hubConnection.on("Disconnect", {
				println("Disconnected.")

				shouldQuit = true
			}, UUID::class.java)

			hubConnection.on("Registered", { id: UUID ->
				println("Registered with the runner $id.")

				service = BotService(id)
			}, UUID::class.java)

			hubConnection.on("ReceiveGameState", {
				val world = it.world
				val gameObjects = it.gameObjects.map(GameObject::fromStateList)
				val playerObjects = it.playerObjects.map(GameObject::fromStateList)
				service!!.gameState = GameState(world, gameObjects, playerObjects)
				moveComputed = false
			}, GameStateDto::class.java)

			hubConnection.on("ReceiveGameComplete", {
				println("Game complete: $it")
			}, String::class.java)

			hubConnection.start().blockingAwait()
			println("Connection established with runner.")

			Thread.sleep(1000)
			hubConnection.send("Register", token, botNickname)

			while (!shouldQuit) {
				Thread.sleep(20)

				val s: BotService = service ?: continue

				if (moveComputed) {
					continue
				}

				moveComputed = true
				val action = s.computeNextPlayerAction()
				if (hubConnection.connectionState == HubConnectionState.CONNECTED) {
					hubConnection.send("SendPlayerAction", action)
				}
			}

			hubConnection.stop().blockingAwait(10, TimeUnit.SECONDS)
			println("Connection closed: ${hubConnection.connectionState}")
		}
	}
}