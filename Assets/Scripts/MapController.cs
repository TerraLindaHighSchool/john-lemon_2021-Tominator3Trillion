using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    private List<GameObject> hunters = new List<GameObject>();
    private List<GameObject> hunterSprites = new List<GameObject>();

    public GameObject hunterSprite;

    private int frame = 0;

    public float multiplierDivisorWidth = 597f;
    public float multiplierMultiplierWidth = 8.8f;
    public float multiplierDivisorHeight = 312f;
    public float multiplierMultiplierHeight = 8.8f;


    //true = width, false = height
    public bool getEqualSide() {
        if(GetComponent<RectTransform>().rect.width/GetComponent<RectTransform>().rect.height > 1572/822) {
            return false;
        } else {
            return true;
        }
        
    }

    public Vector2 playerXZToXY(Vector3 playerXZ) {

        if(getEqualSide()) {
            Debug.Log(GetComponent<RectTransform>().rect.width);
            float multiplier = GetComponent<RectTransform>().rect.width/multiplierDivisorWidth;
            return new Vector2(playerXZ.x*multiplierMultiplierWidth*multiplier, (playerXZ.z-3.49f)*multiplierMultiplierWidth*multiplier);
        } else {
            Debug.Log(GetComponent<RectTransform>().rect.height);
            float multiplier = GetComponent<RectTransform>().rect.height/multiplierDivisorHeight;
            return new Vector2(playerXZ.x*multiplierMultiplierHeight*multiplier, (playerXZ.z-3.49f)*multiplierMultiplierHeight*multiplier);
        }
        
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

    

    void FixedUpdate()
    {
        
        if(frame>=60) {
            int i = 0;
            foreach(GameObject hunter in hunters) {
                hunterSprites[i].GetComponent<RectTransform>().anchoredPosition = playerXZToXY(hunter.transform.position);
                //set huntersprite size based on the width of the screen
                hunterSprites[i].GetComponent<RectTransform>().sizeDelta = new Vector2(GetComponent<RectTransform>().rect.width/3, GetComponent<RectTransform>().rect.height/3);

                i++;
            }
            frame=0;
        }
        frame++;
    }
}
