using System.Collections;
using UnityEngine;

[System.Serializable]
public class TileType
{
    public string name;
    public GameObject tilePrefab;

    public bool walkable;
    public float movementCost = 1;
}
