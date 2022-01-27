using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomLobbyPlayer : MonoBehaviour
{


    private bool isReady = false;
    private bool isHunter = false;

    public static int readyCount = 0;

    private PhotonView view;
    
    public Material hunterMaterial;
    public Material normalMaterial;

    public SkinnedMeshRenderer skinnedMeshRenderer;



    void Start() {
        view = GetComponent<PhotonView>();
    }

    void Update()
    {
        //look at main camera only on the y axis
        transform.LookAt(Camera.main.transform.position, Vector3.up);
    }

    
    public void Hunter() {
        isHunter = !isHunter;
        PlayerMovement.choseHunter = isHunter;
        if(isHunter) {
            skinnedMeshRenderer.material = hunterMaterial;
        } else {
            skinnedMeshRenderer.material = normalMaterial;
        }
        view.RPC("SetHunter", RpcTarget.AllBuffered, isHunter);
        
    }

    [PunRPC]
    public void SetHunter(bool hunter) {
        isHunter = hunter;
        if(isHunter) {
            skinnedMeshRenderer.material = hunterMaterial;
        } else {
            skinnedMeshRenderer.material = normalMaterial;
        }
    }



    public void ReadyUp() {
        isReady = !isReady;

        if(isReady) {
            readyCount++;
        }
        else {
            readyCount--;
        }

        Debug.Log("Ready Count: " + readyCount);
        

        view.RPC("RPC_ReadyUp", RpcTarget.All);
    }

    [PunRPC]
    void RPC_ReadyUp() {
        if(view.IsMine) {
            return;
        }else {
            isReady = !isReady;
        }
            

        if(isReady) {
            Debug.Log("Ready");
            readyCount++;
        }
        else {
            Debug.Log("Not Ready");
            readyCount--;
        }


    }




    public void StartGame() {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        view.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    void RPC_StartGame() {
        PhotonNetwork.LoadLevel("Game");
    }
}
