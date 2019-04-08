using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    bool isFirstTime;
    public Text highscore, coins, sound;
    public GameObject settingsPanel, mainMenuPanel, activeInternetPanel;
    public Button playButton;
    public Animator playerAnimator;
    KinController kinController;
    public Text publicAddress;
    
    // Start is called before the first frame update
    void Start()
    {
        InitialiseGame();
    }

    /// <summary>
    /// Detect if the device has an active internet connection. If not, show the inactive internet popup
    /// </summary>
    public void DetectConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            activeInternetPanel.SetActive(true);
        }
        else
        {
            activeInternetPanel.SetActive(false);
        }
    }

    //when game starts, update the coin text and highscore texts on screen
    void InitialiseGame()
    {
        playerAnimator.SetTrigger("Idle");

        DetectConnection();
        
        highscore.text = "Highscore: " + PlayerPrefs.GetInt("Highscore", 0);
        
        //initialise kin related variables if they exist, We need to do this as kin controller is not destroyed between scene
        //so there is a scenario where main menu loads and the controller already has been initialised
        kinController = GameObject.FindGameObjectWithTag("KinController").GetComponent<KinController>();
        if (kinController.currentFunds > -1)
        {
            UpdateCoins(kinController.currentFunds);
            playButton.interactable = true;
        }

        if (kinController.publicAddress != "")
        {
            publicAddress.text = "Public Address: " + kinController.publicAddress;
        }
        
    }

    /// <summary>
    /// when play is clicked, load the game scene
    /// </summary>
    public void OnClickPlay()
    {
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// when settings is clicked, show the settings menu
    /// </summary>
    public void OnClickSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    /// <summary>
    /// when back is clicked from settings, show the main menu
    /// </summary>
    public void OnClickBack()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    /// <summary>
    /// when sound button is clicked, either turn it on or off
    /// </summary>
    public void OnClickSound()
    {
        bool isSoundOn = (PlayerPrefs.GetInt("Sound", 1) == 1) ? true : false;

        //if sound was on, turn it off and set the audio source
        if (isSoundOn)
        {
            isSoundOn = false;
            PlayerPrefs.SetInt("Sound", 0);
            sound.text = "Sound: Off";
            GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().volume = 0;
        }
        else
        {
            isSoundOn = true;
            PlayerPrefs.SetInt("Sound", 1);
            sound.text = "Sound: On";
            GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().volume = 1;
        }

        //TODO: Set audiosource sound to full or none
    }

    /// <summary>
    /// reset the score to 0
    /// </summary>
    public void OnClickResetScore()
    {
        PlayerPrefs.DeleteKey("Highscore");
        highscore.text = "Highscore: " + 0.ToString();
    }

    /// <summary>
    /// update the coins text
    /// </summary>
    /// <param name="balance"></param>
    public void UpdateCoins(int balance)
    {
        Debug.Log("Updating balance text to: " + balance);
        coins.text = balance.ToString();
        playButton.interactable = true;
    }

    public void UpdatePublicAddress(string address)
    {
        publicAddress.text = "Public Address: " + address;
    }
}
