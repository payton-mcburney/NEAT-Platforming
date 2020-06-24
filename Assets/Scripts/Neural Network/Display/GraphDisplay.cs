using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GraphDisplay
{

    public Genome Genome { get; private set; }
    public GameObject Graph { get; }
    private Dictionary<NodeGene, GameObject> nodes;
    private Dictionary<ConnectionGene, GameObject> edges;
    private List<List<NodeGene>> layers;
    

    // Load prefabs stored in Assets/Prefabs/Resources folder
    private static readonly GameObject NODE_PREFAB = (GameObject) Resources.Load("Node");
    private static readonly GameObject EDGE_PREFAB = (GameObject) Resources.Load("Edge");
    private static readonly GameObject RECURRENT_EDGE_PREFAB = (GameObject)Resources.Load("Recurrent Edge");

    public GraphDisplay(Genome genome)
    {
        Genome = genome;
        Graph = new GameObject("Genome " + genome.Id);
        Graph.transform.SetParent(GameObject.Find("Canvas").transform);

        RectTransform graphTransform = Graph.AddComponent<RectTransform>();
        graphTransform.anchorMin = new Vector2(0, 1);
        graphTransform.anchorMax = new Vector2(0, 1);
        graphTransform.anchoredPosition = new Vector2(50, -50);
        Graph.transform.localScale = new Vector2(25, 25);

        nodes = new Dictionary<NodeGene, GameObject>();
        edges = new Dictionary<ConnectionGene, GameObject>();
        DisplayNodes();
        DisplayEdges();
    }

    public void UpdateDisplay()
    {
        DisplayNodes();
        DisplayEdges();
    }

    private void DisplayNodes()
    {
        // Reset the node layers
        layers = new List<List<NodeGene>>();
        for (int i = 0; i < Genome.NumLayers; i++)
        {
            layers.Add(new List<NodeGene>());
        }

        foreach (NodeGene node in Genome.Nodes)
        {
            // Assign each node to the appropriate layer
            layers[node.Location].Add(node);

            // If this node has not already been displayed
            if (Graph.transform.Find("Node " + node.Id) == null)
            {
                // Instantiate the node game object
                GameObject displayNode = GameObject.Instantiate(NODE_PREFAB, Graph.transform);
                
                // Display the id on the node if id is enabled in node prefab
                TextMeshPro nodeTMPro = displayNode.GetComponentInChildren<TextMeshPro>();
                if (nodeTMPro != null)
                {
                    nodeTMPro.text = node.Id + "";
                }

                displayNode.name = "Node " + node.Id;
                nodes.Add(node, displayNode);
            }
        }

        // Set the positions of the nodes based on their layer
        for (int i = 0; i < layers.Count; i++)
        {
            for (int j = 0; j < layers[i].Count; j++)
            {
                //TODO: should not need this check, find out why it breaks
                nodes[layers[i][j]].transform.localPosition = new Vector2(2.0f * i, -10.0f * j / layers[i].Count);
            }
        }
    }

    private void DisplayEdges()
    {
        
        foreach (ConnectionGene connection in Genome.Connections)
        {
            // If the edge does not exist, create it
            if (Graph.transform.Find("Connection " + connection.Innovation) == null)
            {
                GameObject edge;
                if (connection.InNode == connection.OutNode) edge = GameObject.Instantiate(RECURRENT_EDGE_PREFAB);
                else edge = GameObject.Instantiate(EDGE_PREFAB);
                EdgeController edgeController = edge.GetComponent<EdgeController>();
                edgeController.InNode = (GameObject)nodes[connection.InNode];
                edgeController.OutNode = (GameObject)nodes[connection.OutNode];
                edge.name = "Connection " + connection.Innovation;
                edge.transform.parent = Graph.transform;
                edges.Add(connection, edge);
            }

            // If edge is disabled then do not show the edge
            if (!connection.Enabled && ((GameObject)edges[connection]).activeSelf)
            {
                ((GameObject)edges[connection]).SetActive(false);
            }
        }
    }
}
