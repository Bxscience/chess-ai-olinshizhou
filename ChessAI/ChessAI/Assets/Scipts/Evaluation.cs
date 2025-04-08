using System;
using UnityEngine;
using System.Collections.Generic;

public static class Evaluation
{
    public static readonly int[,] PawnTable = new int[8, 8] {
        {  0,   0,   0,   0,   0,   0,   0,   0 },
        { 50,  50,  50,  50,  50,  50,  50,  50 },
        { 10,  10,  20,  30,  30,  20,  10,  10 },
        {  5,   5,  10,  25,  25,  10,   5,   5 },
        {  0,   0,   0,  20,  20,   0,   0,   0 },
        {  5,  -5, -10,   0,   0, -10,  -5,   5 },
        {  5,  10,  10, -20, -20,  10,  10,   5 },
        {  0,   0,   0,   0,   0,   0,   0,   0 }
    };

    public static readonly int[,] KnightTable = new int[8, 8] {
        { -50, -40, -30, -30, -30, -30, -40, -50 },
        { -40, -20,   0,   0,   0,   0, -20, -40 },
        { -30,   0,  10,  15,  15,  10,   0, -30 },
        { -30,   5,  15,  20,  20,  15,   5, -30 },
        { -30,   0,  15,  20,  20,  15,   0, -30 },
        { -30,   5,  10,  15,  15,  10,   5, -30 },
        { -40, -20,   0,   5,   5,   0, -20, -40 },
        { -50, -40, -30, -30, -30, -30, -40, -50 }
    };

    public static readonly int[,] BishopTable = new int[8, 8] {
        { -20, -10, -10, -10, -10, -10, -10, -20 },
        { -10,   5,   0,   0,   0,   0,   5, -10 },
        { -10,  10,  10,  10,  10,  10,  10, -10 },
        { -10,   0,  10,  10,  10,  10,   0, -10 },
        { -10,   5,   5,  10,  10,   5,   5, -10 },
        { -10,   0,   5,  10,  10,   5,   0, -10 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -20, -10, -10, -10, -10, -10, -10, -20 }
    };

    public static readonly int[,] RookTable = new int[8, 8] {
        {  0,   0,   0,   0,   0,   0,   0,   0 },
        {  5,  10,  10,  10,  10,  10,  10,   5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        {  0,   0,   0,   5,   5,   0,   0,   0 }
    };

    public static readonly int[,] QueenTable = new int[8, 8] {
        { -20, -10, -10,  -5,  -5, -10, -10, -20 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -10,   0,   5,   5,   5,   5,   0, -10 },
        {  -5,   0,   5,   5,   5,   5,   0,  -5 },
        {   0,   0,   5,   5,   5,   5,   0,  -5 },
        { -10,   5,   5,   5,   5,   5,   0, -10 },
        { -10,   0,   5,   0,   0,   0,   0, -10 },
        { -20, -10, -10,  -5,  -5, -10, -10, -20 }
    };

    public static readonly int[,] KingTable = new int[8, 8] {
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -20, -30, -30, -40, -40, -30, -30, -20 },
        { -10, -20, -20, -20, -20, -20, -20, -10 },
        {  20,  20,   0,   0,   0,   0,  20,  20 },
        {  20,  30,  10,   0,   0,  10,  30,  20 }
    };

