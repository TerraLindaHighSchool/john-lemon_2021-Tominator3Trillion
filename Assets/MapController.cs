using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    private List<GameObject> hunters = new List<GameObject>();
    private List<GameObject> hunterSprites = new List<GameObject>();

    public float xDivide = 65;
    public float yDivide = 30;

    public GameObject hunterSprite;

    public Vector2 playerXZToXY(Vector3 playerXZ) {
        //based on map size, create a multiplier to get the equivalent x and y on the map
        float multiplierX = GetComponent<RectTransform>().rect.width/xDivide;
        float multiplierY = GetComponent<RectTransform>().rect.height/yDivide;
        return new Vector2(playerXZ.x*multiplierX, playerXZ.z*multiplierY);
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
                hs.transform.SetParent(gameObject.transform.parent);
                hunterSprites.Add(hs);
            }
        }

    }

    

    void Update()
    {
        int i = 0;
        foreach(GameObject hunter in hunters) {
            hunterSprites[i].GetComponent<RectTransform>().anchoredPosition = playerXZToXY(hunter.transform.position);

            i++;
        }
    }
}
