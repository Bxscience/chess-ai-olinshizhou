using System;
using System.Collections.Generic;

namespace ChessEngine1D
{
    public class ChessBoard
    {
        public const int BOARD_SIZE = 64;
        public const int MAX_PLY = 512;  // Maximum number of plies expected in a game

        /// <summary>
        /// The board is represented as a 1-D array of 64 characters.
        /// Each square contains a piece symbol or '.' for an empty square.
        /// </summary>
        public char[] Board { get; private set; }

        /// <summary>
        /// Indicates whose turn it is.
        /// </summary>
        public PieceColor Turn { get; set; }

        /// <summary>
        /// A stack to record plies (moves) for undo purposes.
        /// </summary>
        public Stack<Ply> MoveHistory { get; private set; }

        public ChessBoard1D()
        {
            Board = new char[BOARD_SIZE];
            MoveHistory = new Stack<Ply>(MAX_PLY);
            InitializeBoard();
            Turn = PieceColor.White;
        }

        /// <summary>
        /// Sets up the board with the standard starting position.
        /// The board indices are mapped as follows:
        /// index 0 = a1, index 7 = h1, index 8 = a2, ..., index 63 = h8.
        /// </summary>
        private void InitializeBoard()
        {
            // Fill the board with empty squares.
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                Board[i] = '.';
            }

            // White pieces on Rank 1 (indices 0-7).
            Board[0] = 'R';
            Board[1] = 'N';
            Board[2] = 'B';
            Board[3] = 'Q';
            Board[4] = 'K';
            Board[5] = 'B';
            Board[6] = 'N';
            Board[7] = 'R';

            // White pawns on Rank 2 (indices 8-15).
            for (int i = 8; i < 16; i++)
            {
                Board[i] = 'P';
            }

            // Ranks 3 through 6 remain empty (indices 16-47).

            // Black pawns on Rank 7 (indices 48-55).
            for (int i = 48; i < 56; i++)
            {
                Board[i] = 'p';
            }

            // Black pieces on Rank 8 (indices 56-63).
            Board[56] = 'r';
            Board[57] = 'n';
            Board[58] = 'b';
            Board[59] = 'q';
            Board[60] = 'k';
            Board[61] = 'b';
            Board[62] = 'n';
            Board[63] = 'r';
        }

        /// <summary>
        /// Creates a deep copy of the current board state.
        /// </summary>
        public ChessBoard Clone()
        {
            ChessBoard clone = new ChessBoard
            {
                Turn = this.Turn
            };

            for (int i = 0; i < BOARD_SIZE; i++)
            {
                clone.Board[i] = this.Board[i];
            }

            // Optionally clone the move history.
            clone.MoveHistory = new Stack<Ply>(new Stack<Ply>(this.MoveHistory));
            return clone;
        }

        /// <summary>
        /// Applies the given ply to the board and pushes it onto the move history stack.
        /// </summary>
        public void ApplyPly(Ply ply)
        {
            // Save the move to history.
            MoveHistory.Push(ply);

            // Make the move.
            Board[ply.ToSquare] = Board[ply.FromSquare];
            Board[ply.FromSquare] = '.';

            // Toggle the turn.
            Turn = (Turn == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        }

        /// <summary>
        /// Undoes the last ply (if one exists) by restoring the previous board state.
        /// </summary>
        public void UndoPly()
        {
            if (MoveHistory.Count > 0)
            {
                Ply lastPly = MoveHistory.Pop();
                // Restore the moved piece to its original square.
                Board[lastPly.FromSquare] = lastPly.MovedPiece;
                // Restore the captured piece (or '.' if none) to the destination square.
                Board[lastPly.ToSquare] = lastPly.CapturedPiece;
                // Toggle the turn back.
                Turn = (Turn == PieceColor.White) ? PieceColor.Black : PieceColor.White;
            }
        }

        /// <summary>
        /// Generates legal plies for the current player.
        /// For demonstration purposes, only pawn moves (one square forward) are generated.
        /// </summary>
        public List<Ply> GenerateLegalPlies()
        {
            List<Ply> plies = new List<Ply>();

            // For simplicity, we only implement pawn moves.
            // White pawn moves: move upward by +8.
            // Black pawn moves: move downward by -8.
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                char piece = Board[i];
                if (Turn == PieceColor.White && piece == 'P')
                {
                    int target = i + 8;
                    if (target < BOARD_SIZE && Board[target] == '.')
                    {
                        plies.Add(new Ply(i, target, 'P', Board[target]));
                    }
                }
                else if (Turn == PieceColor.Black && piece == 'p')
                {
                    int target = i - 8;
                    if (target >= 0 && Board[target] == '.')
                    {
                        plies.Add(new Ply(i, target, 'p', Board[target]));
                    }
                }
            }
            return plies;
        }

        /// <summary>
        /// Evaluates the board state.
        /// A positive score favors White; a negative score favors Black.
        /// This simple evaluation only considers basic material values.
        /// </summary>
        public int Evaluate()
        {
            int score = 0;
            for (int i = 0; i < BOARD_SIZE; i++)
            {
                score += GetPieceValue(Board[i]);
            }
            return score;
        }

        private int GetPieceValue(char piece)
        {
            // Material values: Pawn=10, Knight=30, Bishop=30, Rook=50, Queen=90, King=900.
            switch (piece)
            {
                case 'P': return 10;
                case 'N': return 30;
                case 'B': return 30;
                case 'R': return 50;
                case 'Q': return 90;
                case 'K': return 900;
                case 'p': return -10;
                case 'n': return -30;
                case 'b': return -30;
                case 'r': return -50;
                case 'q': return -90;
                case 'k': return -900;
                default: return 0;
            }
        }

        /// <summary>
        /// Helper method that returns the index offset corresponding to a given move direction.
        /// Note: This method does not check for board boundaries.
        /// </summary>
        public static int GetDirectionOffset(MoveDirection direction)
        {
            switch (direction)
            {
                case MoveDirection.N:  return 8;
                case MoveDirection.S:  return -8;
                case MoveDirection.E:  return 1;
                case MoveDirection.W:  return -1;
                case MoveDirection.NE: return 9;
                case MoveDirection.NW: return 7;
                case MoveDirection.SE: return -7;
                case MoveDirection.SW: return -9;
                default: return 0;
            }
        }

        /// <summary>
        /// Prints the board to the console from rank 8 to rank 1.
        /// </summary>
        public void PrintBoard()
        {
            for (int row = 7; row >= 0; row--)
            {
                string line = "";
                for (int col = 0; col < 8; col++)
                {
                    int index = row * 8 + col;
                    line += Board[index] + " ";
                }
                Console.WriteLine(line);
            }
            Console.WriteLine();
        }
    }
}
