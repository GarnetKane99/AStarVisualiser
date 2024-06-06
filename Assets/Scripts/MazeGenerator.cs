using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Runtime.CompilerServices;

enum GridCoords
{
    Visited,
    NotVisited
}
public class MazeGenerator : MonoBehaviour
{
    public static MazeGenerator instance;

    public Tile bgTile;
    public Tile lineTile;

    public Node node;

    public GameObject runner;
    public GameObject startObj;
    public GameObject endObj;    

    public const int width = 20;
    public const int height = 10;

    private GridCoords[,] grid;
    public Node[,] nodes;

    public Button generateMap;
    public Button placeStartObj;
    public Button placeEndObj;

    public bool mapGenerating;
    public bool mapGenerated = false;

    public GameObject selectedObject;
    public GameObject placedStartObject;
    public GameObject placedEndObject;


    private void Awake()
    {
        instance = this;

        generateMap.onClick.AddListener(GenerateGrid);
        placeStartObj.onClick.AddListener(PlaceStartObject);
        placeEndObj.onClick.AddListener(PlaceEndObject);

        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (mapGenerating) { return; }
        if (AStarManager.instance != null && AStarManager.instance.generatingPath) { return; }

        if (nodes != null)
        {
            for(int x = 0; x < nodes.GetLength(0); x++)
            {
                for(int y = 0; y < nodes.GetLength(1); y++)
                {
                    for(int z = 0; z < nodes[x,y].walls.Count; z++)
                    {
                        if (nodes[x, y].walls[z] != null)
                            Destroy(nodes[x, y].walls[z].gameObject);
                    }
                    Destroy(nodes[x, y].gameObject);
                }
            }

            if (placedStartObject) { Destroy(placedStartObject.gameObject); }
            if (placedEndObject) { Destroy(placedEndObject.gameObject); }

            selectedObject = null;
        }

        if(AStarManager.instance != null && AStarManager.instance.pathObjects.Count > 0)
        {
            foreach(GameObject n in AStarManager.instance.pathObjects)
            {
                Destroy(n.gameObject);
            }
            AStarManager.instance.pathObjects.Clear();
        }

        mapGenerating = true;
        grid = new GridCoords[width, height];
        nodes = new Node[width, height];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {                
                grid[x, y] = GridCoords.NotVisited;
                nodes[x, y] = Instantiate(node, new Vector2(x,y), Quaternion.identity, transform);
                Instantiate(bgTile, new Vector3(x, y, 1), Quaternion.identity);

                List<Tile> walls = new List<Tile>();

                if(x % 2 == 0 && y == height - 1)
                    Instantiate(lineTile, new Vector2(x, y), Quaternion.Euler(0, 0, 90));
                if (x == 0 && y % 2 != 0)
                    Instantiate(lineTile, new Vector2(x, y), Quaternion.Euler(0,0,180));
                if (x == width - 1 && y % 2 == 0)
                    Instantiate(lineTile, new Vector2(x, y), Quaternion.identity);
                if (x % 2 != 0 && y == 0)
                    Instantiate(lineTile, new Vector2(x, y), Quaternion.Euler(0, 0, 270));

                if (x % 2 == 0 && y % 2 == 0 || x % 2 != 0 && y % 2 != 0)
                {
                    for (int z = 0; z < 4; z++)
                    {
                        var obj = Instantiate(lineTile, new Vector2(x, y), Quaternion.Euler(0, 0, 90 * z));
                        walls.Add(obj);
                    }
                }

                nodes[x, y].walls = walls;
            }
        }

