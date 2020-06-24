using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class contains all the methods and attributes for a neural network under NEAT.
/// </summary>
public class Genome
{
    /// <summary>
    /// The unique id of the genome.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// A list that stores all the nodes in the genome.
    /// </summary>
    public List<NodeGene> Nodes { get; }

    /// <summary>
    /// A list that stores all the connections in the genome.
    /// </summary>
    public List<ConnectionGene> Connections { get; }

    /// <summary>
    /// The raw fitness score of the genome; used to rank its performance.
    /// </summary>
    public float Fitness { get; set; }

    /// <summary>
    /// The genome's adjusted fitness after it has been placed in the appropriate species.
    /// </summary>
    public float AdjustedFitness { get; private set; }

    /// <summary>
    /// The number of input nodes for this genome.
    /// </summary>
    public int NumInputs { get; }

    /// <summary>
    /// The number of input nodes for this genome.
    /// </summary>
    public int NumOutputs { get; }

    /// <summary>
    /// The number of layers in this genome.
    /// </summary>
    public int NumLayers { get; private set; }

    /// <summary>
    /// The species this genome is in.
    /// </summary>
    public int Species { get; }



    /// <summary>
    /// Constructs a genome using given lists of nodes and connections.
    /// </summary>
    /// <param name="id">The unique id of the genome.</param>
    /// <param name="nodes">The list of nodes in the genome.</param>
    /// <param name="connections">The list of connections in the genome.</param>
    public Genome(int id, List<NodeGene> nodes, List<ConnectionGene> connections)
    {
        Id = id;
        Nodes = nodes;
        Connections = connections;
    }



    /// <summary>
    /// Constructs a genome given a number of inputs and outputs and whether or not there is a bias node.
    /// </summary>
    /// <param name="id">The unique id of the genome.</param>
    /// <param name="numInputs">The number of inputs in the genome.</param>
    /// <param name="numOutputs">The number of outputs in the genome.</param>
    /// <param name="hasBiasNode">Whether or not the genome will have a bias node.</param>
    /// <param name="innovationGenerator">The innovation generator that creates unique innovation numbers and node ids.</param>
    public Genome(int id, int numInputs, int numOutputs, bool hasBiasNode, InnovationGenerator innovationGenerator)
    {
        // Initialize the hash tables
        Nodes = new List<NodeGene>();
        Connections = new List<ConnectionGene>();

        // Initialize id
        Id = id;

        // Initialize starting nodes id
        int startingNodesId = 0;

        // Generate and add input nodes
        NumInputs = numInputs;
        for (int i = 0; i < numInputs; i++)
        {
            Nodes.Add(new NodeGene(NodeGene.TYPE.INPUT, startingNodesId++));
        }

        // Create the bias node if it exists
        if (hasBiasNode)
        {
            Nodes.Add(new NodeGene(NodeGene.TYPE.BIAS, startingNodesId++));
        }

        // Generate and add output nodes
        NumOutputs = numOutputs;
        for(int i = 0; i < numOutputs; i++)
        {
            Nodes.Add(new NodeGene(NodeGene.TYPE.OUTPUT, startingNodesId++));
        }

        // Add starting nodes innovations if they have not been created previously
        if (innovationGenerator.CheckInnovation(-1, -1, Innovation.TYPE.NEW_NODE) < 0)
        {
            innovationGenerator.CreateStartingNodeInnovations(startingNodesId);
        }

        // Update the locations of the output nodes
        UpdateOutputLocations();
    }



    public void MutateWeights(float mutationRate, float chanceOfNewWeight, float maxWeight, float maxPerturbation)
    {
        // Return based on the mutation rate
        if (Random.Range(0.0f, 1.0f) < mutationRate)
        {
            return;
        }

        // Mutate each weight by either setting a new random weight or perturbing the weight by a small amount
        foreach (ConnectionGene connection in Connections)
        {
            if(Random.Range(0.0f, 1.0f) < chanceOfNewWeight)
            {
                connection.Weight = Random.Range(-maxWeight, maxWeight);   
            }
            else
            {
                connection.Weight += Random.Range(-maxPerturbation, maxPerturbation);
            }
        }
    }



