using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace EternityII_Solver
{
    public static class Util
    {
        public static readonly List<int> side_edges = new List<int>() { 1, 5, 9, 13, 17 };
        public static readonly List<int> middle_edges = new List<int>() { 2, 3, 4, 6, 7, 8, 10, 11, 12, 14, 15, 16, 18, 19, 20, 21, 22 };
        public static List<int> heuristic_sides = new List<int>() { 13, 16, 10 }; // There is a lot of overlap between these sides
        private static List<int> break_indexes_allowed = new List<int>() { 201, 206, 211, 216, 221, 225, 229, 233, 237, 239 };

        public static ushort Calculate_Two_Sides(ushort side1, ushort side2)
        {
            return (ushort)(((side1) * 23) + (side2));
        }

        public static List<RotatedPieceWithLeftBottom> Get_Rotated_Pieces(Piece piece, bool allowbreaks = false)
        {
            int score = 0;

            byte heuristic_side_count = 0;
            foreach (var side in heuristic_sides)
            {
                if (piece.LeftSide == side)
                {
                    score += 100;
                    heuristic_side_count++;
                }
                if (piece.TopSide == side)
                {
                    score += 100;
                    heuristic_side_count++;
                }
                if (piece.RightSide == side)
                {
                    score += 100;
                    heuristic_side_count++;
                }
                if (piece.BottomSide == side)
                {
                    score += 100;
                    heuristic_side_count++;
                }
            }

            List<RotatedPieceWithLeftBottom> rotatedPieces = new List<RotatedPieceWithLeftBottom>();
            for (ushort left = 0; left <= 22; left++)
            {
                for (ushort bottom = 0; bottom <= 22; bottom++)
                {
                    byte rotations_breaks_0 = 0;
                    byte side_breaks_0 = 0;
                    if (piece.LeftSide != left)
                    {
                        rotations_breaks_0++;

                        if (side_edges.Contains(piece.LeftSide))
                        {
                            side_breaks_0++;
                        }
                    }
                    if (piece.BottomSide != bottom)
                    {
                        rotations_breaks_0++;

                        if (side_edges.Contains(piece.BottomSide))
                        {
                            side_breaks_0++;
                        }
                    }

                    if ((rotations_breaks_0 == 0) ||
                        ((rotations_breaks_0 == 1) && (allowbreaks)))
                    {
                        if (side_breaks_0 == 0)
                        {
                            rotatedPieces.Add(new RotatedPieceWithLeftBottom()
                            {
                                LeftBottom = Calculate_Two_Sides(left, bottom),
                                RotatedPiece = new RotatedPiece()
                                {
                                    PieceNumber = piece.PieceNumber,
                                    Rotations = 0,
                                    TopSide = piece.TopSide,
                                    RightSide = piece.RightSide,
                                    Break_Count = rotations_breaks_0,
                                    Heuristic_Side_Count = heuristic_side_count
                                },
                                Score = (score - 100000 * rotations_breaks_0)
                            });
                        }
                    }

                    byte rotations_breaks_1 = 0;
                    byte side_breaks_1 = 0;
                    if (piece.BottomSide != left)
                    {
                        rotations_breaks_1++;
                        if (side_edges.Contains(piece.BottomSide))
                        {
                            side_breaks_1++;
                        }
                    }
                    if (piece.RightSide != bottom)
                    {
                        rotations_breaks_1++;
                        if (side_edges.Contains(piece.RightSide))
                        {
                            side_breaks_1++;
                        }
                    }

                    if ((rotations_breaks_1 == 0) ||
                        ((rotations_breaks_1 == 1) && (allowbreaks)))
                    {
                        if (side_breaks_1 == 0)
                        {
                            rotatedPieces.Add(new RotatedPieceWithLeftBottom()
                            {
                                LeftBottom = Calculate_Two_Sides(left, bottom),
                                RotatedPiece = new RotatedPiece()
                                {
                                    PieceNumber = piece.PieceNumber,
                                    Rotations = 1,
                                    TopSide = piece.LeftSide,
                                    RightSide = piece.TopSide,
                                    Break_Count = rotations_breaks_1,
                                    Heuristic_Side_Count = heuristic_side_count
                                },
                                Score = (score - 100000 * rotations_breaks_1)
                            });
                        }
                    }

                    byte rotations_breaks_2 = 0;
                    byte side_breaks_2 = 0;
                    if (piece.RightSide != left)
                    {
                        rotations_breaks_2++;

                        if (side_edges.Contains(piece.RightSide))
                        {
                            side_breaks_2++;
                        }
                    }
                    if (piece.TopSide != bottom)
                    {
                        rotations_breaks_2++;

                        if (side_edges.Contains(piece.TopSide))
                        {
                            side_breaks_2++;
                        }
                    }

                    if ((rotations_breaks_2 == 0) ||
                        ((rotations_breaks_2 == 1) && (allowbreaks)))
                    {
                        if (side_breaks_2 == 0)
                        {
                            rotatedPieces.Add(new RotatedPieceWithLeftBottom()
                            {
                                LeftBottom = Calculate_Two_Sides(left, bottom),
                                RotatedPiece = new RotatedPiece()
                                {
                                    PieceNumber = piece.PieceNumber,
                                    Rotations = 2,
                                    TopSide = piece.BottomSide,
                                    RightSide = piece.LeftSide,
                                    Break_Count = rotations_breaks_2,
                                    Heuristic_Side_Count = heuristic_side_count
                                },
                                Score = (score - 100000 * rotations_breaks_2)
                            });
                        }
                    }

                    byte rotations_breaks_3 = 0;
                    byte side_breaks_3 = 0;
                    if (piece.TopSide != left)
                    {
                        rotations_breaks_3++;

                        if (side_edges.Contains(piece.TopSide))
                        {
                            side_breaks_3++;
                        }
                    }
                    if (piece.LeftSide != bottom)
                    {
                        rotations_breaks_3++;

                        if (side_edges.Contains(piece.LeftSide))
                        {
                            side_breaks_3++;
                        }
                    }

                    if ((rotations_breaks_3 == 0) ||
                        ((rotations_breaks_3 == 1) && (allowbreaks)))
                    {
                        if (side_breaks_3 == 0)
                        {
                            rotatedPieces.Add(new RotatedPieceWithLeftBottom()
                            {
                                LeftBottom = Calculate_Two_Sides(left, bottom),
                                RotatedPiece = new RotatedPiece()
                                {
                                    PieceNumber = piece.PieceNumber,
                                    Rotations = 3,
                                    TopSide = piece.RightSide,
                                    RightSide = piece.BottomSide,
                                    Break_Count = rotations_breaks_3,
                                    Heuristic_Side_Count = heuristic_side_count
                                },
                                Score = (score - 100000 * rotations_breaks_3)
                            });
                        }
                    }
                }
            }

            return rotatedPieces;
        }

        public static void Save_Board(RotatedPiece[] board, ushort maxSolveIndex)
        {
            var board_pieces = Get_Pieces();

            StringBuilder entire_board = new StringBuilder();
            StringBuilder url = new StringBuilder();

            for (int i = 15; i >= 0; i--)
            {
                StringBuilder row = new StringBuilder();

                for (int j = 0; j < 16; j++)
                {
                    if (board[i * 16 + j].PieceNumber > 0)
                    {
                        row.Append(string.Format("{0}/{1} ", (((int)board[i * 16 + j].PieceNumber)).ToString().PadLeft(3, ' '), board[i * 16 + j].Rotations));
                        Piece p = new Piece();

                        foreach (var k in board_pieces)
                            if (k.PieceNumber == board[i * 16 + j].PieceNumber) { p = k; break; }

                        if (board[i * 16 + j].Rotations == 0)
                        {
                            url.Append((char)(p.TopSide + 'a'));
                            url.Append((char)(p.RightSide + 'a'));
                            url.Append((char)(p.BottomSide + 'a'));
                            url.Append((char)(p.LeftSide + 'a'));
                        }
                        if (board[i * 16 + j].Rotations == 1)
                        {
                            url.Append((char)(p.LeftSide + 'a'));
                            url.Append((char)(p.TopSide + 'a'));
                            url.Append((char)(p.RightSide + 'a'));
                            url.Append((char)(p.BottomSide + 'a'));
                        }
                        if (board[i * 16 + j].Rotations == 2)
                        {
                            url.Append((char)(p.BottomSide + 'a'));
                            url.Append((char)(p.LeftSide + 'a'));
                            url.Append((char)(p.TopSide + 'a'));
                            url.Append((char)(p.RightSide + 'a'));
                        }
                        if (board[i * 16 + j].Rotations == 3)
                        {
                            url.Append((char)(p.RightSide + 'a'));
                            url.Append((char)(p.BottomSide + 'a'));
                            url.Append((char)(p.LeftSide + 'a'));
                            url.Append((char)(p.TopSide + 'a'));
                        }
                    }
                    else
                    {
                        row.Append("---/- ");
                        url.Append("aaaa");
                    }
                }

                entire_board.AppendLine(row.ToString());
            }

            string board_string = entire_board.ToString();
            board_string += "\n";
            board_string += "https://e2.bucas.name/#puzzle=Joshua_Blackwood&board_w=16&board_h=16&board_edges=" + url + "&motifs_order=jblackwood";

            Guid? g = null;

            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(board_string));
                g = new Guid(hash);
            }

            Random rand = new Random();
            string filename = maxSolveIndex.ToString() + "_" + g.Value.ToString() + "_" + rand.Next(0, 1000000).ToString() + ".txt";
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "EternitySolutions");
            Directory.CreateDirectory(path);
            System.IO.File.WriteAllText(Path.Combine(path, filename), board_string);
        }

        public static int First_Break_Index()
        {
            if (break_indexes_allowed.Count == 0)
                return 256;

            return break_indexes_allowed.Min();
        }

        public static byte[] Get_Break_Array()
        {
            byte cumulativebreaksAllowed = 0;
            byte[] cumulativebreakArray = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                if (break_indexes_allowed.Contains(i))
                    cumulativebreaksAllowed++;

                cumulativebreakArray[i] = cumulativebreaksAllowed;
            }

            return cumulativebreakArray;
        }

        public static SearchIndex[] Get_Board_Order()
        {
            var board_order = new int[,] {
                    { 196,   197,    198,    199,    200,    205,    210,    215,    220,    225,    230,    235,    243,    249,    254,    255 },
                    { 191,  192,    193,    194,    195,    204,    209,    214,    219,    224,    229,    234,    242,    248,    252,    253 },
                    { 186,  187,    188,    189,    190,    203,    208,    213,    218,    223,    228,    233,    241,    247,    250,    251 },
                    { 181,  182,    183,    184,    185,    202,    207,    212,    217,    222,    227,    232,    240,    244,    245,    246 },
                    { 176,  177,    178,    179,    180,    201,    206,    211,    216,    221,    226,    231,    236,    237,    238,    239 },
                    { 160,  161,    162,    163,    164,    165,    166,    167,    168,    169,    170,    171,    172,    173,    174,    175 },
                    { 144,  145,    146,    147,    148,    149,    150,    151,    152,    153,    154,    155,    156,    157,    158,    159 },
                    { 128,  129,    130,    131,    132,    133,    134,    135,    136,    137,    138,    139,    140,    141,    142,    143 },
                    { 112,  113,    114,    115,    116,    117,    118,    119,    120,    121,    122,    123,    124,    125,    126,    127 },
                    { 96,   97,     98,     99,     100,    101,    102,    103,    104,    105,    106,    107,    108,    109,    110,    111 },
                    { 80,   81,     82,     83,     84,     85,     86,     87,     88,     89,     90,     91,     92,     93,     94,     95 },
                    { 64,   65,     66,     67,     68,     69,     70,     71,     72,     73,     74,     75,     76,     77,     78,     79 },
                    { 48,   49,     50,     51,     52,     53,     54,     55,     56,     57,     58,     59,     60,     61,     62,     63 },
                    { 32,   33,     34,     35,     36,     37,     38,     39,     40,     41,     42,     43,     44,     45,     46,     47 },
                    { 16,   17,     18,     19,     20,     21,     22,     23,     24,     25,     26,     27,     28,     29,     30,     31 },
                    { 0,    1,  2,  3,  4,  5,  6,  7,  8,  9,  10,     11,     12,     13,     14,     15 }
                };

            SearchIndex[] boardSearchSequence = new SearchIndex[256];
            for (byte row = 0; row < 16; row++)
            {
                for (byte col = 0; col < 16; col++)
                {
                    int pieceSequenceNumber = board_order[15 - row, col];
                    var searchIndex = new SearchIndex() { Row = row, Column = col };
                    boardSearchSequence[pieceSequenceNumber] = searchIndex;
                }
            }

            return boardSearchSequence;
        }

        public static List<Piece> Get_Pieces()
        {
            var pieces = new Piece[]{
                new Piece() { PieceNumber = 1, TopSide = 1, RightSide = 17, BottomSide = 0, LeftSide = 0, PieceType = 2 },
                new Piece() { PieceNumber = 2, TopSide = 1, RightSide = 5, BottomSide = 0, LeftSide = 0, PieceType = 2 },
                new Piece() { PieceNumber = 3, TopSide = 9, RightSide = 17, BottomSide = 0, LeftSide = 0, PieceType = 2 },
                new Piece() { PieceNumber = 4, TopSide = 17, RightSide = 9, BottomSide = 0, LeftSide = 0, PieceType = 2 },
                new Piece() { PieceNumber = 5, TopSide = 2, RightSide = 1, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 6, TopSide = 10, RightSide = 9, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 7, TopSide = 6, RightSide = 1, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 8, TopSide = 6, RightSide = 13, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 9, TopSide = 11, RightSide = 17, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 10, TopSide = 7, RightSide = 5, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 11, TopSide = 15, RightSide = 9, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 12, TopSide = 8, RightSide = 5, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 13, TopSide = 8, RightSide = 13, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 14, TopSide = 21, RightSide = 5, BottomSide = 0, LeftSide = 1, PieceType = 1 },
                new Piece() { PieceNumber = 15, TopSide = 10, RightSide = 1, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 16, TopSide = 18, RightSide = 17, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 17, TopSide = 14, RightSide = 13, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 18, TopSide = 19, RightSide = 13, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 19, TopSide = 7, RightSide = 9, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 20, TopSide = 15, RightSide = 9, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 21, TopSide = 4, RightSide = 5, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 22, TopSide = 12, RightSide = 1, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 23, TopSide = 12, RightSide = 13, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 24, TopSide = 20, RightSide = 1, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 25, TopSide = 21, RightSide = 1, BottomSide = 0, LeftSide = 9, PieceType = 1 },
                new Piece() { PieceNumber = 26, TopSide = 2, RightSide = 9, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 27, TopSide = 2, RightSide = 17, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 28, TopSide = 10, RightSide = 17, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 29, TopSide = 18, RightSide = 17, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 30, TopSide = 7, RightSide = 13, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 31, TopSide = 15, RightSide = 9, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 32, TopSide = 20, RightSide = 17, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 33, TopSide = 8, RightSide = 9, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 34, TopSide = 8, RightSide = 5, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 35, TopSide = 16, RightSide = 13, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 36, TopSide = 22, RightSide = 5, BottomSide = 0, LeftSide = 17, PieceType = 1 },
                new Piece() { PieceNumber = 37, TopSide = 18, RightSide = 1, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 38, TopSide = 3, RightSide = 13, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 39, TopSide = 11, RightSide = 13, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 40, TopSide = 19, RightSide = 9, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 41, TopSide = 19, RightSide = 17, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 42, TopSide = 15, RightSide = 1, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 43, TopSide = 15, RightSide = 9, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 44, TopSide = 15, RightSide = 17, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 45, TopSide = 4, RightSide = 1, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 46, TopSide = 20, RightSide = 5, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 47, TopSide = 8, RightSide = 5, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 48, TopSide = 16, RightSide = 5, BottomSide = 0, LeftSide = 5, PieceType = 1 },
                new Piece() { PieceNumber = 49, TopSide = 2, RightSide = 13, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 50, TopSide = 10, RightSide = 1, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 51, TopSide = 10, RightSide = 9, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 52, TopSide = 6, RightSide = 1, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 53, TopSide = 7, RightSide = 5, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 54, TopSide = 4, RightSide = 5, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 55, TopSide = 4, RightSide = 13, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 56, TopSide = 8, RightSide = 17, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 57, TopSide = 16, RightSide = 1, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 58, TopSide = 16, RightSide = 13, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 59, TopSide = 21, RightSide = 9, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 60, TopSide = 22, RightSide = 17, BottomSide = 0, LeftSide = 13, PieceType = 1 },
                new Piece() { PieceNumber = 61, TopSide = 6, RightSide = 18, BottomSide = 2, LeftSide = 2, PieceType = 0 },
                new Piece() { PieceNumber = 62, TopSide = 14, RightSide = 7, BottomSide = 2, LeftSide = 2, PieceType = 0 },
                new Piece() { PieceNumber = 63, TopSide = 10, RightSide = 3, BottomSide = 2, LeftSide = 10, PieceType = 0 },
                new Piece() { PieceNumber = 64, TopSide = 2, RightSide = 8, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 65, TopSide = 18, RightSide = 22, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 66, TopSide = 14, RightSide = 14, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 67, TopSide = 11, RightSide = 10, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 68, TopSide = 20, RightSide = 6, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 69, TopSide = 22, RightSide = 8, BottomSide = 2, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 70, TopSide = 3, RightSide = 7, BottomSide = 2, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 71, TopSide = 7, RightSide = 12, BottomSide = 2, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 72, TopSide = 14, RightSide = 18, BottomSide = 2, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 73, TopSide = 15, RightSide = 4, BottomSide = 2, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 74, TopSide = 20, RightSide = 15, BottomSide = 2, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 75, TopSide = 8, RightSide = 3, BottomSide = 2, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 76, TopSide = 14, RightSide = 15, BottomSide = 2, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 77, TopSide = 19, RightSide = 15, BottomSide = 2, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 78, TopSide = 3, RightSide = 16, BottomSide = 2, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 79, TopSide = 20, RightSide = 3, BottomSide = 2, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 80, TopSide = 16, RightSide = 21, BottomSide = 2, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 81, TopSide = 19, RightSide = 18, BottomSide = 2, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 82, TopSide = 18, RightSide = 18, BottomSide = 2, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 83, TopSide = 11, RightSide = 4, BottomSide = 2, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 84, TopSide = 18, RightSide = 19, BottomSide = 2, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 85, TopSide = 6, RightSide = 14, BottomSide = 2, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 86, TopSide = 8, RightSide = 12, BottomSide = 2, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 87, TopSide = 16, RightSide = 20, BottomSide = 2, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 88, TopSide = 2, RightSide = 21, BottomSide = 2, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 89, TopSide = 6, RightSide = 22, BottomSide = 2, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 90, TopSide = 4, RightSide = 16, BottomSide = 2, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 91, TopSide = 11, RightSide = 12, BottomSide = 2, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 92, TopSide = 19, RightSide = 15, BottomSide = 2, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 93, TopSide = 19, RightSide = 4, BottomSide = 2, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 94, TopSide = 4, RightSide = 21, BottomSide = 2, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 95, TopSide = 12, RightSide = 14, BottomSide = 2, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 96, TopSide = 21, RightSide = 3, BottomSide = 2, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 97, TopSide = 4, RightSide = 19, BottomSide = 2, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 98, TopSide = 20, RightSide = 8, BottomSide = 2, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 99, TopSide = 21, RightSide = 6, BottomSide = 2, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 100, TopSide = 22, RightSide = 21, BottomSide = 2, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 101, TopSide = 12, RightSide = 15, BottomSide = 10, LeftSide = 10, PieceType = 0 },
                new Piece() { PieceNumber = 102, TopSide = 12, RightSide = 16, BottomSide = 10, LeftSide = 10, PieceType = 0 },
                new Piece() { PieceNumber = 103, TopSide = 16, RightSide = 19, BottomSide = 10, LeftSide = 10, PieceType = 0 },
                new Piece() { PieceNumber = 104, TopSide = 22, RightSide = 6, BottomSide = 10, LeftSide = 10, PieceType = 0 },
                new Piece() { PieceNumber = 105, TopSide = 4, RightSide = 15, BottomSide = 10, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 106, TopSide = 3, RightSide = 8, BottomSide = 10, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 107, TopSide = 19, RightSide = 8, BottomSide = 10, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 108, TopSide = 4, RightSide = 15, BottomSide = 10, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 109, TopSide = 16, RightSide = 11, BottomSide = 10, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 110, TopSide = 15, RightSide = 12, BottomSide = 10, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 111, TopSide = 12, RightSide = 15, BottomSide = 10, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 112, TopSide = 20, RightSide = 19, BottomSide = 10, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 113, TopSide = 20, RightSide = 16, BottomSide = 10, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 114, TopSide = 14, RightSide = 4, BottomSide = 10, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 115, TopSide = 7, RightSide = 12, BottomSide = 10, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 116, TopSide = 12, RightSide = 11, BottomSide = 10, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 117, TopSide = 22, RightSide = 16, BottomSide = 10, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 118, TopSide = 3, RightSide = 21, BottomSide = 10, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 119, TopSide = 16, RightSide = 12, BottomSide = 10, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 120, TopSide = 8, RightSide = 22, BottomSide = 10, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 121, TopSide = 14, RightSide = 22, BottomSide = 10, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 122, TopSide = 6, RightSide = 16, BottomSide = 10, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 123, TopSide = 14, RightSide = 19, BottomSide = 10, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 124, TopSide = 20, RightSide = 15, BottomSide = 10, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 125, TopSide = 12, RightSide = 22, BottomSide = 10, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 126, TopSide = 21, RightSide = 15, BottomSide = 10, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 127, TopSide = 14, RightSide = 6, BottomSide = 10, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 128, TopSide = 19, RightSide = 21, BottomSide = 10, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 129, TopSide = 4, RightSide = 3, BottomSide = 10, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 130, TopSide = 20, RightSide = 8, BottomSide = 10, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 131, TopSide = 6, RightSide = 20, BottomSide = 10, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 132, TopSide = 12, RightSide = 14, BottomSide = 10, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 133, TopSide = 14, RightSide = 16, BottomSide = 10, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 134, TopSide = 11, RightSide = 4, BottomSide = 10, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 135, TopSide = 4, RightSide = 3, BottomSide = 10, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 136, TopSide = 16, RightSide = 20, BottomSide = 10, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 137, TopSide = 20, RightSide = 7, BottomSide = 18, LeftSide = 18, PieceType = 0 },
                new Piece() { PieceNumber = 138, TopSide = 6, RightSide = 3, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 139, TopSide = 6, RightSide = 11, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 140, TopSide = 6, RightSide = 12, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 141, TopSide = 19, RightSide = 21, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 142, TopSide = 15, RightSide = 6, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 143, TopSide = 16, RightSide = 12, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 144, TopSide = 21, RightSide = 21, BottomSide = 18, LeftSide = 6, PieceType = 0 },
                new Piece() { PieceNumber = 145, TopSide = 3, RightSide = 4, BottomSide = 18, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 146, TopSide = 18, RightSide = 12, BottomSide = 18, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 147, TopSide = 18, RightSide = 22, BottomSide = 18, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 148, TopSide = 3, RightSide = 14, BottomSide = 18, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 149, TopSide = 15, RightSide = 12, BottomSide = 18, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 150, TopSide = 6, RightSide = 11, BottomSide = 18, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 151, TopSide = 4, RightSide = 22, BottomSide = 18, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 152, TopSide = 11, RightSide = 11, BottomSide = 18, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 153, TopSide = 11, RightSide = 19, BottomSide = 18, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 154, TopSide = 22, RightSide = 16, BottomSide = 18, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 155, TopSide = 7, RightSide = 7, BottomSide = 18, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 156, TopSide = 7, RightSide = 12, BottomSide = 18, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 157, TopSide = 22, RightSide = 7, BottomSide = 18, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 158, TopSide = 7, RightSide = 16, BottomSide = 18, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 159, TopSide = 8, RightSide = 6, BottomSide = 18, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 160, TopSide = 21, RightSide = 21, BottomSide = 18, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 161, TopSide = 6, RightSide = 20, BottomSide = 18, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 162, TopSide = 14, RightSide = 20, BottomSide = 18, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 163, TopSide = 15, RightSide = 11, BottomSide = 18, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 164, TopSide = 4, RightSide = 16, BottomSide = 18, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 165, TopSide = 3, RightSide = 4, BottomSide = 6, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 166, TopSide = 4, RightSide = 8, BottomSide = 6, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 167, TopSide = 3, RightSide = 3, BottomSide = 6, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 168, TopSide = 11, RightSide = 15, BottomSide = 6, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 169, TopSide = 19, RightSide = 21, BottomSide = 6, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 170, TopSide = 4, RightSide = 8, BottomSide = 6, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 171, TopSide = 20, RightSide = 16, BottomSide = 6, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 172, TopSide = 21, RightSide = 11, BottomSide = 6, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 173, TopSide = 15, RightSide = 15, BottomSide = 6, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 174, TopSide = 12, RightSide = 20, BottomSide = 6, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 175, TopSide = 7, RightSide = 21, BottomSide = 6, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 176, TopSide = 7, RightSide = 19, BottomSide = 6, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 177, TopSide = 14, RightSide = 4, BottomSide = 6, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 178, TopSide = 12, RightSide = 16, BottomSide = 6, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 179, TopSide = 8, RightSide = 15, BottomSide = 6, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 180, TopSide = 7, RightSide = 16, BottomSide = 6, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 181, TopSide = 11, RightSide = 16, BottomSide = 6, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 182, TopSide = 7, RightSide = 11, BottomSide = 6, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 183, TopSide = 19, RightSide = 8, BottomSide = 14, LeftSide = 14, PieceType = 0 },
                new Piece() { PieceNumber = 184, TopSide = 22, RightSide = 7, BottomSide = 14, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 185, TopSide = 19, RightSide = 12, BottomSide = 14, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 186, TopSide = 8, RightSide = 8, BottomSide = 14, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 187, TopSide = 21, RightSide = 7, BottomSide = 14, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 188, TopSide = 14, RightSide = 21, BottomSide = 14, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 189, TopSide = 3, RightSide = 19, BottomSide = 14, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 190, TopSide = 16, RightSide = 19, BottomSide = 14, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 191, TopSide = 3, RightSide = 3, BottomSide = 14, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 192, TopSide = 15, RightSide = 20, BottomSide = 14, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 193, TopSide = 11, RightSide = 7, BottomSide = 14, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 194, TopSide = 21, RightSide = 11, BottomSide = 14, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 195, TopSide = 21, RightSide = 22, BottomSide = 14, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 196, TopSide = 22, RightSide = 15, BottomSide = 14, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 197, TopSide = 11, RightSide = 22, BottomSide = 14, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 198, TopSide = 19, RightSide = 8, BottomSide = 14, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 199, TopSide = 20, RightSide = 20, BottomSide = 14, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 200, TopSide = 19, RightSide = 3, BottomSide = 14, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 201, TopSide = 21, RightSide = 8, BottomSide = 14, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 202, TopSide = 22, RightSide = 7, BottomSide = 14, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 203, TopSide = 12, RightSide = 19, BottomSide = 14, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 204, TopSide = 12, RightSide = 8, BottomSide = 14, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 205, TopSide = 16, RightSide = 3, BottomSide = 14, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 206, TopSide = 22, RightSide = 21, BottomSide = 14, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 207, TopSide = 22, RightSide = 7, BottomSide = 3, LeftSide = 3, PieceType = 0 },
                new Piece() { PieceNumber = 208, TopSide = 19, RightSide = 22, BottomSide = 3, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 209, TopSide = 8, RightSide = 15, BottomSide = 3, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 210, TopSide = 11, RightSide = 19, BottomSide = 3, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 211, TopSide = 16, RightSide = 15, BottomSide = 3, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 212, TopSide = 3, RightSide = 16, BottomSide = 3, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 213, TopSide = 8, RightSide = 8, BottomSide = 3, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 214, TopSide = 3, RightSide = 20, BottomSide = 3, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 215, TopSide = 4, RightSide = 22, BottomSide = 3, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 216, TopSide = 22, RightSide = 21, BottomSide = 3, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 217, TopSide = 19, RightSide = 15, BottomSide = 3, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 218, TopSide = 4, RightSide = 12, BottomSide = 3, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 219, TopSide = 11, RightSide = 4, BottomSide = 3, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 220, TopSide = 11, RightSide = 16, BottomSide = 3, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 221, TopSide = 21, RightSide = 21, BottomSide = 3, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 222, TopSide = 21, RightSide = 22, BottomSide = 3, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 223, TopSide = 12, RightSide = 22, BottomSide = 11, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 224, TopSide = 20, RightSide = 7, BottomSide = 11, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 225, TopSide = 16, RightSide = 15, BottomSide = 11, LeftSide = 11, PieceType = 0 },
                new Piece() { PieceNumber = 226, TopSide = 19, RightSide = 15, BottomSide = 11, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 227, TopSide = 12, RightSide = 12, BottomSide = 11, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 228, TopSide = 19, RightSide = 8, BottomSide = 11, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 229, TopSide = 7, RightSide = 22, BottomSide = 11, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 230, TopSide = 16, RightSide = 8, BottomSide = 11, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 231, TopSide = 12, RightSide = 20, BottomSide = 11, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 232, TopSide = 12, RightSide = 21, BottomSide = 11, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 233, TopSide = 19, RightSide = 20, BottomSide = 19, LeftSide = 19, PieceType = 0 },
                new Piece() { PieceNumber = 234, TopSide = 16, RightSide = 4, BottomSide = 19, LeftSide = 7, PieceType = 0 },
                new Piece() { PieceNumber = 235, TopSide = 7, RightSide = 4, BottomSide = 19, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 236, TopSide = 7, RightSide = 20, BottomSide = 19, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 237, TopSide = 12, RightSide = 15, BottomSide = 19, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 238, TopSide = 4, RightSide = 16, BottomSide = 19, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 239, TopSide = 15, RightSide = 22, BottomSide = 19, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 240, TopSide = 21, RightSide = 15, BottomSide = 19, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 241, TopSide = 7, RightSide = 21, BottomSide = 19, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 242, TopSide = 4, RightSide = 21, BottomSide = 19, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 243, TopSide = 15, RightSide = 12, BottomSide = 7, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 244, TopSide = 20, RightSide = 8, BottomSide = 7, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 245, TopSide = 22, RightSide = 20, BottomSide = 7, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 246, TopSide = 16, RightSide = 22, BottomSide = 7, LeftSide = 21, PieceType = 0 },
                new Piece() { PieceNumber = 247, TopSide = 21, RightSide = 22, BottomSide = 15, LeftSide = 15, PieceType = 0 },
                new Piece() { PieceNumber = 248, TopSide = 12, RightSide = 4, BottomSide = 15, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 249, TopSide = 4, RightSide = 21, BottomSide = 15, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 250, TopSide = 16, RightSide = 21, BottomSide = 15, LeftSide = 20, PieceType = 0 },
                new Piece() { PieceNumber = 251, TopSide = 22, RightSide = 8, BottomSide = 4, LeftSide = 4, PieceType = 0 },
                new Piece() { PieceNumber = 252, TopSide = 8, RightSide = 12, BottomSide = 4, LeftSide = 12, PieceType = 0 },
                new Piece() { PieceNumber = 253, TopSide = 16, RightSide = 20, BottomSide = 12, LeftSide = 8, PieceType = 0 },
                new Piece() { PieceNumber = 254, TopSide = 21, RightSide = 16, BottomSide = 20, LeftSide = 16, PieceType = 0 },
                new Piece() { PieceNumber = 255, TopSide = 16, RightSide = 22, BottomSide = 20, LeftSide = 22, PieceType = 0 },
                new Piece() { PieceNumber = 256, TopSide = 21, RightSide = 22, BottomSide = 8, LeftSide = 22, PieceType = 0 }
            };

            if (pieces.Count() != 256)
                throw new Exception("You need 256 pieces!!");

            return pieces.ToList();
        }
    }
}
