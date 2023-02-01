package eternityii

data class Piece(
    val pieceNumber: UShort,
    val topSide: UByte,
    val rightSide: UByte,
    val bottomSide: UByte,
    val leftSide: UByte,
    val pieceType: UByte // 2 for corners, 1 for sides, and 0 for middles
)
