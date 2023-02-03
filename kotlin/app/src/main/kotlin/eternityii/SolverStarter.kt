@file:OptIn(ExperimentalUnsignedTypes::class)

package eternityii

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.async
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.withContext
import java.util.concurrent.ConcurrentHashMap
import kotlin.random.Random

class SolverStarter(
    private val numCores: Int
) {
    private var maxDepth = 0

    fun run() {
        val overallStartTimeMs = System.currentTimeMillis()
        var totalIndexCount = 0L
        var loopCount = 0

        while (true) {
            loopCount++
            val dynamicPieceData = preparePiecesAndHeuristics()

            println("Solving with $numCores cores...")

            val indexCounts: ConcurrentHashMap<Int, Long> = ConcurrentHashMap<Int, Long>()
            for (j in 0..256) {
                indexCounts[j] = 0L
            }

            // Use all but one core.
            runBlocking {
                withContext(Dispatchers.IO) {
                    for (core in 1 until numCores) {
                        async(Dispatchers.IO) { runOneSolver(core, loopCount, indexCounts, dynamicPieceData) }
                    }
                }
            }

            for (idx in 0..256) {
                // This will only print valid numbers if you let the solver count how far you are.
                val thisIndexCount = indexCounts[idx]!!
                println("$idx ${thisIndexCount.fmt()}")
                totalIndexCount += thisIndexCount
            }
            val overallElapsedTimeSeconds = (System.currentTimeMillis() - overallStartTimeMs) / 1000
            val rate = if (overallElapsedTimeSeconds == 0L) { 0 } else { totalIndexCount / overallElapsedTimeSeconds }
            println(
                "Total ${totalIndexCount.fmt()} nodes in $overallElapsedTimeSeconds seconds, " +
                    "${rate.fmt()} per second, max depth $maxDepth"
            )
        }
    }

    private fun runOneSolver(
        core: Int,
        loopCount: Int,
        indexCounts: ConcurrentHashMap<Int, Long>,
        dynamicPieceData: DynamicPieceData
    ) {
        for (repeat in 1..5) {
            println("Core $core: start loop $loopCount, repeat $repeat")
            val startTimeMs = System.currentTimeMillis()

            val solverResult = Solver(dynamicPieceData, core).run()

            for (j in 0..256) {
                indexCounts[j] = indexCounts[j]!! + solverResult.solveIndexCounts[j]
            }

            if (solverResult.maxDepth > maxDepth) {
                maxDepth = solverResult.maxDepth
            }

            val elapsedTimeSeconds = (System.currentTimeMillis() - startTimeMs) / 1000
            println("Core $core: finish loop $loopCount, repeat $repeat in $elapsedTimeSeconds seconds")
        }
    }

    private fun buildArray(
        inputMap: Map<UShort, List<RotatedPieceWithLeftBottom>>,
        rand: Random
    ): Array<Array<RotatedPiece>?> {
        val outputArray: Array<Array<RotatedPiece>?> = Array(529) { null }
        inputMap.forEach { (key, rotatedPieceWithLeftBottoms) ->
            // You can't use rand inside the sortedByDescending block because it is called multiple times and relies on
            // having the same value every time.
            rotatedPieceWithLeftBottoms.forEach { rotatedPiece ->
                rotatedPiece.randScore = rotatedPiece.score + rand.nextInt(0, 99)
            }
            outputArray[key.toInt()] = rotatedPieceWithLeftBottoms.sortedByDescending { it.randScore }
                .map { it.rotatedPiece }
                .toTypedArray()
        }
        return outputArray
    }

    private fun buildRotatedPiecesMap(
        inputList: List<RotatedPieceWithLeftBottom>,
        rotation: Int
    ): Map<UShort, List<RotatedPieceWithLeftBottom>> =
        inputList
            .filter { it.rotatedPiece.rotations.toInt() == rotation }
            .groupBy { it.leftBottom }
            .toMap()

    private fun preparePiecesAndHeuristics(): DynamicPieceData {
        val cornerPieces = pieces.filter { it.pieceType == 2.toUByte() }.toList()
        val sidePieces = pieces.filter { it.pieceType == 1.toUByte() }.toList()
        val middlePieces = pieces.filter {
            it.pieceType == 0.toUByte() && it.pieceNumber != 139.toUShort()
        }.toList()
        val startPiece = pieces.filter { it.pieceNumber == 139.toUShort() }.toList()

        val cornerPiecesRotated = cornerPieces
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
            .groupBy { it.leftBottom }
            .toMap()

        val sidesWithoutBreaks = sidePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
        val sidesWithBreaks = sidePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece, allowBreaks = true) }
            .flatten()

        val bottomSidePiecesRotated = buildRotatedPiecesMap(sidesWithoutBreaks, 0)
        val leftSidePiecesRotated = buildRotatedPiecesMap(sidesWithoutBreaks, 1)
        val rightSidePiecesWithBreaksRotated = buildRotatedPiecesMap(sidesWithBreaks, 3)
        val rightSidePiecesWithoutBreaksRotated = buildRotatedPiecesMap(sidesWithoutBreaks, 3)
        val topSidePiecesRotated = buildRotatedPiecesMap(sidesWithBreaks, 2)

        val middlePiecesRotatedWithoutBreaks = middlePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
            .groupBy { it.leftBottom }
            .toMap()

        val middlePiecesRotatedWithBreaks = middlePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece, allowBreaks = true) }
            .flatten()
            .groupBy { it.leftBottom }
            .toMap()

        val southStartPieceRotated = middlePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
            .filter { it.rotatedPiece.topSide == 6.toUByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val westStartPieceRotated = middlePieces
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
            .filter { it.rotatedPiece.rightSide == 11.toUByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val startPieceRotated = startPiece
            .map { piece -> RotatedPieces.rotatedPieces(piece) }
            .flatten()
            .filter { it.rotatedPiece.rotations == 2.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val rand = Random.Default

        val corners = buildArray(cornerPiecesRotated, rand)
        val leftSides = buildArray(leftSidePiecesRotated, rand)
        val topSides = buildArray(topSidePiecesRotated, rand)
        val rightSidesWithBreaks = buildArray(rightSidePiecesWithBreaksRotated, rand)
        val rightSidesWithoutBreaks = buildArray(rightSidePiecesWithoutBreaksRotated, rand)
        val middlesWithBreak = buildArray(middlePiecesRotatedWithBreaks, rand)
        val middlesNoBreak = buildArray(middlePiecesRotatedWithoutBreaks, rand)
        val southStart = buildArray(southStartPieceRotated, rand)
        val westStart = buildArray(westStartPieceRotated, rand)
        val start = buildArray(startPieceRotated, rand)

        val masterPieceLookup: Array<Array<Array<RotatedPiece>?>?> = Array(256) { null }
        val boardSearchSequence = BoardOrder.boardSearchSequence

        for (i in 0..255) {
            val row = boardSearchSequence[i].row.toInt()
            val col = boardSearchSequence[i].col.toInt()

            val lookup = when (row) {
                15 -> {
                    if (col == 15 || col == 0) {
                        corners
                    } else {
                        topSides
                    }
                }

                0 -> {
                    // Don't populate the master lookup table since we randomize every time.
                    null
                }

                else -> when (col) {
                    15 -> {
                        if (i < Breaks.firstBreakIndex) {
                            rightSidesWithoutBreaks
                        } else {
                            rightSidesWithBreaks
                        }
                    }

                    0 -> leftSides

                    else -> when (row) {
                        7 -> when (col) {
                            7 -> start
                            6 -> westStart
                            else -> if (i < Breaks.firstBreakIndex) {
                                middlesNoBreak
                            } else {
                                middlesWithBreak
                            }
                        }

                        6 -> if (col == 7) {
                            southStart
                        } else if (i < Breaks.firstBreakIndex) {
                            middlesNoBreak
                        } else {
                            middlesWithBreak
                        }

                        else -> if (i < Breaks.firstBreakIndex) {
                            middlesNoBreak
                        } else {
                            middlesWithBreak
                        }
                    }
                }
            }

            if (lookup != null) {
                masterPieceLookup[row * 16 + col] = lookup
            }
        }

        return DynamicPieceData(corners, bottomSidePiecesRotated, masterPieceLookup)
    }
}