    /// <summary>
    /// Adds a new random connection to the genome if possible.
    /// </summary>
    /// <param name="mutationRate">The chance that this mutation occurs.</param>
    /// <param name="chanceOfLooped">The chance that the connection will be from a node to itself.</param>
    /// <param name="weightLimit">The weight will be a random float between positive and negative weightLimit.</param>
    /// <param name="innovationGenerator">The generator that tracks innovation numbers.</param>
    /// <param name="numTrysToFindConnection">The number of attempts made to find a connection to create.</param>
    /// <param name="numTrysToFindLoop">The number of attempts made to find a possible connection from a node to itself.</param>
    public void AddConnection(float mutationRate, 
                              float chanceOfLooped, 
                              float weightLimit,
                              InnovationGenerator innovationGenerator, 
                              int numTrysToFindLoop,
                              int numTrysToFindConnection)
    {
        // Return depending on the mutation rate
        if (Random.Range(0.0f, 1.0f) > mutationRate) return;

        // Initialize nodes we will use to create the connection
        NodeGene inNode = null;
        NodeGene outNode = null;

        // Whether or not the connection is recurrent
        bool recurrent = false;

        // Check to see if connection should be made from a node to itself
        //int[] nodeKeys = new int[Nodes.Count];
        //Nodes.Keys.CopyTo(nodeKeys, 0);
        if (Random.Range(0.0f, 1.0f) < chanceOfLooped)
        {
            // Check numTrysToFindLoop times to find node that is not bias
            // or input and does not have recurrent connection
            while (numTrysToFindLoop-- > 0)
            {
                
                NodeGene node = Nodes[Random.Range(0, Nodes.Count)];
                if (node.Type != NodeGene.TYPE.INPUT && node.Type != NodeGene.TYPE.BIAS && !node.Recurrent)
                {
                    node.Recurrent = true;
                    inNode = outNode = node;
                    recurrent = true;
                    numTrysToFindLoop = 0;
                }
            }
        }
        else
        {
            // Check numTrysToFindConnection times to find two nodes with
            // no existing connection between them
            while (numTrysToFindConnection-- > 0)
            {
                // Get two nodes randomly
                int inNodeIndex = Random.Range(0, Nodes.Count);
                inNode = Nodes[inNodeIndex];
                // Ensure that outNode is not the same node as inNode
                outNode = Nodes[(inNodeIndex + Random.Range(1, Nodes.Count)) % Nodes.Count];

                // The second node cannot be an input or bias
                if (outNode.Type == NodeGene.TYPE.INPUT || outNode.Type == NodeGene.TYPE.BIAS)
                {
                    inNode = null;
                    outNode = null;
                    continue;
                }

                // If this connection does not already exist, we have found our new connection
                if (!IsDuplicateConnection(inNode, outNode))
                {
                    numTrysToFindConnection = 0;
                }
                else
                {
                    inNode = null;
                    outNode = null;
                }
            }
        }
        
        // If we did not find two nodes that can have a connection, return
        if (inNode == null || outNode == null) return;

        // Check if this innovation already exists
        int id = innovationGenerator.CheckInnovation(inNode.Id, outNode.Id, Innovation.TYPE.NEW_CONNECTION);

        // Check the location of the nodes to check if the connection is recurrent
        if (outNode.Location <= inNode.Location)
        {
            recurrent = true;
        }

        // Get random weight
        float weight = Random.Range(-weightLimit, weightLimit);

        // Create a new innovation if it does not exist and get its id
        if (id < 0)
        {
            id = innovationGenerator.CurrentInnovation;
            innovationGenerator.CreateNewInnovation(inNode.Id, outNode.Id, Innovation.TYPE.NEW_CONNECTION);
        }

        // Add this connection to the existing connections
        ConnectionGene newConnection = new ConnectionGene(inNode, outNode, weight, id, recurrent);
        Connections.Add(newConnection);

        // If this new connection is not recurrent, we may need to update node locations
        if (!newConnection.Recurrent)
        {
            // Add outNode to inNode's connections
            inNode.AddConnectedNode(outNode);

            // Update node locations
            int newNodeLocation = inNode.Location + 1;
            if (outNode.Location < newNodeLocation)
            {
                outNode.Location = newNodeLocation;
                outNode.UpdateLocations();
            }
            UpdateOutputLocations();
        }

        //// Print for testing
        //string connectedNodesAsString = "All nodes:\n";
        //foreach (NodeGene node in Nodes.Values)
        //{
        //    connectedNodesAsString += node.ToString() + ":\n";
        //    foreach (NodeGene connectedNode in node.ConnectedNodes.Values)
        //    {
        //        connectedNodesAsString += connectedNode.Id + " " + connectedNode.Location + "\n";
        //    }
            
        //}
        //Debug.Log(connectedNodesAsString);

        // ***
        // Old implementation
        // ***
        // Pick two nodes randomly from existing nodes
        //int[] keys = new int[Nodes.Count];
        //Nodes.Keys.CopyTo(keys, 0);
        //int node1Key = keys[Random.Range(0, keys.Length)];
        //inNode = (NodeGene) Nodes[node1Key];
        //outNode = (NodeGene) Nodes[(node1Key + Random.Range(1, keys.Length)) % keys.Length]; // ensures that this is not the same node as inNode

        // Check if outNode is in a layer before inNode
        //bool reversed = false;
        //if (inNode.Type == NodeGene.TYPE.HIDDEN && outNode.Type == NodeGene.TYPE.INPUT)
        //{
        //    reversed = true;
        //}
        //else if (inNode.Type == NodeGene.TYPE.OUTPUT && outNode.Type == NodeGene.TYPE.HIDDEN)
        //{
        //    reversed = true;
        //}
        //else if (inNode.Type == NodeGene.TYPE.OUTPUT && (outNode.Type == NodeGene.TYPE.INPUT || outNode.Type == NodeGene.TYPE.BIAS))
        //{
        //    reversed = true;
        //}

        // Make sure the connection does not already exist
        //bool connectionExists = false;
        //foreach (ConnectionGene gene in Connections.Values)
        //{
        //    if (gene.InNode.Equals(inNode) && gene.OutNode.Equals(outNode))
        //    {
        //        connectionExists = true;
        //        break;
        //    }
        //    else if (gene.InNode.Equals(outNode) && gene.OutNode.Equals(inNode))
        //    {
        //        connectionExists = true;
        //        break;
        //    }
        //}

        // Make sure the connection is not between two nodes in the input layer or two in output layer
        //bool connectionImpossible = false;
        //if ((inNode.Type == NodeGene.TYPE.INPUT || inNode.Type == NodeGene.TYPE.BIAS) && (outNode.Type == NodeGene.TYPE.INPUT || outNode.Type == NodeGene.TYPE.BIAS))
        //{
        //    connectionImpossible = true;
        //}
        //else if (inNode.Type == NodeGene.TYPE.OUTPUT && outNode.Type == NodeGene.TYPE.OUTPUT)
        //{
        //    connectionImpossible = true;
        //}

        // If the connection already exists or is not possible, try again, otherwise add this new connection
        //if (connectionExists || connectionImpossible)
        //{
        //    AddConnection(innovationGenerator);
        //}
        //else
        //{
        //    ConnectionGene newConnection;
        //    if (reversed) {
        //        newConnection = new ConnectionGene(outNode, inNode, weight, true, innovationGenerator.GetConnectionInnovation(outNode.Id, inNode.Id));
        //    }
        //    else
        //    {
        //        newConnection = new ConnectionGene(inNode, outNode, weight, true, innovationGenerator.GetConnectionInnovation(inNode.Id, outNode.Id));
        //    }
        //    Connections.Add(newConnection.Innovation, newConnection);
        //}
    }



