@file:OptIn(ExperimentalUnsignedTypes::class)

package eternityii

import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.async
import kotlinx.coroutines.runBlocking
import kotlinx.coroutines.withContext
import java.util.concurrent.ConcurrentHashMap
import kotlin.random.Random

class Solver(
    private val numCores: Int
) {
    private val breakArray = Breaks.breakArray
    private val heuristicArray = Heuristics.heuristicArray
    private val boardSearchSequence = BoardOrder.boardSearchSequence

    fun run() {
        var loopCount = 0
        while (true) {
            loopCount++
            val dynamicPieceData = preparePiecesAndHeuristics()

            println("Solving with $numCores threads...")

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
                println("$idx ${indexCounts[idx]}")
                totalIndexCount += indexCounts[idx]!!
            }
            println("Total $totalIndexCount")
        }
    }

    private fun runOneSolver(
        core: Int,
        loopCount: Int,
        indexCounts: ConcurrentHashMap<Int, Long>,
        dynamicPieceData: DynamicPieceData
    ) {
        for (repeat in 1..5) {
            println("Start core $core, loop $loopCount, repeat $repeat")
            val startTimeMs = System.currentTimeMillis()

            val solveIndexes = solvePuzzle(dynamicPieceData)

            for (j in 0..256) {
                indexCounts[j] = indexCounts[j]!! + solveIndexes[j]
            }

            val elapsedTimeSeconds = (System.currentTimeMillis() - startTimeMs) / 1000
            println("Finish core $core loop $loopCount, repeat $repeat, $elapsedTimeSeconds s")
        }
    }

    private fun solvePuzzle(dynamicPieceData: DynamicPieceData): LongArray {
        val pieceUsed = BooleanArray(257)
        val cumulativeHeuristicSideCount = ByteArray(256)
        val pieceIndexToTryNext = ByteArray(256)
        val cumulativeBreaks = ByteArray(256)
        val solveIndexCounts = LongArray(257)
        val board: Array<RotatedPiece> = Array(256) { RotatedPiece.nullPiece }
        val rand = Random.Default

        val bottomSides: Array<Array<RotatedPiece>?> = Array(529) { null }
        dynamicPieceData.bottomSidePiecesRotated.forEach { (key, rotatedPieceWithLeftBottoms) ->
            bottomSides[key.toInt()] = rotatedPieceWithLeftBottoms.sortedByDescending {
                if (it.rotatedPiece.heuristicSideCount > 0) { 100 } else { 0 } + rand.nextInt(0, 99)
            }
                .map { it.rotatedPiece }
                .toTypedArray()
        }

        // Get rid of pieces 1 or 2 first.
        board[0] = dynamicPieceData.corners[0]!!.toList().sortedBy { rand.nextInt(1, 1000) }.first()

        pieceUsed[board[0].pieceNumber.toInt()] = true
        cumulativeBreaks[0] = 0
        cumulativeHeuristicSideCount[0] = board[0].heuristicSideCount

        // This goes from 0....255; we've solved #0 already, so start at #1.
        var solveIndex = 1
        var maxSolveIndex = solveIndex
        var nodeCount = 0L

        while (true) {
            nodeCount++

            // Uncomment to get this info printed.
            // solveIndexCounts[solveIndex] = solveIndexCounts[solveIndex] + 1

            if (solveIndex > maxSolveIndex) {
                maxSolveIndex = solveIndex

                if (solveIndex >= 252) {
                    Store.saveBoard(board.toList(), solveIndex.toUShort())

                    if (solveIndex >= 256) {
                        return solveIndexCounts
                    }
                }
            }

            if (nodeCount > 50000000000) {
                return solveIndexCounts
            }

            val row = boardSearchSequence[solveIndex].row
            val col = boardSearchSequence[solveIndex].col

            if (board[row * 16 + col].pieceNumber > 0U) {
                // require(pieceUsed[board[row * 16 + col].pieceNumber.toInt()]) { "ERROR!" }
                pieceUsed[board[row * 16 + col].pieceNumber.toInt()] = false
                board[row * 16 + col] = RotatedPiece.nullPiece
            }

            val pieceCandidates = if (row == 0.toByte()) {
                if (col < 15) {
                    bottomSides[board[col - 1].rightSide.toInt() * 23 + 0]
                } else {
                    dynamicPieceData.corners[board[col - 1].rightSide.toInt() * 23 + 0]
                }
            } else {
                val leftSide: Int = if (col == 0.toByte()) {
                    0
                } else {
                    board[row * 16 + (col - 1)].rightSide.toInt()
                }
                dynamicPieceData.masterPieceLookup[
                    row * 16 + col
                ]!![leftSide * 23 + board[(row - 1) * 16 + col].topSide.toInt()]
            }

            var foundPiece = false

            if (pieceCandidates != null) {
                val breaksThisTurn = breakArray[solveIndex] - cumulativeBreaks[solveIndex - 1].toUByte()
                val tryIndex = pieceIndexToTryNext[solveIndex]
                val pieceCandidateLength = pieceCandidates.size

                for (i in tryIndex until pieceCandidateLength) {
                    if (pieceCandidates[i].breakCount > breaksThisTurn.toInt()) {
                        break
                    }

                    if (!pieceUsed[pieceCandidates[i].pieceNumber.toInt()]) {
                        if (solveIndex <= Heuristics.MAX_HEURISTIC_INDEX) {
                            if ((cumulativeHeuristicSideCount[solveIndex - 1] + pieceCandidates[i].heuristicSideCount) <
                                heuristicArray[solveIndex]
                            ) {
                                break
                            }
                        }
                        foundPiece = true

                        val piece = pieceCandidates[i]
                        board[row * 16 + col] = piece
                        pieceUsed[piece.pieceNumber.toInt()] = true

                        cumulativeBreaks[solveIndex] = (cumulativeBreaks[solveIndex - 1] + piece.breakCount).toByte()
                        cumulativeHeuristicSideCount[solveIndex] =
                            (cumulativeHeuristicSideCount[solveIndex - 1] + piece.heuristicSideCount).toByte()

                        pieceIndexToTryNext[solveIndex] = (i + 1).toByte()
                        solveIndex++
                        break
                    }
                }
            }

            if (!foundPiece) {
                pieceIndexToTryNext[solveIndex] = 0
                solveIndex--
            }
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
