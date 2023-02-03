using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace EternityII_Solver
{
    class Program
    {
        static int number_virtual_cores = 0;
        static int max_depth = 0;

        static void Main()
        {
            var cores = Environment.GetEnvironmentVariable("CORES");
            if (cores != null)
            {
                number_virtual_cores = Int32.Parse(cores);
            }
            else
            {
                number_virtual_cores = Environment.ProcessorCount;
            }
            
            Stopwatch overallStopwatch = new Stopwatch();
            overallStopwatch.Start();

            long total_index_count = 0;
            int loop_count = 0;
            
            while (true) // Solve for Eternity.
            {
                loop_count++;

                Prepare_Pieces_And_Heuristics();

                Console.WriteLine("Solving with {0} cores...", number_virtual_cores);

                ConcurrentDictionary<int, long> index_counts = new ConcurrentDictionary<int, long>();

                // This only runs number_vcpu-1 threads; we need to save one for the us
                var result = Parallel.For(1, number_virtual_cores, (core, state) =>
                {
                    for (int repeat = 1; repeat <= 5; repeat++)
                    {
                        Console.WriteLine("Core {0}: start loop {1}, repeat {2}", core, loop_count, repeat);
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        SolverResult solver_result = SolvePuzzle();

                        for (int j = 0; j < 257; j++)
                            index_counts.AddOrUpdate(j, solver_result.solve_indexes[j], (id, count) => count + solver_result.solve_indexes[j]);

                        if (solver_result.max_depth > max_depth)
                        {
                            max_depth = solver_result.max_depth;
                        }

                        stopwatch.Stop();
                        Console.WriteLine("Core {0}: finish loop {1}, repeat {2} in {3} seconds", core, loop_count, repeat, (stopwatch.ElapsedMilliseconds) / 1000);
                    }
                });
                Console.WriteLine("Result {0}", result);

                // This will only print valid numbers if you let the solver count how far you are.
                for (int i = 0; i < 257; i++)
                {
                    Console.WriteLine("{0} {1}", i, Util.fmt(index_counts[i])); 
                    total_index_count += index_counts[i];
                }

                var elapsed_time_seconds = (overallStopwatch.ElapsedMilliseconds / 1000);
                var rate = total_index_count / elapsed_time_seconds;
                Console.WriteLine("Total {0} nodes in {1} seconds, {2} per second, max depth {3}",
                    Util.fmt(total_index_count),
                    elapsed_time_seconds,
                    Util.fmt(rate),
                    max_depth
                );
            }
        }

        static unsafe SolverResult SolvePuzzle()
        {
            bool* piece_used = stackalloc bool[257];
            byte* cumulative_heuristic_side_count = stackalloc byte[256];
            byte* piece_index_to_try_next = stackalloc byte[256];
            byte* cumulative_breaks = stackalloc byte[256];
            long[] solve_index_counts = new long[257];
            RotatedPiece* board = stackalloc RotatedPiece[256];

            Random rand = new Random();

            var bottom_sides = new RotatedPiece[529][];
            foreach (var m in bottom_side_pieces_rotated)
                bottom_sides[m.Key] = m.Value.OrderByDescending(x => (x.RotatedPiece.Heuristic_Side_Count > 0 ? 100 : 0) + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            board[0] = corners[0].ToList().OrderBy(x => rand.Next(1, 1000)).First(); // Get rid of pieces 1 or 2 first
            piece_used[board[0].PieceNumber] = true;
            cumulative_breaks[0] = 0;
            cumulative_heuristic_side_count[0] = board[0].Heuristic_Side_Count;

            int solve_index = 1; // this goes from 0....255; we've solved #0 already, so start at #1.
            int max_solve_index = solve_index;
            long node_count = 0;

            while (true)
            {
                node_count++;

                // Uncomment to get this info printed.
                // solve_index_counts[solve_index] = solve_index_counts[solve_index] + 1;

                if (solve_index > max_solve_index)
                {
                    max_solve_index = solve_index;

                    if (solve_index >= 252)
                    {
                        RotatedPiece[] board_to_save = new RotatedPiece[256];

                        for (int i = 0; i < 256; i++)
                            board_to_save[i] = board[i]; // convert to managed just in case

                        Util.Save_Board(board_to_save, (ushort)solve_index);

                        if (solve_index >= 256)
                            return new SolverResult() { solve_indexes = solve_index_counts, max_depth = max_solve_index };
                    }
                }

                if (node_count > 50000000000)
                {
                    return new SolverResult() { solve_indexes = solve_index_counts, max_depth = max_solve_index };
                }

                byte row = board_search_sequence[solve_index].Row;
                byte col = board_search_sequence[solve_index].Column;

                if (board[row * 16 + col].PieceNumber > 0)
                {
                    piece_used[board[row * 16 + col].PieceNumber] = false;
                    board[row * 16 + col].PieceNumber = 0;
                }

                RotatedPiece[] piece_candidates;

                if (row == 0)
                {
                    if (col < 15)
                        piece_candidates = bottom_sides[board[row * 16 + (col - 1)].RightSide * 23 + 0];
                    else
                    {
                        piece_candidates = corners[board[row * 16 + (col - 1)].RightSide * 23 + 0];
                    }
                }
                else
                {
                    var leftSide = (col == 0) ? 0 : board[row * 16 + (col - 1)].RightSide;
                    piece_candidates = master_piece_lookup[row * 16 + col][leftSide * 23 + board[(row - 1) * 16 + col].TopSide];
                }

                bool found_piece = false;
                if (piece_candidates != null)
                {
                    byte breaks_this_turn = (byte)(break_array[solve_index] - cumulative_breaks[solve_index - 1]);
                    int try_index = piece_index_to_try_next[solve_index];

                    int pieceCandidateLength = piece_candidates.Length;
                    for (int i = try_index; i < pieceCandidateLength; i++)
                    {
                        if (piece_candidates[i].Break_Count > breaks_this_turn)
                            break;

                        if (!piece_used[piece_candidates[i].PieceNumber])
                        {
                            if (solve_index <= max_heuristic_index)
                            {
                                if ((cumulative_heuristic_side_count[solve_index - 1] + piece_candidates[i].Heuristic_Side_Count) < heuristic_array[solve_index])
                                    break;
                            }

                            found_piece = true;

                            var piece = piece_candidates[i];

                            board[row * 16 + col] = piece;
                            piece_used[piece.PieceNumber] = true;

                            cumulative_breaks[solve_index] = (byte)(cumulative_breaks[solve_index - 1] + piece.Break_Count);
                            cumulative_heuristic_side_count[solve_index] = (byte)(cumulative_heuristic_side_count[solve_index - 1] + piece.Heuristic_Side_Count);

                            piece_index_to_try_next[solve_index] = (byte)(i + 1);
                            solve_index++;
                            break;
                        }
                    }
                }

                if (!found_piece)
                {
                    piece_index_to_try_next[solve_index] = 0;
                    solve_index--;
                }
            }
        }

        static void Prepare_Pieces_And_Heuristics()
        {
            var board_pieces = Util.Get_Pieces();
            var corner_pieces = board_pieces.Where(x => x.PieceType == 2).ToList();
            var side_pieces = board_pieces.Where(x => x.PieceType == 1).ToList();
            var middle_pieces = board_pieces.Where(x => x.PieceType == 0).Where(x => x.PieceNumber != 139).ToList(); // exclude start piece
            var start_piece = board_pieces.Where(x => x.PieceNumber == 139).ToList();

            // corners
            var corner_pieces_rotated = corner_pieces.Select(x => Util.Get_Rotated_Pieces(x)).SelectMany(x => x).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());

            // sides
            var sides_without_breaks = side_pieces.Select(x => Util.Get_Rotated_Pieces(x)).SelectMany(x => x);
            var sides_with_breaks = side_pieces.Select(x => Util.Get_Rotated_Pieces(x, true)).SelectMany(x => x);
            bottom_side_pieces_rotated = sides_without_breaks.Where(x => x.RotatedPiece.Rotations == 0).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var left_side_pieces_rotated = sides_without_breaks.Where(x => x.RotatedPiece.Rotations == 1).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var right_side_pieces_with_breaks_rotated = sides_with_breaks.Where(x => x.RotatedPiece.Rotations == 3).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var right_side_pieces_without_breaks_rotated = sides_without_breaks.Where(x => x.RotatedPiece.Rotations == 3).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var top_side_pieces_rotated = sides_with_breaks.Where(x => x.RotatedPiece.Rotations == 2).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());

            // middles
            var middle_pieces_rotated_with_breaks = middle_pieces.Select(x => Util.Get_Rotated_Pieces(x, true)).SelectMany(x => x).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var middle_pieces_rotated_without_breaks = middle_pieces.Select(x => Util.Get_Rotated_Pieces(x, false)).SelectMany(x => x).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var south_start_piece_rotated = middle_pieces.Select(x => Util.Get_Rotated_Pieces(x)).SelectMany(x => x).Where(x => x.RotatedPiece.TopSide == 6).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var west_start_piece_rotated = middle_pieces.Select(x => Util.Get_Rotated_Pieces(x)).SelectMany(x => x).Where(x => x.RotatedPiece.RightSide == 11).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());
            var start_piece_rotated = start_piece.Select(x => Util.Get_Rotated_Pieces(x)).SelectMany(x => x).Where(x => x.RotatedPiece.Rotations == 2).GroupBy(x => x.LeftBottom).ToDictionary(x => x.Key, y => y.ToList());

            Random rand = new Random();

            corners = new RotatedPiece[529][];
            foreach (var m in corner_pieces_rotated)
                corners[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            left_sides = new RotatedPiece[529][];
            foreach (var m in left_side_pieces_rotated)
                left_sides[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            top_sides = new RotatedPiece[529][];
            foreach (var m in top_side_pieces_rotated)
                top_sides[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            right_sides_with_breaks = new RotatedPiece[529][];
            foreach (var m in right_side_pieces_with_breaks_rotated)
                right_sides_with_breaks[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            right_sides_without_breaks = new RotatedPiece[529][];
            foreach (var m in right_side_pieces_without_breaks_rotated)
                right_sides_without_breaks[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            middles_with_break = new RotatedPiece[529][];
            foreach (var m in middle_pieces_rotated_with_breaks)
                middles_with_break[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            middles_no_break = new RotatedPiece[529][];
            foreach (var m in middle_pieces_rotated_without_breaks)
                middles_no_break[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            south_start = new RotatedPiece[529][];
            foreach (var m in south_start_piece_rotated)
                south_start[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            west_start = new RotatedPiece[529][];
            foreach (var m in west_start_piece_rotated)
                west_start[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            start = new RotatedPiece[529][];
            foreach (var m in start_piece_rotated)
                start[m.Key] = m.Value.OrderByDescending(x => x.Score + rand.Next(0, 99)).Select(x => x.RotatedPiece).ToArray();

            board_search_sequence = Util.Get_Board_Order();
            break_array = Util.Get_Break_Array();
            master_piece_lookup = new RotatedPiece[256][][];
            for (int i = 0; i < 256; i++)
            {
                int row = board_search_sequence[i].Row;
                int col = board_search_sequence[i].Column;

                if (row == 15)
                {
                    if ((col == 15) || (col == 0))
                        master_piece_lookup[row * 16 + col] = corners;
                    else
                        master_piece_lookup[row * 16 + col] = top_sides;
                }
                else if (row == 0)
                {
                    // Don't populate the master lookup table since we randomize every time.
                }
                else
                {
                    if (col == 15)
                    {
                        if (i < Util.First_Break_Index())
                            master_piece_lookup[row * 16 + col] = right_sides_without_breaks;
                        else
                            master_piece_lookup[row * 16 + col] = right_sides_with_breaks;
                    }
                    else if (col == 0)
                        master_piece_lookup[row * 16 + col] = left_sides;
                    else
                    {
                        if (row == 7)
                        {
                            if (col == 7)
                                master_piece_lookup[row * 16 + col] = start;
                            else if (col == 6)
                                master_piece_lookup[row * 16 + col] = west_start;
                            else
                            {
                                if (i < Util.First_Break_Index())
                                    master_piece_lookup[row * 16 + col] = middles_no_break;
                                else
                                    master_piece_lookup[row * 16 + col] = middles_with_break;
                            }
                        }
                        else if (row == 6)
                        {
                            if (col == 7)
                                master_piece_lookup[row * 16 + col] = south_start;
                            else
                            {
                                if (i < Util.First_Break_Index())
                                    master_piece_lookup[row * 16 + col] = middles_no_break;
                                else
                                    master_piece_lookup[row * 16 + col] = middles_with_break;
                            }
                        }
                        else
                        {
                            if (i < Util.First_Break_Index())
                                master_piece_lookup[row * 16 + col] = middles_no_break;
                            else
                                master_piece_lookup[row * 16 + col] = middles_with_break;
                        }
                    }
                }
            }

            heuristic_array = new int[256];
            for (int i = 0; i < 256; i++)
            {
                if (i <= 16)
                    heuristic_array[i] = 0;
                else if (i <= 26)
                    heuristic_array[i] = (int)(((float)i - 16) * (float)2.8);
                else if (i <= 56)
                    heuristic_array[i] = (int)((((float)i - 26) * (float)1.43333) + 28);
                else if (i <= 76)
                    heuristic_array[i] = (int)(((((float)i - 56) * (float)0.9)) + 71);
                else if (i <= 102)
                    heuristic_array[i] = (int)(((((float)i - 76) * (float)0.6538)) + 89);
                else if (i <= max_heuristic_index)
                    heuristic_array[i] = (int)(((((float)i - 102) / 4.4615)) + 106);
            }
        }

        static RotatedPiece[][] corners;
        static RotatedPiece[][] left_sides;
        static RotatedPiece[][] right_sides_with_breaks;
        static RotatedPiece[][] right_sides_without_breaks;
        static RotatedPiece[][] top_sides;
        static RotatedPiece[][] middles_with_break;
        static RotatedPiece[][] middles_no_break;
        static RotatedPiece[][] south_start;
        static RotatedPiece[][] west_start;
        static RotatedPiece[][] start;
        static Dictionary<ushort, List<RotatedPieceWithLeftBottom>> bottom_side_pieces_rotated;
        static RotatedPiece[][][] master_piece_lookup;

        static SearchIndex[] board_search_sequence;
        static byte[] break_array;
        static int[] heuristic_array;
        const int max_heuristic_index = 160;
    }
}
