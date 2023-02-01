@file:OptIn(ExperimentalUnsignedTypes::class)

package eternityii

object Breaks {
    private val breakIndicesAllowed = listOf(201, 206, 211, 216, 221, 225, 229, 233, 237, 239)

    val firstBreakIndex: Int = if (breakIndicesAllowed.size == 0) {
        256
    } else {
        breakIndicesAllowed.min()
    }

    val breakArray: UByteArray = run {
        var cumulativebreaksAllowed: UByte = 0U
        val cumulativebreakArray = UByteArray(256)
        for (idx in 0..255) {
            if (breakIndicesAllowed.contains(idx)) {
                cumulativebreaksAllowed++
            }

            cumulativebreakArray[idx] = cumulativebreaksAllowed
        }

        cumulativebreakArray
    }
}
