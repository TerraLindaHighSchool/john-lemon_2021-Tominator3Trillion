using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    private List<GameObject> hunters = new List<GameObject>();
    private List<GameObject> hunterSprites = new List<GameObject>();

    public GameObject hunterSprite;


    public Vector2 playerXZToXY(Vector3 playerXZ) {

        Debug.Log(GetComponent<RectTransform>().rect.width);
        return new Vector2(playerXZ.x*8.8f, (playerXZ.z-3.49f)*8.8f);
    }

    void Start()
    {
        //loop through all player tags and find the hunters
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach(GameObject player in players) {
            if(player.GetComponent<PlayerMovement>().isHunter) {
                hunters.Add(player);
                //create hunter sprite on canvas
                GameObject hs = Instantiate(hunterSprite, playerXZToXY(player.transform.position), Quaternion.identity);
                hs.transform.SetParent(gameObject.transform);
                hunterSprites.Add(hs);
            }
        }

    }

    

    void Update()
    {
        int i = 0;
        foreach(GameObject hunter in hunters) {
            hunterSprites[i].GetComponent<RectTransform>().anchoredPosition = playerXZToXY(hunter.transform.position);
            //set huntersprite size based on the width of the screen
            hunterSprites[i].GetComponent<RectTransform>(). = new Vector2(GetComponent<RectTransform>().rect.width/30, GetComponent<RectTransform>().rect.width/30);

            i++;
        }
    }
}
