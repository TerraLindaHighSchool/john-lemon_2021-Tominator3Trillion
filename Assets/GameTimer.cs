using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class GameTimer : MonoBehaviour
{

    private TextMeshProUGUI timerText;

    public float startingTime = 60f * 5f;

    public GameObject hunterWin;
    public GameObject hiderWin;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }


    void Update()
    {
        //Update the timer
        startingTime -= Time.deltaTime;
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
        


        yield return new WaitForSeconds(5f);

        PhotonNetwork.LoadLevel("Menu");
    }
}