    // --- Main Evaluation Function ---
    public static int EvaluateBoard(SimulatedState state)
    {
        int mgMaterial = EvaluateMaterial(state);
        int mgPositional = EvaluatePositional(state);
        int mgMobility = EvaluateMobility(state);
        int mgKingSafety = EvaluateKingSafety(state);
        int mgPawnStructure = EvaluatePawnStructure(state);

        // Advanced evaluation factors
        int bishopPair = EvaluateBishopPair(state);
        int rookOpenFile = EvaluateRookOnOpenFile(state);
        int passedPawn = EvaluatePassedPawns(state);
        int kingExposure = EvaluateKingExposure(state);

        // Additional advanced factors
        int advancedPawnStructure = EvaluatePawnStructureAdvanced(state);
        int knightOutpost = EvaluateKnightOutpost(state);
        int rookBehindPassed = EvaluateRookBehindPassedPawn(state);
        int kingShield = EvaluateKingShield(state);
        int threats = EvaluateThreats(state);
        int spaceControl = EvaluateSpaceControl(state);
        int coordination = EvaluatePieceCoordination(state);

        // --- Dynamic Game Phase Factor (Tapered Evaluation) ---
        int totalMaterial = 0;
        foreach (GameObject piece in state.currentPlayer.pieces)
            if (piece != null)
                totalMaterial += GetPieceValue(piece);
        foreach (GameObject piece in state.otherPlayer.pieces)
            if (piece != null)
                totalMaterial += GetPieceValue(piece);
        double phase = Math.Clamp((double)totalMaterial / 4000.0, 0.0, 1.0);

        // Combine evaluation terms; note that some terms are scaled by the phase factor.
        int evaluation = mgMaterial 
                       + mgPositional 
                       + (int)(mgMobility * 10 * phase)
                       + (int)(mgKingSafety * 5 * phase)
                       + mgPawnStructure
                       + bishopPair
                       + rookOpenFile
                       + passedPawn
                       + kingExposure
                       + advancedPawnStructure
                       + knightOutpost
                       + rookBehindPassed
                       + kingShield
                       + threats
                       + spaceControl
                       + coordination;
        return evaluation;
    }

    // --- Original Evaluation Functions ---
    private static int EvaluateMaterial(SimulatedState state)
    {
        int score = 0;
        foreach (GameObject piece in state.currentPlayer.pieces)
            if (piece != null)
                score += GetPieceValue(piece);
        foreach (GameObject piece in state.otherPlayer.pieces)
            if (piece != null)
                score -= GetPieceValue(piece);
        return score;
    }

    public static int GetPieceValue(GameObject piece)
    {
        PieceType type = piece.GetComponent<Piece>().type;
        switch (type)
        {
            case PieceType.Pawn:   return 100;
            case PieceType.Knight: return 320;
            case PieceType.Bishop: return 330;
            case PieceType.Rook:   return 500;
            case PieceType.Queen:  return 900;
            case PieceType.King:   return 2000;
            default: return 0;
        }
    }

    private static int EvaluatePositional(SimulatedState state)
    {
        int score = 0;
        foreach (GameObject piece in state.currentPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            score += GetPieceSquareValue(piece.GetComponent<Piece>().type, pos);
        }
        foreach (GameObject piece in state.otherPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            score -= GetPieceSquareValue(piece.GetComponent<Piece>().type, pos);
        }
        return score;
    }

