using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used on the player. It detects user input to move the bird, detects collisions for game over and triggers for score and coin increments
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float upForce = 500f;
    public bool shouldMove = true;

    private Rigidbody2D rigidBody;
    private Animator animator;

    public AudioClip coin, death, flap;

    AudioSource soundSource;
    // Start is called before the first frame update
    void Start()
    {
        //initialise and set rigidbodies and animator
        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        soundSource = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //if the player can't move (i.e if it's paused, or game over etc) then exit the function. Meaning that the player can't move right now
        if (!shouldMove) return;

        //on input apply an upwards force and set the animation
        if (Input.GetMouseButtonDown(0))
        {
            rigidBody.velocity = Vector2.zero;
            rigidBody.AddForce(new Vector2(0, upForce));

            animator.SetTrigger("Flap");
            GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().PlayOneShot(flap);
        }
    }

    //if the player collides with anything, game is over. Player should die and set sequence for game over
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!GameController.Instance.isGameOver)
        {
            //disable animator in order to allow
            shouldMove = false;
            animator.SetTrigger("Die");
            animator.enabled = false;
            rigidBody.velocity = Vector2.zero;
            soundSource.PlayOneShot(death);
            GameController.Instance.GameOver();
        }
    }

    //if player goes through a trigger (score or coin) do their respective actions
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Score")
        {
            //if player goes through a pillar, report a score
            GameController.Instance.IncrementScore(collision.transform);
        }
        else if (collision.gameObject.tag == "Coin")
        {
            //if player touches a coin, destroy the coin and carry out the kin functionality
            Destroy(collision.gameObject);
            GameController.Instance.IncrementCoins();
            soundSource.PlayOneShot(coin);
        }
    }

    public void GiveControl(bool shouldGiveControl)
    {
        if (shouldGiveControl)
        {
            rigidBody.isKinematic = false;
        }
        else
        {
            rigidBody.isKinematic = true;
            rigidBody.velocity = Vector2.zero;
        }
        shouldMove = shouldGiveControl;
    }

    public void ResetPlayer(Transform point)
    {
        animator.enabled = true;
        transform.position = point.position;
        GiveControl(false);
        animator.SetTrigger("Revive");
        transform.rotation = Quaternion.identity;
        rigidBody.velocity = Vector2.zero;
        rigidBody.angularVelocity = 0;
    }
}
