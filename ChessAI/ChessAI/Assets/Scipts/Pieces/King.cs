using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{
    public override List<Vector2Int> MoveLocations(Vector2Int gridPoint)
    {
        List<Vector2Int> locations = new List<Vector2Int>();
        List<Vector2Int> directions = new List<Vector2Int>(BishopDirections);
        directions.AddRange(RookDirections);

        foreach (Vector2Int dir in directions)
        {
            Vector2Int nextGridPoint = new Vector2Int(gridPoint.x + dir.x, gridPoint.y + dir.y);
            locations.Add(nextGridPoint);
        }

        if (GameManager.instance.currentPlayer.kingSideCastlingRights && GameManager.instance.canKingSideCastle(gridPoint) == true)
        {
            Vector2Int kingSideCastleGridPoint = new Vector2Int(gridPoint.x + 2, gridPoint.y);
            locations.Add(kingSideCastleGridPoint);
        }

        if (GameManager.instance.currentPlayer.queenSideCastlingRights)
        {
            Vector2Int queenSideCastleGridPoint = new Vector2Int(gridPoint.x - 2, gridPoint.y);
            locations.Add(queenSideCastleGridPoint);
        }

        return locations;
    }
}