    public static int GetPieceSquareValue(PieceType type, Vector2Int pos)
    {
        if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7)
            return 0;
        switch (type)
        {
            case PieceType.Pawn:   return PawnTable[pos.y, pos.x];
            case PieceType.Knight: return KnightTable[pos.y, pos.x];
            case PieceType.Bishop: return BishopTable[pos.y, pos.x];
            case PieceType.Rook:   return RookTable[pos.y, pos.x];
            case PieceType.Queen:  return QueenTable[pos.y, pos.x];
            case PieceType.King:   return KingTable[pos.y, pos.x];
            default: return 0;
        }
    }

    private static int EvaluateMobility(SimulatedState state)
    {
        int mobilityCurrent = 0;
        foreach (GameObject piece in state.currentPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            mobilityCurrent += piece.GetComponent<Piece>().MoveLocations(pos).Count;
        }
        int mobilityOpponent = 0;
        foreach (GameObject piece in state.otherPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            mobilityOpponent += piece.GetComponent<Piece>().MoveLocations(pos).Count;
        }
        return mobilityCurrent - mobilityOpponent;
    }

    private static int EvaluateKingSafety(SimulatedState state)
    {
        int safety = 0;
        GameObject king = state.currentPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
        if (king != null)
        {
            Vector2Int kingPos = GetGridForPiece(king, state.pieces);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int pos = new Vector2Int(kingPos.x + dx, kingPos.y + dy);
                    if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7) continue;
                    GameObject neighbor = state.pieces[pos.x, pos.y];
                    if (neighbor != null && state.currentPlayer.pieces.Contains(neighbor))
                        safety += 20;
                }
        }
        king = state.otherPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
        if (king != null)
        {
            Vector2Int kingPos = GetGridForPiece(king, state.pieces);
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    Vector2Int pos = new Vector2Int(kingPos.x + dx, kingPos.y + dy);
                    if (pos.x < 0 || pos.x > 7 || pos.y < 0 || pos.y > 7) continue;
                    GameObject neighbor = state.pieces[pos.x, pos.y];
                    if (neighbor != null && state.otherPlayer.pieces.Contains(neighbor))
                        safety -= 20;
                }
        }
        return safety;
    }

    private static int EvaluatePawnStructure(SimulatedState state)
    {
        int score = 0;
        List<Vector2Int> pawns = new List<Vector2Int>();
        foreach (GameObject p in state.currentPlayer.pieces)
            if (p != null && p.GetComponent<Piece>().type == PieceType.Pawn)
                pawns.Add(GetGridForPiece(p, state.pieces));
        foreach (Vector2Int pawn in pawns)
        {
            bool hasAdjacent = pawns.Exists(p => Mathf.Abs(p.x - pawn.x) == 1);
            if (!hasAdjacent) score -= 15;
            int sameFile = pawns.FindAll(p => p.x == pawn.x).Count;
            if (sameFile > 1) score -= 10 * (sameFile - 1);
        }
        return score;
    }

    // --- Advanced Evaluation Factors ---

    // 1. Advanced Pawn Structure: penalties for isolated, doubled, and backward pawns
    private static int EvaluatePawnStructureAdvanced(SimulatedState state)
    {
        int score = 0;
        List<Vector2Int> whitePawns = new List<Vector2Int>();
        List<Vector2Int> blackPawns = new List<Vector2Int>();
        foreach (GameObject p in state.currentPlayer.pieces)
            if (p != null && p.GetComponent<Piece>().type == PieceType.Pawn)
                whitePawns.Add(GetGridForPiece(p, state.pieces));
        foreach (GameObject p in state.otherPlayer.pieces)
            if (p != null && p.GetComponent<Piece>().type == PieceType.Pawn)
                blackPawns.Add(GetGridForPiece(p, state.pieces));

        // For white pawns: isolated and backward pawn penalties
        foreach (Vector2Int pawn in whitePawns)
        {
            bool hasFriendlyOnAdjFiles = false;
            for (int dx = -1; dx <= 1; dx += 2)
            {
                int file = pawn.x + dx;
                if (file < 0 || file > 7) continue;
                if (whitePawns.Exists(p => p.x == file))
                {
                    hasFriendlyOnAdjFiles = true;
                    break;
                }
            }
            if (!hasFriendlyOnAdjFiles) score -= 20; // isolated pawn
            int sameFileCount = whitePawns.FindAll(p => p.x == pawn.x).Count;
            if (sameFileCount > 1) score -= 15 * (sameFileCount - 1); // doubled pawns
        }
        // For black pawns: similar logic (with opposite sign)
        foreach (Vector2Int pawn in blackPawns)
        {
            bool hasFriendlyOnAdjFiles = false;
            for (int dx = -1; dx <= 1; dx += 2)
            {
                int file = pawn.x + dx;
                if (file < 0 || file > 7) continue;
                if (blackPawns.Exists(p => p.x == file))
                {
                    hasFriendlyOnAdjFiles = true;
                    break;
                }
            }
            if (!hasFriendlyOnAdjFiles) score += 20;
            int sameFileCount = blackPawns.FindAll(p => p.x == pawn.x).Count;
            if (sameFileCount > 1) score += 15 * (sameFileCount - 1);
        }
        return score;
    }

    // 2. Knight Outpost: bonus for knights in central advanced squares not easily attacked by enemy pawns.
    private static int EvaluateKnightOutpost(SimulatedState state)
    {
        int score = 0;
        // For white knights
        foreach (GameObject piece in state.currentPlayer.pieces)
        {
            if (piece != null && piece.GetComponent<Piece>().type == PieceType.Knight)
            {
                Vector2Int pos = GetGridForPiece(piece, state.pieces);
                if (pos.y >= 4 && pos.x >= 2 && pos.x <= 5) // central advanced zone
                {
                    bool enemyPawnCanAttack = false;
                    foreach (GameObject enemy in state.otherPlayer.pieces)
                    {
                        if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                        {
                            Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                            // Check if enemy pawn can move diagonally forward (for black, downwards)
                            if (enemyPos.y - 1 == pos.y && Mathf.Abs(enemyPos.x - pos.x) == 1)
                            {
                                enemyPawnCanAttack = true;
                                break;
                            }
                        }
                    }
                    if (!enemyPawnCanAttack) score += 25;
                }
            }
        }
        // For black knights (mirror bonus)
        foreach (GameObject piece in state.otherPlayer.pieces)
        {
            if (piece != null && piece.GetComponent<Piece>().type == PieceType.Knight)
            {
                Vector2Int pos = GetGridForPiece(piece, state.pieces);
                if (pos.y <= 3 && pos.x >= 2 && pos.x <= 5)
                {
                    bool enemyPawnCanAttack = false;
                    foreach (GameObject enemy in state.currentPlayer.pieces)
                    {
                        if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                        {
                            Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                            if (enemyPos.y + 1 == pos.y && Mathf.Abs(enemyPos.x - pos.x) == 1)
                            {
                                enemyPawnCanAttack = true;
                                break;
                            }
                        }
                    }
                    if (!enemyPawnCanAttack) score -= 25;
                }
            }
        }
        return score;
    }

    // 3. Rook Behind Passed Pawn: bonus if a rook is positioned behind an advanced passed pawn.
    private static int EvaluateRookBehindPassedPawn(SimulatedState state)
    {
        int score = 0;
        // Evaluate for white: rook on same file and behind a passed pawn
        foreach (GameObject piece in state.currentPlayer.pieces)
        {
            if (piece != null && piece.GetComponent<Piece>().type == PieceType.Rook)
            {
                Vector2Int rookPos = GetGridForPiece(piece, state.pieces);
                foreach (GameObject pawn in state.currentPlayer.pieces)
                {
                    if (pawn != null && pawn.GetComponent<Piece>().type == PieceType.Pawn)
                    {
                        Vector2Int pawnPos = GetGridForPiece(pawn, state.pieces);
                        if (pawnPos.x == rookPos.x && pawnPos.y > rookPos.y) // rook is behind pawn
                        {
                            // Check that the pawn is passed (simplistic: no enemy pawn ahead in same or adjacent file)
                            bool passed = true;
                            for (int i = pawnPos.x - 1; i <= pawnPos.x + 1; i++)
                            {
                                if (i < 0 || i > 7) continue;
                                foreach (GameObject enemy in state.otherPlayer.pieces)
                                {
                                    if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                                    {
                                        Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                                        if (enemyPos.x == i && enemyPos.y > pawnPos.y)
                                        {
                                            passed = false;
                                            break;
                                        }
                                    }
                                }
                                if (!passed) break;
                            }
                            if (passed) score += 20;
                        }
                    }
                }
            }
        }
        // Mirror for black rooks
        foreach (GameObject piece in state.otherPlayer.pieces)
        {
            if (piece != null && piece.GetComponent<Piece>().type == PieceType.Rook)
            {
                Vector2Int rookPos = GetGridForPiece(piece, state.pieces);
                foreach (GameObject pawn in state.otherPlayer.pieces)
                {
                    if (pawn != null && pawn.GetComponent<Piece>().type == PieceType.Pawn)
                    {
                        Vector2Int pawnPos = GetGridForPiece(pawn, state.pieces);
                        if (pawnPos.x == rookPos.x && pawnPos.y < rookPos.y)
                        {
                            bool passed = true;
                            for (int i = pawnPos.x - 1; i <= pawnPos.x + 1; i++)
                            {
                                if (i < 0 || i > 7) continue;
                                foreach (GameObject enemy in state.currentPlayer.pieces)
                                {
                                    if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                                    {
                                        Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                                        if (enemyPos.x == i && enemyPos.y < pawnPos.y)
                                        {
                                            passed = false;
                                            break;
                                        }
                                    }
                                }
                                if (!passed) break;
                            }
                            if (passed) score -= 20;
                        }
                    }
                }
            }
        }
        return score;
    }

    // 4. King Shield: evaluate the pawn shield in front of the king.
    private static int EvaluateKingShield(SimulatedState state)
    {
        int score = 0;
        // For white king, check the three squares in front of it.
        GameObject whiteKing = state.currentPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
        if (whiteKing != null)
        {
            Vector2Int kingPos = GetGridForPiece(whiteKing, state.pieces);
            for (int dx = -1; dx <= 1; dx++)
            {
                Vector2Int shieldPos = new Vector2Int(kingPos.x + dx, kingPos.y + 1);
                if (shieldPos.x < 0 || shieldPos.x > 7 || shieldPos.y < 0 || shieldPos.y > 7) continue;
                GameObject pawn = state.pieces[shieldPos.x, shieldPos.y];
                if (pawn != null && pawn.GetComponent<Piece>().type == PieceType.Pawn)
                    score += 15;
                else
                    score -= 10;
            }
        }
        // For black king (mirror)
        GameObject blackKing = state.otherPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
        if (blackKing != null)
        {
            Vector2Int kingPos = GetGridForPiece(blackKing, state.pieces);
            for (int dx = -1; dx <= 1; dx++)
            {
                Vector2Int shieldPos = new Vector2Int(kingPos.x + dx, kingPos.y - 1);
                if (shieldPos.x < 0 || shieldPos.x > 7 || shieldPos.y < 0 || shieldPos.y > 7) continue;
                GameObject pawn = state.pieces[shieldPos.x, shieldPos.y];
                if (pawn != null && pawn.GetComponent<Piece>().type == PieceType.Pawn)
                    score -= 15;
                else
                    score += 10;
            }
        }
        return score;
    }

    // 5. Threats: evaluate if enemy pieces are attacked by multiple friendly pieces.
    private static int EvaluateThreats(SimulatedState state)
    {
        int score = 0;
        // For each enemy piece, if it is attacked by more than one friendly piece, give bonus to attacker.
        foreach (GameObject enemy in state.otherPlayer.pieces)
        {
            if (enemy == null) continue;
            Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
            int attackers = 0;
            foreach (GameObject friend in state.currentPlayer.pieces)
            {
                if (friend == null) continue;
                List<Vector2Int> moves = friend.GetComponent<Piece>().MoveLocations(GetGridForPiece(friend, state.pieces));
                if (moves.Contains(enemyPos))
                    attackers++;
            }
            if (attackers >= 2) score += 10 * (attackers - 1);
        }
        // Mirror for enemy threats on your own pieces.
        foreach (GameObject friend in state.currentPlayer.pieces)
        {
            if (friend == null) continue;
            Vector2Int friendPos = GetGridForPiece(friend, state.pieces);
            int enemyAttackers = 0;
            foreach (GameObject enemy in state.otherPlayer.pieces)
            {
                if (enemy == null) continue;
                List<Vector2Int> moves = enemy.GetComponent<Piece>().MoveLocations(GetGridForPiece(enemy, state.pieces));
                if (moves.Contains(friendPos))
                    enemyAttackers++;
            }
            if (enemyAttackers >= 2) score -= 10 * (enemyAttackers - 1);
        }
        return score;
    }

    // 6. Space Control: bonus for control of central squares.
    private static int EvaluateSpaceControl(SimulatedState state)
    {
        int score = 0;
        // Central squares: d4, e4, d5, e5 (and maybe extend to surrounding squares)
        Vector2Int[] central = new Vector2Int[]
        {
            new Vector2Int(3,3), new Vector2Int(4,3),
            new Vector2Int(3,4), new Vector2Int(4,4)
        };
        foreach (Vector2Int square in central)
        {
            if (state.pieces[square.x, square.y] == null)
            {
                // Count attackers for both sides
                int whiteAttackers = 0, blackAttackers = 0;
                foreach (GameObject piece in state.currentPlayer.pieces)
                {
                    if (piece != null && piece.GetComponent<Piece>().MoveLocations(GetGridForPiece(piece, state.pieces)).Contains(square))
                        whiteAttackers++;
                }
                foreach (GameObject piece in state.otherPlayer.pieces)
                {
                    if (piece != null && piece.GetComponent<Piece>().MoveLocations(GetGridForPiece(piece, state.pieces)).Contains(square))
                        blackAttackers++;
                }
                score += 5 * (whiteAttackers - blackAttackers);
            }
        }
        return score;
    }

    // 7. Piece Coordination: bonus if friendly pieces are supporting each other.
    private static int EvaluatePieceCoordination(SimulatedState state)
    {
        int score = 0;
        // For every piece, check if adjacent friendly pieces are present
        foreach (GameObject piece in state.currentPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            int supportCount = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int neighbor = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (neighbor.x < 0 || neighbor.x > 7 || neighbor.y < 0 || neighbor.y > 7) continue;
                    if (state.pieces[neighbor.x, neighbor.y] != null &&
                        state.currentPlayer.pieces.Contains(state.pieces[neighbor.x, neighbor.y]))
                        supportCount++;
                }
            score += supportCount * 3;
        }
        // Mirror for enemy coordination
        foreach (GameObject piece in state.otherPlayer.pieces)
        {
            if (piece == null) continue;
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            int supportCount = 0;
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    Vector2Int neighbor = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (neighbor.x < 0 || neighbor.x > 7 || neighbor.y < 0 || neighbor.y > 7) continue;
                    if (state.pieces[neighbor.x, neighbor.y] != null &&
                        state.otherPlayer.pieces.Contains(state.pieces[neighbor.x, neighbor.y]))
                        supportCount++;
                }
            score -= supportCount * 3;
        }
        return score;
    }

    // --- Helper: Find grid position of a piece on the board array ---
    public static Vector2Int GetGridForPiece(GameObject piece, GameObject[,] board)
    {
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 8; j++)
                if (board[i, j] == piece)
                    return new Vector2Int(i, j);
        return new Vector2Int(-1, -1);
    }

    private static int EvaluateBishopPair(SimulatedState state)
{
    int score = 0;
    int bishopCountCurrent = 0, bishopCountOther = 0;
    foreach (GameObject piece in state.currentPlayer.pieces)
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Bishop)
            bishopCountCurrent++;
    foreach (GameObject piece in state.otherPlayer.pieces)
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Bishop)
            bishopCountOther++;
    if (bishopCountCurrent >= 2) score += 50;
    if (bishopCountOther >= 2) score -= 50;
    return score;
}

