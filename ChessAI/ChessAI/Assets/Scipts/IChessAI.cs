namespace ChessEngine1D
{
    /// <summary>
    /// Interface for a chess AI that works with a 1-D board and half-moves (plies).
    /// </summary>
    public interface IChessAI
    {
        /// <summary>
        /// Returns the chosen ply (half-move) for the current board state.
        /// </summary>
        /// <param name="board">The current board state.</param>
        /// <param name="turn">The side to move.</param>
        /// <returns>A Ply representing the move, or null if no legal plies exist.</returns>
        Ply? GetPly(ChessBoard1D board, PieceColor turn);

        /// <summary>
        /// A name for the AI (useful for logging and display).
        /// </summary>
        string Name { get; }
    }
}
