package eternityii

object RotatedPieces {
    private val sideEdges = listOf<UByte>(1U, 5U, 9U, 13U, 17U)
    // private val middleEdges = listOf<UByte>(
    //     2U, 3U, 4U, 6U, 7U, 8U, 10U, 11U, 12U, 14U, 15U, 16U, 18U, 19U, 20U, 21U, 22U
    // )

    /** There is a lot of overlap between these sides. */
    private val heuristicSides = listOf<UByte>(13U, 16U, 10U)

    private fun calculateTwoSides(side1: Int, side2: Int): UShort = (side1 * 23 + side2).toUShort()

    fun rotatedPieces(
        piece: Piece,
        allowBreaks: Boolean = false
    ): List<RotatedPieceWithLeftBottom> {
        var score = 0
        var heuristicSideCount: Byte = 0

        heuristicSides.forEach { side ->
            if (piece.leftSide == side) {
                score += 100
                heuristicSideCount++
            }
            if (piece.topSide == side) {
                score += 100
                heuristicSideCount++
            }
            if (piece.rightSide == side) {
                score += 100
                heuristicSideCount++
            }
            if (piece.bottomSide == side) {
                score += 100
                heuristicSideCount++
            }
        }

        val rotatedPieces: MutableList<RotatedPieceWithLeftBottom> = mutableListOf()

        for (left in 0..22) {
            for (bottom in 0..22) {
                var rotationsBreaks0 = 0
                var sideBreaks0 = 0
                if (piece.leftSide != left.toUByte()) {
                    rotationsBreaks0++

                    if (sideEdges.contains(piece.leftSide)) {
                        sideBreaks0++
                    }
                }
                if (piece.bottomSide != bottom.toUByte()) {
                    rotationsBreaks0++

                    if (sideEdges.contains(piece.bottomSide)) {
                        sideBreaks0++
                    }
                }

                if ((rotationsBreaks0 == 0) || ((rotationsBreaks0 == 1) && allowBreaks)) {
                    if (sideBreaks0 == 0) {
                        rotatedPieces.add(
                            RotatedPieceWithLeftBottom(
                                leftBottom = calculateTwoSides(left, bottom),
                                score = (score - 100000 * rotationsBreaks0),
                                rotatedPiece = RotatedPiece(
                                    pieceNumber = piece.pieceNumber,
                                    rotations = 0,
                                    topSide = piece.topSide,
                                    rightSide = piece.rightSide,
                                    breakCount = rotationsBreaks0.toByte(),
                                    heuristicSideCount = heuristicSideCount
                                )
                            )
                        )
                    }
                }

                var rotationsBreaks1 = 0
                var sideBreaks1 = 0
                if (piece.bottomSide != left.toUByte()) {
                    rotationsBreaks1++
                    if (sideEdges.contains(piece.bottomSide)) {
                        sideBreaks1++
                    }
                }
                if (piece.rightSide != bottom.toUByte()) {
                    rotationsBreaks1++
                    if (sideEdges.contains(piece.rightSide)) {
                        sideBreaks1++
                    }
                }

                if ((rotationsBreaks1 == 0) || ((rotationsBreaks1 == 1) && allowBreaks)) {
                    if (sideBreaks1 == 0) {
                        rotatedPieces.add(
                            RotatedPieceWithLeftBottom(
                                leftBottom = calculateTwoSides(left, bottom),
                                score = (score - 100000 * rotationsBreaks1),
                                rotatedPiece = RotatedPiece(
                                    pieceNumber = piece.pieceNumber,
                                    rotations = 1,
                                    topSide = piece.leftSide,
                                    rightSide = piece.topSide,
                                    breakCount = rotationsBreaks1.toByte(),
                                    heuristicSideCount = heuristicSideCount
                                )
                            )
                        )
                    }
                }

                var rotationsBreaks2 = 0
                var sideBreaks2 = 0
                if (piece.rightSide != left.toUByte()) {
                    rotationsBreaks2++

                    if (sideEdges.contains(piece.rightSide)) {
                        sideBreaks2++
                    }
                }
                if (piece.topSide != bottom.toUByte()) {
                    rotationsBreaks2++

                    if (sideEdges.contains(piece.topSide)) {
                        sideBreaks2++
                    }
                }

                if ((rotationsBreaks2 == 0) || ((rotationsBreaks2 == 1) && allowBreaks)) {
                    if (sideBreaks2 == 0) {
                        rotatedPieces.add(
                            RotatedPieceWithLeftBottom(
                                leftBottom = calculateTwoSides(left, bottom),
                                score = (score - 100000 * rotationsBreaks2),
                                rotatedPiece = RotatedPiece(
                                    pieceNumber = piece.pieceNumber,
                                    rotations = 2,
                                    topSide = piece.bottomSide,
                                    rightSide = piece.leftSide,
                                    breakCount = rotationsBreaks2.toByte(),
                                    heuristicSideCount = heuristicSideCount
                                )
                            )
                        )
                    }
                }

                var rotationsBreaks3 = 0
                var sideBreaks3 = 0
                if (piece.topSide != left.toUByte()) {
                    rotationsBreaks3++

                    if (sideEdges.contains(piece.topSide)) {
                        sideBreaks3++
                    }
                }
                if (piece.leftSide != bottom.toUByte()) {
                    rotationsBreaks3++

                    if (sideEdges.contains(piece.leftSide)) {
                        sideBreaks3++
                    }
                }

                if ((rotationsBreaks3 == 0) || ((rotationsBreaks3 == 1) && allowBreaks)) {
                    if (sideBreaks3 == 0) {
                        rotatedPieces.add(
                            RotatedPieceWithLeftBottom(
                                leftBottom = calculateTwoSides(left, bottom),
                                score = (score - 100000 * rotationsBreaks3),
                                rotatedPiece = RotatedPiece(
                                    pieceNumber = piece.pieceNumber,
                                    rotations = 3,
                                    topSide = piece.rightSide,
                                    rightSide = piece.bottomSide,
                                    breakCount = rotationsBreaks3.toByte(),
                                    heuristicSideCount = heuristicSideCount
                                )
                            )
                        )
                    }
                }
            }
        }

        return rotatedPieces.toList()
    }
}
