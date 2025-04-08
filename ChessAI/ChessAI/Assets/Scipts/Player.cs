using System.Collections.Generic;
using UnityEngine;

public class Player
{
    public List<GameObject> pieces;
    public List<GameObject> capturedPieces;

    public string name;
    public int forward;
    public bool kingSideCastlingRights, queenSideCastlingRights, hasCastled;
    public bool isHuman; 

    public Player(string name, bool positiveZMovement)
    {
        this.name = name;
        this.isHuman = isHuman;
        pieces = new List<GameObject>();
        capturedPieces = new List<GameObject>();
        kingSideCastlingRights = false;
        queenSideCastlingRights = false;
        hasCastled = false;

        if (positiveZMovement == true)
        {
            this.forward = 1;
        }
        else
        {
            this.forward = -1;
        }
    }


    public Player(Player other)
    {
        this.name = other.name;
        this.isHuman = other.isHuman;
        this.forward = other.forward;
        this.pieces = new List<GameObject>(other.pieces);
        this.capturedPieces = new List<GameObject>(other.capturedPieces);
    }
}
