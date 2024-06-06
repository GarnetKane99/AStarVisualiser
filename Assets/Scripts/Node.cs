using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public List<Tile> walls;
    public List<Node> connections;

    public float gScore;
    public float hScore;

    public Node cameFrom;

    public float FScore()
    {
        return gScore + hScore;
    }
}
