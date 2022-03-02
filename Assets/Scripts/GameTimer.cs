using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class GameTimer : MonoBehaviour
{

    private TextMeshProUGUI timerText;

    public static float timeMultiplier = 1f;

    public float startingTime = 60f * 5f;

    public GameObject hunterWin;
    public GameObject hiderWin;

    private bool gameEnding = false;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }


    void Update()
    {

        if (gameEnding)
        {
            return;
        }
        //Update the timer
        startingTime -= Time.deltaTime*timeMultiplier;
        timerText.text = FormatTime(startingTime);

        //change the color of the timer based on how much time is left
        if (startingTime <= 5)
        {
            //flash the timer red and white
            if(startingTime % 1f < 0.5f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
        else if (startingTime <= 20)
        {
            timerText.color = Color.red;
        }
        else if (startingTime <= 60)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }

        //if the timer reaches 0, end the game
        if (startingTime <= 0)
        {
            StartCoroutine(EndGame());
            
        }
    }

    string FormatTime(float time)
    {
        //Format the time to a string
        return string.Format("{0:00}:{1:00}", Mathf.FloorToInt(time / 60), Mathf.FloorToInt(time % 60));
    }

    //game end couroutine
    public IEnumerator EndGame()
    {
        if(gameEnding) {
            yield break;
        }
        gameEnding = true;
        bool allSuccesful = true;
        bool allDead = true;
        //check if all the hiders have walked the required distance
        foreach (GameObject hider in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (!hider.GetComponent<PlayerMovement>().isHunter)
            {
                if(!hider.GetComponent<PlayerMovement>().reachedRequiredDistance)
                {
                    allSuccesful = false;
                }
                if(hider.GetComponent<PlayerMovement>().isAlive)
                {
                    allDead = false;
                }
            }
        }

        if(allDead || !allSuccesful) {
            hunterWin.SetActive(true);
        } else {
            hiderWin.SetActive(true);
        }
        


        yield return new WaitForSeconds(8f);

        PhotonNetwork.LoadLevel("Menu");
    }
}
