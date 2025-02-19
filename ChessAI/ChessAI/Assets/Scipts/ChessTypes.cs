using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PieceType {
    None, Pawn, Knight, Bishop, Rook, Queen, King
}

public enum PieceColor { 
    None, White, Black
    }

public struct ChessPiece {
    public PieceType type;
    public PieceColor color;

    public ChessPiece(PieceType type, PieceColor color) {
        this.type = type;
        this.color = color;
    }
}
