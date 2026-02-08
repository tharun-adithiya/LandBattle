using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class ShipController : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("VFX")]
    [SerializeField] public ParticleSystem smokeVFX;

    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothing;                                //Modifiers for drag and feedback
    [SerializeField] private float snapSpeed;

    [Header("Parent")]
    [SerializeField] private Transform parentPosition;                          //Ship Holders (Parent)

    [Header("Board")]
    [SerializeField] private BoardManager boardManager;                        //Board where the player places the ships

    [Header("Placement")]
    [SerializeField] public int shipSize;                                      //Ship size to calculate cells that will be occupied by the ship. IsHorizontal checks for orientation (rotation)
    [SerializeField] public bool isHorizontal;

    private bool isDragging;                                                   //Bools to maintain ship states. 
    private bool isShipPlacedAtTile;

    private Vector3 offset;                                                    //Fields for pointer events
    private Tween moveTween;

    private List<Vector3Int> currentShipOccupiedCells = new List<Vector3Int>();        //Tracks the cells occupied this ship.

    public void OnBeginDrag(PointerEventData eventData)                                
    {
        offset = transform.position - GetWorldPosition(eventData);
        isDragging = true;

        FreeOccupiedCells();
        moveTween?.Kill();
        boardManager.ClearHighlights();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 targetPos = ClampToBoard(GetWorldPosition(eventData) + offset);

        moveTween = transform.DOMove(targetPos, dragSmoothing);

        Vector3Int cell = boardManager.GetStartCell(targetPos, shipSize, isHorizontal);         //Calculating start cell for displaying tile highlights
        boardManager.UpdateHighlights(cell, shipSize, isHorizontal);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 targetPos = ClampToBoard(GetWorldPosition(eventData) + offset);                 
        Vector3Int startCell = boardManager.GetStartCell(targetPos, shipSize, isHorizontal);

        if (boardManager.CanPlaceShip(startCell, shipSize, isHorizontal, out List<Vector3Int> cells))
            PlaceShip(cells);
        else
            SnapToParent();

        boardManager.ClearHighlights();
        isDragging = false;
    }

    void PlaceShip(List<Vector3Int> cells)                                   //This method gets called after placement validation
    {
        currentShipOccupiedCells.Clear();
        currentShipOccupiedCells.AddRange(cells);
        boardManager.OccupyCells(cells,this);

        Vector3 offsetVector = isHorizontal ?
            new Vector3((shipSize - 1) * 0.5f, 0, 0) :
            new Vector3(0, (shipSize - 1) * 0.5f, 0);

        Vector3 worldPos = boardManager.GetWorldFromCell(cells[0]) + offsetVector;

        transform.DOMove(worldPos, 0f);
        if (!isShipPlacedAtTile) boardManager.RegisterShipsOnBoard(this);
        Debug.Log(BoardManager.placedShipCount + " ships are placed");
        isShipPlacedAtTile = true;
        
    }

    void FreeOccupiedCells()                                                //If the ship is moved after placing, this method will be called to free up the previously occupied cells
    {
        boardManager.FreeCells(currentShipOccupiedCells);
        currentShipOccupiedCells.Clear();
    }

    public void SnapToParent()                                                //Snaps to parent if the ship is placed at invalid positions.
    {
        transform.DOMove(parentPosition.position, snapSpeed);
        isShipPlacedAtTile = false;
    }

    Vector3 ClampToBoard(Vector3 position)                                                  //Once the ship is dragged into the board, this method will clamp the ship within the board bounds
    {
        if (!isShipPlacedAtTile) return position;

        Bounds bounds = boardManager.GetBounds().bounds;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 extents = sr.bounds.extents;

        position.x = Mathf.Clamp(position.x, bounds.min.x + extents.x, bounds.max.x - extents.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y + extents.y, bounds.max.y - extents.y);

        return position;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging && isShipPlacedAtTile)
            TryRotateShip();
    }

    private void TryRotateShip()                                                            //This method tries to rotate the ship. It checks for neighbouring occupied cells and board boundaries
    {
        if (moveTween != null && moveTween.IsActive())
            moveTween.Complete();

        FreeOccupiedCells();

        bool original = isHorizontal;
        Vector3Int start = boardManager.GetStartCell(transform.position, shipSize, isHorizontal);

        int pivot = (shipSize - 1) / 2;
        Vector3Int pivotCell = start + (isHorizontal ? Vector3Int.right * pivot : Vector3Int.up * pivot);

        isHorizontal = !isHorizontal;

        Vector3Int newStart = pivotCell - (isHorizontal ? Vector3Int.right * pivot : Vector3Int.up * pivot);

        if (boardManager.CanPlaceShip(newStart, shipSize, isHorizontal, out List<Vector3Int> cells))
        {
            transform.rotation = Quaternion.Euler(0, 0, isHorizontal ? 0 : -90);
            PlaceShip(cells);
        }
        else
        {
            isHorizontal = original;
            boardManager.CanPlaceShip(start, shipSize, isHorizontal, out cells);
            PlaceShip(cells);
            transform.DOShakePosition(0.5f, 0.5f);
        }
    }

    Vector3 GetWorldPosition(PointerEventData eventData)            //Helper method for pointer events
    {
        Vector3 pos = eventData.position;
        pos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(pos);
    }
}
