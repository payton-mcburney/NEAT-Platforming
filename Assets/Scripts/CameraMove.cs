using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    // The lower limit on the zoom (zoom in)
    public float zoomLowerLimit = 2.0f;

    // The upper limit on the zoom (zoom out)
    public float zoomUpperLimit = 7.0f;

    // The amount of zoom for the camera
    private float zoomSize;

    // The start position of the camera when it is moved
    private Vector3 startMousePos;

    // Whether the camera is being moved
    private bool isBeingMoved;

    // The difference between the current camera position and the mouse position
    private Vector3 cameraDifference;

    // Start is called before the first frame update
    void Start()
    {
        zoomSize = GetComponent<Camera>().orthographicSize;
        isBeingMoved = false;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        HandleZoom();
        HandleMouseDrag();
    }

    //TODO: Make sure mouse is on screen and (maybe) not over a drop down if possible
    // Handles zooming in and out with the camera
    private void HandleZoom()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && zoomSize > zoomLowerLimit)
        {
            zoomSize--;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && zoomSize < zoomUpperLimit)
        {
            zoomSize++;
        }

        GetComponent<Camera>().orthographicSize = zoomSize;
    }

    // Handles camera movement by right clicking the screen and dragging the mouse
    private void HandleMouseDrag()
    {
        // If the right mouse button is clicked, then we are going to move the camera based on mouse position
        if (Input.GetMouseButton(1))
        {
            cameraDifference = Camera.main.ScreenToWorldPoint(Input.mousePosition) - Camera.main.transform.position;
            if (isBeingMoved == false)
            {
                isBeingMoved = true;
                startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }
        else
        {
            isBeingMoved = false;
        }

        // Move the camera
        if (isBeingMoved == true)
        {
            Camera.main.transform.position = startMousePos - cameraDifference;
        }
    }
}
