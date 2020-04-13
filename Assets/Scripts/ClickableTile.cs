using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTile : MonoBehaviour
{
    public Vector2Int tile;
    public Map map;
    private void OnMouseUp()
    {
        map.GeneratePathTo(tile.x, tile.y);
        map.Next();
    }
}
