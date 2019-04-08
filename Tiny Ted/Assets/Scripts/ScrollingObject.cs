using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used on objects that scroll to the left (to give the impression that the player is moving to the right.
/// It's attached to the background and the pillars.
/// </summary>
/// 
public class ScrollingObject : MonoBehaviour
{
    private Rigidbody2D rigidBody;
    float scrollSpeed = -1.5f;

    private void Awake()
    {
        //on start, initialise rigidbody
        rigidBody = GetComponent<Rigidbody2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        //If there is no game controller in the scene, set the scrollspeed. Otherwise, get the scrollspeed from the gamecontroller. 
        //This is for the clouds in the menu so they scroll randomly and no game controller is needed
        if (GameController.Instance == null)
        {
            scrollSpeed = -0.5f;
            OnStart();
        }
        else
            scrollSpeed = GameController.Instance.scrollSpeed;
    }
    
    //update scrolling speed when the user levels up
    public void UpdateSpeed()
    {
        scrollSpeed = GameController.Instance.scrollSpeed;
        rigidBody.velocity = new Vector2(scrollSpeed, 0);
    }

    //start scrolling the object
    public void OnStart()
    {
        if(GameController.Instance.hasGameStarted && !GameController.Instance.isGamePaused && !GameController.Instance.isGameOver)
            rigidBody.velocity = new Vector2(scrollSpeed, 0);
    }

    //pause scrolling the object
    public void OnStop()
    {
        rigidBody.velocity = Vector2.zero;
    }
    
}
