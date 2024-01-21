use crate::pieces::PIECES;
use crate::structs::{Piece, RotatedPiece, RotatedPieceWithLeftBottom, SearchIndex, SolverResult};
use crate::utils::{
    first_break_index, get_board_order, get_break_array, get_rotated_pieces, save_board,
};
use itertools::Itertools;
use rand::rngs::ThreadRng;
use rand::Rng;
use rayon::prelude::*;
use std::collections::HashMap;
use std::env;
use std::sync::{Arc, Mutex};
use std::time::Instant;
use thousands::Separable;

mod board_order;
mod pieces;
mod structs;
mod utils;

const MAX_HEURISTIC_INDEX: usize = 160;

fn get_num_cores() -> usize {
    match env::var("CORES") {
        Ok(value) => value.parse::<usize>().unwrap(),
        Err(_e) => num_cpus::get(),
    }
}

fn main() {
    let num_virtual_cores = get_num_cores();
    println!("Using {num_virtual_cores} cores");
    let overall_stopwatch = Instant::now();

    let max_depth = Arc::new(Mutex::new(0));
    let mut total_index_count: u64 = 0;
    let mut loop_count: u64 = 0;

    let empty_vec: Vec<Vec<RotatedPiece>> = vec![vec![]];

    loop {
        loop_count += 1;

        let data = prepare_pieces_and_heuristics();
        let data2 = prepare_master_piece_lookup(&data, &empty_vec);
        println!("Solving with {num_virtual_cores} cores...");

        let index_counts: Arc<Mutex<HashMap<u32, u64>>> = Arc::new(Mutex::new(HashMap::new()));

        (0..num_virtual_cores).into_par_iter().for_each(|core| {
            let max_depth = Arc::clone(&max_depth);
            let index_counts = Arc::clone(&index_counts);

            for repeat in 1..=5 {
                println!("Core {core}: start loop {loop_count}, repeat {repeat}");
                let stopwatch = Instant::now();
                let solver_result = solve_puzzle(&data, &data2);

                {
                    let mut index_counts = index_counts.lock().unwrap();
                    for j in 0..=256 {
                        let count = solver_result.solve_indexes[j];
                        (*index_counts)
                            .entry(j as u32)
                            .and_modify(|e| *e += count)
                            .or_insert(count);
                    }
                }

                {
                    let mut max_depth = max_depth.lock().unwrap();
                    if solver_result.max_depth > *max_depth {
                        *max_depth = solver_result.max_depth;
                    }
                }

                println!(
                    "Core {core}: finish loop {loop_count}, repeat {repeat} in {} seconds",
                    stopwatch.elapsed().as_secs()
                );
            }
        });
        println!("Result"); // No equivalent to C# Parallel.For result.

        // This will only print valid numbers if you let the solver count how far you are.
        let index_counts_clone = index_counts.clone();
        let index_counts_locked = index_counts_clone.lock().unwrap();
        for i in 0..=256 {
            if index_counts_locked[&i] != 0 {
                println!("{i} {}", index_counts_locked[&i].separate_with_commas());
            }
            total_index_count += index_counts_locked[&i];
        }

        let elapsed_time_seconds = overall_stopwatch.elapsed().as_secs();
        let rate = total_index_count / elapsed_time_seconds;
        println!(
            "Total {} nodes in {elapsed_time_seconds} seconds, {} per second, max depth {}",
            total_index_count.separate_with_commas(),
            rate.separate_with_commas(),
            *max_depth.lock().unwrap()
        );
    }
}

