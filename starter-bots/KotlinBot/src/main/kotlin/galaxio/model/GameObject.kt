package galaxio.model

import galaxio.model.enums.ObjectTypes
import java.util.UUID

data class GameObject(
	val id: UUID,
	val size: Int,
	val speed: Int,
	val currentHeading: Int,
	val position: Position,
	val gameObjectType: ObjectTypes
) {
	companion object {
		fun fromStateList(entry: Map.Entry<String, List<Int>>): GameObject =
			fromStateList(UUID.fromString(entry.key), entry.value)

		fun fromStateList(id: UUID, stateList: List<Int>): GameObject {
			val position = Position(stateList[4], stateList[5])
			return GameObject(id, stateList[0], stateList[1], stateList[2], position, ObjectTypes.valueOf(stateList[3]))
		}
	}
}