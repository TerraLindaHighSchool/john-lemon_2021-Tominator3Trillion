using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorLobby : MonoBehaviour
{
    public GameObject doorOpen;

    public void Open(bool open) {
        doorOpen.SetActive(open);
    }
}
