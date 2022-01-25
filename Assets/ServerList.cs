using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon;
using UnityEngine.UI;

public class ServerList : MonoBehaviourPunCallbacks
{
    
    public GameObject content;
    public GameObject serverDropdown;


    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();
    
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        for(int i=0; i<roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
            {
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                cachedRoomList[info.Name] = info;
            }
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateCachedRoomList(roomList);
        
    }
}
