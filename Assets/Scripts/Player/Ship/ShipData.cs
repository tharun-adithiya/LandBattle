using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShipData                               //Generalized class to handle separate ship data
{
    public List<Vector3Int> cells = new();
    public int hitCount;
    public int hitsInThisTurn;

    public int originalSize;                     

    public bool Contains(Vector3Int cell)
    {
        return cells.Contains(cell);
    }

    // NEW — called after placement
    public void Initialize()
    {
        originalSize = cells.Count;
    }

    public void RegisterHit()
    {
        hitCount++;
        hitsInThisTurn++;
    }

    public bool IsSunk()
    {
        return hitCount >= originalSize;         
    }

    public void ResetTurnHits()
    {
        Debug.Log("reseting combo");
        hitsInThisTurn = 0;
    }

    public bool WasShipDestroyedInATurn()
    {
        return hitsInThisTurn >= originalSize;  
    }
}