    /// <summary>
    /// Checks if a connection already exists between two nodes.
    /// </summary>
    /// <param name="inNode">The node at the beginning of the connection.</param>
    /// <param name="outNode">The node at the end of the connection.</param>
    /// <returns>Returns true if the connection exists, false otherwise.</returns>
    private bool IsDuplicateConnection(NodeGene inNode, NodeGene outNode)
    {
        // Iterate through every connection gene and check if this connection already exists
        bool connectionExists = false;
        foreach (ConnectionGene connection in Connections)
        {
            if (connection.InNode.Equals(inNode) && connection.OutNode.Equals(outNode))
            {
                connectionExists = true;
                break;
            }
        }

        return connectionExists;
    }



    //TODO: Investigate whether recurrent connections should be able to get new nodes
    /// <summary>
    /// Adds a node in a random connection if possible, disables that connection, and creates two new connections.
    /// </summary>
    /// <param name="mutationRate">The chance that this mutation occurs.</param>
    /// <param name="innovationGenerator">The generator that tracks innovation numbers.</param>
    /// <param name="numTrysToFindOldConnection">The number of attempts that will be made to find an old connection for the new node.</param>
    public void AddNode(float mutationRate,
                        InnovationGenerator innovationGenerator,
                        int numTrysToFindOldConnection)
    {
        // Return if there are no connections to add nodes to
        if (Connections.Count == 0) return;

        // Return depending on the mutation rate
        if (Random.Range(0.0f, 1.0f) > mutationRate) return;

        // Set to true if a valid connection is found for the new node
        bool connectionFound = false;

        // The innovation of the connection we are replacing
        ConnectionGene oldConnection = null;

        // If the genome has fewer than 5 hidden nodes, we select a connection
        // with a bias towards older nodes, otherwise we select without bias
        int sizeThreshold = NumInputs + NumOutputs + 5;
        if (Nodes.Count < sizeThreshold)
        {
            while(numTrysToFindOldConnection-- > 0)
            {
                // Select a random connection with bias towards earlier connections
                oldConnection = Connections[Random.Range(0, Connections.Count - 1 - (int)Mathf.Sqrt(Connections.Count))];

                // Make sure this connection is enabled, not recurrent, and not originating
                // from a bias node
                if (oldConnection.Enabled &&
                    !oldConnection.Recurrent &&
                    oldConnection.InNode.Type != NodeGene.TYPE.BIAS)
                {
                    connectionFound = true;
                    numTrysToFindOldConnection = 0;
                }
            }

            // Return if no old connection was found
            if (!connectionFound)
            {
                return;
            }
        }
        else
        {
            while (!connectionFound)
            {
                // This genome is big enough to select a connection uniformly randomly
                oldConnection = Connections[Random.Range(0, Connections.Count)];

                // Make sure this connection is enabled, not recurrent, and not originating
                // from a bias node
                if (oldConnection.Enabled &&
                    !oldConnection.Recurrent &&
                    oldConnection.InNode.Type != NodeGene.TYPE.BIAS)
                {
                    connectionFound = true;
                }
            }
        }

        // Disable the old connection
        oldConnection.Enabled = false;

        // Get the in and out nodes for this connection
        NodeGene inNode = oldConnection.InNode;
        NodeGene outNode = oldConnection.OutNode;

        // Check if this innovation has occured for another genome in the population
        int id = innovationGenerator.CheckInnovation(inNode.Id, outNode.Id, Innovation.TYPE.NEW_NODE);
        
        // Since the connection can be re-enabled later, it is possible that multiple nodes will be created for the same
        // connection, in which case we must assign it a new id
        if (id >= 0 && AlreadyHasNodeId(id))
        {
            id = -1;
        }

        // We know that this innovation is new if id = -1
        if (id < 0)
        {
            // --------- New Node ----------
            // Create new node and add to list of nodes
            NodeGene newNode = new NodeGene(NodeGene.TYPE.HIDDEN, innovationGenerator.CurrentNodeId);
            Nodes.Add(newNode);
            // Create new node innovation
            innovationGenerator.CreateNewInnovation(inNode.Id, outNode.Id, Innovation.TYPE.NEW_NODE);

            // --------- First connection ----------
            // Create new connection and add to list of connections
            Connections.Add(new ConnectionGene(inNode, newNode, 1.0f, innovationGenerator.CurrentInnovation, false));

            // Create new connection innovation
            innovationGenerator.CreateNewInnovation(inNode.Id, newNode.Id, Innovation.TYPE.NEW_CONNECTION);

            // --------- Second connection ----------
            // Create new connection and add to list of connections
            Connections.Add(new ConnectionGene(newNode, outNode, oldConnection.Weight, innovationGenerator.CurrentInnovation, false));

            // Create new connection innovation
            innovationGenerator.CreateNewInnovation(newNode.Id, outNode.Id, Innovation.TYPE.NEW_CONNECTION);

            // --------- Update locations ----------
            // Add newNode to inNode's connections
            inNode.AddConnectedNode(newNode);

            // Add outNode to newNode's connections
            newNode.AddConnectedNode(outNode);

            // Update location of newNode
            newNode.Location = inNode.Location + 1;

            // Update location of outNode
            int newNodeLocation = newNode.Location + 1;
            if (outNode.Location < newNodeLocation)
            {
                outNode.Location = newNodeLocation;
                outNode.UpdateLocations();
            }
            UpdateOutputLocations();
        }
        else
        {
            // Get the innovations for the already existing connections
            int innovation1 = innovationGenerator.CheckInnovation(inNode.Id, id, Innovation.TYPE.NEW_CONNECTION);
            int innovation2 = innovationGenerator.CheckInnovation(id, outNode.Id, Innovation.TYPE.NEW_CONNECTION);

            // TODO: This happened, need to figure out why and fix, probably nodes not being assigned correct values (again)
            // This should not happen, as the connections should exist, but if not diplay an error and return
            if (innovation1 < 0 || innovation2 < 0)
            {
                Debug.LogError("Error in Genome.AddNode()!");
                return;
            }

            // Create node and add to node list
            NodeGene newNode = new NodeGene(NodeGene.TYPE.HIDDEN, id);
            Nodes.Add(newNode);

            // Create connections and add them to connection list
            Connections.Add(new ConnectionGene(inNode, newNode, 1.0f, innovation1, false));
            Connections.Add(new ConnectionGene(newNode, outNode, oldConnection.Weight, innovation2, false));

            // --------- Update locations ----------
            // Add newNode to inNode's connections
            inNode.AddConnectedNode(newNode);

            // Add outNode to newNode's connections
            newNode.AddConnectedNode(outNode);

            // Update location of newNode
            newNode.Location = inNode.Location + 1;

            // Update location of outNode
            int newNodeLocation = newNode.Location + 1;
            if (outNode.Location < newNodeLocation)
            {
                outNode.Location = newNodeLocation;
                outNode.UpdateLocations();
            }
            UpdateOutputLocations();
        }

        // ***
        // Old implementation
        // ***
        //// If there are no connections return
        //if (Connections.Count == 0) return;

        //// Get a random connection gene by grabbing a random key from the hashtable
        //int[] keys = new int[Connections.Count];
        //Connections.Keys.CopyTo(keys, 0);
        //ConnectionGene gene = (ConnectionGene)Connections[keys[Random.Range(0, keys.Length)]];

        //// Disable that connection
        //gene.Enabled = false;

        //// Create a new node and connect it to the inNode and outNode of the connection gene
        //NodeGene newNode = new NodeGene(NodeGene.TYPE.HIDDEN, innovationGenerator.GetNodeInnovation(gene.InNode.Id, gene.OutNode.Id));
        //ConnectionGene inToNew = new ConnectionGene(gene.InNode, newNode, 1f, innovationGenerator.GetConnectionInnovation(gene.InNode.Id, newNode.Id));
        //ConnectionGene newToOut = new ConnectionGene(newNode, gene.OutNode, gene.Weight, innovationGenerator.GetConnectionInnovation(newNode.Id, gene.OutNode.Id));
        //Debug.Log(gene.InNode.Id + " " + gene.OutNode.Id + " " + newNode.Id);
        //// Add the new node and connection genes to lists
        //Nodes.Add(newNode.Id, newNode);
        //Connections.Add(inToNew.Innovation, inToNew);
        //Connections.Add(newToOut.Innovation, newToOut);
    }



