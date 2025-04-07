using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public Board board;

    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;

    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;
    
    private GameObject[,] pieces;
    private List<GameObject> movedPieces;

    private Player white;
    private Player black;
    public Player currentPlayer;
    public Player otherPlayer;

    void Awake()
    {
        instance = this;
    }

    void Start ()
    {
        pieces = new GameObject[8, 8];
        movedPieces = new List<GameObject>();

        white = new Player("white", true);
        black = new Player("black", false);

        currentPlayer = white;
        otherPlayer = black;

        InitialSetup();
        Debug.Log(currentPlayer.name + "'s turn");

    }

    private void InitialSetup()
    {
        AddPiece(whiteRook, white, 0, 0);
        AddPiece(whiteKnight, white, 1, 0);
        AddPiece(whiteBishop, white, 2, 0);
        AddPiece(whiteQueen, white, 3, 0);
        AddPiece(whiteKing, white, 4, 0);
        AddPiece(whiteBishop, white, 5, 0);
        AddPiece(whiteKnight, white, 6, 0);
        AddPiece(whiteRook, white, 7, 0);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(whitePawn, white, i, 1);
        }

        AddPiece(blackRook, black, 0, 7);
        AddPiece(blackKnight, black, 1, 7);
        AddPiece(blackBishop, black, 2, 7);
        AddPiece(blackQueen, black, 3, 7);
        AddPiece(blackKing, black, 4, 7);
        AddPiece(blackBishop, black, 5, 7);
        AddPiece(blackKnight, black, 6, 7);
        AddPiece(blackRook, black, 7, 7);

        for (int i = 0; i < 8; i++)
        {
            AddPiece(blackPawn, black, i, 6);
        }
    }

    public void AddPiece(GameObject prefab, Player player, int col, int row)
    {
        GameObject pieceObject = board.AddPiece(prefab, col, row);
        player.pieces.Add(pieceObject);
        pieces[col, row] = pieceObject;
    }

    public void SelectPieceAtGrid(Vector2Int gridPoint)
    {
        GameObject selectedPiece = pieces[gridPoint.x, gridPoint.y];
        if (selectedPiece)
        {
            board.SelectPiece(selectedPiece);
        }
    }


    public List<Vector2Int> MovesForPiece(GameObject pieceObject)
    {
        Piece piece = pieceObject.GetComponent<Piece>();
        Vector2Int gridPoint = GridForPiece(pieceObject);
        List<Vector2Int> locations = piece.MoveLocations(gridPoint);

        // filter out offboard locations
        locations.RemoveAll(gp => gp.x < 0 || gp.x > 7 || gp.y < 0 || gp.y > 7);

        // filter out locations with friendly piece
        locations.RemoveAll(gp => FriendlyPieceAt(gp));

        return locations;
    }

    // Making moves in 2D array of pieces
    public void Move(GameObject piece, Vector2Int gridPoint)
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        Vector2Int startGridPoint = GridForPiece(piece); // Initial position
        pieces[startGridPoint.x, startGridPoint.y] = null;
        pieces[gridPoint.x, gridPoint.y] = piece; // Final position
        board.MovePiece(piece, gridPoint); // Physical board update

        if (pieceComponent.type == PieceType.Pawn && !HasPawnMoved(piece))
        {
            movedPieces.Add(piece);
        }

        if (pieceComponent.type == PieceType.King && !HasKingMoved(piece))
        {
            movedPieces.Add(piece);
            removeCastlingRights();
        }

        if ((startGridPoint.y == 0 || startGridPoint.y == 7) && canKingSideCastle(startGridPoint) && !currentPlayer.hasCastled)
        {
            giveKingSideCastlingRights();
        }
        
        if ((startGridPoint.y == 0 || startGridPoint.y == 7) && canQueenSideCastle(startGridPoint) && !currentPlayer.hasCastled)
        {
            giveQueenSideCastlingRights();
        }

    }

    public bool HasPawnMoved(GameObject pawn)
    {
        return movedPieces.Contains(pawn);
    }

    public bool HasKingMoved(GameObject king)
    {
        return movedPieces.Contains(king);
    }

    public bool canKingSideCastle(Vector2Int gridPoint)
    {
        if (currentPlayer.hasCastled)
            return false;
        
        Vector2Int kingSideOldRookLocation;
        if(currentPlayer.name == "white")
        {
            kingSideOldRookLocation = GridForPiece(pieces[7,0]);
        }
        else
        {
            kingSideOldRookLocation = GridForPiece(pieces[7,7]);
        }
        Vector2Int bishopLocation = new Vector2Int(5,gridPoint.y);
        Vector2Int knightLocation = new Vector2Int(6,gridPoint.y);
        return (PieceAtGrid(bishopLocation) == false && PieceAtGrid(knightLocation) == false) && PieceAtGrid(kingSideOldRookLocation).GetComponent<Piece>().type == PieceType.Rook;
    }

    public bool canQueenSideCastle(Vector2Int gridPoint)
    {
        if (currentPlayer.hasCastled)
            return false;
        
        Vector2Int queenSideOldRookLocation;
        if(currentPlayer.name == "white")
        {
            queenSideOldRookLocation = GridForPiece(pieces[0,0]);
        }
        else
        {
            queenSideOldRookLocation = GridForPiece(pieces[0,7]);
        }
        Vector2Int queenLocation = new Vector2Int(3,gridPoint.y);
        Vector2Int bishopLocation = new Vector2Int(2,gridPoint.y);
        Vector2Int knightLocation = new Vector2Int(1,gridPoint.y);
        return (PieceAtGrid(bishopLocation) == false && PieceAtGrid(knightLocation) == false && PieceAtGrid(queenLocation) == false) && PieceAtGrid(queenSideOldRookLocation).GetComponent<Piece>().type == PieceType.Rook;
    }

    // Giving castling rights to player
    public void giveKingSideCastlingRights()
    {
        Debug.Log(currentPlayer.name + " has king side castle available");
        currentPlayer.kingSideCastlingRights = true;
    }

    // Giving castling rights to player
    public void giveQueenSideCastlingRights()
    {
        Debug.Log(currentPlayer.name + " has queen side castle available");
        currentPlayer.queenSideCastlingRights = true;
    }

    // Removes castling rights from player
    public void removeCastlingRights()
    {
        currentPlayer.kingSideCastlingRights = false;
        currentPlayer.queenSideCastlingRights = false;
        currentPlayer.hasCastled = true;
        Debug.Log(currentPlayer.name + " lost castling rights");
    }


    public void CapturePieceAt(Vector2Int gridPoint)
    {
        GameObject pieceToCapture = PieceAtGrid(gridPoint);
        if (pieceToCapture.GetComponent<Piece>().type == PieceType.King)
        {
            Debug.Log(currentPlayer.name + " wins!");
            Destroy(board.GetComponent<TileSelector>());
            Destroy(board.GetComponent<MoveSelector>());
        }
        currentPlayer.capturedPieces.Add(pieceToCapture);
        pieces[gridPoint.x, gridPoint.y] = null;
        Destroy(pieceToCapture);
    }

    public void SelectPiece(GameObject piece)
    {
        board.SelectPiece(piece);
    }

    public void DeselectPiece(GameObject piece)
    {
        board.DeselectPiece(piece);
    }

    public bool DoesPieceBelongToCurrentPlayer(GameObject piece)
    {
        return currentPlayer.pieces.Contains(piece);
    }

    public GameObject PieceAtGrid(Vector2Int gridPoint)
    {
        if (gridPoint.x > 7 || gridPoint.y > 7 || gridPoint.x < 0 || gridPoint.y < 0)
        {
            return null;
        }
        return pieces[gridPoint.x, gridPoint.y];

    }

    public Vector2Int GridForPiece(GameObject piece)
    {
        for (int i = 0; i < 8; i++) 
        {
            for (int j = 0; j < 8; j++)
            {
                if (pieces[i, j] == piece)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        return new Vector2Int(-1, -1);
    }

    public bool FriendlyPieceAt(Vector2Int gridPoint)
    {
        GameObject piece = PieceAtGrid(gridPoint);

        if (piece == null) {
            return false;
        }

        if (otherPlayer.pieces.Contains(piece))
        {
            return false;
        }

        return true;
    }

    public void NextPlayer()
    {
        Player tempPlayer = currentPlayer;
        currentPlayer = otherPlayer;
        otherPlayer = tempPlayer;
        Debug.Log(currentPlayer.name + "'s turn");
    }
}
