/// <summary>
/// This object stores all the attributes and methods needed for a connection in a neural network under NEAT.
/// </summary>
public class ConnectionGene
{
    /// <summary>
    /// The node at the beginning of this connection.
    /// </summary>
    public NodeGene InNode { get; }
    /// <summary>
    /// The node at the end of this connection.
    /// </summary>
    public NodeGene OutNode { get; }
    /// <summary>
    /// The weight of this connection.
    /// </summary>
    public float Weight { get; set; }
    /// <summary>
    /// Whether or not this connection is enabled.
    /// </summary>
    public bool Enabled { get; set; }
    /// <summary>
    /// The unique innovation number for this connection.
    /// </summary>
    public int Innovation { get; }
    /// <summary>
    /// Whether the connection is recurrent or not, meaning the in node is in a layer after the out node.
    /// </summary>
    public bool Recurrent { get; }

    /// <summary>
    /// Constructs a new innovation given an in node, out node, weight, and innovation number; it is enabled by default.
    /// </summary>
    /// <param name="inNode">The node at the beginning of this connection.</param>
    /// <param name="outNode">The node at the end of this connection.</param>
    /// <param name="weight">The weight of this connection.</param>
    /// <param name="innovation">The unique innovation number for this connection.</param>
    /// <param name="recurrent">True if the in node is in a layer after the out node.</param>
    public ConnectionGene(NodeGene inNode, NodeGene outNode, float weight, int innovation, bool recurrent)
    {
        InNode = inNode;
        OutNode = outNode;
        Weight = weight;
        Enabled = true;
        Innovation = innovation;
        Recurrent = recurrent;
    }

    /// <summary>
    /// Creates a new connection gene with the same attributes as the original.
    /// </summary>
    /// <param name="connection">The connection that is being cloned.</param>
    /// <returns>Returns a new connection gene with same attributes as the original.</returns>
    public static ConnectionGene DeepClone(ConnectionGene connection)
    {
        return new ConnectionGene(connection.InNode, 
                                  connection.OutNode, 
                                  connection.Weight, 
                                  connection.Innovation, 
                                  connection.Recurrent);
    }

    /// <summary>
    /// Determines equality of two connection genes by comparing their innovation numbers.
    /// </summary>
    /// <param name="obj">The object we are comparing this connection gene to.</param>
    /// <returns>Returns true if both objects are connection genes and share innovation numbers.</returns>
    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            ConnectionGene node = (ConnectionGene) obj;
            return this.Innovation == node.Innovation;
        }
    }

    /// <summary>
    /// Converts the connection into a string for testing.
    /// </summary>
    /// <returns>Returns the attributes of this connection separated by the string " | ".</returns>
    public override string ToString()
    {
        string geneString = "";
        geneString += "InNode: " + InNode + " | ";
        geneString += "OutNode: " + OutNode + " | ";
        geneString += "Weight: " + Weight + " | ";
        geneString += "Enabled: " + Enabled + " | ";
        geneString += "Innovation: " + Innovation + " | ";
        return geneString;
    }

}
