@file:OptIn(ExperimentalUnsignedTypes::class)

package eternityii

import kotlin.random.Random

class Solver(
    private val dynamicPieceData: DynamicPieceData,
    private val core: Int
) {
    private val breakArray = Breaks.breakArray
    private val heuristicArray = Heuristics.heuristicArray
    private val boardSearchSequence = BoardOrder.boardSearchSequence
    private val pieceUsed = BooleanArray(257)
    private val cumulativeHeuristicSideCount = ByteArray(256)
    private val pieceIndexToTryNext = ByteArray(256)
    private val cumulativeBreaks = ByteArray(256)
    private val solveIndexCounts = LongArray(257)
    private val board: Array<RotatedPiece> = Array(256) { RotatedPiece.nullPiece }
    private val bottomSides: Array<Array<RotatedPiece>?> = Array(529) { null }

    /** This goes from 0....255; we've solved #0 already, so start at #1. */
    private var solveIndex = 1
    private var maxSolveIndex = solveIndex
    private var nodeCount = 0L

    private val progressTracker: ProgressTracker = ProgressTracker(this::reportProgress)

    private fun reportProgress(elapsedTimeSeconds: Long) {
        val rate = if (elapsedTimeSeconds == 0L) { 0 } else { nodeCount / elapsedTimeSeconds }
        println(
            "Core $core: ${nodeCount.fmt()} nodes in $elapsedTimeSeconds seconds, ${rate.fmt()} per second, " +
                "max depth $maxSolveIndex"
        )
    }

    init {
        val rand = Random.Default

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
    }

    fun run(): SolverResult {
        while (true) {
            nodeCount++

            // Uncomment to get this info printed.
            // solveIndexCounts[solveIndex] = solveIndexCounts[solveIndex] + 1

            if (solveIndex > maxSolveIndex) {
                maxSolveIndex = solveIndex

                if (solveIndex >= 252) {
                    Store.saveBoard(board.toList(), solveIndex.toUShort())

                    if (solveIndex >= 256) {
                        progressTracker.cancel()
                        return SolverResult(solveIndexCounts, maxSolveIndex)
                    }
                }
            }

            if (nodeCount > 50000000000) {
                progressTracker.cancel()
                return SolverResult(solveIndexCounts, maxSolveIndex)
            }

            val row = boardSearchSequence[solveIndex].row
            val col = boardSearchSequence[solveIndex].col
            val rowCol = row * 16 + col

            if (board[rowCol].pieceNumber > 0U) {
                // require(pieceUsed[board[rowCol].pieceNumber.toInt()]) { "ERROR!" }
                pieceUsed[board[rowCol].pieceNumber.toInt()] = false
                board[rowCol] = RotatedPiece.nullPiece
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
                    rowCol
                ]!![leftSide * 23 + board[(row - 1) * 16 + col].topSide.toInt()]
            }

            var foundPiece = false

            if (pieceCandidates != null) {
                val breaksThisTurn = breakArray[solveIndex].toInt() - cumulativeBreaks[solveIndex - 1]
                val tryIndex = pieceIndexToTryNext[solveIndex]

                for (i in tryIndex until pieceCandidates.size) {
                    if (pieceCandidates[i].breakCount > breaksThisTurn) {
                        break
                    }

                    if (!pieceUsed[pieceCandidates[i].pieceNumber.toInt()]) {
                        if (solveIndex <= Heuristics.MAX_HEURISTIC_INDEX &&
                            (cumulativeHeuristicSideCount[solveIndex - 1] + pieceCandidates[i].heuristicSideCount) <
                                heuristicArray[solveIndex]
                        ) {
                            break
                        }

                        foundPiece = true

                        val piece = pieceCandidates[i]
                        board[rowCol] = piece
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
}
