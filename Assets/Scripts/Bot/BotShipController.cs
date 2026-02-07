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

    private void OnEnable()
    {
        botBoardManager = GameObject.FindFirstObjectByType<BotBoardManager>();
    }

    // Called by BotManager
    public void PlaceAtCell(Vector3Int startCell)
    {
        if (botBoardManager.CanPlaceShip(startCell, shipSize, isHorizontal, out List<Vector3Int> cells))
        {
            PlaceShip(cells);
        }
        else
        {
            Debug.LogError($"{name} cannot be placed at {startCell}");
        }
    }

    // Same logic as player PlaceShip
    void PlaceShip(List<Vector3Int> cells)
    {
        currentShipOccupiedCells.Clear();
        currentShipOccupiedCells.AddRange(cells);
        botBoardManager.OccupyCells(cells);

        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        Vector3 worldPos = botBoardManager.GetWorldFromCell(cells[0]) + offsetVector;

        transform.position = worldPos;
        transform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 0 : -90);
    }

    void FreeOccupiedCells()                                    //Cleans up the board
    {
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
        botBoardManager.FreeCells(currentShipOccupiedCells);
    }
}
