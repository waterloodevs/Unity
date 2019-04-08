using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// script used for initial countdown before scene begins
/// </summary>
public class TimerScript : MonoBehaviour
{
    //variable for countdown time
    public int timer;
    
    /// <summary>
    /// start the timer coroutine
    /// </summary>
    public void StartTimer()
    {
        StartCoroutine(InitiateTimer());
    }
    
    IEnumerator InitiateTimer()
    {
        int seconds = timer;
        
        while (seconds > 0)
        {
            //pauses the timer in case game is paused
            if (!GameController.Instance.isGamePaused)
            {
                gameObject.GetComponent<Text>().text = seconds.ToString();
                seconds--;
                yield return new WaitForSeconds(1);
            }
            else
            {
                yield return null;
            }
        }

        //when countdown is finished, start the game scene
        GameController.Instance.StartGame();
    }
}
