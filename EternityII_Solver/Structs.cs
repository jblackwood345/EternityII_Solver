
namespace EternityII_Solver
{
    public struct Piece
    {
        public ushort PieceNumber { get; set; }
        public byte TopSide { get; set; }
        public byte RightSide { get; set; }
        public byte BottomSide { get; set; }
        public byte LeftSide { get; set; }
        public byte PieceType { get; set; } // 2 for corners, 1 for sides, and 0 for middles
    }

    public struct RotatedPiece
    {
        public ushort PieceNumber { get; set; }
        public byte Rotations { get; set; }
        public byte TopSide { get; set; }
        public byte RightSide { get; set; }
        public byte Break_Count { get; set; }
        public byte Heuristic_Side_Count { get; set; }
    }

    public struct RotatedPieceWithLeftBottom
    {
        public ushort LeftBottom { get; set; }
        public int Score { get; set; }
        public RotatedPiece RotatedPiece { get; set; }
    }

    public struct SearchIndex
    {
        public byte Row { get; set; }
        public byte Column { get; set; }
    }

    public struct SolverResult
    {
        public long[] solve_indexes { get; set; }
        public int max_depth { get; set; }
    }
}
