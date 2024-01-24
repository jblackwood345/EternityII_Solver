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

fn add_rotated_piece(
    rotated_pieces: &mut Vec<RotatedPieceWithLeftBottom>,
    left: u8,
    bottom: u8,
    piece_left: u8,
    piece_bottom: u8,
    allow_breaks: bool,
    score: isize,
    rotations: u8,
    piece_number: u16,
    piece_top: u8,
    piece_right: u8,
    heuristic_side_count: u8,
) {
    let mut rotations_breaks: u8 = 0;
    let mut side_breaks: u8 = 0;

    if piece_left != left {
        rotations_breaks += 1;

        if SIDE_EDGES.contains(&piece_left) {
            side_breaks += 1;
        }
    }

    if piece_bottom != bottom {
        rotations_breaks += 1;

        if SIDE_EDGES.contains(&piece_bottom) {
            side_breaks += 1;
        }
    }

    if ((rotations_breaks == 0) || ((rotations_breaks == 1) && allow_breaks)) && side_breaks == 0 {
        rotated_pieces.push(RotatedPieceWithLeftBottom {
            left_bottom: calculate_two_sides(left, bottom),
            score: score - 100_000 * rotations_breaks as isize,
            rotated_piece: RotatedPiece {
                piece_number,
                rotations,
                top: piece_top,
                right: piece_right,
                break_count: rotations_breaks,
                heuristic_side_count,
            },
        });
    }
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
            add_rotated_piece(
                &mut rotated_pieces,
                left,
                bottom,
                piece.left,
                piece.bottom,
                allow_breaks,
                score,
                0,
                piece.piece_number,
                piece.top,
                piece.right,
                heuristic_side_count,
            );
            add_rotated_piece(
                &mut rotated_pieces,
                left,
                bottom,
                piece.bottom,
                piece.right,
                allow_breaks,
                score,
                1,
                piece.piece_number,
                piece.left,
                piece.top,
                heuristic_side_count,
            );
            add_rotated_piece(
                &mut rotated_pieces,
                left,
                bottom,
                piece.right,
                piece.top,
                allow_breaks,
                score,
                2,
                piece.piece_number,
                piece.bottom,
                piece.left,
                heuristic_side_count,
            );
            add_rotated_piece(
                &mut rotated_pieces,
                left,
                bottom,
                piece.top,
                piece.left,
                allow_breaks,
                score,
                3,
                piece.piece_number,
                piece.right,
                piece.bottom,
                heuristic_side_count,
            );
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
