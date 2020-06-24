/// <summary>
/// This object stores all the information associated with a node or connection innovation.
/// </summary>
public class Innovation
{
    /// <summary>
    /// The type of innovation this is.
    /// </summary>
    public enum TYPE
    {
        /// <summary>
        /// The innovation is a new node that was created.
        /// </summary>
        NEW_NODE,
        /// <summary>
        /// The innovation is a new connection that was created.
        /// </summary>
        NEW_CONNECTION
    }

    /// <summary>
    /// The unique id for this innovation, or innovation number.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The type of innovation this is.
    /// </summary>
    public TYPE Type { get; }

    /// <summary>
    /// The node in the middle of the connection it was created in; -1 if this is a connection innovation.
    /// </summary>
    public int NodeId { get; }

    /// <summary>
    /// The node at the beginning of the connection; -1 if this is a input, bias, or output node innovation
    /// </summary>
    public int InNodeId { get; }

    /// <summary>
    /// The node at the end of the connection; -1 if this is a input, bias, or output node innovation
    /// </summary>
    public int OutNodeId { get; }

    /// <summary>
    /// Constructor for the Innovation class, which tracks all unique nodes and connections created across all generations.
    /// </summary>
    /// <param name="id">The unique id for this innovation.</param>
    /// <param name="isNodeInnovation">Whether or not this innovation is a node or connection innovation.</param>
    /// <param name="node">The node in the middle of the connection it was created in.</param>
    /// <param name="inNode">The node at the beginning of the connection.</param>
    /// <param name="outNode">The node at the end of the connection.</param>
    public Innovation(int id, TYPE type, int node, int inNode, int outNode)
    {
        Id = id;
        Type = type;
        NodeId = node;
        InNodeId = inNode;
        OutNodeId = outNode;
    }

    public override string ToString()
    {
        return "Id: " + Id + " | InNode: " + InNodeId + " | OutNode: " + OutNodeId + " | Node: " + NodeId + " | Type: " + Type;
    }

}