    /// <summary>
    /// Checks if a node with a given id already exists in this genome.
    /// </summary>
    /// <param name="nodeId">The node id we are searching for.</param>
    /// <returns>Returns true if a node already exists with the given id, false otherwise.</returns>
    private bool AlreadyHasNodeId(int nodeId)
    {
        // Iterate through every node gene and check if it has the id we are looking for
        bool nodeIdExists = false;
        foreach (NodeGene node in Nodes)
        {
            if (node.Id == nodeId)
            {
                nodeIdExists = true;
                break;
            }
        }

        return nodeIdExists;
    }



    private void UpdateOutputLocations()
    {
        // Get the max location out of all non output nodes
        int maxLocation = 0;
        int biasNodeCount = 0;
        foreach (NodeGene node in Nodes)
        {
            if (node.Type != NodeGene.TYPE.OUTPUT && node.Location > maxLocation) maxLocation = node.Location;
            if (node.Type == NodeGene.TYPE.BIAS) biasNodeCount++;
        }

        // Number of layers is maxLocation + 2 because indexing starts at 0 and the output layer has not been counted
        NumLayers = maxLocation + 2;

        // Set the location of all output nodes to one more than max
        for (int i = NumInputs + biasNodeCount; i < NumInputs + NumOutputs + biasNodeCount; i++)
        {
            Nodes[i].Location = NumLayers - 1;
        }
    }



