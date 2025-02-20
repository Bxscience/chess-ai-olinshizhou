using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public const int BoardSize = 8;
    public ChessPiece[,] board = new ChessPiece[BoardSize, BoardSize];
    public GameObject chessBoard;
    
    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        Instantiate(chessBoard, new Vector3(0,0,0), Quaternion.Euler(-90, 0, 0));

        // Initialize all squares as empty
        for (int x = 0; x < BoardSize; x++) {
            for (int y = 0; y < BoardSize; y++) {
                board[x, y] = new ChessPiece(PieceType.None, PieceColor.None);
            }
        }
    }
}