fn solve_puzzle(data: &Data, data2: &Data2) -> SolverResult {
    let mut piece_used: [bool; 257] = [false; 257];
    let mut cumulative_heuristic_side_count: [u8; 256] = [0; 256];
    let mut piece_index_to_try_next: [u8; 256] = [0; 256];
    let mut cumulative_breaks: [u8; 256] = [0; 256];
    let mut solve_index_counts: [u64; 257] = [0; 257];
    solve_index_counts[0] = 0; // Avoid warning when unused.
    let null_rotated_piece = RotatedPiece {
        piece_number: 0,
        rotations: 0,
        top: 0,
        right: 0,
        break_count: 0,
        heuristic_side_count: 0,
    };
    let mut board: [RotatedPiece; 256] = [null_rotated_piece; 256];

    let mut rng = rand::thread_rng();

    let mut bottom_sides: Vec<Vec<RotatedPiece>> = vec![vec![]; 529];

    for (key, value) in data.bottom_side_pieces_rotated.iter() {
        let mut sorted_pieces = value.clone();
        sorted_pieces.sort_by(|a, b| {
            let score_a = (if a.rotated_piece.heuristic_side_count > 0 {
                100
            } else {
                0
            }) + rng.gen_range(0..99);
            let score_b = (if b.rotated_piece.heuristic_side_count > 0 {
                100
            } else {
                0
            }) + rng.gen_range(0..99);
            score_b.cmp(&score_a) // Descending order.
        });

        bottom_sides[*key as usize] = sorted_pieces.into_iter().map(|x| x.rotated_piece).collect();
    }

    if let Some(first_corner) = data.corners.first() {
        if let Some(piece) = first_corner.iter().min_by_key(|_| rng.gen_range(1..1000)) {
            board[0] = *piece;
        }
    }

    piece_used[board[0].piece_number as usize] = true;
    cumulative_breaks[0] = 0;
    cumulative_heuristic_side_count[0] = board[0].heuristic_side_count;

    let mut solve_index: usize = 1; // This goes from 0....255; we've solved #0 already, so start at #1.
    let mut max_solve_index: usize = solve_index;
    let mut node_count: u64 = 0;

    loop {
        node_count += 1;

        // Uncomment to get this info printed.
        // solve_index_counts[solve_index] = solve_index_counts[solve_index] + 1;

        if solve_index > max_solve_index {
            max_solve_index = solve_index;

            // TODO reinstate if solve_index >= 252 {
            if solve_index >= 20 {
                save_board(&board.clone(), max_solve_index);

                if solve_index >= 256 {
                    return SolverResult {
                        solve_indexes: solve_index_counts,
                        max_depth: max_solve_index,
                    };
                }
            }
        }

        // TODO reinstate if node_count > 50_000_000_000 {
        if node_count > 50_000_000 {
            return SolverResult {
                solve_indexes: solve_index_counts,
                max_depth: max_solve_index,
            };
        }

        let row = data.board_search_sequence[solve_index].row as usize;
        let col = data.board_search_sequence[solve_index].col as usize;

        if board[row * 16 + col].piece_number > 0 {
            piece_used[board[row * 16 + col].piece_number as usize] = false;
            board[row * 16 + col].piece_number = 0;
        }

        let piece_candidates: &Vec<RotatedPiece> = if row != 0 {
            let left_side = if col == 0 {
                0
            } else {
                board[row * 16 + (col - 1)].right as usize
            };
            let x = data2.master_piece_lookup[row * 16 + col];
            &x[left_side * 23 + board[(row - 1) * 16 + col].top as usize]
        } else if col < 15 {
            &bottom_sides[board[row * 16 + (col - 1)].right as usize * 23]
        } else {
            &data.corners[board[row * 16 + (col - 1)].right as usize * 23]
        };

        let mut found_piece = false;

        if !piece_candidates.is_empty() {
            let breaks_this_turn =
                data.break_array[solve_index] - cumulative_breaks[solve_index - 1];
            let try_index = piece_index_to_try_next[solve_index] as usize;

            let piece_candidate_length = piece_candidates.len();
            for i in try_index..piece_candidate_length {
                if piece_candidates[i].break_count > breaks_this_turn {
                    break;
                }

                if !piece_used[piece_candidates[i].piece_number as usize] {
                    if solve_index <= MAX_HEURISTIC_INDEX
                        && u32::from(
                            cumulative_heuristic_side_count[solve_index - 1]
                                + piece_candidates[i].heuristic_side_count,
                        ) < data.heuristic_array[solve_index]
                    {
                        break;
                    }

                    found_piece = true;

                    let piece = piece_candidates[i];

                    board[row * 16 + col] = piece;
                    piece_used[piece.piece_number as usize] = true;

                    cumulative_breaks[solve_index] =
                        cumulative_breaks[solve_index - 1] + piece.break_count;
                    cumulative_heuristic_side_count[solve_index] = cumulative_heuristic_side_count
                        [solve_index - 1]
                        + piece.heuristic_side_count;

                    piece_index_to_try_next[solve_index] = (i + 1) as u8;
                    solve_index += 1;
                    break;
                }
            }
        }

        if !found_piece {
            piece_index_to_try_next[solve_index] = 0;
            solve_index -= 1;
        }
    }
}

