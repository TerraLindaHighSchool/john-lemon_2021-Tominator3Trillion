using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCam : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate teh camera on the world y axis when q or e are pressed
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * -100, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 100, Space.World);
        }
    }
}
