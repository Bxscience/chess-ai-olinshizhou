using UnityEngine;

public class Board : MonoBehaviour
{
    public Material defaultMaterial;
    public Material selectedMaterial;

    public GameObject AddPiece(GameObject piece, int col, int row)
    {
        Vector2Int gridPoint = Geometry.GridPoint(col, row);
        GameObject newPiece = Instantiate(piece, Geometry.PointFromGrid(gridPoint), Quaternion.identity, gameObject.transform);
        return newPiece;
    }

    public void RemovePiece(GameObject piece)
    {
        Destroy(piece);
    }
    
    public void MovePiece(GameObject piece, Vector2Int gridPoint) // Physical board movement
    {
        Piece pieceComponent = piece.GetComponent<Piece>();
        piece.transform.position = Geometry.PointFromGrid(gridPoint); // Move passed paramater piece
        
        // Extra Condition to allow for castling rights :: GameManager.instance.PieceAtGrid(queenSideOldRookLocation).GetComponent<Piece>().type == PieceType.Rook
        // If a castle move is performed:
        Vector2Int kingSideOldRookLocation = new Vector2Int(gridPoint.x + 1, gridPoint.y);
        Vector2Int kingSideNewRookLocation = new Vector2Int(gridPoint.x - 1, gridPoint.y);
        Vector2Int queenSideOldRookLocation = new Vector2Int(gridPoint.x - 2, gridPoint.y);
        Vector2Int queenSideNewRookLocation = new Vector2Int(gridPoint.x + 1, gridPoint.y);
        
        // KingSide Castling
        if (piece.GetComponent<Piece>().type == PieceType.King && gridPoint.x == 6 && GameManager.instance.currentPlayer.kingSideCastlingRights)
        {
            KingSideCastle(kingSideOldRookLocation, kingSideNewRookLocation);
            GameManager.instance.removeCastlingRights();
        }

        // QueenSide Castling
        if (piece.GetComponent<Piece>().type == PieceType.King && gridPoint.x == 2 && GameManager.instance.currentPlayer.queenSideCastlingRights)
        {
            QueenSideCastle(queenSideOldRookLocation, queenSideNewRookLocation);
            GameManager.instance.removeCastlingRights();
        }
    }

    public void KingSideCastle(Vector2Int oldRookLocation, Vector2Int newRookLocation)
    {
        GameManager.instance.Move(GameManager.instance.PieceAtGrid(oldRookLocation), newRookLocation);
        Debug.Log("KingSideCastled");
    }

    public void QueenSideCastle(Vector2Int oldRookLocation, Vector2Int newRookLocation)
    {
        GameManager.instance.Move(GameManager.instance.PieceAtGrid(oldRookLocation), newRookLocation);
        Debug.Log("KingSideCastled");
    }

    public void SelectPiece(GameObject piece)
    {
        MeshRenderer renderers = piece.GetComponentInChildren<MeshRenderer>();
        renderers.material = selectedMaterial;
    }

    public void DeselectPiece(GameObject piece)
    {
        MeshRenderer renderers = piece.GetComponentInChildren<MeshRenderer>();
        renderers.material = defaultMaterial;
    }
}
