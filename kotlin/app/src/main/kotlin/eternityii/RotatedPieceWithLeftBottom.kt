package eternityii

data class RotatedPieceWithLeftBottom(
    val leftBottom: UShort,
    val score: Int,
    val rotatedPiece: RotatedPiece,
    /** Temporary storage. */
    var randScore: Int = 0
)