private static int EvaluateRookOnOpenFile(SimulatedState state)
{
    int score = 0;
    // Evaluate for white rooks
    foreach (GameObject piece in state.currentPlayer.pieces)
    {
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Rook)
        {
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            bool friendlyPawnOnFile = false;
            foreach (GameObject p in state.currentPlayer.pieces)
            {
                if (p != null && p.GetComponent<Piece>().type == PieceType.Pawn)
                {
                    Vector2Int pawnPos = GetGridForPiece(p, state.pieces);
                    if (pawnPos.x == pos.x)
                    {
                        friendlyPawnOnFile = true;
                        break;
                    }
                }
            }
            if (!friendlyPawnOnFile) score += 20;
        }
    }
    // Evaluate for black rooks (mirror)
    foreach (GameObject piece in state.otherPlayer.pieces)
    {
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Rook)
        {
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            bool friendlyPawnOnFile = false;
            foreach (GameObject p in state.otherPlayer.pieces)
            {
                if (p != null && p.GetComponent<Piece>().type == PieceType.Pawn)
                {
                    Vector2Int pawnPos = GetGridForPiece(p, state.pieces);
                    if (pawnPos.x == pos.x)
                    {
                        friendlyPawnOnFile = true;
                        break;
                    }
                }
            }
            if (!friendlyPawnOnFile) score -= 20;
        }
    }
    return score;
}

