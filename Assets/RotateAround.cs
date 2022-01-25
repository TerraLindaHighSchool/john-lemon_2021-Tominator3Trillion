using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour
{
    void Update()
    {
        //lerp rotate y axis  to 360 when q or e are pressed
        if (Input.GetKey(KeyCode.Q))
        {
            transform.Rotate(Vector3.up * Time.deltaTime * 40, Space.World);
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Rotate(Vector3.down * Time.deltaTime * 40, Space.World);
        }
        
    }
}
