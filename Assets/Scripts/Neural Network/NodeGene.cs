using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This object stores all the attributes and methods for a node in a neural network under NEAT.
/// </summary>
public class NodeGene
{
    /// <summary>
    /// The type of node based on the layer the node is found in.
    /// </summary>
    public enum TYPE
    {
        /// <summary>
        /// A node found in the input layer.
        /// </summary>
        INPUT,
        /// <summary>
        /// An input node that is always on.
        /// </summary>
        BIAS,
        /// <summary>
        /// A node that is neither in the input or output layers.
        /// </summary>
        HIDDEN,
        /// <summary>
        /// A node found in the output layer.
        /// </summary>
        OUTPUT,
        /// <summary>
        /// A nonexistent node.
        /// </summary>
        NONE
    }

    /// <summary>
    /// The type of node gene this node is.
    /// </summary>
    public TYPE Type { get; }

    /// <summary>
    /// The unique id for this node gene.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Whether this node has a recurrent connection to itself.
    /// </summary>
    public bool Recurrent { get; set; }

    /// <summary>
    /// The location of this node, or the longest distance from this node to an input node
    /// </summary>
    public int Location { get; set; }

    /// <summary>
    /// A list of nodes this node is connected to by non-recurrent connections.
    /// </summary>
    public List<NodeGene> ConnectedNodes { get; }

    /// <summary>
    /// The output for this node
    /// </summary>
    public float Output { get; set; }


    /// <summary>
    /// Constructs a node gene of a given type and id.
    /// </summary>
    /// <param name="type">The type of node gene.</param>
    /// <param name="id">The unique id of this node gene.</param>
    public NodeGene(TYPE type, int id)
    {
        Type = type;
        Id = id;
        ConnectedNodes = new List<NodeGene>();
    }



    /// <summary>
    /// Adds node to list of connected nodes.
    /// </summary>
    /// <param name="node">The node that will be added to the list of connected nodes.</param>
    public void AddConnectedNode(NodeGene node)
    {
        ConnectedNodes.Add(node);
    }



    /// <summary>
    /// Updates the locations of every node connected to this node.
    /// </summary>
    public void UpdateLocations()
    {
        foreach (NodeGene node in ConnectedNodes)
        {
            int newLocation = Location + 1;
            if (node.Type != TYPE.OUTPUT && !node.Recurrent && newLocation > node.Location)
            {
                node.Location = newLocation;
                node.UpdateLocations();
                //Debug.Log(Id);
            }
        }
    }



    /// <summary>
    /// Determines equality of two node genes by comparing their ids
    /// </summary>
    /// <param name="obj">The object we are comparing this node gene to.</param>
    /// <returns>Returns true if these objects are both node genes and share the same id.</returns>
    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            NodeGene node = (NodeGene) obj;
            return this.Id == node.Id;
        }
    }



    /// <summary>
    /// Creates a new node gene with the same type and id as the original.
    /// </summary>
    /// <param name="node">The node that is being cloned.</param>
    /// <returns>Returns a new node gene with same type and id as the original.</returns>
    public static NodeGene DeepClone(NodeGene node)
    {
        return new NodeGene(node.Type, node.Id);
    }



    /// <summary>
    /// Converts the node into a string for testing.
    /// </summary>
    /// <returns>Returns a string containing the type and id of the node separated by a single space.</returns>
    public override string ToString()
    {
        return Type + " " + Id + " " + Location;
    }
}
