using System;
using System.Collections.Generic;
using UnityEngine;

public class AIEngine
{
    // Transposition Table Entry 
    private class TranspositionEntry
    {
        public int depth;
        public int evaluation;
        public AIMove bestMove;
        public int flag; // 0 = exact, 1 = lower bound, 2 = upper bound
    }
    
    // Undo Structure 
    public struct MoveUndoInfo
    {
        public Vector2Int start;
        public Vector2Int end;
        public GameObject movedPiece;
        public GameObject capturedPiece;
        public bool pawnWasMoved;
        public int hashBefore;
        // Fields for restoring turn information
        public Player currentPlayerBefore;
        public Player otherPlayerBefore;
        // Field for the owner of a captured piece
        public Player capturedPieceOwner;
    }

    // Adjusted killer bonus – reduced from 900 to 500 for tuning.
    private const int KILLER_BONUS = 500; 
    private Dictionary<int, AIMove> killerMoves = new Dictionary<int, AIMove>();
    // Removed historyHeuristic dictionary
    private Dictionary<int, TranspositionEntry> transpositionTable = new Dictionary<int, TranspositionEntry>();
    private const int MAX_QUIESCENCE_DEPTH = 6;
    private AIMove bestMoveFromLastSearch = null;
    private int lastStateHash = 0;  // To track board state changes

    // Reusable move buffers
    private List<AIMove> moveBuffer = new List<AIMove>(64);
    private List<AIMove> captureBuffer = new List<AIMove>(32);

    // Counters for search stats
    private int nodesSearched = 0;
    private int nodesQuiescence = 0;

    // Debug flag
    private bool debugEnabled = false;

    // Repetition detection – keyed by state.hash
    private Dictionary<int, int> positionHistory = new Dictionary<int, int>();

