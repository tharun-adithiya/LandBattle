using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotBoardManager : MonoBehaviour
{
    public static event Action<Vector3Int,BotShipController> OnBotShipSunk;
    public static event Action OnAllBotShipsDestroyed;
    private int numberOfShipsDestroyed=0;
    [SerializeField] private Tilemap botTilemap;
    [SerializeField] private BoxCollider2D boardBounds;
    private HashSet<Vector3Int> occupiedCells = new();

    private List<ShipData> ships = new();
    private Dictionary<Vector3Int, BotShipController> cellToShip = new();

    public void ResetBoard()
    {
        occupiedCells.Clear();
        ships.Clear();
        cellToShip.Clear();
        numberOfShipsDestroyed = 0;
    }

    public Vector3Int GetStartCell(Vector3 worldPosition, int shipSize, bool isHorizontal)
    {
        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        return botTilemap.WorldToCell(worldPosition - offsetVector);
    }

    public bool CanPlaceShip(Vector3Int startCell, int shipSize, bool isHorizontal, out List<Vector3Int> cells)
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

    public bool TryPlacngShip(Vector3Int preferredCell, int shipSize, bool isHorizontal, out List<Vector3Int> cells)
    {
        if (CanPlaceShip(preferredCell, shipSize, isHorizontal, out cells))
            return true;

        Debug.LogError($"Invalid bot placement at {preferredCell} size:{shipSize} horizontal:{isHorizontal}");

        cells = new();
        return false;
    }

    public void OccupyCells(List<Vector3Int> cells, BotShipController botShipController)
    {
        foreach (var cell in cells)
        {
            occupiedCells.Add(cell);
            cellToShip[cell] = botShipController;
        }

        ShipData ship = new ShipData();
        ship.cells.AddRange(cells);
        ship.Initialize();
        ships.Add(ship);

        Debug.Log($"Bot ship registered with {cells.Count} cells. Total ships: {ships.Count}");
    }

    public void FreeCells(List<Vector3Int> cells)
    {
        if (cells == null || cells.Count == 0) return;

        ShipData shipToRemove = GetShipAt(cells[0]);
        if (shipToRemove != null)
            ships.Remove(shipToRemove);

        foreach (var cell in cells)
        {
            occupiedCells.Remove(cell);
            cellToShip.Remove(cell);
        }
    }

    public Vector3 GetWorldFromCell(Vector3Int cell)
    {
        return botTilemap.GetCellCenterWorld(cell);
    }

    public BoxCollider2D GetBounds() => boardBounds;

    public ShipData GetShipAt(Vector3Int cell)
    {
        foreach (var ship in ships)
        {
            if (ship.Contains(cell))
                return ship;
        }

        return null;
    }

    public void RegisterHit(Vector3Int cell)
    {
        ShipData ship = GetShipAt(cell);

        if (ship == null)
            return;

        ship.cells.Remove(cell);
        ship.RegisterHit();
        occupiedCells.Remove(cell);

        if (ship.IsSunk())
        {
            

            if (numberOfShipsDestroyed <= 5)
            {
                
                numberOfShipsDestroyed++;
                Debug.Log("Number of ships sunk:" + numberOfShipsDestroyed);
            }
            if (numberOfShipsDestroyed >= 5)
            {
                Debug.Log("Won");
                OnAllBotShipsDestroyed?.Invoke();
                GameManager.Instance.SetGameState(GameState.Win);
                return;
            }
            Debug.Log("Bot ship sunk!");
            if (cellToShip.ContainsKey(cell))
            {
                BotShipController currentShip = cellToShip[cell];
                OnBotShipSunk?.Invoke(cell, currentShip);
            }
            
            if (ship.WasShipDestroyedInATurn())
            {
                Debug.Log("Explosive Shot unlocked!");
                ShootController.isPowerShotActivatedForPlayer = true;
            }
        }
    }

    public void ResetTurnHits()
    {
        foreach (var ship in ships)
            ship.ResetTurnHits();
    }
}