    //TODO: handle fitnesses and ties instead of assuming parent 1 is more fit
    /// <summary>
    /// Creates a new genome by comparing each gene of the parent genomes.
    /// </summary>
    /// <param name="id">The id to assign to the new genome.</param>
    /// <param name="parent1">The parent genome that is more fit.</param>
    /// <param name="parent2">The parent genome that is less fit.</param>
    public static Genome Crossover(int id, Genome parent1, Genome parent2)
    {
        // The best parent is the one with the highest fitness. If they have the
        // same fitness, then choose the smaller parent. If they have the same size
        // and fitness, choose one at random
        //bool parent1Best;
        //if (parent1.Fitness == parent2.Fitness)
        //{
        //    if (parent1.numg)
        //}
        return parent1;
        // ***
        // Old implementation
        // ***

        //Hashtable childNodes = new Hashtable();
        //Hashtable childGenes = new Hashtable();

        //// The child inherits all nodes from the more fit parent
        //foreach (NodeGene parent1Node in parent1.Nodes)
        //{
        //    NodeGene childNode = NodeGene.DeepClone(parent1Node);
        //    childNodes.Add(childNode.Id, childNode);
        //}

        //// Pick which genes from the parents to pass onto child
        //foreach (ConnectionGene parent1Gene in parent1.Connections)
        //{
        //    ConnectionGene childGene;

        //    if (parent2.Connections.ContainsKey(parent1Gene.Innovation))
        //    {
        //        // Pick which parent gene to use randomly for matching genes
        //        if (Random.value > 0.5f)
        //        {
        //            childGene = ConnectionGene.DeepClone(parent1Gene);
                    
        //        }
        //        else
        //        {
        //            childGene = ConnectionGene.DeepClone(((ConnectionGene) parent2.Connections[parent1Gene.Innovation]));
        //        }
        //    }
        //    else
        //    {
        //        // Always include the disjoint or excess genes of the more fit parent
        //        childGene = ConnectionGene.DeepClone(parent1Gene);
        //    }

        //    childGenes.Add(childGene.Innovation, childGene);
        //}

        //return new Genome(id, childNodes, childGenes);
    }