struct Data {
    corners: Vec<Vec<RotatedPiece>>,
    left_sides: Vec<Vec<RotatedPiece>>,
    right_sides_with_breaks: Vec<Vec<RotatedPiece>>,
    right_sides_without_breaks: Vec<Vec<RotatedPiece>>,
    top_sides: Vec<Vec<RotatedPiece>>,
    middles_with_break: Vec<Vec<RotatedPiece>>,
    middles_no_break: Vec<Vec<RotatedPiece>>,
    south_start: Vec<Vec<RotatedPiece>>,
    west_start: Vec<Vec<RotatedPiece>>,
    start: Vec<Vec<RotatedPiece>>,
    bottom_side_pieces_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>>,
    board_search_sequence: [SearchIndex; 256],
    break_array: [u8; 256],
    heuristic_array: [u32; 256],
}

struct Data2<'a> {
    master_piece_lookup: [&'a Vec<Vec<RotatedPiece>>; 256],
}

fn prepare_pieces_and_heuristics() -> Data {
    let corner_pieces: Vec<&Piece> = PIECES
        .iter()
        .filter(|piece| piece.piece_type == 2)
        .collect();
    let side_pieces: Vec<&Piece> = PIECES
        .iter()
        .filter(|piece| piece.piece_type == 1)
        .collect();
    // Exclude start piece.
    let middle_pieces: Vec<&Piece> = PIECES
        .iter()
        .filter(|piece| piece.piece_type == 0 && piece.piece_number != 139)
        .collect();
    let start_piece: Vec<&Piece> = PIECES
        .iter()
        .filter(|piece| piece.piece_number == 139)
        .collect();

    // Corners
    let corner_pieces_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>> = corner_pieces
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();

    // Sides
    let sides_without_breaks: Vec<RotatedPieceWithLeftBottom> = side_pieces
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .collect();
    let sides_with_breaks: Vec<RotatedPieceWithLeftBottom> = side_pieces
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, true))
        .collect();
    let bottom_side_pieces_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>> =
        sides_without_breaks
            .clone()
            .into_iter()
            .filter(|piece| piece.rotated_piece.rotations == 0)
            .group_by(|piece| piece.left_bottom)
            .into_iter()
            .map(|(key, group)| (key, group.collect()))
            .collect();
    let left_side_pieces_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>> =
        sides_without_breaks
            .clone()
            .into_iter()
            .filter(|piece| piece.rotated_piece.rotations == 1)
            .group_by(|piece| piece.left_bottom)
            .into_iter()
            .map(|(key, group)| (key, group.collect()))
            .collect();
    let right_side_pieces_with_breaks_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>> =
        sides_with_breaks
            .clone()
            .into_iter()
            .filter(|piece| piece.rotated_piece.rotations == 3)
            .group_by(|piece| piece.left_bottom)
            .into_iter()
            .map(|(key, group)| (key, group.collect()))
            .collect();
    let right_side_pieces_without_breaks_rotated: HashMap<u16, Vec<RotatedPieceWithLeftBottom>> =
        sides_without_breaks
            .clone()
            .into_iter()
            .filter(|piece| piece.rotated_piece.rotations == 3)
            .group_by(|piece| piece.left_bottom)
            .into_iter()
            .map(|(key, group)| (key, group.collect()))
            .collect();
    let top_side_pieces_rotated: HashMap<_, Vec<_>> = sides_with_breaks
        .clone()
        .into_iter()
        .filter(|piece| piece.rotated_piece.rotations == 2)
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();

    // Middles
    let middle_pieces_rotated_with_breaks: HashMap<_, Vec<_>> = middle_pieces
        .clone()
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, true))
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();
    let middle_pieces_rotated_without_breaks: HashMap<_, Vec<_>> = middle_pieces
        .clone()
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();
    let south_start_piece_rotated: HashMap<_, Vec<_>> = middle_pieces
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .filter(|piece| piece.rotated_piece.top == 6)
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();
    let west_start_piece_rotated: HashMap<_, Vec<_>> = middle_pieces
        .clone()
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .filter(|piece| piece.rotated_piece.right == 11)
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();
    let start_piece_rotated: HashMap<_, Vec<_>> = start_piece
        .iter()
        .flat_map(|piece| get_rotated_pieces(piece, false))
        .filter(|piece| piece.rotated_piece.rotations == 2)
        .group_by(|piece| piece.left_bottom)
        .into_iter()
        .map(|(key, group)| (key, group.collect()))
        .collect();

    let mut rng = rand::thread_rng();

    let corners = build_array(&mut rng, &corner_pieces_rotated);
    let left_sides = build_array(&mut rng, &left_side_pieces_rotated);
    let top_sides = build_array(&mut rng, &top_side_pieces_rotated);
    let right_sides_with_breaks = build_array(&mut rng, &right_side_pieces_with_breaks_rotated);
    let right_sides_without_breaks =
        build_array(&mut rng, &right_side_pieces_without_breaks_rotated);
    let middles_with_break = build_array(&mut rng, &middle_pieces_rotated_with_breaks);
    let middles_no_break = build_array(&mut rng, &middle_pieces_rotated_without_breaks);
    let south_start = build_array(&mut rng, &south_start_piece_rotated);
    let west_start = build_array(&mut rng, &west_start_piece_rotated);
    let start = build_array(&mut rng, &start_piece_rotated);

    let board_search_sequence = get_board_order();
    let break_array = get_break_array();

    let mut heuristic_array: [u32; 256] = [0; 256];
    for i in 0..256 {
        heuristic_array[i] = if i <= 16 {
            0
        } else if i <= 26 {
            ((i as f64 - 16.0) * 2.8f64) as u32
        } else if i <= 56 {
            ((i as f64 - 26.0) * 1.43333f64 + 28.0) as u32
        } else if i <= 76 {
            ((i as f64 - 56.0) * 0.9f64 + 71.0) as u32
        } else if i <= 102 {
            ((i as f64 - 76.0) * 0.6538f64 + 89.0) as u32
        } else if i <= MAX_HEURISTIC_INDEX {
            ((i as f64 - 102.0) / 4.4615f64 + 106.0) as u32
        } else {
            0
        };
    }

    Data {
        corners,
        left_sides,
        right_sides_with_breaks,
        right_sides_without_breaks,
        top_sides,
        middles_with_break,
        middles_no_break,
        south_start,
        west_start,
        start,
        bottom_side_pieces_rotated,
        board_search_sequence,
        break_array,
        heuristic_array,
    }
}

