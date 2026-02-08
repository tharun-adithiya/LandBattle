using System.Collections.Generic;
using UnityEngine;

public class BotShipController : MonoBehaviour
{
    [Header("Board")]
    [SerializeField] private BotBoardManager botBoardManager;

    [Header("Placement")]
    public int shipSize;
    public bool isHorizontal;

    private List<Vector3Int> currentShipOccupiedCells = new();

    // NEW
    private bool isPlaced;

    // Called by BotManager to inject the board manager reference
    public void SetBoardManager(BotBoardManager manager)
    {
        botBoardManager = manager;
    }

    // Called by BotManager
    public void PlaceAtCell(Vector3Int startCell)
    {
        if (isPlaced) return;                     // NEW

        if (botBoardManager == null)
        {
            Debug.LogError($"{name}: BotBoardManager is null! Make sure it's assigned in the Inspector.");
            return;
        }

        if (botBoardManager.TryPlacngShip(startCell, shipSize, isHorizontal, out List<Vector3Int> cells))
        {
            PlaceShip(cells);
            isPlaced = true;                     // NEW
            Debug.Log($"{name} successfully placed with {cells.Count} cells registered");
        }
        else
        {
            Debug.LogError($"{name} could not be placed near {startCell}. Size: {shipSize}, Horizontal: {isHorizontal}");
        }
    }

    // Same logic as player PlaceShip
    void PlaceShip(List<Vector3Int> cells)
    {
        currentShipOccupiedCells.Clear();
        currentShipOccupiedCells.AddRange(cells);
        botBoardManager.OccupyCells(cells,this);

        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        Vector3 worldPos = botBoardManager.GetWorldFromCell(cells[0]) + offsetVector;

        transform.position = worldPos;
        transform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 0 : -90);
    }

    void FreeOccupiedCells()                                    //Cleans up the board
    {
        if (botBoardManager == null) return;        // NEW

        botBoardManager.FreeCells(currentShipOccupiedCells);
        currentShipOccupiedCells.Clear();
    }

    //  This method is called only if the ship placed by bot is vertical.
    public void Rotate()
    {
        FreeOccupiedCells();

        bool original = isHorizontal;

        Vector3Int start = botBoardManager.GetStartCell(transform.position, shipSize, isHorizontal);

        int pivot = (shipSize - 1) / 2;
        Vector3Int pivotCell = start + (isHorizontal ? Vector3Int.right * pivot : Vector3Int.up * pivot);

        isHorizontal = !isHorizontal;

        Vector3Int newStart =
            pivotCell - (isHorizontal ? Vector3Int.right * pivot : Vector3Int.up * pivot);

        if (botBoardManager.CanPlaceShip(newStart, shipSize, isHorizontal, out List<Vector3Int> cells))
        {
            PlaceShip(cells);
        }
        else
        {
            // revert
            isHorizontal = original;
            botBoardManager.CanPlaceShip(start, shipSize, isHorizontal, out cells);
            PlaceShip(cells);
        }
    }

    private void OnDestroy()
    {
        if (botBoardManager != null)               // NEW
            botBoardManager.FreeCells(currentShipOccupiedCells);
    }
}
