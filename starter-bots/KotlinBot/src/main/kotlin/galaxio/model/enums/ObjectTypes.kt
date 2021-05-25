package galaxio.model.enums

enum class ObjectTypes(val value: Int) {
	PLAYER(1),
	FOOD(2),
	WORMHOLE(3),
	GAS_CLOUD(4),
	ASTEROID_FIELD(5),
	TORPEDO_SALVO(6),
	SUPERFOOD(7);

	companion object {
		fun valueOf(value: Int): ObjectTypes = values().first { it.value == value }
	}
}