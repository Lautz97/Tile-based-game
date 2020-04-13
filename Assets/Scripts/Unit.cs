using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{

    public int moveSpeed = 2;
    float remainingMovement = 2;
    public float stepSpeed = 5;
    public Vector2Int tile;
    public Map map;
    public List<Node> currentPath = null;

    private void Update()
    {
        if (currentPath != null)
        {
            int currNode = 0;
            while (currNode < currentPath.Count - 1)
            {
                Vector3 start = new Vector3(currentPath[currNode].x, currentPath[currNode].y);
                Vector3 end = new Vector3(currentPath[currNode + 1].x, currentPath[currNode + 1].y);
                Debug.DrawLine(start, end);
                currNode++;
            }
        }

        if (Vector3.Distance(transform.position, new Vector3(tile.x, tile.y)) < 0.1f
            && Vector3.Distance(transform.position, new Vector3(tile.x, tile.y)) > 0)
        {
            AdvancePathing();
        }

        // Smoothly animate towards the correct map tile.
        transform.position = Vector3.Lerp(transform.position, new Vector3(tile.x, tile.y), stepSpeed * Time.deltaTime);

    }

    // Advances our pathfinding progress by one tile.
    void AdvancePathing()
    {
        if (currentPath == null)
            return;

        if (remainingMovement <= 0)
            return;

        // avoid teleport us to our correct "current" position, in case we
        // haven't finished the animation yet.
        transform.position = new Vector3(tile.x, tile.y, transform.position.z);

        // Get cost from current tile to next tile
        remainingMovement -= map.CostToEnterTile(currentPath[0].x, currentPath[0].y, currentPath[1].x, currentPath[1].y);

        // Move us to the next tile in the sequence
        tile = new Vector2Int(currentPath[1].x, currentPath[1].y);

        // Remove the old "current" tile from the pathfinding list
        currentPath.RemoveAt(0);

        if (currentPath.Count == 1)
        {
            // We only have one tile left in the path, and that tile MUST be our ultimate
            // destination -- and we are standing on it!
            // So let's just clear our pathfinding info.
            currentPath = null;
        }

    }

    // The "Next Turn" button calls this.
    public void NextTurn()
    {
        // Make sure to wrap-up any outstanding movement left over.
        while (currentPath != null && remainingMovement > 0)
        {
            AdvancePathing();
        }

        // Reset our available movement points.
        remainingMovement = moveSpeed;
    }

}
