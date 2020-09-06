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
        public static List<int> heuristic_sides = new List<int>() { 17, 2, 18 }; // There is a lot of overlap between these sides
        private static List<int> break_indexes_allowed = new List<int>() { 201, 206, 211, 216, 221, 225, 229, 233, 237, 239, 241 };

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
            StringBuilder entire_board = new StringBuilder();

            for (int i = 15; i >= 0; i--)
            {
                StringBuilder row = new StringBuilder();

                for (int j = 0; j < 16; j++)
                {
                    if (board[i * 16 + j].PieceNumber > 0)
                    {
                        row.Append(string.Format("{0}/{1} ", (((int)board[i * 16 + j].PieceNumber)).ToString().PadLeft(3, ' '), board[i * 16 + j].Rotations));
                    }
                    else
                    {
                        row.Append("---/- ");
                    }
                }

                entire_board.AppendLine(row.ToString());
            }

            string board_string = entire_board.ToString();
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
                new Piece() { PieceNumber = 1, TopSide = 1, RightSide = 17, BottomSide = 0, LeftSide = 0 },
                new Piece() { PieceNumber = 2, TopSide = 1, RightSide = 5, BottomSide = 0, LeftSide = 0 },
                new Piece() { PieceNumber = 3, TopSide = 9, RightSide = 17, BottomSide = 0, LeftSide = 0 },
                new Piece() { PieceNumber = 4, TopSide = 17, RightSide = 9, BottomSide = 0, LeftSide = 0 },

                new Piece() { PieceNumber = 8, TopSide = 6, RightSide = 13, BottomSide = 0, LeftSide = 1 },

                new Piece() { PieceNumber = 139, TopSide = 6, RightSide = 11, BottomSide = 18, LeftSide = 6 }
            };

            if (pieces.Count() != 256)
                throw new Exception("You need 256 pieces!!");

            return pieces.ToList();
        }
    }
}