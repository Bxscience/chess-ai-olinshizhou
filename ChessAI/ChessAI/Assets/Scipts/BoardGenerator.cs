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
        // Builds physical chess board
        Instantiate(chessBoard, new Vector3(0,0,0), Quaternion.Euler(-90, 0, 0));

        // Initialize all squares as empty
        for (int x = 0; x < BoardSize; x++) {
            for (int y = 0; y < BoardSize; y++) {
                board[x, y] = new ChessPiece(PieceType.None, PieceColor.None);
            }
        }
        
        // Place white pieces (bottom of the board)
        board[0, 0] = new ChessPiece(PieceType.Rook, PieceColor.White);
        board[0, 1] = new ChessPiece(PieceType.Knight, PieceColor.White);
        board[0, 2] = new ChessPiece(PieceType.Bishop, PieceColor.White);
        board[0, 3] = new ChessPiece(PieceType.Queen, PieceColor.White);
        board[0, 4] = new ChessPiece(PieceType.King, PieceColor.White);
        board[0, 5] = new ChessPiece(PieceType.Bishop, PieceColor.White);
        board[0, 6] = new ChessPiece(PieceType.Knight, PieceColor.White);
        board[0, 7] = new ChessPiece(PieceType.Rook, PieceColor.White);
        
        for (int y = 0; y < BoardSize; y++) {
            board[1, y] = new ChessPiece(PieceType.Pawn, PieceColor.White);
        }
        
        // Place black pieces (top of the board)
            board[7, 0] = new ChessPiece(PieceType.Rook, 
    PieceColor.Black);
            board[7, 1] = new ChessPiece(PieceType.Knight, 
    PieceColor.Black);
            board[7, 2] = new ChessPiece(PieceType.Bishop, 
    PieceColor.Black);
            board[7, 3] = new ChessPiece(PieceType.Queen, 
    PieceColor.Black);
            board[7, 4] = new ChessPiece(PieceType.King, 
    PieceColor.Black);
            board[7, 5] = new ChessPiece(PieceType.Bishop, 
    PieceColor.Black);
            board[7, 6] = new ChessPiece(PieceType.Knight, 
    PieceColor.Black);
            board[7, 7] = new ChessPiece(PieceType.Rook, 
    PieceColor.Black);

        for (int y = 0; y < BoardSize; y++) {
            board[6, y] = new ChessPiece(PieceType.Pawn, 
PieceColor.Black);
        }
    }
}