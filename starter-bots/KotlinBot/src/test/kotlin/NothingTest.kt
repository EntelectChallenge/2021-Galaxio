import junit.framework.Assert
import org.junit.jupiter.api.Test
import org.junit.jupiter.params.ParameterizedTest
import org.junit.jupiter.params.provider.Arguments
import org.junit.jupiter.params.provider.MethodSource

class NothingTest {
	companion object {
		@JvmStatic
		fun nothingDataSource() = arrayOf(
			Arguments.of(1, "hello"),
			Arguments.of(2, "world")
		)
	}

	@Test
	fun nothing() {
		Assert.assertTrue(true)
	}

	@ParameterizedTest
	@MethodSource("nothingDataSource")
	fun `should pass`(num: Int, str: String) {
		if (num == 1) {
			Assert.assertEquals("hello", str)
		} else if (num == 2) {
			Assert.assertEquals("world", str)
		} else {
			Assert.fail()
		}
	}
}
