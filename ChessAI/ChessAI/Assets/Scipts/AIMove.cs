using System.Collections.Generic;
using UnityEngine;

public class AIMove
{
    public GameObject piece;
    public Vector2Int startGridPoint;
    public Vector2Int targetGridPoint;
    public GameObject capturedPiece;
    public int score;
}

// Converted to a struct to avoid extra GC pressure.
public struct MoveUndoInfo
{
    public Vector2Int start;
    public Vector2Int end;
    public GameObject movedPiece;
    public GameObject capturedPiece;
    public bool pawnWasMoved;
    public int hashBefore;
}

    public class SimulatedState
{
    public GameObject[,] pieces;
    public List<GameObject> movedPawns;
    public Player currentPlayer;
    public Player otherPlayer;
    public List<GameObject> currentCaptured;
    public List<GameObject> otherCaptured;
    public int hash;

     public Dictionary<int, int> positionHistory = new Dictionary<int, int>();

    public SimulatedState(GameObject[,] originalPieces, List<GameObject> originalMovedPawns,
                          Player current, Player other,
                          List<GameObject> currentCaptured, List<GameObject> otherCaptured)
    {
        // Deep copy the board array.
        pieces = new GameObject[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                pieces[i, j] = originalPieces[i, j];
            }
        }

        // Ensure lists are not null.
        movedPawns = originalMovedPawns != null ? new List<GameObject>(originalMovedPawns) : new List<GameObject>();
        this.currentCaptured = currentCaptured != null ? new List<GameObject>(currentCaptured) : new List<GameObject>();
        this.otherCaptured = otherCaptured != null ? new List<GameObject>(otherCaptured) : new List<GameObject>();

        // Create new Player instances so simulation changes don't affect the live game.
        // Assumes your Player class has a constructor that accepts a name and a boolean.
        currentPlayer = new Player(current.name, current.isHuman);
        currentPlayer.pieces = new List<GameObject>(current.pieces);
        currentPlayer.capturedPieces = new List<GameObject>(this.currentCaptured);

        otherPlayer = new Player(other.name, other.isHuman);
        otherPlayer.pieces = new List<GameObject>(other.pieces);
        otherPlayer.capturedPieces = new List<GameObject>(this.otherCaptured);

        // Initialize hash.
        hash = 0;
    }

    // Optionally, include a Clone method for creating a duplicate state.
    public SimulatedState Clone()
    {
        GameObject[,] clonedPieces = new GameObject[8, 8];
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                clonedPieces[i, j] = this.pieces[i, j];
            }
        }

        List<GameObject> clonedMovedPawns = new List<GameObject>(this.movedPawns);
        List<GameObject> clonedCurrentCaptured = new List<GameObject>(this.currentCaptured);
        List<GameObject> clonedOtherCaptured = new List<GameObject>(this.otherCaptured);

        // Assuming Player has a copy constructor.
        Player clonedCurrent = new Player(this.currentPlayer);
        Player clonedOther = new Player(this.otherPlayer);

        SimulatedState clone = new SimulatedState(clonedPieces, clonedMovedPawns, clonedCurrent, clonedOther,
                                                  clonedCurrentCaptured, clonedOtherCaptured);
        clone.hash = this.hash;
        return clone;
    }

    
}   

    