        StartCoroutine(GenerateMaze());
    }

    private IEnumerator GenerateMaze()
    {
        Vector2Int start = new Vector2Int(Random.Range(0, width), Random.Range(0, height));

        Vector2Int curPos = start;
        GameObject runnerInstance = Instantiate(runner, new Vector2(curPos.x, curPos.y), Quaternion.identity);
        grid[curPos.x, curPos.y] = GridCoords.Visited;

        bool allVisited = false;

        while(allVisited == false)
        {
            Vector2Int dir = ChooseDirection();
            if ((curPos + dir).x > width - 1 || (curPos + dir).x < 0 || (curPos + dir).y > height - 1|| (curPos + dir).y < 0)
                continue;

            runnerInstance.transform.position = new Vector3(curPos.x + dir.x, curPos.y + dir.y);
            curPos = curPos + dir;
            if (grid[curPos.x, curPos.y] == GridCoords.NotVisited)
            {
                RemoveWalls(nodes[curPos.x, curPos.y].walls.Count > 0 ?
                    nodes[curPos.x, curPos.y] : nodes[curPos.x - dir.x, curPos.y - dir.y], dir,
                    nodes[curPos.x,curPos.y].walls.Count > 0);

                grid[curPos.x, curPos.y] = GridCoords.Visited;
            }
            yield return new WaitForSeconds(0.001f);
            int val = 0;
            for(int x = 0; x < grid.GetLength(0); x++)
            {
                for(int y = 0; y < grid.GetLength(1); y++)
                {
                    if (grid[x, y] == GridCoords.NotVisited)
                    {
                        val = 1;
                        break;
                    };
                }
                if(val == 1) { break; }
            }
            allVisited = val == 0;
        }

        Destroy(runnerInstance.gameObject);
        AddConnections();
        mapGenerating = false;
        mapGenerated = true;
    }

    private void RemoveWalls(Node node, Vector2Int dirToRemove, bool opp)
    {
        //if(node.walls.Count == 0) { return; }
        if (dirToRemove.x == 1)
        {
            Destroy(node.walls[opp ? 2 : 0].gameObject);
        } 
        else if (dirToRemove.x == -1)
        {
            Destroy(node.walls[opp ? 0 : 2].gameObject);
        } 
        else if (dirToRemove.y == 1)
        {
            Destroy(node.walls[opp ? 3 : 1].gameObject);
        }
        else if(dirToRemove.y == -1)
        {
            Destroy(node.walls[opp ? 1 : 3].gameObject);
        }
    }

    Vector2Int ChooseDirection()
    {
        int val = Mathf.FloorToInt(Random.value * 3.99f);
        switch (val)
        {
            case 0:
                return Vector2Int.down;
            case 1:
                return Vector2Int.left;
            case 2:
                return Vector2Int.right;
            default:
                return Vector2Int.up;
        }
    }

    private void AddConnections()
    {
        for(int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                if (x > 0)
                {
                    if (nodes[x - 1, y].walls.Count > 0)
                    {
                        if (nodes[x - 1, y].walls[0] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x - 1, y]);
                        }
                    }else if (nodes[x,y].walls.Count > 0)
                    {
                        if (nodes[x, y].walls[2] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x - 1, y]);
                        }
                    }
                }
                if (x < nodes.GetLength(0) - 1)
                {
                    if (nodes[x+1,y].walls.Count > 0)
                    {
                        if (nodes[x + 1, y].walls[2] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x + 1, y]);
                        }
                    }else if (nodes[x,y].walls.Count > 0)
                    {
                        if (nodes[x, y].walls[0] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x + 1, y]);
                        }
                    }
                }
                if(y > 0)
                {
                    if (nodes[x,y-1].walls.Count > 0)
                    {
                        if (nodes[x, y - 1].walls[1] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x, y - 1]);
                        }
                    }else if (nodes[x,y].walls.Count > 0)
                    {
                        if (nodes[x, y].walls[3] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x, y - 1]);
                        }
                    }
                }
                if(y < nodes.GetLength(1) - 1)
                {
                    if (nodes[x, y + 1].walls.Count > 0)
                    {
                        if (nodes[x, y + 1].walls[3] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x, y + 1]);
                        }
                    }
                    else if (nodes[x, y].walls.Count > 0)
                    {
                        if (nodes[x, y].walls[1] == null)
                        {
                            nodes[x, y].connections.Add(nodes[x, y + 1]);
                        }
                    }
                }
            }
        }
    }

    private void PlaceStartObject()
    {
        if (mapGenerating) { return; }
        if (AStarManager.instance.generatingPath) { return; }

        selectedObject = startObj;
    }
    private void PlaceEndObject()
    {
        if (mapGenerating) { return; }
        if (AStarManager.instance.generatingPath) { return; }

        selectedObject = endObj;
    }

    private void Update()
    {
        if (mapGenerating) { return; }

        if (!mapGenerated) { return; }

        if (AStarManager.instance.generatingPath) { return; }

        if (selectedObject == null) { return; }

        if (Input.GetMouseButtonDown(0))
        {
            if(selectedObject == startObj)
            {
                Vector2Int nearest = NearestCoordinates(new Vector2(Input.mousePosition.x + 0.5f, Input.mousePosition.y + 0.5f));

                if(nearest.x >= 0 && nearest.x < grid.GetLength(0) && nearest.y >= 0 && nearest.y < grid.GetLength(1))
                {
                    if (placedStartObject) { Destroy(placedStartObject.gameObject); }

                    if (placedEndObject)
                    {
                        if(Vector2.Distance(nearest, placedEndObject.transform.position) < 0.1f)
                        {
                            Destroy(placedEndObject.gameObject);
                        }
                    }

                    placedStartObject = Instantiate(selectedObject, new Vector2(nearest.x, nearest.y), Quaternion.identity);
                }
            }
            if(selectedObject == endObj)
            {
                Vector2Int nearest = NearestCoordinates(new Vector2(Input.mousePosition.x + 0.5f, Input.mousePosition.y + 0.5f));

                if (nearest.x >= 0 && nearest.x < grid.GetLength(0) && nearest.y >= 0 && nearest.y < grid.GetLength(1))
                {
                    if (placedEndObject) { Destroy(placedEndObject.gameObject); }

                    if (placedStartObject)
                    {
                        if (Vector2.Distance(nearest, placedStartObject.transform.position) < 0.1f)
                        {
                            Destroy(placedStartObject.gameObject);
                        }
                    }

                    placedEndObject = Instantiate(selectedObject, new Vector2(nearest.x, nearest.y), Quaternion.identity);
                }
            }
        }
    }

    private Vector2Int NearestCoordinates(Vector2 selectedPosition)
    {
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(selectedPosition);

        //Vector2 mousePos = Input.mousePosition;
        return WorldToGrid(mouseWorldPosition);

    }

    Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // Translate world position to grid origin
        //Vector3 positionRelativeToGridOrigin = worldPosition - (Vector3)gridOrigin;

        // Calculate grid coordinates
        int gridX = Mathf.FloorToInt(worldPosition.x / 1);
        int gridY = Mathf.FloorToInt(worldPosition.y / 1);

        return new Vector2Int(gridX, gridY);
    }
}