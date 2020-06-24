using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private LayerMask platformsLayerMask;
    private Rigidbody2D rigidBody2D;
    private BoxCollider2D boxCollider2D;
    private readonly float JUMP_VELOCITY = 5f;
    private readonly float MOVE_SPEED = 3f;
    private readonly float GROUND_ACCELERATION = 5f;
    private readonly float AIR_ACCELERATION = 2f;

    private NeuralNetwork neuralNetwork;
    private bool userHasControl;
    private bool isDead;

    private void Awake()
    {
        platformsLayerMask = LayerMask.GetMask("Platforms");
        rigidBody2D = transform.GetComponent<Rigidbody2D>();
        boxCollider2D = transform.GetComponent<BoxCollider2D>();
        userHasControl = false;
        int[] layers = { 4, 4, 3 };
        neuralNetwork = new NeuralNetwork(layers);
        isDead = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        // Controls whether the user or neural network is controlling the player
        if (Input.GetKeyDown(KeyCode.N))
        {
            userHasControl = false;
        }
        else if (Input.GetKeyDown(KeyCode.U))
        {
            userHasControl = true;
        }

        // Handles movement
        if (userHasControl)
        {
            HandleUserMovement();
        }
        else
        {
            HandleNeuralNetworkMovement();
        }
        
        // Reset position if falls below a certain point
        if (transform.position.y < -5f)
        {
            isDead = true;
        }

        if (isDead)
        {
            Freeze();
        }
    }

    private void Freeze()
    {
        rigidBody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void ResetPosition()
    {
        isDead = false;
        //rigidBody2D.constraints = RigidbodyConstraints2D.None;
        rigidBody2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        transform.position = new Vector2(0f, 0.5f);
        rigidBody2D.velocity = Vector2.zero;
    }

    private void HandleNeuralNetworkMovement()
    {
        float[] inputs = GetInputs();
        float[] outputs = neuralNetwork.FeedForward(inputs);

        if(outputs[0] < 0)
        {
            MoveRight();
        }
        if(outputs[1] < 0)
        {
            MoveLeft();
        }
        if(outputs[2] < 0)
        {
            Jump();
        }
    }

    private float[] GetInputs()
    {
        float[] inputs = new float[4];
        inputs[0] = Physics.CheckSphere(new Vector2(transform.position.x + 0.25f, transform.position.y), 0.1f) ? 1f : -1f;
        inputs[1] = Physics.CheckSphere(new Vector2(transform.position.x, transform.position.y - 0.25f), 0.1f) ? 1f : -1f;
        inputs[2] = Physics.CheckSphere(new Vector2(transform.position.x - 0.25f, transform.position.y), 0.1f) ? 1f : -1f;
        inputs[3] = Physics.CheckSphere(new Vector2(transform.position.x, transform.position.y + 0.25f), 0.1f) ? 1f : -1f;
        return inputs;
    }

    private void HandleUserMovement()
    {
        // Horizontal Movement
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MoveLeft();
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            MoveRight();
        }
        else
        {
            // No keys pressed
            MoveZero();

        }

        // Vertical Movement
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }
    }

    private void MoveLeft()
    {
        if (IsGrounded())
        {
            rigidBody2D.velocity += new Vector2(-MOVE_SPEED * GROUND_ACCELERATION * Time.deltaTime, 0);
            rigidBody2D.velocity = new Vector2(Mathf.Clamp(rigidBody2D.velocity.x, -MOVE_SPEED, MOVE_SPEED), rigidBody2D.velocity.y);
        }
        else
        {
            rigidBody2D.velocity += new Vector2(-MOVE_SPEED * AIR_ACCELERATION * Time.deltaTime, 0);
            rigidBody2D.velocity = new Vector2(Mathf.Clamp(rigidBody2D.velocity.x, -MOVE_SPEED, MOVE_SPEED), rigidBody2D.velocity.y);
        }
    }

    private void MoveRight()
    {
        if (IsGrounded())
        {
            rigidBody2D.velocity += new Vector2(MOVE_SPEED * GROUND_ACCELERATION * Time.deltaTime, 0);
            rigidBody2D.velocity = new Vector2(Mathf.Clamp(rigidBody2D.velocity.x, -MOVE_SPEED, MOVE_SPEED), rigidBody2D.velocity.y);
        }
        else
        {
            rigidBody2D.velocity += new Vector2(MOVE_SPEED * AIR_ACCELERATION * Time.deltaTime, 0);
            rigidBody2D.velocity = new Vector2(Mathf.Clamp(rigidBody2D.velocity.x, -MOVE_SPEED, MOVE_SPEED), rigidBody2D.velocity.y);
        }
    }

    private void MoveZero()
    {
        if (IsGrounded())
        {
            rigidBody2D.velocity = new Vector2(0f, rigidBody2D.velocity.y);
        }
    }

    private void Jump()
    {
        if (IsGrounded())
        {
            rigidBody2D.velocity = new Vector2(rigidBody2D.velocity.x, JUMP_VELOCITY);
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(boxCollider2D.bounds.center, boxCollider2D.bounds.size, 0f,
            Vector2.down, 0.1f, platformsLayerMask);
        return raycastHit2D.collider != null;
    }

    public NeuralNetwork GetNeuralNetwork()
    {
        return neuralNetwork;
    }
}
