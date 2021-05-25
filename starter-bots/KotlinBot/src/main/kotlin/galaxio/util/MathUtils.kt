package galaxio.util

import galaxio.model.Position
import kotlin.math.*

object MathUtils {
	fun getHeadingBetween(a: Position, b: Position): Int {
		val angle = atan2((b.y - a.y).toDouble(), (b.x - a.x).toDouble())
		return (Math.toDegrees(angle).toInt() + 360) % 360
	}

	/** @return the distance between position a and b calculated using the Pythagorean theorem. */
	fun getDistanceBetween(a: Position, b: Position): Double {
		val x = abs(a.x - b.x)
		val y = abs(a.y - b.y)
		return pythagoras(x.toDouble(), y.toDouble())
	}

	fun pythagoras(x: Double, y: Double): Double = sqrt(x * x + y * y)
}