    private int ComputeHash(SimulatedState state)
    {
        int hash = 17;

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                if (state.pieces[x, y] != null)
                    hash = hash * 31 + state.pieces[x, y].GetInstanceID();
            }
        }

        hash = hash * 31 + state.currentPlayer.GetHashCode();
        hash = hash * 31 + state.otherPlayer.GetHashCode();
        return hash;
    }

    // --- Added: EvaluatePosition to add a mobility bonus to the evaluation ---
    // This function wraps your Evaluation.EvaluateBoard call and adds a bonus 
    // for mobility (number of legal moves) to help account for “what you can do”
    private int EvaluatePosition(SimulatedState state)
    {
        int staticEval = Evaluation.EvaluateBoard(state);
        // Count mobility for the current player (the player whose turn it is)
        int mobility = GenerateAllPossibleMoves(state.currentPlayer, state, new List<AIMove>()).Count;
        // Multiply mobility by an arbitrary weight (10) which can be tuned as needed.
        return staticEval + mobility * 10;
    }

    // --- Public API ---
    public AIMove GetBestMove(SimulatedState state, int maxDepth)
    {
        nodesSearched = 0;
        nodesQuiescence = 0;

        if (state.hash != lastStateHash)
        {
            bestMoveFromLastSearch = null;
            lastStateHash = state.hash;
        }

        if (transpositionTable.Count > 1000000)
            transpositionTable.Clear();

        AIMove bestMove = null;
        if (!positionHistory.ContainsKey(state.hash))
            positionHistory[state.hash] = 0;
        positionHistory[state.hash]++;

        for (int currentDepth = 1; currentDepth <= maxDepth; currentDepth++)
        {
            int alpha = int.MinValue + 100;
            int beta = int.MaxValue - 100;

            List<AIMove> moves = GenerateAllPossibleMoves(state.currentPlayer, state, new List<AIMove>(64));

            if (bestMoveFromLastSearch != null)
            {
                foreach (AIMove move in moves)
                {
                    if (IsSameMove(move, bestMoveFromLastSearch))
                    {
                        move.score += 500; 
                        break;
                    }
                }
            }

            moves = SortMovesAdvanced(moves, currentDepth, state);

            int bestValue = int.MinValue;
            AIMove currentBestMove = null;
            List<AIMove> movesLocal = new List<AIMove>(moves);
            foreach (AIMove move in movesLocal)
            {
                MoveUndoInfo undo = ApplyMove(move, state);

                // Skip moves that leave the king in check.
                if (IsInCheck(state.otherPlayer, state))
                {
                    UndoMove(undo, state);
                    continue;
                }

                int value = -AlphaBeta(currentDepth - 1, -beta, -alpha, state);
                UndoMove(undo, state);

                if (value > bestValue)
                {
                    bestValue = value;
                    currentBestMove = move;
                    if (bestValue > alpha)
                        alpha = bestValue;
                }
            }

            if (currentBestMove != null)
            {
                bestMove = currentBestMove;
                bestMoveFromLastSearch = bestMove;
                if (debugEnabled)
                {
                    Debug.Log($"Depth {currentDepth}: Best move is {bestMove.piece.name} from {bestMove.startGridPoint} to {bestMove.targetGridPoint} with eval {bestValue}");
                    Debug.Log($"Nodes searched: {nodesSearched}, Quiescence: {nodesQuiescence}");
                }
            }
        }

        if (bestMove != null)
        {
            MoveUndoInfo undo = ApplyMove(bestMove, state);
            if (!positionHistory.ContainsKey(state.hash))
                positionHistory[state.hash] = 0;
            positionHistory[state.hash]++;
            UndoMove(undo, state);
        }

        return bestMove;
    }

    // --- AlphaBeta Search ---
    private int AlphaBeta(int depth, int alpha, int beta, SimulatedState state)
    {
        nodesSearched++;

        int originalAlpha = alpha;
        int currentHash = state.hash;
        if (transpositionTable.TryGetValue(currentHash, out TranspositionEntry entry) && entry.depth >= depth)
        {
            if (entry.flag == 0)
                return entry.evaluation;
            else if (entry.flag == 1)
                alpha = Math.Max(alpha, entry.evaluation);
            else if (entry.flag == 2)
                beta = Math.Min(beta, entry.evaluation);

            if (alpha >= beta)
                return entry.evaluation;
        }

        if (depth <= 0)
            // Use our improved evaluation in quiescence search.
            return QuiescenceSearch(alpha, beta, state, 0, MAX_QUIESCENCE_DEPTH);

        List<AIMove> moves = GenerateAllPossibleMoves(state.currentPlayer, state, moveBuffer);
        moves = SortMovesAdvanced(moves, depth, state);

        if (moves.Count == 0)
        {
            if (IsInCheck(state.currentPlayer, state))
                return int.MinValue + 200;
            else
                return 0;
        }

        int bestScore = int.MinValue + 100;
        AIMove bestMove = null;
        bool hasLegalMove = false;
        List<AIMove> movesLocal = new List<AIMove>(moves);
        foreach (AIMove move in movesLocal)
        {
            MoveUndoInfo undo = ApplyMove(move, state);
            if (IsInCheck(state.otherPlayer, state))
            {
                UndoMove(undo, state);
                continue;
            }
            hasLegalMove = true;
            int score = -AlphaBeta(depth - 1, -beta, -alpha, state);
            UndoMove(undo, state);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
            if (score > alpha)
            {
                alpha = score;
            }
            if (alpha >= beta)
            {
                killerMoves[depth] = move;
                TranspositionEntry newEntry = new TranspositionEntry
                {
                    depth = depth,
                    evaluation = alpha,
                    bestMove = move,
                    flag = 1 // Lower bound
                };
                transpositionTable[currentHash] = newEntry;
                return alpha;
            }
        }

        if (!hasLegalMove)
        {
            if (IsInCheck(state.currentPlayer, state))
                return int.MinValue + 200;
            else
                return 0;
        }

        int flag = 0;
        if (bestScore <= originalAlpha)
            flag = 2;
        else if (bestScore >= beta)
            flag = 1;

        TranspositionEntry finalEntry = new TranspositionEntry
        {
            depth = depth,
            evaluation = bestScore,
            bestMove = bestMove,
            flag = flag
        };
        transpositionTable[currentHash] = finalEntry;

        return bestScore;
    }

    // --- Quiescence Search ---
    private int QuiescenceSearch(int alpha, int beta, SimulatedState state, int qsDepth, int maxQsDepth)
    {
        nodesQuiescence++;
        // Use the new EvaluatePosition 
        int standPat = EvaluatePosition(state);
        if (qsDepth >= maxQsDepth)
            return standPat;
        if (standPat >= beta)
            return beta;
        if (standPat > alpha)
            alpha = standPat;

        List<AIMove> captureMoves = GenerateCaptureMoves(state, captureBuffer);
        List<AIMove> localCaptureMoves = new List<AIMove>(captureMoves);
        localCaptureMoves = SortMovesAdvanced(localCaptureMoves, 0, state);
        foreach (AIMove move in localCaptureMoves)
        {
            MoveUndoInfo undo = ApplyMove(move, state);
            if (IsInCheck(state.otherPlayer, state))
            {
                UndoMove(undo, state);
                continue;
            }
            int score = -QuiescenceSearch(-beta, -alpha, state, qsDepth + 1, maxQsDepth);
            UndoMove(undo, state);
            if (score >= beta)
                return beta;
            if (score > alpha)
                alpha = score;
        }
        return alpha;
    }

    // --- Move Generation ---
    private List<AIMove> GenerateAllPossibleMoves(Player player, SimulatedState state, List<AIMove> buffer)
    {
        buffer.Clear();
        foreach (GameObject piece in new List<GameObject>(player.pieces))
        {
            if (piece == null)
                continue;
            Vector2Int gridPoint = Evaluation.GetGridForPiece(piece, state.pieces);
            if (!IsValidGridPoint(gridPoint))
                continue;
            Piece pieceComponent = piece.GetComponent<Piece>();
            if (pieceComponent == null)
                continue;
            List<Vector2Int> validMoves = pieceComponent.MoveLocations(gridPoint);
            for (int i = 0; i < validMoves.Count; i++)
            {
                Vector2Int target = validMoves[i];
                if (!IsValidGridPoint(target))
                    continue;
                GameObject targetPiece = state.pieces[target.x, target.y];
                if (targetPiece != null && player.pieces.Contains(targetPiece))
                    continue;
                AIMove move = new AIMove()
                {
                    piece = piece,
                    startGridPoint = gridPoint,
                    targetGridPoint = target,
                    capturedPiece = targetPiece,
                    score = 0
                };
                if (targetPiece != null)
                {
                    Piece targetPieceComponent = targetPiece.GetComponent<Piece>();
                    Piece movingPieceComponent = piece.GetComponent<Piece>();
                    if (targetPieceComponent != null && movingPieceComponent != null)
                    {
                        int capturedValue = Evaluation.GetPieceValue(targetPiece);
                        int moverValue = Evaluation.GetPieceValue(piece);
                        move.score = (capturedValue * 100) - moverValue;
                    }
                }
                if (pieceComponent.type == PieceType.Pawn)
                {
                    string color = pieceComponent.color;
                    if ((color == "white" && target.y == 7) ||
                        (color == "black" && target.y == 0))
                    {
                        move.score += 8000;
                    }
                }
                buffer.Add(move);
            }
        }
        return buffer;
    }

    /// <summary>
    /// Fixed Static Exchange Evaluation (SEE) that correctly simulates alternating captures.
    /// </summary>
    private int StaticExchangeEvaluation(AIMove move, SimulatedState state)
    {
        int movingValue = Evaluation.GetPieceValue(move.piece);
        int gain0;
        if (move.capturedPiece != null)
        {
            gain0 = Evaluation.GetPieceValue(move.capturedPiece) - movingValue;
        }
        else
        {
            var pc = move.piece.GetComponent<Piece>();
            int beforePS = Evaluation.GetPieceSquareValue(pc.type, move.startGridPoint);
            int afterPS  = Evaluation.GetPieceSquareValue(pc.type, move.targetGridPoint);
            gain0 = afterPS - beforePS;
        }
        bool us = state.currentPlayer.pieces.Contains(move.piece);
        Vector2Int square = move.targetGridPoint;
        List<int> usAttackers = new List<int>();
        List<int> themAttackers = new List<int>();
        foreach (var piece in state.currentPlayer.pieces)
        {
            if (piece == null || piece == move.piece) continue;
            Vector2Int pos = Evaluation.GetGridForPiece(piece, state.pieces);
            if (piece.GetComponent<Piece>().MoveLocations(pos).Contains(square))
                usAttackers.Add(Evaluation.GetPieceValue(piece));
        }
        foreach (var piece in state.otherPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = Evaluation.GetGridForPiece(piece, state.pieces);
            if (piece.GetComponent<Piece>().MoveLocations(pos).Contains(square))
                themAttackers.Add(Evaluation.GetPieceValue(piece));
        }
        usAttackers.Sort();
        themAttackers.Sort();
        int total = usAttackers.Count + themAttackers.Count;
        int[] gain = new int[total + 1];
        gain[0] = gain0;
        int d = 0;
        int usIdx = 0, themIdx = 0;
        bool ourTurn = false; // false: opponent's turn; true: our turn.
        while (true)
        {
            if (!ourTurn)
            {
                if (themIdx >= themAttackers.Count) break;
                int attackerVal = themAttackers[themIdx++];
                d++;
                gain[d] = attackerVal - gain[d - 1];
            }
            else
            {
                if (usIdx >= usAttackers.Count) break;
                int attackerVal = usAttackers[usIdx++];
                d++;
                gain[d] = attackerVal - gain[d - 1];
            }
            ourTurn = !ourTurn;
        }
        for (int i = d - 1; i >= 0; i--)
            gain[i] = -Math.Max(-gain[i], gain[i + 1]);
        return gain[0];
    }

    private List<AIMove> SortMovesAdvanced(List<AIMove> moves, int currentDepth, SimulatedState state)
    {
        foreach (var move in moves)
        {
            int see = StaticExchangeEvaluation(move, state);
            // Apply a clamped bonus/penalty based on SEE – multiplier can be tuned (here using 50)
            move.score += Math.Clamp(see * 50, -500, 500);

            // Apply killer move bonus (using our new KILLER_BONUS value)
            if (killerMoves.TryGetValue(currentDepth, out var killer) && IsSameMove(move, killer))
                move.score += KILLER_BONUS;

            // Reduced bonus for moves giving check (from 500 to 300)
            if (DoesMoveGiveCheck(move, state))
                move.score += 300;
        }
        moves.Sort((a, b) => b.score.CompareTo(a.score));
        return moves;
    }

    // --- Generate Capture Moves ---
    private List<AIMove> GenerateCaptureMoves(SimulatedState state, List<AIMove> capMove)
    {
        capMove.Clear();
        foreach (GameObject piece in new List<GameObject>(state.currentPlayer.pieces))
        {
            if (piece == null)
                continue;
            Vector2Int gridPoint = Evaluation.GetGridForPiece(piece, state.pieces);
            if (!IsValidGridPoint(gridPoint))
                continue;
            Piece pieceComponent = piece.GetComponent<Piece>();
            if (pieceComponent == null)
                continue;
            List<Vector2Int> validMoves = pieceComponent.MoveLocations(gridPoint);
            for (int i = 0; i < validMoves.Count; i++)
            {
                Vector2Int target = validMoves[i];
                if (!IsValidGridPoint(target))
                    continue;
                GameObject targetPiece = state.pieces[target.x, target.y];
                if (targetPiece != null && !state.currentPlayer.pieces.Contains(targetPiece))
                {
                    AIMove move = new AIMove()
                    {
                        piece = piece,
                        startGridPoint = gridPoint,
                        targetGridPoint = target,
                        capturedPiece = targetPiece,
                        score = 0
                    };
                    int capturedValue = Evaluation.GetPieceValue(targetPiece);
                    int moverValue = Evaluation.GetPieceValue(piece);
                    move.score = (capturedValue * 100) - moverValue;
                    int seeResult = StaticExchangeEvaluation(move, state);
                    move.score += Math.Clamp(seeResult * 50, -500, 500);
                    capMove.Add(move);
                }
            }
        }
        return capMove;
    }

    private string GetMoveKey(AIMove move)
    {
        return move.piece.GetInstanceID() + "_" + move.startGridPoint.x + "_" + move.startGridPoint.y + "_" +
               move.targetGridPoint.x + "_" + move.targetGridPoint.y;
    }

    // --- Check if Move Gives Check ---
    private bool DoesMoveGiveCheck(AIMove move, SimulatedState state)
    {
        MoveUndoInfo undo = ApplyMove(move, state);
        bool givesCheck = IsInCheck(state.currentPlayer, state);
        UndoMove(undo, state);
        return givesCheck;
    }

    // --- Validate Grid Coordinates ---
    private bool IsValidGridPoint(Vector2Int point)
    {
        return point.x >= 0 && point.x < 8 && point.y >= 0 && point.y < 8;
    }

    // --- Apply Move ---
    private MoveUndoInfo ApplyMove(AIMove move, SimulatedState state)
    {
        if (move == null || move.piece == null)
        {
            Debug.LogError("ApplyMove: move or move.piece is null.");
            return default;
        }
        Piece pieceComponent = move.piece.GetComponent<Piece>();
        if (pieceComponent == null)
        {
            Debug.LogError("ApplyMove: Missing Piece component on " + move.piece.name);
            return default;
        }
        if (!IsValidGridPoint(move.startGridPoint) || !IsValidGridPoint(move.targetGridPoint))
        {
            Debug.LogError("ApplyMove: Invalid move coordinates: start " + move.startGridPoint + ", target " + move.targetGridPoint);
            return default;
        }
        MoveUndoInfo undo = new MoveUndoInfo();
        undo.start = move.startGridPoint;
        undo.end = move.targetGridPoint;
        undo.movedPiece = move.piece;
        undo.capturedPiece = state.pieces[move.targetGridPoint.x, move.targetGridPoint.y];
        undo.pawnWasMoved = state.movedPawns.Contains(move.piece);
        undo.hashBefore = state.hash;
        undo.currentPlayerBefore = state.currentPlayer;
        undo.otherPlayerBefore = state.otherPlayer;
        if (undo.capturedPiece != null)
        {
            if (state.currentPlayer.pieces.Contains(undo.capturedPiece))
            {
                undo.capturedPieceOwner = state.currentPlayer;
                state.currentPlayer.pieces.Remove(undo.capturedPiece);
            }
            else if (state.otherPlayer.pieces.Contains(undo.capturedPiece))
            {
                undo.capturedPieceOwner = state.otherPlayer;
                state.otherPlayer.pieces.Remove(undo.capturedPiece);
            }
            if (state.currentCaptured != null)
                state.currentCaptured.Add(undo.capturedPiece);
        }
        state.pieces[move.startGridPoint.x, move.startGridPoint.y] = null;
        state.pieces[move.targetGridPoint.x, move.targetGridPoint.y] = move.piece;

        if (pieceComponent.type == PieceType.Pawn && !undo.pawnWasMoved)
            state.movedPawns.Add(move.piece);

        // Swap players
        Player temp = state.currentPlayer;
        state.currentPlayer = state.otherPlayer;
        state.otherPlayer = temp;

        // *** Update state.hash after the move to reflect the new board state ***
        state.hash = ComputeHash(state);

        return undo;
    }

    // --- Undo Move ---
    private void UndoMove(MoveUndoInfo undo, SimulatedState state)
    {
        if (state == null)
        {
            Debug.LogError("UndoMove: state is null.");
            return;
        }
        state.hash = undo.hashBefore;
        state.pieces[undo.start.x, undo.start.y] = undo.movedPiece;
        state.pieces[undo.end.x, undo.end.y] = undo.capturedPiece;
        if (!undo.pawnWasMoved)
            state.movedPawns.Remove(undo.movedPiece);
        if (undo.capturedPiece != null)
        {
            if (undo.capturedPieceOwner != null && undo.capturedPieceOwner.pieces != null)
                undo.capturedPieceOwner.pieces.Add(undo.capturedPiece);
            if (state.currentCaptured != null)
                state.currentCaptured.Remove(undo.capturedPiece);
        }
        state.currentPlayer = undo.currentPlayerBefore;
        state.otherPlayer = undo.otherPlayerBefore;
    }

    private bool IsSameMove(AIMove a, AIMove b)
    {
        if (a == null || b == null)
            return false;
        return a.piece == b.piece &&
               a.startGridPoint.Equals(b.startGridPoint) &&
               a.targetGridPoint.Equals(b.targetGridPoint);
    }

    private bool IsInCheck(Player player, SimulatedState state)
    {
        GameObject king = null;
        foreach (GameObject piece in player.pieces)
        {
            if (piece != null && piece.GetComponent<Piece>() != null && piece.GetComponent<Piece>().type == PieceType.King)
            {
                king = piece;
                break;
            }
        }
        if (king == null)
            return true;
        Vector2Int kingPos = Evaluation.GetGridForPiece(king, state.pieces);
        Player opponent = (player == state.currentPlayer) ? state.otherPlayer : state.currentPlayer;
        foreach (GameObject piece in new List<GameObject>(opponent.pieces))
        {
            if (piece == null || piece.GetComponent<Piece>() == null)
                continue;
            Vector2Int piecePos = Evaluation.GetGridForPiece(piece, state.pieces);
            if (!IsValidGridPoint(piecePos))
                continue;
            List<Vector2Int> moves = piece.GetComponent<Piece>().MoveLocations(piecePos);
            if (moves.Contains(kingPos))
                return true;
        }
        return false;
    }

    // Modified repetition detection: only flag as repetition if the position has occurred 3 or more times.
    private bool IsMoveRepetition(AIMove move, SimulatedState state)
    {
        MoveUndoInfo undo = ApplyMove(move, state);
        int positionHash = state.hash;
        bool isRepetition = positionHistory.ContainsKey(positionHash) && positionHistory[positionHash] >= 3;
        UndoMove(undo, state);
        return isRepetition;
    }
}
