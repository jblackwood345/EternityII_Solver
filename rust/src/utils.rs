use crate::board_order::BOARD_ORDER;
use crate::pieces::PIECES;
use crate::structs::{Piece, RotatedPiece, RotatedPieceWithLeftBottom, SearchIndex};
use directories::UserDirs;
use md5::Digest;
use rand::Rng;
use std::fs;
use std::fs::File;
use std::io::Write;
use std::path::PathBuf;
use string_builder::Builder;
use uuid::Uuid;

const SIDE_EDGES: [u8; 5] = [1, 5, 9, 13, 17];
const HEURISTIC_SIDES: [u8; 3] = [13, 16, 10]; // There is a lot of overlap between these sides
const BREAK_INDEXES_ALLOWED: [u8; 10] = [201, 206, 211, 216, 221, 225, 229, 233, 237, 239];

fn calculate_two_sides(side1: u8, side2: u8) -> u16 {
    side1 as u16 * 23 + side2 as u16
}

pub fn get_rotated_pieces(piece: &Piece, allow_breaks: bool) -> Vec<RotatedPieceWithLeftBottom> {
    let mut score: isize = 0;
    let mut heuristic_side_count: u8 = 0;

    HEURISTIC_SIDES.iter().for_each(|side| {
        if piece.left == *side {
            score += 100;
            heuristic_side_count += 1;
        }
        if piece.top == *side {
            score += 100;
            heuristic_side_count += 1;
        }
        if piece.right == *side {
            score += 100;
            heuristic_side_count += 1;
        }
        if piece.bottom == *side {
            score += 100;
            heuristic_side_count += 1;
        }
    });

    let mut rotated_pieces: Vec<RotatedPieceWithLeftBottom> = vec![];

    for left in 0..=22 {
        for bottom in 0..=22 {
            let mut rotations_breaks_0: u8 = 0;
            let mut side_breaks_0: u8 = 0;

            if piece.left != left {
                rotations_breaks_0 += 1;

                if SIDE_EDGES.contains(&piece.left) {
                    side_breaks_0 += 1;
                }
            }

            if piece.bottom != bottom {
                rotations_breaks_0 += 1;

                if SIDE_EDGES.contains(&piece.bottom) {
                    side_breaks_0 += 1;
                }
            }

            if ((rotations_breaks_0 == 0) || ((rotations_breaks_0 == 1) && allow_breaks))
                && side_breaks_0 == 0
            {
                rotated_pieces.push(RotatedPieceWithLeftBottom {
                    left_bottom: calculate_two_sides(left, bottom),
                    score: score - 100_000 * rotations_breaks_0 as isize,
                    rotated_piece: RotatedPiece {
                        piece_number: piece.piece_number,
                        rotations: 0,
                        top: piece.top,
                        right: piece.right,
                        break_count: rotations_breaks_0,
                        heuristic_side_count,
                    },
                });
            }

            let mut rotations_breaks_1: u8 = 0;
            let mut side_breaks_1: u8 = 0;

            if piece.bottom != left {
                rotations_breaks_1 += 1;

                if SIDE_EDGES.contains(&piece.bottom) {
                    side_breaks_1 += 1;
                }
            }

            if piece.right != bottom {
                rotations_breaks_1 += 1;

                if SIDE_EDGES.contains(&piece.right) {
                    side_breaks_1 += 1;
                }
            }

            if ((rotations_breaks_1 == 0) || ((rotations_breaks_1 == 1) && allow_breaks))
                && side_breaks_1 == 0
            {
                rotated_pieces.push(RotatedPieceWithLeftBottom {
                    left_bottom: calculate_two_sides(left, bottom),
                    score: score - 100_000 * rotations_breaks_1 as isize,
                    rotated_piece: RotatedPiece {
                        piece_number: piece.piece_number,
                        rotations: 1,
                        top: piece.left,
                        right: piece.top,
                        break_count: rotations_breaks_1,
                        heuristic_side_count,
                    },
                });
            }

            let mut rotations_breaks_2: u8 = 0;
            let mut side_breaks_2: u8 = 0;

            if piece.right != left {
                rotations_breaks_2 += 1;

                if SIDE_EDGES.contains(&piece.right) {
                    side_breaks_2 += 1;
                }
            }

            if piece.top != bottom {
                rotations_breaks_2 += 1;

                if SIDE_EDGES.contains(&piece.top) {
                    side_breaks_2 += 1;
                }
            }

            if ((rotations_breaks_2 == 0) || ((rotations_breaks_2 == 1) && allow_breaks))
                && side_breaks_2 == 0
            {
                rotated_pieces.push(RotatedPieceWithLeftBottom {
                    left_bottom: calculate_two_sides(left, bottom),
                    score: score - 100_000 * rotations_breaks_2 as isize,
                    rotated_piece: RotatedPiece {
                        piece_number: piece.piece_number,
                        rotations: 2,
                        top: piece.bottom,
                        right: piece.left,
                        break_count: rotations_breaks_2,
                        heuristic_side_count,
                    },
                });
            }

            let mut rotations_breaks_3: u8 = 0;
            let mut side_breaks_3: u8 = 0;

            if piece.top != left {
                rotations_breaks_3 += 1;

                if SIDE_EDGES.contains(&piece.top) {
                    side_breaks_3 += 1;
                }
            }

            if piece.left != bottom {
                rotations_breaks_3 += 1;

                if SIDE_EDGES.contains(&piece.left) {
                    side_breaks_3 += 1;
                }
            }

            if ((rotations_breaks_3 == 0) || ((rotations_breaks_3 == 1) && allow_breaks))
                && side_breaks_3 == 0
            {
                rotated_pieces.push(RotatedPieceWithLeftBottom {
                    left_bottom: calculate_two_sides(left, bottom),
                    score: score - 100_000 * rotations_breaks_3 as isize,
                    rotated_piece: RotatedPiece {
                        piece_number: piece.piece_number,
                        rotations: 3,
                        top: piece.right,
                        right: piece.bottom,
                        break_count: rotations_breaks_3,
                        heuristic_side_count,
                    },
                });
            }
        }
    }

    rotated_pieces
}

