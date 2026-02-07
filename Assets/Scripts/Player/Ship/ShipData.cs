using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData                               //Generalized class to handle separate ship data
{
    public List<Vector3Int> cells = new();
    public int hitCount;
    public int hitsInThisTurn;

    public bool Contains(Vector3Int cell)
    {
        return cells.Contains(cell);
    }

    public void RegisterHit()
    {
        hitCount++;
        hitsInThisTurn++;
    }

    public bool IsSunk()
    {
        return hitCount >= cells.Count;
    }
    public void ResetTurnHits()
    {
        Debug.Log("reseting combo");
        hitsInThisTurn = 0;
    }

    public bool WasShipDestroyedInATurn()
    {
        return hitsInThisTurn>=cells.Count;
    }
}