private static int EvaluatePassedPawns(SimulatedState state)
{
    int score = 0;
    // Evaluate white passed pawns
    foreach (GameObject piece in state.currentPlayer.pieces)
    {
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Pawn)
        {
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            bool isPassed = true;
            for (int i = pos.x - 1; i <= pos.x + 1; i++)
            {
                if (i < 0 || i > 7) continue;
                foreach (GameObject enemy in state.otherPlayer.pieces)
                {
                    if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                    {
                        Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                        if (enemyPos.x == i && enemyPos.y > pos.y)
                        {
                            isPassed = false;
                            break;
                        }
                    }
                }
                if (!isPassed) break;
            }
            if (isPassed) score += 30;
        }
    }
    // Evaluate black passed pawns
    foreach (GameObject piece in state.otherPlayer.pieces)
    {
        if (piece != null && piece.GetComponent<Piece>().type == PieceType.Pawn)
        {
            Vector2Int pos = GetGridForPiece(piece, state.pieces);
            bool isPassed = true;
            for (int i = pos.x - 1; i <= pos.x + 1; i++)
            {
                if (i < 0 || i > 7) continue;
                foreach (GameObject enemy in state.currentPlayer.pieces)
                {
                    if (enemy != null && enemy.GetComponent<Piece>().type == PieceType.Pawn)
                    {
                        Vector2Int enemyPos = GetGridForPiece(enemy, state.pieces);
                        if (enemyPos.x == i && enemyPos.y < pos.y)
                        {
                            isPassed = false;
                            break;
                        }
                    }
                }
                if (!isPassed) break;
            }
            if (isPassed) score -= 30;
        }
    }
    return score;
}

