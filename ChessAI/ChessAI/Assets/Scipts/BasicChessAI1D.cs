using System;
using System.Collections.Generic;

namespace ChessEngine
{
    public class BasicChessAI1D : IChessAI
    {
        private Random random;
        public string Name { get; private set; }

        public BasicChessAI1D(string name = "Basic1D")
        {
            Name = name;
            random = new Random();
        }

        public Ply? GetPly(ChessBoard1D board, PieceColor turn)
        {
            // Get a list of legal plies.
            List<Ply> plies = board.GenerateLegalPlies();

            if (plies.Count == 0)
            {
                // No legal moves available.
                return null;
            }

            // Pick a random ply.
            int index = random.Next(plies.Count);
            return plies[index];
        }
    }
}
