using System.Collections.Generic;

/// <summary>
/// This object tracks which innovations have been created over all generations and handles the creation of new innovations.
/// </summary>
public class InnovationGenerator
{
    /// <summary>
    /// The current unique innovation number we will assign to the next innovation created.
    /// </summary>
    public int CurrentInnovation { get; private set; }

    /// <summary>
    /// The current unique node id we will assign to the next node innovation created.
    /// </summary>
    public int CurrentNodeId { get; set; }

    /// <summary>
    /// The list of existing innovations.
    /// </summary>
    private List<Innovation> ExistingInnovations;

    /// <summary>
    /// The constructor for the InnovationGenerator class. Initializes currentInnovation and currentNodeId to 0.
    /// </summary>
    public InnovationGenerator()
    {
        CurrentInnovation = 0;
        CurrentNodeId = 0;
        ExistingInnovations = new List<Innovation>();
    }
    
    /// <summary>
    /// Adds the starting nodes to the existing innovations list. These include the input, output, and bias nodes.
    /// </summary>
    /// <param name="numStartingNodes">The number of starting nodes to add.</param>
    public void CreateStartingNodeInnovations(int numStartingNodes)
    {
        for (int i = 0; i < numStartingNodes; i++)
        {
            ExistingInnovations.Add(new Innovation(CurrentInnovation++, Innovation.TYPE.NEW_NODE, CurrentNodeId++, -1, -1));
        }
    }

    /// <summary>
    /// Checks the hash table of existing innovations for the innovation specified from the in node and out nodes.
    /// </summary>
    /// <param name="inNodeId">The id of the node that is at the beginning of the associated connection.</param>
    /// <param name="outNodeId">The id of the node that is at the end of the associated connection.</param>
    /// <param name="innovationType">The type of innovation that is being checked.</param>
    /// <returns>Returns the node id if this is a node innovation, otherwise returns innovation id. Returns -1 if the innovation does not yet exist.</returns>
    public int CheckInnovation(int inNodeId, int outNodeId, Innovation.TYPE innovationType)
    {
        // Iterate through all existing innovations to check if this innovation exists
        
        foreach (Innovation innovation in ExistingInnovations)
        {
            if (innovation.InNodeId == inNodeId && innovation.OutNodeId == outNodeId && innovation.Type == innovationType)
            {
                if (innovationType == Innovation.TYPE.NEW_NODE)
                {
                    return innovation.NodeId;
                }
                else if (innovationType == Innovation.TYPE.NEW_CONNECTION)
                {
                    return innovation.Id;
                }
            }
        }

        // Return -1 if this innovation does not yet exist
        return -1;
    }

    /// <summary>
    /// Creates and adds the new innovation defined by the in node and out node to the hash table of existing innovations.
    /// </summary>
    /// <param name="inNodeId">The id of the node that is at the beginning of the associated connection.</param>
    /// <param name="outNodeId">The id of the node that is at the end of the associated connection.</param>
    /// <param name="innovationType">The type of innovation that is being created.</param>
    public void CreateNewInnovation(int inNodeId, int outNodeId, Innovation.TYPE innovationType)
    {
        // Create the new innovation to add
        Innovation newInnovation = null;

        // Add a new innovation to the existing innovations depending on whether it is a node or connection innovation
        if (innovationType == Innovation.TYPE.NEW_NODE)
        {
            newInnovation = new Innovation(CurrentInnovation++, innovationType, CurrentNodeId++, inNodeId, outNodeId);
        }
        else if (innovationType == Innovation.TYPE.NEW_CONNECTION)
        {
            newInnovation = new Innovation(CurrentInnovation++, innovationType, -1, inNodeId, outNodeId);
        }
        
        ExistingInnovations.Add(newInnovation);
    }

    //public int GetConnectionInnovation(int inNodeId, int outNodeId)
    //{
    //    // Check if this connection already exists and return the innovation number if it does
    //    foreach (Innovation innovation in ExistingInnovations.Values)
    //    {
    //        if (!innovation.IsNodeInnovation && innovation.InNodeId == inNodeId && innovation.OutNodeId == outNodeId)
    //        {
    //            return innovation.Id;
    //        }
    //    }

    //    // If the connection does not exist already, create a new node and connection innovation
    //    Innovation newInnovation = new Innovation(currentInnovation, false, -1, inNodeId, outNodeId);
    //    ExistingInnovations.Add(newInnovation.Id, newInnovation);
    //    return currentInnovation++;
    //}

    //public int GetNodeInnovation(int inNodeId, int outNodeId)
    //{
    //    // If this is an input, output, or bias node there will be no incoming or outgoing node
    //    if (inNodeId != -1 && outNodeId != -1)
    //    {
    //        // Check if this node innovation already exists and return the node id if it does
    //        foreach (Innovation innovation in ExistingInnovations.Values)
    //        {
    //            if (innovation.IsNodeInnovation && innovation.InNodeId == inNodeId && innovation.OutNodeId == outNodeId)
    //            {
    //                return innovation.NodeId;
    //            }
    //        }
    //    }

    //    // If the node is an input, output, or bias node, or does not exist, create a new node innovation
    //    Innovation newInnovation = new Innovation(currentInnovation++, true, currentNodeId, inNodeId, outNodeId);
    //    ExistingInnovations.Add(newInnovation.Id, newInnovation);
    //    return currentNodeId++;
    //}

    public override string ToString()
    {
        string innovations = "";
        foreach(Innovation innovation in ExistingInnovations)
        {
            innovations += innovation.ToString() + "\n";
        }

        return innovations;
    }

}