private static int EvaluateKingExposure(SimulatedState state)
{
    int score = 0;
    // White king exposure
    GameObject whiteKing = state.currentPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
    if (whiteKing != null)
    {
        Vector2Int pos = GetGridForPiece(whiteKing, state.pieces);
        int enemyCount = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int checkPos = new Vector2Int(pos.x + dx, pos.y + dy);
                if (checkPos.x < 0 || checkPos.x > 7 || checkPos.y < 0 || checkPos.y > 7) continue;
                GameObject enemy = state.pieces[checkPos.x, checkPos.y];
                if (enemy != null && state.otherPlayer.pieces.Contains(enemy))
                    enemyCount++;
            }
        score -= enemyCount * 10;
    }
    // Black king exposure
    GameObject blackKing = state.otherPlayer.pieces.Find(p => p != null && p.GetComponent<Piece>().type == PieceType.King);
    if (blackKing != null)
    {
        Vector2Int pos = GetGridForPiece(blackKing, state.pieces);
        int enemyCount = 0;
        for (int dx = -1; dx <= 1; dx++)
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector2Int checkPos = new Vector2Int(pos.x + dx, pos.y + dy);
                if (checkPos.x < 0 || checkPos.x > 7 || checkPos.y < 0 || checkPos.y > 7) continue;
                GameObject enemy = state.pieces[checkPos.x, checkPos.y];
                if (enemy != null && state.currentPlayer.pieces.Contains(enemy))
                    enemyCount++;
            }
        score += enemyCount * 10;
    }
    return score;
}
}
