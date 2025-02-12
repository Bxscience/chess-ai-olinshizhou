using System;

namespace ChessEngine
{
    public class ChessGameManager1D
    {
        private IChessAI1D whiteAI;
        private IChessAI1D blackAI;
        private ChessBoard1D board;

        public ChessGameManager1D(IChessAI1D white, IChessAI1D black)
        {
            whiteAI = white;
            blackAI = black;
            board = new ChessBoard1D();
        }

        /// <summary>
        /// Plays a game until one side has no legal plies.
        /// (For demonstration, the game stops after 100 plies.)
        /// </summary>
        public void PlayGame()
        {
            int plyCount = 0;
            while (true)
            {
                board.PrintBoard();
                IChessAI1D currentAI = (board.Turn == PieceColor.White) ? whiteAI : blackAI;
                Console.WriteLine($"It is {board.Turn}'s turn ({currentAI.Name}).");

                // Use a clone of the board for the AI so that its internal simulation does not affect the actual board.
                Ply? ply = currentAI.GetPly(board.Clone(), board.Turn);
                if (!ply.HasValue)
                {
                    Console.WriteLine($"{board.Turn} has no legal plies. Game over.");
                    break;
                }
                Console.WriteLine($"{currentAI.Name} plays ply: {ply.Value}");
                board.ApplyPly(ply.Value);
                plyCount++;

                // For demonstration purposes, stop after 100 plies.
                if (plyCount > 100)
                {
                    Console.WriteLine("Game ended after 100 plies.");
                    break;
                }
            }
        }

        // Uncomment the following Main() method to run as a console application.
        /*
        public static void Main(string[] args)
        {
            IChessAI1D white = new BasicChessAI1D("WhiteBasic1D");
            IChessAI1D black = new BasicChessAI1D("BlackBasic1D");
            ChessGameManager1D game = new ChessGameManager1D(white, black);
            game.PlayGame();
        }
        */
    }
}
