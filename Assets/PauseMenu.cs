using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PauseMenu : MonoBehaviour
{

    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;


    
    void Update()
    {
     if(Input.GetKeyDown(KeyCode.Escape))
     {
         if(GameIsPaused)
         {
             Resume();
         }
         else
         {
             Pause();
         }
     }
    }

    public void Resume()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
    }

    public void Pause()
    {
        GameIsPaused = true;
        pauseMenuUI.SetActive(true);
    }

    public void LoadMenu()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Menu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
