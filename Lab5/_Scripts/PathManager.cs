using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public GameObject tile { get; private set; }
    public List<PathConnection> connections;

    public PathNode(GameObject tile)
    {
        this.tile = tile;
        connections = new List<PathConnection>();
    }

    public void AddConnection(PathConnection connection)
    {
        connections.Add(connection);
    }
}

[System.Serializable]
public class PathConnection
{
    public float cost { get; set; }
    public PathNode fromNode { get; private set; }
    public PathNode toNode { get; private set; }

    public PathConnection(PathNode from, PathNode to, float cost = 1.0f)
    {
        fromNode = from;
        toNode = to;
        this.cost = cost;
    }
}

public class NodeRecord
{
    public PathNode node { get; set; }
    public NodeRecord fromRecord { get; set; }
    public PathConnection pathConnection { get; set; }
    public float costSoFar { get; set; }

    public NodeRecord(PathNode node = null)
    {
        this.node = node;
        pathConnection = null;
        fromRecord = null;
        costSoFar = 0.0f;
    }
}

[System.Serializable]
public class PathManager : MonoBehaviour
{
    public List<NodeRecord> openList;
    public List<NodeRecord> closedList;
    public List<PathConnection> path;

    public static PathManager instance { get; private set; }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Initialize();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        openList = new List<NodeRecord>();
        closedList = new List<NodeRecord>();
        path = new List<PathConnection>();
    }

    public void GetShortestPath(PathNode start, PathNode goal)
    {
        if (path.Count > 0)
        {
            GridManager.Instance.SetTileStatuses();
            path.Clear();
        }
        NodeRecord currentRecord = null;
        openList.Add(new NodeRecord(start));

        while (openList.Count > 0)
        {
            currentRecord = GetSmallestNode();
            if (currentRecord.node == goal)
            {
                openList.Remove(currentRecord);
                closedList.Add(currentRecord);
                currentRecord.node.tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                break;
            }

            List<PathConnection> connections = currentRecord.node.connections;
            for (int i = 0; i < connections.Count; i++)
            {
                PathNode endNode = connections[i].toNode;
                NodeRecord endNodeRecord;
                float endNodeCost = currentRecord.costSoFar + connections[i].cost;
                if (ContainNode(closedList, endNode)) continue;
                else if (ContainNode(openList, endNode))
                {
                    endNodeRecord = GetNodeRecord(openList, endNode);
                    if (endNodeRecord.costSoFar <= endNodeCost) continue;
                }
                else
                {
                    endNodeRecord = new NodeRecord();
                    endNodeRecord.node = endNode;
                }
                endNodeRecord.costSoFar = endNodeCost;
                endNodeRecord.pathConnection = connections[i];
                endNodeRecord.fromRecord = currentRecord;
                if (!ContainNode(openList, endNode))
                {
                    openList.Add(endNodeRecord);
                    endNodeRecord.node.tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
                }
            }
            openList.Remove(currentRecord);
            closedList.Add(currentRecord);
            currentRecord.node.tile.GetComponent<TileScript>().SetStatus(TileStatus.CLOSED);
        }
        if (currentRecord == null) return;
        if (currentRecord.node == goal)
        {
            while (currentRecord.node != start)
            {
                path.Add(currentRecord.pathConnection);
                currentRecord.node.tile.GetComponent<TileScript>().SetStatus(TileStatus.PATH);
                currentRecord = currentRecord.fromRecord;
            }
            path.Reverse();
        }
        openList.Clear();
        closedList.Clear();
    }

    public NodeRecord GetSmallestNode()
    {
        NodeRecord smallestNode = openList[0];

        for (int i = 1; i < openList.Count; i++)
        {
            if (openList[i].costSoFar < smallestNode.costSoFar)
            {
                smallestNode = openList[i];
            }
            else if (openList[i].costSoFar == smallestNode.costSoFar)
            {
                smallestNode = Random.value < 0.5 ? openList[i] : smallestNode;
            }
        }

        return smallestNode;
    }

    public bool ContainNode(List<NodeRecord> list, PathNode node)
    {
        foreach (NodeRecord record in list)
        {
            if (record.node == node) return true;
        }

        return false;
    }

    public NodeRecord GetNodeRecord(List<NodeRecord> list, PathNode node)
    {
        foreach (NodeRecord record in list)
        {
            if (record.node == node) return record;
        }

        return null;
    }
}