    public float[] Update(float[] inputs)
    {
        // Create the array that will store the outputs for this genome
        float[] outputs = new float[NumOutputs];

        // Sort the nodes by layer
        SortNodeGenes();

        // Set the inputs of input nodes to the values in inputs array
        int nodeIndex = 0;
        while (nodeIndex < NumInputs)
        {
            Nodes[nodeIndex].Output = inputs[nodeIndex];
            nodeIndex++;
        }

        // Set the bias node's input to 1
        Nodes[nodeIndex].Output = 1;
        nodeIndex++;

        // Go through the rest of the network one node at a time
        int outputIndex = 0;
        while (nodeIndex < Nodes.Count)
        {
            NodeGene currNode = Nodes[nodeIndex];

            // This will hold the sum of all inputs * weights into this node
            float sum = 0.0f;
            
            // Go through each connection into the current node and add its output * weight to sum
            foreach (ConnectionGene connection in Connections)
            {
                if (connection.Enabled && connection.OutNode.Equals(currNode))
                {
                    sum += connection.InNode.Output * connection.Weight;
                }
            }

            // Pass sum through the activation function and assign it to this node's output
            currNode.Output = Logistic(sum);

            // If this is an output node, then add its output to our output array
            if (currNode.Type == NodeGene.TYPE.OUTPUT)
            {
                outputs[outputIndex] = currNode.Output;
                outputIndex++;
            }

            nodeIndex++;
        }

        //string outputsString = "";
        //for (int i = 0; i < outputs.Length; i++)
        //{
        //    outputsString += outputs[i] + " ";
        //}
        //Debug.Log(outputsString);

        return outputs;
    }


