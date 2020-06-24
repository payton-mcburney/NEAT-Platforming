using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeController : MonoBehaviour
{
    private Vector2 startPos;
    private bool isBeingHeld = false;

    // Update is called once per frame
    void Update()
    {
        // Make the node appear
        gameObject.GetComponent<SpriteRenderer>().enabled = true;

        // Move the node to where the mouse is if the user is clicking on the node
        if (isBeingHeld)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            this.transform.position = mousePos - startPos;
        }
    }

    private void OnMouseDown()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        startPos = mousePos - (Vector2) this.transform.position;
        isBeingHeld = true;
    }

    private void OnMouseUp()
    {
        isBeingHeld = false;
    }

}
