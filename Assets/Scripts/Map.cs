using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Map : MonoBehaviour
{
    public Transform selectedUnit;

    [SerializeField]
    TileType[] tileTypes;

    int[,] tiles;
    Node[,] graph;

    Vector2Int mapSize = new Vector2Int(10, 10);

    public void Next()
    {
        selectedUnit.GetComponent<Unit>().NextTurn();
    }

    void Start()
    {

        selectedUnit.GetComponent<Unit>().tile =
            new Vector2Int((int)selectedUnit.transform.position.x, (int)selectedUnit.transform.position.y);
        selectedUnit.GetComponent<Unit>().map = this;

        tiles = new int[mapSize.x, mapSize.y];

        GenerateMapData();
        GeneratePathFindingGraph();
        InstantiateTiles();

    }

    void GenerateMapData()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                tiles[x, y] = Random.Range(0, tileTypes.Length);
            }
        }
        tiles[5, 5] = 0;
    }

    void InstantiateTiles()
    {
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                GameObject nt = Instantiate(tileTypes[tiles[x, y]].tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                nt.name = tileTypes[tiles[x, y]].name + "_" + x + "_" + y;
                nt.transform.parent = this.transform;
                if (!nt.GetComponent<ClickableTile>()) nt.AddComponent<ClickableTile>();
                ClickableTile ct = nt.GetComponent<ClickableTile>();
                ct.tile = new Vector2Int(x, y);
                ct.map = this;
            }
        }
    }

    public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY)
    {
        TileType tt = tileTypes[tiles[targetX, targetY]];

        float cost = tt.movementCost;

        if (sourceX != targetX && sourceY != targetY)
        {
            cost += 0.001f;
        }

        if (!UnitCanEnterTile(targetX, targetY))
            return Mathf.Infinity;

        return cost;
    }

    public bool UnitCanEnterTile(int x, int y)
    {
        //this unit can walk/hover/fly over this tile?
        selectedUnit.GetComponent<Unit>();
        //advanced path finding will include flying ass

        return tileTypes[tiles[x, y]].walkable;
    }

    public void GeneratePathTo(int x, int y)
    {
        selectedUnit.GetComponent<Unit>().currentPath = null;

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        Node source = graph[selectedUnit.GetComponent<Unit>().tile.x, selectedUnit.GetComponent<Unit>().tile.y];

        Node target = graph[x, y];

        dist[source] = 0;
        prev[source] = null;

        List<Node> unvisited = new List<Node>();

        //Initialize all to have inf distance cause we do not know the actual distance
        //Also possible that something is impossible to reach
        foreach (Node v in graph)
        {
            if (v != source)
            {
                dist[v] = Mathf.Infinity;
                prev[v] = null;
            }

            unvisited.Add(v);
        }

        while (unvisited.Count > 0)
        {
            // u is the unvisited node ith less distance
            Node u = null;
            foreach (Node possibleU in unvisited)
            {
                if (u == null || dist[possibleU] < dist[u])
                {
                    u = possibleU;
                }
            }

            if (u == target)
            {
                break;
            }

            unvisited.Remove(u);

            foreach (Node v in u.neighbours)
            {
                //float alt = dist[u] + u.DistanceTo(v);
                float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
                if (alt < dist[v])
                {
                    dist[v] = alt;
                    prev[v] = u;
                }
            }

        }

        //route found or no route possible
        if (prev[target] == null)
        {
            //no routes possible to target starting at source
            return;
        }
        List<Node> currentPath = new List<Node>();
        Node curr = target;
        while (curr != null)
        {
            currentPath.Add(curr);
            curr = prev[curr];
        }

        currentPath.Reverse();

        selectedUnit.GetComponent<Unit>().currentPath = currentPath;

    }

    #region graph
    void GeneratePathFindingGraph()
    {
        //Init graph
        graph = new Node[mapSize.x, mapSize.y];
        //populate graph
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                graph[x, y] = new Node();

                graph[x, y].x = x;
                graph[x, y].y = y;
            }
        }
        //find neighbours
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                //movimento a 4 direzioni
                // if (x != 0)
                //     graph[x, y].neighbours.Add(graph[x - 1, y]);
                // if (x != mapSize.x - 1)
                //     graph[x, y].neighbours.Add(graph[x + 1, y]);
                // if (y != 0)
                //     graph[x, y].neighbours.Add(graph[x, y - 1]);
                // if (y != mapSize.y - 1)
                //     graph[x, y].neighbours.Add(graph[x, y + 1]);

                //movimento a 8 direzioni
                if (x != 0)
                {
                    graph[x, y].neighbours.Add(graph[x - 1, y]);
                    if (y != 0)
                        graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
                    if (y != mapSize.y - 1)
                        graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
                }

                if (x != mapSize.x - 1)
                {
                    graph[x, y].neighbours.Add(graph[x + 1, y]);
                    if (y != 0)
                        graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
                    if (y != mapSize.y - 1)
                        graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
                }

                if (y != 0)
                    graph[x, y].neighbours.Add(graph[x, y - 1]);
                if (y != mapSize.y - 1)
                    graph[x, y].neighbours.Add(graph[x, y + 1]);


            }
        }
    }
    #endregion

}
