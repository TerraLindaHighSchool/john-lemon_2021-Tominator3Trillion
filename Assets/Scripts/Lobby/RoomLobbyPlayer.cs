﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class RoomLobbyPlayer : MonoBehaviour
{


    public bool isReady = false;
    private bool isHunter = false;

    public static int readyCount = 0;

    private PhotonView view;
    
    public Material hunterMaterial;
    public Material normalMaterial;

    public SkinnedMeshRenderer skinnedMeshRenderer;
    private Animator animator;

    public GameObject emoteUI;

    public AudioClip[] danceClips;
    private AudioSource audioSource;

    public DoorLobby door;

    private DoorLobby bigDoor;

    public GameObject loadingCanvas;

    void OnDestroy() {
        
        if(!view.IsMine && isReady) {
            readyCount--;
        }
        if(PhotonNetwork.IsMasterClient) {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void Start() {
        if(bigDoor==null)
            Init();
    }

    void Init() {
        view = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        //loop through all objects in the scene and find the big door
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("Door")) {
            if(obj.name == "BigDoor") {
                bigDoor = obj.GetComponent<DoorLobby>();
            }
        }
        PlayerMovement.choseHunter = false;
    }

    void Update()
    {
        //look at main camera only on the y axis
        transform.LookAt(Camera.main.transform.position, Vector3.up);

        if(view.IsMine)
        {
            if(Input.GetKeyDown(KeyCode.Z))
            {
                emoteUI.SetActive(true);
            } else if(Input.GetKeyUp(KeyCode.Z))
            {
                emoteUI.SetActive(false);
            }
        }
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
        if(bigDoor==null)
            Init();
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
            door.Open(true);
            readyCount++;
        }
        else {
            door.Open(false);
            readyCount--;
        }

        Debug.Log(readyCount + ", " + PhotonNetwork.CurrentRoom.PlayerCount);
        if(readyCount == PhotonNetwork.CurrentRoom.PlayerCount) {
            Debug.Log("All players are ready");
            bigDoor.Open(true);
        } else {
            bigDoor.Open(false);
        }

        Debug.Log("Ready Count: " + readyCount);
        

        view.RPC("RPC_ReadyUp", RpcTarget.AllBuffered);
    }


    [PunRPC]
    void RPC_ReadyUp() {
        if(bigDoor==null)
            Init();

        if(view.IsMine) return;

        isReady = !isReady;
            

        if(isReady) {
            Debug.Log("Ready");
            door.Open(true);
            readyCount++;
        }
        else {
            Debug.Log("Not Ready");
            door.Open(false);
            readyCount--;
        }

        Debug.Log(readyCount + ", " + PhotonNetwork.CurrentRoom.PlayerCount);
        if(readyCount >= PhotonNetwork.CurrentRoom.PlayerCount) {
            Debug.Log("All players are ready");
            bigDoor.Open(true);
        } else {
            bigDoor.Open(false);
        }


    }

    




    public void StartGame() {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        Debug.Log("Starting Game");
        view.RPC("RPC_StartGame", RpcTarget.All);
    }

    [PunRPC]
    void RPC_StartGame() {
        Camera.main.GetComponent<GoTo>().moving = true;
        //loop through all UI elements and disable them
        foreach(GameObject obj in GameObject.FindGameObjectsWithTag("UI")) {
            obj.SetActive(false);
        }
        StartCoroutine(LoadGameWDealy());
        
    }

    //enumerator to load the Game leve after a delay
    IEnumerator LoadGameWDealy() {
        yield return new WaitForSeconds(2);
        loadingCanvas.SetActive(true);
        PhotonNetwork.LoadLevel("Game");
    }
    

    public void CallEmote(int id) {
        view.RPC("RPC_Emote", RpcTarget.AllBuffered, id);
        Emote(id);
    }

    [PunRPC]
    void RPC_Emote(int id) {
        if(bigDoor==null)
            Init();
        Debug.Log("Emote: " + id);
        Emote(id);
    }

    private void Emote(int id) {
        animator.SetInteger("Emote", id);
        if(id != -1) {
            audioSource.clip = danceClips[id];
            audioSource.Play();
        } else {
            audioSource.Stop();
        }
    }
}