    /// <summary>
    /// Sorts the node genes by location (layer), then by id
    /// </summary>
    public void SortNodeGenes()
    {
        Nodes.Sort(delegate (NodeGene a, NodeGene b)
        {
            if (a.Location.CompareTo(b.Location) == 0) return a.Id.CompareTo(b.Id);
            else return a.Location.CompareTo(b.Location);
        });
    }



    /// <summary>
    /// Calculates the output of the Logistic curve (normalized between -1 and 1 with slope 1 at the origin).
    /// </summary>
    /// <param name="n">The input to the Logistic function.</param>
    /// <returns>The output of the Logistic function (2 / (1 + e^0.5(-n)) - 1).</returns>
    public float Logistic(float n)
    {
        return 2.0f / (1.0f + (float) Mathf.Exp(-0.5f * n)) - 1.0f;
    }



    public void AdjustFitnesses()
    {
        AdjustedFitness = Fitness;
    }



    public int CompareTo(Genome other)
    {
        if (other == null) return 1;

        if (AdjustedFitness > other.AdjustedFitness)
            return 1;
        else if (AdjustedFitness < other.AdjustedFitness)
            return -1;
        else
            return 0;
    }

    //For testing purposes
    public override string ToString()
    {
        string genomeString = "Nodes:\n";
        foreach (var node in Nodes)
        {
            genomeString += node + "\n";
        }

        genomeString += "\nConnections:\n";
        foreach (var connection in Connections)
        {
            genomeString += connection + "\n";
        }

        return genomeString;
    }



    // For testing purposes
    public void AddManualConnection(ConnectionGene connection)
    {
        Connections.Add(connection);
    }
}