pub fn save_board(board: &[&RotatedPiece; 256], max_solve_index: usize) {
    let mut entire_board = Builder::default();
    let mut url = Builder::default();

    for i in (0..=15).rev() {
        let mut row = Builder::default();

        for j in 0..=15 {
            if board[i * 16 + j].piece_number > 0 {
                row.append(format!(
                    "{: >3}/{} ",
                    board[i * 16 + j].piece_number,
                    board[i * 16 + j].rotations
                ));

                let mut found_piece: Option<Piece> = None;

                for piece in PIECES {
                    if piece.piece_number == board[i * 16 + j].piece_number {
                        found_piece = Some(piece);
                        break;
                    }
                }
                let p = found_piece.unwrap();

                match board[i * 16 + j].rotations {
                    0 => {
                        url.append(letter_from(p.top));
                        url.append(letter_from(p.right));
                        url.append(letter_from(p.bottom));
                        url.append(letter_from(p.left));
                    }
                    1 => {
                        url.append(letter_from(p.left));
                        url.append(letter_from(p.top));
                        url.append(letter_from(p.right));
                        url.append(letter_from(p.bottom));
                    }
                    2 => {
                        url.append(letter_from(p.bottom));
                        url.append(letter_from(p.left));
                        url.append(letter_from(p.top));
                        url.append(letter_from(p.right));
                    }
                    3 => {
                        url.append(letter_from(p.right));
                        url.append(letter_from(p.bottom));
                        url.append(letter_from(p.left));
                        url.append(letter_from(p.top));
                    }
                    _ => unreachable!(),
                }
            } else {
                row.append("---/- ");
                url.append("aaaa");
            }
        }

        row.append("\n");
        entire_board.append(row.string().unwrap())
    }

    entire_board.append("\n");
    entire_board.append(
        "https://e2.bucas.name/#puzzle=Joshua_Blackwood&board_w=16&board_h=16&board_edges=",
    );
    entire_board.append(url.string().unwrap());
    entire_board.append("&motifs_order=jblackwood");
    entire_board.append("\n");
    let board_string = entire_board.string().unwrap();

    let hash: Digest = md5::compute(board_string.clone());
    let uuid = Uuid::from_slice(hash.as_ref()).unwrap();
    let mut rng = rand::thread_rng();
    let filename = format!(
        "{max_solve_index}_{uuid}_{}.txt",
        rng.gen_range(0..1_000_000)
    );
    let user_dirs = UserDirs::new().expect("Unable to find user directories");
    let documents_dir = user_dirs
        .document_dir()
        .expect("Unable to find documents directory");
    let mut folder_path = PathBuf::from(documents_dir);
    folder_path.push("EternitySolutionsRust");
    fs::create_dir_all(&folder_path).unwrap();
    let mut file_path = folder_path;
    file_path.push(filename);
    let mut file = File::create(file_path).unwrap();
    file.write_all(board_string.as_bytes()).unwrap();
}

fn letter_from(input: u8) -> char {
    char::from_u32(input as u32 + 'a' as u32).unwrap()
}

pub fn first_break_index() -> usize {
    BREAK_INDEXES_ALLOWED[0] as usize
}

#[allow(clippy::needless_range_loop)]
pub fn get_break_array() -> [u8; 256] {
    let mut cumulative_breaks_allowed = 0;
    let mut cumulative_break_array: [u8; 256] = [0; 256];
    for i in 0..256 {
        if BREAK_INDEXES_ALLOWED.contains(&(i as u8)) {
            cumulative_breaks_allowed += 1;
        }
        cumulative_break_array[i] = cumulative_breaks_allowed;
    }

    cumulative_break_array
}

pub fn get_board_order() -> [SearchIndex; 256] {
    let null_search_index = SearchIndex { row: 0, col: 0 };
    let mut board_search_sequence: [SearchIndex; 256] = [null_search_index; 256];
    for row in 0..16 {
        for col in 0..16 {
            let piece_sequence_number = BOARD_ORDER[15 - row][col];
            board_search_sequence[piece_sequence_number as usize] = SearchIndex {
                row: row as u8,
                col: col as u8,
            };
        }
    }
    board_search_sequence
}