fn prepare_master_piece_lookup<'a>(
    data: &'a Data,
    empty_vec: &'a Vec<Vec<RotatedPiece>>,
) -> Data2<'a> {
    let mut master_piece_lookup: [&Vec<Vec<RotatedPiece>>; 256] = [empty_vec; 256];

    for i in 0..256 {
        let row = data.board_search_sequence[i].row as usize;
        let col = data.board_search_sequence[i].col as usize;

        master_piece_lookup[row * 16 + col] = match row {
            15 => {
                if col == 15 || col == 0 {
                    &data.corners
                } else {
                    &data.top_sides
                }
            }
            0 => {
                // Don't populate the master lookup table since we randomize every time.
                empty_vec
            }
            _ => match col {
                15 => {
                    if i < first_break_index() {
                        &data.right_sides_without_breaks
                    } else {
                        &data.right_sides_with_breaks
                    }
                }
                0 => &data.left_sides,
                _ => match row {
                    7 => match col {
                        7 => &data.start,
                        6 => &data.west_start,
                        _ => {
                            if i < first_break_index() {
                                &data.middles_no_break
                            } else {
                                &data.middles_with_break
                            }
                        }
                    },
                    6 => {
                        if col == 7 {
                            &data.south_start
                        } else if i < first_break_index() {
                            &data.middles_no_break
                        } else {
                            &data.middles_with_break
                        }
                    }
                    _ => {
                        if i < first_break_index() {
                            &data.middles_no_break
                        } else {
                            &data.middles_with_break
                        }
                    }
                },
            },
        }
    }

    Data2 {
        master_piece_lookup,
    }
}

fn build_array(
    rng: &mut ThreadRng,
    input: &HashMap<u16, Vec<RotatedPieceWithLeftBottom>>,
) -> Vec<Vec<RotatedPiece>> {
    let mut output: Vec<Vec<RotatedPiece>> = vec![vec![]; 529];
    for (key, value) in input {
        let mut sorted_pieces = value.clone();
        sorted_pieces.sort_by(|a, b| {
            (b.score + rng.gen_range(0..99)).cmp(&(a.score + rng.gen_range(0..99)))
        });
        output[*key as usize] = sorted_pieces.into_iter().map(|x| x.rotated_piece).collect();
    }
    output
}
