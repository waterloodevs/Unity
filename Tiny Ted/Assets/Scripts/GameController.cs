using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This script handles the main game scene. It keeps track of any important variables such as score and increment, and handles the UI
/// </summary>
public class GameController : MonoBehaviour
{
    //Instance used for singleton
    public static GameController Instance;
    
    //variable to control how fast the world passes
    public float scrollSpeed = -1.5f;

    //multiple variables to control the scene
    public bool isGameOver;
    public bool hasGameStarted;
    public bool isGamePaused;

    //current user score of the game
    private int score = 0;
    private int coins;
    private int totalCoinsEarned;

    //various UI elements
    public Text scoreText, gameOverText;
    public Text coinText, earnedCoinText;
    public GameObject EndGameMenu, PauseMenu, StartUI;

    //revive button
    public Button ReviveYesButton;

    //last score checkpoint
    public Transform lastScore;

    //pause button image element and images to set
    public Image pauseButtonImage;
    public Sprite[] pauseImages;

    GameObject[] scrollObjects;
    public List<objectDetails> scrollObjectsPos = new List<objectDetails>();

    public PlayerController player;
    
    public TimerScript timer;

    // Start is called before the first frame update
    void Awake()
    {
        //Set gamecontroller as a singleton to be referenced across scripts
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

    }

    private void Start()
    {
        //Instantiate score text
        scoreText.text = "Score: " + score;
        
        //initialise variables at the start of the scene
        isGameOver = false;
        hasGameStarted = false;
        isGamePaused = false;

        //dont allow player to move as game has not yet started
        player.GiveControl(false);
        timer.StartTimer();
        
        //get current funds from the account and update the game text at start
        coins = KinController.Instance.currentFunds;
        UpdateCoinTexts();

        //set scene sound
        bool isSoundOn = (PlayerPrefs.GetInt("Sound", 1) == 1) ? true : false;
        if (isSoundOn)
            GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().volume = 1;

        else
            GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().volume = 0;

        totalCoinsEarned = 0;
    }

    public void IncrementScore(Transform scorePoint)
    {
        //if the game is not in play state, exit the function
        if (isGameOver || !hasGameStarted || isGamePaused)
            return;

        //increment the score and show it in the text
        score++;
        scoreText.text = "Score: " + score;

        //used to revive the player to the last point
        lastScore = scorePoint;

        //clear any scrolling data save for previous checkpoint
        scrollObjectsPos.Clear();

        GameObject[] scrollingObjects = GameObject.FindGameObjectsWithTag("ScrollingObject");
        GameObject[] coinObjects = GameObject.FindGameObjectsWithTag("Coin");

        //increment scrolling speed after a score of 10
        if (score % 10 == 0)
        {
            scrollSpeed -= 0.5f;
            UpdateScrollingSpeed(scrollingObjects);
            UpdateScrollingSpeed(coinObjects);
        }

        //save scroll data for current position in world (in case of revival later)
        SaveScrollData(scrollingObjects);
        SaveScrollData(coinObjects);
    }

    //update the scroll speed of all scrolling objects
    void UpdateScrollingSpeed(GameObject[] objects)
    {
        foreach(GameObject obj in objects){
            obj.GetComponent<ScrollingObject>().UpdateSpeed();
        }

        gameObject.GetComponent<PillarPool>().UpdateSpawnRate();
    }

    //store the data of scrolling objects in the array for retreival in case of revive
    void SaveScrollData(GameObject[] objects)
    {
        foreach (GameObject obj in objects)
        {
            objectDetails scrollObjectToSave = new objectDetails();

            scrollObjectToSave.gameobject = obj;
            scrollObjectToSave.pos = obj.transform.position;
            scrollObjectsPos.Add(scrollObjectToSave);
        }
    }

    //simple function to update all coin texts
    void UpdateCoinTexts()
    {
        coinText.text = coins.ToString();
    }

    //when a coin is picked, increment it and update its text
    public void IncrementCoins()
    {
        coins++;
        totalCoinsEarned++;
        UpdateCoinTexts();
    }

    //function called on game over
    public void GameOver()
    {
        //set the variable (referenced in other scripts to stop them), and show game over menu. Make sure player no longer has control of bird
        isGameOver = true;
        EndGameMenu.SetActive(true);
        hasGameStarted = false;

        //reset next pillar spawn. Not doing this will have current pool disappear before it goes out of screen
        gameObject.GetComponent<PillarPool>().OnGameOver();

        //stop the scene
        StopScrollObjects();

        if(lastScore == null)
        {
            EndGameMenu.transform.GetChild(0).gameObject.SetActive(false);
            EndGameMenu.GetComponent<Animator>().SetTrigger("GameOver");
        }

        //check if a checkpoint was passed through and if user has enough coins for a revive. If so, make the clickable
        bool canRevive = (coins >= 10);
        ReviveYesButton.interactable = canRevive;

        //get highscore. If user scored more than the highscore, then show the respective text and save user score
        int highscore = PlayerPrefs.GetInt("Highscore", 0);

        if (score > highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            gameOverText.text = "New highscore: " + score;
        }
        else
        {
            gameOverText.text = "Score: " + score + "\nBest: " + highscore;
        }

        //check how many coins were earned in game
        int coinsEarned = ((coins - KinController.Instance.currentFunds) < 0) ? 0 : (coins - KinController.Instance.currentFunds);

        //Save the funds to kin (add difference between previous funds and new coins gathered, if any new coins were collected)
        if (coinsEarned > 0)
            KinController.Instance.AddFunds(coins - KinController.Instance.currentFunds);

        //update coins earned text
        earnedCoinText.text = totalCoinsEarned.ToString();
    }

