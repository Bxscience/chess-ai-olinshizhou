using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardGenerator : MonoBehaviour
{
    public const int BoardSize = 8;
    public ChessPiece[,] board = new ChessPiece[BoardSize, BoardSize];
    public GameObject chessBoard;
    public GameObject whitePawnPrefab, whiteRookPrefab, whiteKnightPrefab, whiteBishopPrefab, whiteQueenPrefab, whiteKingPrefab;
    public GameObject blackPawnPrefab, blackRookPrefab, blackKnightPrefab, blackBishopPrefab, blackQueenPrefab, blackKingPrefab;
    
    void Start()
    {
        InitializeBoard();
        SpawnPieces();
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

    void SpawnPieces() {
        for (int x = 0; x < BoardSize; x++) {
            for (int y = 0; y < BoardSize; y++) {
                ChessPiece piece = board[x, y];
                if (piece.type != PieceType.None) {
                    GameObject piecePrefab = GetPrefabForPiece(piece);
                    if (piecePrefab != null) {
                        Instantiate(piecePrefab, new Vector3(x, 0.5f, y), Quaternion.Euler(-90, 0, 0));
                    }
                }
            }
        }
    }

    GameObject GetPrefabForPiece(ChessPiece piece) {
        if (piece.color == PieceColor.White) {
            switch (piece.type) {
                case PieceType.Pawn: return whitePawnPrefab;
                case PieceType.Rook: return whiteRookPrefab;
                case PieceType.Knight: return whiteKnightPrefab;
                case PieceType.Bishop: return whiteBishopPrefab;
                case PieceType.Queen: return whiteQueenPrefab;
                case PieceType.King: return whiteKingPrefab;
            }
        } else if (piece.color == PieceColor.Black) {
            switch (piece.type) {
                case PieceType.Pawn: return blackPawnPrefab;
                case PieceType.Rook: return blackRookPrefab;
                case PieceType.Knight: return blackKnightPrefab;
                case PieceType.Bishop: return blackBishopPrefab;
                case PieceType.Queen: return blackQueenPrefab;
                case PieceType.King: return blackKingPrefab;
            }
        }
        return null;
    }

}