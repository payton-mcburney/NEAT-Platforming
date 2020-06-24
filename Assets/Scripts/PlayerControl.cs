using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Player))]
public class PlayerControl : MonoBehaviour
{
    private Player m_Character;
    private bool m_Jump;
    public Genome Genome { get; set; }
    //private NeuralNetwork neuralNetwork;
    public bool userHasControl = false;
    private float[] genomeOutputs;

    private void Awake()
    {
        m_Character = GetComponent<Player>();
        //int[] layers = { 4, 4, 3 };
        //neuralNetwork = new NeuralNetwork(layers);
    }

    public void AssociateGenome(Genome genome)
    {
        Genome = genome;
        genomeOutputs = new float[genome.NumOutputs];
    }

    private void Update()
    {
        if (!m_Jump)
        {
            // Read the jump input in Update so button presses aren't missed.
            if (userHasControl)
            {
                // Read jump input from user.
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }
            else
            {
                // Read jump input from neural network.
                m_Jump = genomeOutputs[1] > 0 ? true : false;
            }
        }
    }


    private void FixedUpdate()
    {
        // Controls whether the user or neural network is controlling the player.
        //if (Input.GetKeyDown(KeyCode.N))
        //{
        //    int[] layers = { 4, 4, 3 };
        //    neuralNetwork = new NeuralNetwork(layers);
        //    userHasControl = false;
        //}
        //else if (Input.GetKeyDown(KeyCode.U))
        //{
        //    userHasControl = true;
        //}

        bool crouch; float h;
        if (userHasControl)
        {
            // Read the inputs from user.
            h = CrossPlatformInputManager.GetAxis("Horizontal");
            crouch = Input.GetKey(KeyCode.LeftControl);
        }
        else
        {
            // Read the outputs from neural network.
            GetNeuralNetworkOutputs();
            h = genomeOutputs[0];
            crouch = genomeOutputs[2] > 0 ? true : false ;
        }

        // Pass all parameters to the character control script.
        m_Character.Move(h, crouch, m_Jump);
        m_Jump = false;
    }


    private float[] GetNeuralNetworkInputs()
    {
        
        float[] inputs = new float[4];
        inputs[0] = Physics.CheckSphere(new Vector2(transform.position.x + 0.25f, transform.position.y), 0.1f) ? 1f : -1f;
        inputs[1] = Physics.CheckSphere(new Vector2(transform.position.x, transform.position.y - 0.25f), 0.1f) ? 1f : -1f;
        inputs[2] = Physics.CheckSphere(new Vector2(transform.position.x - 0.25f, transform.position.y), 0.1f) ? 1f : -1f;
        inputs[3] = Physics.CheckSphere(new Vector2(transform.position.x, transform.position.y + 0.25f), 0.1f) ? 1f : -1f;
        return inputs;
    }


    private void GetNeuralNetworkOutputs()
    {
        float[] inputs = GetNeuralNetworkInputs();
        genomeOutputs = Genome.Update(inputs);
    }

}
