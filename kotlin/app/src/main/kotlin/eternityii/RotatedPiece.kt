package eternityii

data class RotatedPiece(
    val pieceNumber: UShort,
    val rotations: Byte,
    val topSide: UByte,
    val rightSide: UByte,
    val breakCount: Byte,
    val heuristicSideCount: Byte
) {
    companion object {
        /** For storing in arrays of non-null values. */
        val nullPiece = RotatedPiece(0U, 0, 0U, 0U, 0, 0)
    }
}
