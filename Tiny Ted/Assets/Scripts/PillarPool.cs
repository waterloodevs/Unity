using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script is used to generate a pool of pillars and position them periodically (so they repeat with a random y path for the player)
/// </summary>
public class PillarPool : MonoBehaviour
{
    //how quickly the pillars should spawn
    public float spawnRate;

    //a distance variable used to keep the pillars at a constant distance
    public float distance;

    //amount of pillars to instantiate and keep track of
    public int poolSize = 5;

    //maximum pillar x position (used to instantiate pillars at random y pos)
    public float pillarMax = 4f;

    //minimum pillar x position 
    public float pillarMin = 1f;

    //how much time has passed isnce last pillar was spawn (used with spawn rate to spawn a new pillar)
    private float timeSinceLastSpawn;

    //spawn x position (positions it to the right of the camera outside the view)
    private float spawnXPos = 13f;

    //which pillar was currently tracked and positioned
    private int currentPillar = 0;

    //prefabs for normal pillar and pillar with coin
    public GameObject pillarPrefab, coinPrefab;

    //total pillars in the pool
    public GameObject[] pillars;

    //a position outisde the camera to spawn the pillars initially
    private Vector2 ObjectPoolPosition = new Vector2(-15, -25);

    //keep track of how many pillars have been done
    private int totalPillars;

    // Start is called before the first frame update
    void Start()
    {
        pillars = new GameObject[poolSize];
        for(int i = 0; i < poolSize; i++)
        {
            //spawn a coin pillar as the first pillar (it will circle back, so every 10th pillar is the one with the coin)
            pillars[i] = (GameObject)Instantiate(pillarPrefab, ObjectPoolPosition, Quaternion.identity);
        }

        totalPillars = 0;
        
        UpdateSpawnRate();

        //speed up the first time pillar start spawning
        timeSinceLastSpawn = spawnRate;

        SpawnInitialPillars();
    }

    // Update is called once per frame
    void Update()
    {
        //if the game has ended, or was paused or has not started, then exit the function and don't track the pillars
        if (GameController.Instance.isGameOver || !GameController.Instance.hasGameStarted || GameController.Instance.isGamePaused)
           return;
        
        //increment the timer
        timeSinceLastSpawn += Time.deltaTime;

        //if it's time to spawn (position) a new pillar
        if (timeSinceLastSpawn >= spawnRate)
        {
            //reset the timer
            timeSinceLastSpawn = 0;

            SpawnPillar(spawnXPos, currentPillar);
            
            //generate a coin after 10 pillars
            if (totalPillars == 0 || totalPillars % 10 == 0)
                SpawnCoin(pillars[currentPillar]);

            //move on to the next pillar
            currentPillar++;

            //if we've tracked all the pillars in the pool, circle back to the first one
            if (currentPillar >= poolSize)
                currentPillar = 0;
        }
    }

    /// <summary>
    /// spawn an initial pillar in scene to make the pillars appear sooner
    /// </summary>
    void SpawnInitialPillars()
    {
        SpawnPillar(spawnXPos - distance * 2, currentPillar);
        SpawnCoin(pillars[currentPillar]);
        currentPillar++;
        SpawnPillar(spawnXPos - distance, currentPillar);
        currentPillar++;
    }

    /// <summary>
    /// function to spawn a pillar at a particular distance
    /// </summary>
    /// <param name="xPos"></param>
    /// <param name="ind"></param>
    void SpawnPillar(float xPos, int ind) {

        //get a random y position for the pillar
        float spawnYpos = Random.Range(pillarMin, pillarMax);

        //position the pillar
        pillars[currentPillar].transform.position = new Vector2(xPos, spawnYpos);

        totalPillars++;

    }

    public void OnGameOver()
    {
        timeSinceLastSpawn = 0;
    }

    public void UpdateSpawnRate()
    {
        //calculate spawnRate. We know that time = distance / speed. 
        //We have the scroll sped from game controller, and we get a distance number using hit and trial
        spawnRate = distance / Mathf.Abs(GameController.Instance.scrollSpeed);
    }

    float coinXoffset = 1.5f;
    float coinYoffset = 2f;

    /// <summary>
    /// Generate a coin at an odd but reachable position
    /// </summary>
    /// <param name="pillar"></param>
    void SpawnCoin(GameObject pillar)
    {
        Transform collider = pillar.transform.Find("Score");
        GameObject coin = Instantiate(coinPrefab);

        coin.transform.position = collider.position;

        //if the coin is going to be too far up, position it downwards
        if(coin.transform.position.y + coinYoffset > 3)
        {
            coin.transform.position = new Vector3(coin.transform.position.x + coinXoffset, coin.transform.position.y - coinYoffset, 0);
        }
        //else position it upwards
        else
        {
            coin.transform.position = new Vector3(coin.transform.position.x + coinXoffset, coin.transform.position.y + coinYoffset, 0);
        }

        //start scrolling the coin
        coin.GetComponent<ScrollingObject>().OnStart();
    }
}
