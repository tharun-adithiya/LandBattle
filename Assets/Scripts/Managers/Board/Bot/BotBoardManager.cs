using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotBoardManager : MonoBehaviour
{
    [SerializeField] private Tilemap botTilemap;                    
    [SerializeField] private BoxCollider2D boardBounds;
    [SerializeField] private GameObject highlightTilePrefab;
                                        

    private HashSet<Vector3Int> occupiedCells = new();                      //Tracks cells occupied by the ships int the board

    private List<ShipData> ships = new();                                   //Tracks the ships marked in the cells

    public Vector3Int GetStartCell(Vector3 worldPosition, int shipSize, bool isHorizontal)          //Decides the start cell based on the size of the ship. This will provide an offset to handle faulty placements
    {
        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        return botTilemap.WorldToCell(worldPosition - offsetVector);                                        
    }

    public bool CanPlaceShip(Vector3Int startCell, int shipSize, bool isHorizontal, out List<Vector3Int> cells)         //Validates ship placement
    {
        cells = new();

        if (shipSize <= 0)
            return false;

        for (int i = 0; i < shipSize; i++)
        {
            Vector3Int cell = startCell +
                (isHorizontal ? new Vector3Int(i, 0, 0) : new Vector3Int(0, i, 0));

            if (botTilemap.GetTile(cell) == null)
                return false;

            Vector3 worldPos = botTilemap.GetCellCenterWorld(cell);

            if (!boardBounds.bounds.Contains(worldPos))
                return false;

            if (occupiedCells.Contains(cell))
                return false;

            cells.Add(cell);
        }

        return true;
    }

    public void OccupyCells(List<Vector3Int> cells)                         //Registers cells that were occupied by the ships placed on the board.
    {
        foreach (var cell in cells)
            occupiedCells.Add(cell);

        ShipData ship = new ShipData();                                   
        ship.cells.AddRange(cells);
        ships.Add(ship);                                                    //Registers ships placed on the board.
    }

    public void FreeCells(List<Vector3Int> cells)                          //Cleans up the board
    {
        foreach (var cell in cells)
            occupiedCells.Remove(cell);
    }

    //Helper Methods
    public Vector3 GetWorldFromCell(Vector3Int cell)                        //Returns CellCenterWorld from BotTilemap for the provided the cell
    {
        return botTilemap.GetCellCenterWorld(cell);
    }

    public BoxCollider2D GetBounds() => boardBounds;                        //Returns bot's tilemap bounds

    public ShipData GetShipAt(Vector3Int cell)                              //Returns ship at the given position if exists.
    {
        foreach (var ship in ships)
        {
            if (ship.Contains(cell))
                return ship;
        }

        return null;
    }

    public void RegisterHit(Vector3Int cell)                             //Registers hit on board if there is a ship in the tile that got shot.
    {
        ShipData ship = GetShipAt(cell);

        if (ship == null)
            return;

        ship.RegisterHit();
        occupiedCells.Remove(cell);

        if (ship.IsSunk())
        {
            Debug.Log("Bot ship sunk!");

            if (ship.WasShipDestroyedInATurn())
            {
                Debug.Log("Explosive Shot unlocked!");
                ShootController.isPowerShotActivatedForPlayer = true;
            }
        }
    }
    public void ResetTurnHits()                                      // Resets combo counter for every ships.
    {
        foreach (var ship in ships)
            ship.ResetTurnHits();
    }

}
