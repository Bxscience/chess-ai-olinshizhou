using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public const int BoardSize = 8;
    public ChessPiece[,] board = new ChessPiece[BoardSize, BoardSize];
    
    void Start()
    {
        InitalizeBoard();
    }

    void InitializeBoard()
    {
        // Initialize all squares as empty
        for (int x = 0; x < BoardSize; x++) {
            for (int y = 0; y < BoardSize; y++) {
                board[x, y] = new ChessPiece(PieceType.None, PieceColor.None);
            }
        }
    }
}