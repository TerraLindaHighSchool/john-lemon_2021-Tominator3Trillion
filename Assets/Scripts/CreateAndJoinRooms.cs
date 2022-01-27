using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class CreateAndJoinRooms : MonoBehaviourPunCallbacks
{

    public TMP_InputField createInput;
    public TMP_InputField joinInput;

    public void CreateRoom() {
        string roomName = createInput.text;
        if (roomName == "") {
            roomName = "Room " + Random.Range(0, 10000);
        }
        //RoomOptions roomOptions = new RoomOptions() { IsVisible = true, MaxPlayers = 4 };
        PhotonNetwork.CreateRoom(roomName);//, roomOptions);
    }

    public void JoinRoom() {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnCreateRoomFailed(short returnCode, string message) {
        Debug.Log("Failed to create room: " + message);
    }

    public override void OnJoinedRoom() {
        Debug.Log("Joined room");
        PhotonNetwork.LoadLevel("RoomLobby");
    }
}
