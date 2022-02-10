using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class RoomLobbyManager : MonoBehaviour
{

    public GameObject playerPrefab;

    public TextMeshProUGUI roomCode;

    private GameObject player;

    public GameObject startGameButton;

    


    void Start()
    {   
        Vector3 spawnPosition = new Vector3(Random.Range(-2, 2), 0, Random.Range(-8, -2));
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity, 0);

        roomCode.text = PhotonNetwork.CurrentRoom.Name;

        startGameButton.SetActive(false);

        //create a PhotonNetwork variable called numberReady
    }

    void Update()
    {
        if(RoomLobbyPlayer.readyCount == PhotonNetwork.CurrentRoom.PlayerCount) {
            if(PhotonNetwork.IsMasterClient) {
                startGameButton.SetActive(true);
                
            }
        } else {
            startGameButton.SetActive(false);
        }
    }

    
    


    public void CopyRoomCode() {
        TextEditor te = new TextEditor();
        te.text = PhotonNetwork.CurrentRoom.Name;
        te.SelectAll();
        te.Copy();
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LoadLevel("Lobby");
    }

    public void ReadyUp() {
        player.GetComponent<RoomLobbyPlayer>().ReadyUp();
    }

    public void Hunter() {
        player.GetComponent<RoomLobbyPlayer>().Hunter();
    }

    public void StartGame() {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        player.GetComponent<RoomLobbyPlayer>().StartGame();
    }



    



    
}
