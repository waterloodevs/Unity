using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used on objects that repeat (background). When it moves out of the screen, it positions to the right of the camera
/// </summary>
public class RepeatBackground : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    private float horizontalLength;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        horizontalLength = boxCollider.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < -horizontalLength)
            RepositionBackground();
    }

    void RepositionBackground()
    {
        Vector2 offset = new Vector2(horizontalLength * 2, 0);
        transform.position = (Vector2) transform.position + offset;
    }
}
