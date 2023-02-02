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
    fun run() {
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

            var totalIndexCount = 0L
            for (idx in 0..256) {
                // This will only print valid numbers if you let the solver count how far you are.
                println("$idx ${indexCounts[idx]!!.fmt()}")
                totalIndexCount += indexCounts[idx]!!
            }
            println("Total ${totalIndexCount.fmt()}")
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

            val solveIndexes = Solver(dynamicPieceData, core).run()

            for (j in 0..256) {
                indexCounts[j] = indexCounts[j]!! + solveIndexes[j]
            }

            val elapsedTimeSeconds = (System.currentTimeMillis() - startTimeMs) / 1000
            println("Core $core: finish loop $loopCount, repeat $repeat in $elapsedTimeSeconds seconds")
        }
    }

    private fun buildArray(
        inputMap: Map<UShort, List<RotatedPieceWithLeftBottom>>,
        outputArray: Array<Array<RotatedPiece>?>,
        rand: Random
    ) {
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
    }

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

        val bottomSidePiecesRotated = sidesWithoutBreaks
            .filter { it.rotatedPiece.rotations == 0.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val leftSidePiecesRotated = sidesWithoutBreaks
            .filter { it.rotatedPiece.rotations == 1.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val rightSidePiecesWithBreaksRotated = sidesWithBreaks
            .filter { it.rotatedPiece.rotations == 3.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val rightSidePiecesWithoutBreaksRotated = sidesWithoutBreaks
            .filter { it.rotatedPiece.rotations == 3.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

        val topSidePiecesRotated = sidesWithBreaks
            .filter { it.rotatedPiece.rotations == 2.toByte() }
            .groupBy { it.leftBottom }
            .toMap()

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

        val corners: Array<Array<RotatedPiece>?> = Array(529) { null }
        val leftSides: Array<Array<RotatedPiece>?> = Array(529) { null }
        val topSides: Array<Array<RotatedPiece>?> = Array(529) { null }
        val rightSidesWithBreaks: Array<Array<RotatedPiece>?> = Array(529) { null }
        val rightSidesWithoutBreaks: Array<Array<RotatedPiece>?> = Array(529) { null }
        val middlesWithBreak: Array<Array<RotatedPiece>?> = Array(529) { null }
        val middlesNoBreak: Array<Array<RotatedPiece>?> = Array(529) { null }
        val southStart: Array<Array<RotatedPiece>?> = Array(529) { null }
        val westStart: Array<Array<RotatedPiece>?> = Array(529) { null }
        val start: Array<Array<RotatedPiece>?> = Array(529) { null }

        buildArray(cornerPiecesRotated, corners, rand)
        buildArray(leftSidePiecesRotated, leftSides, rand)
        buildArray(topSidePiecesRotated, topSides, rand)
        buildArray(rightSidePiecesWithBreaksRotated, rightSidesWithBreaks, rand)
        buildArray(rightSidePiecesWithoutBreaksRotated, rightSidesWithoutBreaks, rand)
        buildArray(middlePiecesRotatedWithBreaks, middlesWithBreak, rand)
        buildArray(middlePiecesRotatedWithoutBreaks, middlesNoBreak, rand)
        buildArray(southStartPieceRotated, southStart, rand)
        buildArray(westStartPieceRotated, westStart, rand)
        buildArray(startPieceRotated, start, rand)

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
