using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BoardManager : MonoBehaviour
{
    public static event Action<Vector3Int, ShipController> OnPlayerShipSunk;
    public static event Action OnAllPlayerShipsDestroyed;
    private int numberOfShipsSunk = 0;
    private Dictionary<Vector3Int, ShipController> cellToShip = new();
    public static int placedShipCount = 0;
    public static event Action OnPlacedAllShips;

    [SerializeField] private Tilemap playerbaseTilemap;
    [SerializeField] private BoxCollider2D boardBounds;
    [SerializeField] private GameObject highlightTilePrefab;

    private List<GameObject> placedShips = new();
    private List<GameObject> highlightPool = new();

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    private List<ShipData> ships = new();
    private int totalShipsInGame = 5;

    private void Awake()
    {
        ResetBoard();
    }

    public void ResetBoard()
    {
        ships.Clear();
        occupiedCells.Clear();
        cellToShip.Clear();
        placedShipCount = 0;
        numberOfShipsSunk = 0;

        foreach (var ship in placedShips)
            if (ship) Destroy(ship);
        placedShips.Clear();

        foreach (var h in highlightPool)
            if (h) Destroy(h);
        highlightPool.Clear();
    }

    private void TriggerOnPlacedAllShips()
    {
        Debug.Log("Fired " + OnPlacedAllShips);
        OnPlacedAllShips?.Invoke();
    }

    public Vector3Int GetStartCell(Vector3 worldPosition, int shipSize, bool isHorizontal)
    {
        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        return playerbaseTilemap.WorldToCell(worldPosition - offsetVector);
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

            if (playerbaseTilemap.GetTile(cell) == null)
                return false;

            Vector3 worldPos = playerbaseTilemap.GetCellCenterWorld(cell);

            if (!boardBounds.bounds.Contains(worldPos))
                return false;

            if (occupiedCells.Contains(cell))
                return false;

            cells.Add(cell);
        }

        return true;
    }

    public void OccupyCells(List<Vector3Int> cells, ShipController shipController)
    {
        if (ships.Count >= totalShipsInGame) return;

        foreach (var cell in cells)
        {
            occupiedCells.Add(cell);
            cellToShip[cell] = shipController;
        }

        ShipData ship = new ShipData();
        ship.cells.AddRange(cells);
        ship.Initialize();
        ship.hitCount = 0;
        ship.hitsInThisTurn = 0;

        ships.Add(ship);
    }

    public void RegisterShipsOnBoard(ShipController ship)
    {
        if (placedShipCount < totalShipsInGame)
        {
            placedShipCount++;
            placedShips.Add(ship.gameObject);
        }

        if (placedShipCount >= totalShipsInGame)
            TriggerOnPlacedAllShips();
    }

    public void LockShips()
    {
        foreach (GameObject ship in placedShips)
        {
            if (ship == null) continue;
            ship.GetComponent<ShipController>().enabled = false;
            ship.GetComponent<BoxCollider2D>().enabled = true;

            Color c = ship.GetComponent<SpriteRenderer>().color;
            c.a = 0.57f;
            ship.GetComponent<SpriteRenderer>().color = c;
        }

        GameManager.Instance.SetGameState(GameState.BotPlacementTurn);
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
        return playerbaseTilemap.GetCellCenterWorld(cell);
    }

    public void UpdateHighlights(Vector3Int startCell, int shipSize, bool isHorizontal)
    {
        ClearHighlights();

        if (CanPlaceShip(startCell, shipSize, isHorizontal, out List<Vector3Int> shipCells))
        {
            for (int i = 0; i < shipCells.Count; i++)
            {
                GameObject highlight = GetHighlight(i);
                highlight.transform.position = GetWorldFromCell(shipCells[i]);
                highlight.SetActive(true);
            }
        }
    }

    GameObject GetHighlight(int index)
    {
        if (index >= highlightPool.Count)
        {
            GameObject obj = Instantiate(highlightTilePrefab);
            highlightPool.Add(obj);
        }

        return highlightPool[index];
    }

    public void ClearHighlights()
    {
        foreach (var h in highlightPool)
            h.SetActive(false);
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
            Debug.Log("Ship sunk!");

            if (numberOfShipsSunk >= totalShipsInGame)
            {
                OnAllPlayerShipsDestroyed?.Invoke();
                GameManager.Instance.SetGameState(GameState.Lose);
                return;
            }
            if (numberOfShipsSunk <= totalShipsInGame)
            {
                numberOfShipsSunk++;
            }
            if (cellToShip.ContainsKey(cell))
            {
                ShipController currentShip = cellToShip[cell];
                OnPlayerShipSunk?.Invoke(cell, currentShip);
            }
            
            if (ship.WasShipDestroyedInATurn())
            {
                Debug.Log("Explosive Shot unlocked for bot!");
                ShootController.isPowerShotActivatedForBot = true;
            }
        }
    }

    public void ResetTurnHits()
    {
        foreach (var ship in ships)
            ship.ResetTurnHits();
    }
}
