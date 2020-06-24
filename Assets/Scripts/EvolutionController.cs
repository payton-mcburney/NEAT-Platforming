using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EvolutionController : MonoBehaviour
{
    public int numPerGeneration;
    public float timeLimit;
    public TMP_Dropdown genomeDropdown;

    private List<GameObject> players;
    private List<PlayerControl> playerControls;
    private List<float> startTimes;
    private int generation;
    private int currentGenomeId;
    private InnovationGenerator innovationGenerator;
    GameObject playerSpawner;

    private bool showNeuralNetwork = false;
    private Dictionary<string, Genome> genomes;
    private GraphDisplay graphDisplay;
    private GameObject playerFollowing;
    private CameraFollow cameraFollow;

    private GameObject bestPlayer;

    // Start is called before the first frame update
    void Awake()
    {
        players = new List<GameObject>();
        playerControls = new List<PlayerControl>();
        startTimes = new List<float>();
        generation = 1;
        currentGenomeId = 1;
        genomes = new Dictionary<string, Genome>();
        cameraFollow = GetComponent<CameraFollow>();
        innovationGenerator = new InnovationGenerator();
        GameObject playerPrefab = (GameObject) Resources.Load("Player");
        playerSpawner = GameObject.Find("Player Spawner");

        genomeDropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(genomeDropdown);
        });

        for (int i = 0; i < numPerGeneration; i++)
        {
            players.Add(GameObject.Instantiate(playerPrefab));
            players[i].transform.position = playerSpawner.transform.position;
            playerControls.Add(players[i].GetComponent<PlayerControl>());
            Genome newGenome = new Genome(currentGenomeId++, 4, 3, true, innovationGenerator);
            playerControls[i].AssociateGenome(newGenome);
            genomes.Add("Genome " + newGenome.Id, newGenome);
            UpdateDropdowns("Genome " + newGenome.Id);
            startTimes.Add(Time.realtimeSinceStartup);
        }

        bestPlayer = players[0];
    }

    // Update is called once per frame
    void Update()
    {
        // Make all but the focused player semi-transparent
        foreach (GameObject player in players)
        {
            Color color = player.GetComponent<SpriteRenderer>().material.color;
            color.a = 0.3f;
            player.GetComponent<SpriteRenderer>().material.color = color;
            player.GetComponent<SpriteRenderer>().sortingOrder = -1;
        }

        UpdateFitnesses();
        
        if (IsTrainingDone())
        {
            CreateNewGeneration();
        }

        GameObject text = GameObject.Find("Text");
        text.GetComponent<Text>().text = "Generation: " + generation;
    }


    private void UpdateFitnesses()
    {
        // If the time has run out set the fitness of the neural network to the current y position of the player
        for (int i = 0; i < numPerGeneration; i++)
        {
            if (startTimes[i] != -1f && Time.realtimeSinceStartup - startTimes[i] >= timeLimit)
            {
                startTimes[i] = -1f;
                playerControls[i].Genome.Fitness = players[i].transform.position.x;
                if (players[i].transform.position.y < -5) playerControls[i].Genome.Fitness = 0.0f;
                playerControls[i].Genome.AdjustFitnesses();
            }
        }
    }

    //Checks if this generation is done
    private bool IsTrainingDone()
    {
        bool isTrainingDone = true; // is training done this generation

        for (int i = 0; i < numPerGeneration && isTrainingDone; i++)
        {
            if (startTimes[i] != -1f)
            {
                isTrainingDone = false;
            }
        }

        return isTrainingDone;
    }

    private void CreateNewGeneration()
    {
        SortByFitness();
        bestPlayer = players[0];
        Debug.Log("Best Player: " + playerControls[0].Genome.Fitness);
        Debug.Log("Worst Player: " + playerControls[playerControls.Count - 1].Genome.Fitness);

        // Delete the poorest performing half of the previous generation (assumes numPerGeneration is even)
        for (int i = numPerGeneration - 1; i >= numPerGeneration / 2; i--)
        {
            GameObject.Destroy(players[i]);
            players.RemoveAt(i);
            playerControls.RemoveAt(i);
        }

        // Clone the first half of the previous generation and mutate the original
        for(int i = 0; i < numPerGeneration / 2; i++)
        {
            GameObject newPlayer = GameObject.Instantiate(players[i]);
            players.Add(newPlayer);
            PlayerControl newPlayerControl = newPlayer.GetComponent<PlayerControl>();
            newPlayerControl.AssociateGenome(playerControls[i].Genome);
            playerControls.Add(newPlayerControl);
            playerControls[i].Genome.AddConnection(0.7f, 0.1f, 1.0f, innovationGenerator, 20, 20);
            playerControls[i].Genome.AddNode(0.7f, innovationGenerator, 20);
        }

        // Reset startTime and player positions
        for(int i = 0; i < numPerGeneration; i++)
        {
            startTimes[i] = Time.realtimeSinceStartup;
            players[i].transform.position = playerSpawner.transform.position;
            players[i].transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        generation++;
    }

    // Sort the previous generation by fitness
    private void SortByFitness()
    {
        for (int i = 0; i < numPerGeneration; i++)
        {
            for (int j = i; j < numPerGeneration; j++)
            {
                if (playerControls[j].Genome.CompareTo(playerControls[i].Genome) > 0)
                {
                    // Swap players[i] and players[j]
                    GameObject tempPlayer = players[i];
                    players[i] = players[j];
                    players[j] = tempPlayer;

                    // Swap playerControllers[i] and playerControllers[j]
                    PlayerControl tempPlayerControl = playerControls[i];
                    playerControls[i] = playerControls[j];
                    playerControls[j] = tempPlayerControl;
                }
            }
        }
    }

    public void ToggleNeuralNetwork()
    {
        showNeuralNetwork = !showNeuralNetwork;
        if (showNeuralNetwork) CreateNewGraphDisplay(genomes[genomeDropdown.options[genomeDropdown.value].text]);
        else RemoveGraphDisplay();
    }

    // Create new graph display if the dropdown changes
    void DropdownValueChanged(TMP_Dropdown change)
    {
        CreateNewGraphDisplay(genomes[genomeDropdown.options[change.value].text]);
    }

    // Removes the previous graph display if there is one and creates a new graph display on screen
    private void CreateNewGraphDisplay(Genome genome)
    {
        RemoveGraphDisplay();
        graphDisplay = new GraphDisplay(genome);
        genomeDropdown.value = genomeDropdown.options.FindIndex((i) => { return i.text.Equals(graphDisplay.Graph.name); });
    }

    // Removes the current graph display
    private void RemoveGraphDisplay()
    {
        if (graphDisplay != null) Destroy(graphDisplay.Graph);
    }

    // Update the dropdown menus to include all genomes that have been created
    private void UpdateDropdowns(string genomeName)
    {
        // Put the new genome name into a list
        List<string> option = new List<string>();
        option.Add(genomeName);

        // Add the genome name to options
        genomeDropdown.AddOptions(option);
    }
}
