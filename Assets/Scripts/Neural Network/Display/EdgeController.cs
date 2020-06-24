using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EdgeController : MonoBehaviour
{
    // The game object representing the node at the beginning of this edge
    public GameObject InNode;
    
    // The game object representing the node at the end of this edge
    public GameObject OutNode;

    // The width of the line that makes up the edge
    public float Width = 0.2f;

    // Update is called once per frame
    void Update()
    {
        // Set width of edge
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        Width = InNode.transform.lossyScale.x / 500;
        lineRenderer.widthMultiplier = Width;

        // Set positions of edge based on whether the edge connects a node with itself or the position of the nodes
        LineRendererArrow lineRendererArrow = GetComponent<LineRendererArrow>();
        if (GameObject.ReferenceEquals(InNode, OutNode))
        {
            lineRendererArrow.ArrowOrigin = new Vector2(InNode.transform.position.x + Width, InNode.transform.position.y + 5.0f * Width);
            lineRendererArrow.ArrowTarget = new Vector2(OutNode.transform.position.x + Width, OutNode.transform.position.y + 2.5f * Width);

            LineRenderer loopLineRenderer = this.gameObject.transform.GetChild(0).gameObject.GetComponent<LineRenderer>();
            loopLineRenderer.SetPositions(new Vector3[] {
                new Vector2(InNode.transform.position.x + Width, InNode.transform.position.y + 5.0f * Width)
                , new Vector2(InNode.transform.position.x + 5.0f * Width, InNode.transform.position.y + 5.0f * Width)
                , new Vector2(InNode.transform.position.x + 5.0f * Width, InNode.transform.position.y + Width)
                , new Vector2(InNode.transform.position.x + 2.5f * Width, InNode.transform.position.y + Width) });
        }
        else if (Mathf.Abs(InNode.transform.position.x - OutNode.transform.position.x) <= 5.0f * Width)
        {
            if (InNode.transform.position.y < OutNode.transform.position.y)
            {
                lineRendererArrow.ArrowOrigin = new Vector2(InNode.transform.position.x - Width, InNode.transform.position.y + 2.5f * Width);
                lineRendererArrow.ArrowTarget = new Vector2(OutNode.transform.position.x + Width, OutNode.transform.position.y - 2.5f * Width);
            }
            else
            {
                lineRendererArrow.ArrowOrigin = new Vector2(InNode.transform.position.x - Width, InNode.transform.position.y - 2.5f * Width);
                lineRendererArrow.ArrowTarget = new Vector2(OutNode.transform.position.x + Width, OutNode.transform.position.y + 2.5f * Width);
            }
        }
        else if (InNode.transform.position.x < OutNode.transform.position.x)
        {
            lineRendererArrow.ArrowOrigin = new Vector2(InNode.transform.position.x + 2.5f * Width, InNode.transform.position.y + Width);
            lineRendererArrow.ArrowTarget = new Vector2(OutNode.transform.position.x - 2.5f * Width, OutNode.transform.position.y - Width);
        }
        else if (InNode.transform.position.x > OutNode.transform.position.x)
        {
            lineRendererArrow.ArrowOrigin = new Vector2(InNode.transform.position.x - 2.5f * Width, InNode.transform.position.y + Width);
            lineRendererArrow.ArrowTarget = new Vector2(OutNode.transform.position.x + 2.5f * Width, OutNode.transform.position.y - Width);
        }
        lineRendererArrow.UpdateArrow();

        // Make the edge appear on both a normal edge and recurrent edge. The objects are invisible
        // when they are first created and made visible when their position has been changed accordingly
        gameObject.GetComponent<LineRenderer>().enabled = true;
        if (gameObject.transform.childCount > 0) gameObject.transform.GetChild(0).GetComponent<LineRenderer>().enabled = true;
    }

}
