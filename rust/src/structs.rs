#[derive(Clone, Debug)]
pub struct Piece {
    pub piece_number: u16,
    pub top: u8,
    pub right: u8,
    pub bottom: u8,
    pub left: u8,
    pub piece_type: u8, // 2 for corners, 1 for sides, and 0 for middles
}

#[derive(Clone, Copy, Debug)]
pub struct RotatedPiece {
    pub piece_number: u16,
    pub rotations: u8,
    pub top: u8,
    pub right: u8,
    pub break_count: u8,
    pub heuristic_side_count: u8,
}

#[derive(Clone)]
pub struct RotatedPieceWithLeftBottom {
    pub left_bottom: u16,
    pub score: isize,
    pub rotated_piece: RotatedPiece,
}

#[derive(Clone, Copy)]
pub struct SearchIndex {
    pub row: u8,
    pub col: u8,
}

pub struct SolverResult {
    pub solve_indexes: [u64; 257],
    pub max_depth: usize,
}
