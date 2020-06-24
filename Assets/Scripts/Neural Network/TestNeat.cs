using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestNeat: MonoBehaviour
{
    // The number of inputs in the genomes
    public int NumInputs = 4;

    // The number of outputs in the genomes
    public int NumOutputs = 3;

    // Whether the genomes have bias nodes or not
    public bool HasBiasNode = true;

    // The dropdown that is used to select which genome we are adding nodes and connections to,
    // as well as the first genome in the crossover
    public TMP_Dropdown firstGenomeDropdown;

    // The dropdown that determines the second genome in the crossover
    public TMP_Dropdown secondGenomeDropdown;

    // The innovation generator for this test
    private InnovationGenerator innovationGenerator;

    // All the genomes that have been created with their name as key
    private Dictionary<string, Genome> genomes;

    // The currently displayed graph
    private GraphDisplay graphDisplay;

    // The id number of the current genome
    private int currentGenomeId;

    // Start is called before the first frame update
    void Start()
    {
        innovationGenerator = new InnovationGenerator();
        genomes = new Dictionary<string, Genome>();
        currentGenomeId = 0;

        firstGenomeDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(firstGenomeDropdown);
        });

        //Genome parent = new Genome(0, 4, 3, true, innovationGenerator);

        //for (int i = 0; i < 50; i++)
        //{
        //    parent.AddConnection(1.0f, 0.1f, 1.0f, innovationGenerator, 20, 20);
        //    Debug.Log(innovationGenerator.CurrentInnovation);
        //}
        //PrintGenome("ParentMutated", parent);
        //GraphDisplay graphDisplay = new GraphDisplay(parent);
    }

    // Called when the Add Genome button is pressed
    public void CreateNewGenome()
    {
        Genome genome = new Genome(currentGenomeId++, NumInputs, NumOutputs, HasBiasNode, innovationGenerator);
        CreateNewGraphDisplay(genome);
    }

    // Called when the Add Connection button is pressed
    public void CreateNewConnection()
    {
        string genomeName = firstGenomeDropdown.options[firstGenomeDropdown.value].text;
        genomes[genomeName].AddConnection(1.0f, 0.1f, 1.0f, innovationGenerator, 20, 20);
        graphDisplay.UpdateDisplay();
    }

    // Called when the Add Node button is pressed
    public void CreateNewNode()
    {
        string genomeName = firstGenomeDropdown.options[firstGenomeDropdown.value].text;
        genomes[genomeName].AddNode(1.0f, innovationGenerator, 20);
        graphDisplay.UpdateDisplay();
    }

    // Called when the Crossover button is pressed
    public void Crossover()
    {
        Genome genome = Genome.Crossover(currentGenomeId++, genomes[firstGenomeDropdown.options[firstGenomeDropdown.value].text], 
                         genomes[secondGenomeDropdown.options[firstGenomeDropdown.value].text]);
        CreateNewGraphDisplay(genome);
    }

    
    // Update the dropdown menus to include all genomes that have been created
    private void UpdateDropdowns(string genomeName)
    {
        // Put the new genome name into a list
        List<string> option = new List<string>();
        option.Add(genomeName);

        // Add the genome name to options
        firstGenomeDropdown.AddOptions(option);
        secondGenomeDropdown.AddOptions(option);
    }

    // Print the list of innovations in innovationGenerator
    public void PrintInnovations()
    {
        Debug.Log("Innovations: \n" + innovationGenerator.ToString());
    }

    // Print the genome in the first dropdown menu
    public void PrintGenome()
    {
        string genomeName = firstGenomeDropdown.options[firstGenomeDropdown.value].text;
        Debug.Log(genomeName + ":\n" + genomes[genomeName].ToString());
    }

    // Displays a new graph display and deletes the current
    private void CreateNewGraphDisplay(Genome genome)
    {
        if (graphDisplay != null) Destroy(graphDisplay.Graph);
        graphDisplay = new GraphDisplay(genome);

        if (!genomes.ContainsKey(graphDisplay.Graph.name))
        {
            genomes.Add(graphDisplay.Graph.name, genome);
            UpdateDropdowns(graphDisplay.Graph.name);
        }

        firstGenomeDropdown.value = firstGenomeDropdown.options.FindIndex((i) => { return i.text.Equals(graphDisplay.Graph.name); });
    }

    // Create new graph display if the dropdown changes
    void DropdownValueChanged(TMP_Dropdown change)
    {
        CreateNewGraphDisplay(genomes[firstGenomeDropdown.options[change.value].text]);
    }

}
