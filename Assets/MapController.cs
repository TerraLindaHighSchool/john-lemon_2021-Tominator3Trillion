using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{

    public RawImage map;

    private bool mapOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        map.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        //if m clicked then open map
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (mapOpen == false)
            {
                map.enabled = true;
                mapOpen = true;
            }
            else
            {
                map.enabled = false;
                mapOpen = false;
            }
        } else if(mapOpen == true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                map.enabled = false;
                mapOpen = false;
            }
        }

        if(mapOpen) {
            //
        }
    }
}
