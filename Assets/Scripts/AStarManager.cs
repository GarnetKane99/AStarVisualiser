using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class AStarManager : MonoBehaviour
{
    public static AStarManager instance;
    public List<GameObject> pathObjects;

    public Button generatePath;
    public Button clearMap;

    public GameObject pathObj;
    public GameObject visited;

    public bool generatingPath;


    private void Awake()
    {
        instance = this;
        generatePath.onClick.AddListener(() =>
        {
            if (MazeGenerator.instance == null) { return; }

            if (MazeGenerator.instance.mapGenerating || MazeGenerator.instance.mapGenerating && !MazeGenerator.instance.mapGenerated) { return; }

            if (MazeGenerator.instance.placedStartObject == null) { return; }
            if (MazeGenerator.instance.placedEndObject == null) { return; }
            if (generatingPath) { return; }

            if (pathObjects.Count > 0)
            {
                foreach (GameObject n in AStarManager.instance.pathObjects)
                {
                    Destroy(n.gameObject);
                }
                pathObjects.Clear();
            }

            //List<Node> nodes = GeneratePath(NearestNode(MazeGenerator.instance.placedStartObject.transform.position), NearestNode(MazeGenerator.instance.placedEndObject.transform.position));
            Node start = NearestNode(MazeGenerator.instance.placedStartObject.transform.position);
            Node end = NearestNode(MazeGenerator.instance.placedEndObject.transform.position);
            List<Node> nodes = new List<Node>();
            StartCoroutine(GeneratePathRoutine(MazeComplete, start, end));
        });

        clearMap.onClick.AddListener(() =>
        {
            if (generatingPath) { return; }

            if (pathObjects.Count > 0)
            {
                foreach (GameObject n in AStarManager.instance.pathObjects)
                {
                    Destroy(n.gameObject);
                }
                pathObjects.Clear();
            }
        });
    }

    public void MazeComplete(List<Node> nodes)
    {
        StartCoroutine(GeneratePath(nodes));
    }

    public IEnumerator GeneratePath(List<Node> nodes)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (Node n in nodes)
        {
            GameObject path = Instantiate(pathObj, new Vector3(n.transform.position.x, n.transform.position.y, 1), Quaternion.identity);
            pathObjects.Add(path);
            yield return new WaitForSeconds(0.1f);
        }
        generatingPath = false;
    }

    public IEnumerator GeneratePathRoutine(System.Action<List<Node>> callback, Node start, Node end)
    {
        generatingPath = true;
        List<Node> openSet = new List<Node>();

        foreach (Node n in MazeGenerator.instance.nodes)
        {
            n.gScore = float.MaxValue;
        }
        start.gScore = 0;
        start.hScore = Vector2.Distance(start.transform.position, end.transform.position);
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            int lowestF = default;

            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FScore() < openSet[lowestF].FScore())
                {
                    lowestF = i;
                }
            }

            Node currentNode = openSet[lowestF];

            openSet.Remove(currentNode);

            if (currentNode == end)
            {
                List<Node> path = new List<Node>();
                path.Insert(0, end);

                currentNode = end;
                while (currentNode != start)
                {
                    currentNode = currentNode.cameFrom;
                    path.Add(currentNode);
                }
                //return path;
                callback(path);
                yield break;
            }

            foreach (Node connectedNode in currentNode.connections)
            {
                float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);
                if (heldGScore < connectedNode.gScore)
                {
                    yield return new WaitForSeconds(0.1f);
                    GameObject path = Instantiate(visited, new Vector3(connectedNode.transform.position.x, connectedNode.transform.position.y, 1), Quaternion.identity);
                    pathObjects.Add(path);

                    connectedNode.cameFrom = currentNode;
                    connectedNode.gScore = heldGScore;
                    connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);
                    if (!openSet.Contains(connectedNode))
                    {
                        openSet.Add(connectedNode);
                    }
                }

            }
        }
    }

    /*    public List<Node> GeneratePath(Node start, Node end)
        {
            List<Node> openSet = new List<Node>();

            foreach (Node n in MazeGenerator.instance.nodes)
            {
                n.gScore = float.MaxValue;
            }
            start.gScore = 0;
            start.hScore = Vector2.Distance(start.transform.position, end.transform.position);
            openSet.Add(start);

            while (openSet.Count > 0)
            {
                int lowestF = default;

                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].FScore() < openSet[lowestF].FScore())
                    {
                        lowestF = i;
                    }
                }

                Node currentNode = openSet[lowestF];

                openSet.Remove(currentNode);

                if (currentNode == end)
                {
                    List<Node> path = new List<Node>();
                    path.Insert(0, end);

                    currentNode = end;
                    while (currentNode != start)
                    {
                        currentNode = currentNode.cameFrom;
                        path.Add(currentNode);
                    }
                    return path;
                }

                foreach (Node connectedNode in currentNode.connections)
                {
                    float heldGScore = currentNode.gScore + Vector2.Distance(currentNode.transform.position, connectedNode.transform.position);
                    if (heldGScore < connectedNode.gScore)
                    {
                        connectedNode.cameFrom = currentNode;
                        connectedNode.gScore = heldGScore;
                        connectedNode.hScore = Vector2.Distance(connectedNode.transform.position, end.transform.position);
                        if (!openSet.Contains(connectedNode))
                            openSet.Add(connectedNode);
                    }
                }
            }

            return null;
        }*/

    public Node NearestNode(Vector2 pos)
    {
        Node nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Node n in MazeGenerator.instance.nodes)
        {
            float curDist = Vector2.Distance(pos, n.transform.position);
            if (curDist < nearestDist)
            {
                nearestDist = curDist;
                nearest = n;
            }
        }

        return nearest;
    }
}
