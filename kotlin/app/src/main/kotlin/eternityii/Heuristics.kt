@file:OptIn(ExperimentalUnsignedTypes::class)

package eternityii

object Heuristics {
    const val MAX_HEURISTIC_INDEX = 160

    val heuristicArray: IntArray = run {
        val heuristicArray = IntArray(256)

        for (i in 0..255) {
            heuristicArray[i] =
                when {
                    i <= 16 -> 0
                    i <= 26 -> ((i.toFloat() - 16) * 2.8F).toInt()
                    i <= 56 -> ((i.toFloat() - 26) * 1.43333F + 28).toInt()
                    i <= 76 -> (((i.toFloat() - 56) * 0.9F) + 71).toInt()
                    i <= 102 -> ((((i.toFloat() - 76) * 0.6538F)) + 89).toInt()
                    i <= MAX_HEURISTIC_INDEX -> ((((i.toFloat() - 102) / 4.4615F)) + 106).toInt()
                    else -> 0
                }
        }
        heuristicArray
    }
}
