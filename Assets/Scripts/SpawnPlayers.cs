using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnPlayers : MonoBehaviour
{

    public GameObject playerPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 spawnPosition = new Vector3(-18, 0, -1);
        if(PlayerMovement.choseHunter) {
            spawnPosition = new Vector3(22, 0, 1);
        }
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, Quaternion.identity, 0);
    }

}
