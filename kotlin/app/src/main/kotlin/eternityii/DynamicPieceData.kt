package eternityii

data class DynamicPieceData(
    val corners: Array<Array<RotatedPiece>?>,
    val bottomSidePiecesRotated: Map<UShort, List<RotatedPieceWithLeftBottom>>,
    val masterPieceLookup: Array<Array<Array<RotatedPiece>?>?>
)