    //function called on start game once the start countdown finishes
    public void StartGame()
    {
        hasGameStarted = true;
        isGameOver = false;

        StartUI.SetActive(false);
        player.GiveControl(true);

        //start the scene
        StartScrollObjects();
        
    }

    //stop and freeze the scene motion
    void StopScrollObjects()
    {
        //find and freeze all scrolling objects
        scrollObjects = GameObject.FindGameObjectsWithTag("ScrollingObject");
        foreach (GameObject scrollObject in scrollObjects)
        {
            scrollObject.GetComponent<ScrollingObject>().OnStop();
        }

        scrollObjects = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject scrollObject in scrollObjects)
        {
            scrollObject.GetComponent<ScrollingObject>().OnStop();
        }
    }

    //start or resume the scene motion
    void StartScrollObjects()
    {
        //find and freeze all scrolling objects
        scrollObjects = GameObject.FindGameObjectsWithTag("ScrollingObject");
        foreach (GameObject scrollObject in scrollObjects)
        {
            scrollObject.GetComponent<ScrollingObject>().OnStart();
        }
        
        scrollObjects = GameObject.FindGameObjectsWithTag("Coin");
        foreach (GameObject scrollObject in scrollObjects)
        {
            scrollObject.GetComponent<ScrollingObject>().OnStart();
        }
    }

    #region menu functions

    //when pause button is clicked, freeze the scene, show pause menu and take control from user. Vice versa for unpause
    public void OnClickPause()
    {
        //if game was previously unpaused, pause it and stop the scene
        if (!isGamePaused)
        {
            isGamePaused = true;
            PauseMenu.SetActive(true);
            pauseButtonImage.sprite = pauseImages[0];

            if (!hasGameStarted)
                return;
            player.GiveControl(false);
            StopScrollObjects();
        }
        //vice versa if game was paused
        else
        {
            isGamePaused = false;
            PauseMenu.SetActive(false);
            pauseButtonImage.sprite = pauseImages[1];

            if (!hasGameStarted)
                return;
            player.GiveControl(true);
            StartScrollObjects();
        }
    }
    
    public void OnClickMainMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

    public void OnClickRestartLevel()
    {
        SceneManager.LoadScene("GameScene");
    }

    //when revive is clicked, decrement the coins and start the scene
    public void OnClickRevive()
    {
        //show the loading gif as the transaction takes place
        ReviveYesButton.transform.Find("YesIcon").gameObject.SetActive(false);
        ReviveYesButton.transform.Find("LoadingSpinner").gameObject.SetActive(true);
        ReviveYesButton.interactable = false;
        
        //carry out a transaction of 10 for revive
        KinController.Instance.TransferKin(10, wasSuccesful => {
            //if the transaction took place successfully
            if(wasSuccesful){
                //update current funds to match, and update coin text
                KinController.Instance.currentFunds -= 10;
                coins = KinController.Instance.currentFunds;
                UpdateCoinTexts();

                //reset the scene
                EndGameMenu.SetActive(false);
                StartUI.SetActive(true);

                //retrieve where the scene was when user passed through a checkpoint
                for (int i = 0; i < scrollObjectsPos.ToArray().Length; i++)
                {
                    if (scrollObjectsPos[i].gameobject != null)
                        scrollObjectsPos[i].gameobject.transform.position = scrollObjectsPos[i].pos;
                }

                //reset the player to last checkpoint position
                player.ResetPlayer(lastScore);

                //start countdown
                timer.StartTimer();

                //update revive button to take out loading spinner for next death
                ReviveYesButton.transform.Find("YesIcon").gameObject.SetActive(true);
                ReviveYesButton.transform.Find("LoadingSpinner").gameObject.SetActive(false);
            }
            //if transaction failed for some reason, go to game over
            else
            {
                OnClickCancelRevive();
            }
        });
        
    }

    //if revive is cancelled, show the game over screen with score
    public void OnClickCancelRevive()
    {
        EndGameMenu.GetComponent<Animator>().SetTrigger("GameOver");
    }


    #endregion
}

/// <summary>
/// class used for storing scene data when player uses a revive
/// </summary>
public class objectDetails
{
    public GameObject gameobject;
    public Vector3 pos;
}
