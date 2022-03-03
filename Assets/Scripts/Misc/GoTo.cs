using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTo : MonoBehaviour
{

    public GameObject target;
    public float speed;
    public bool moving = false;


    void Update()
    {
        //move towards target
        if(moving) {
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        }
    }
}
