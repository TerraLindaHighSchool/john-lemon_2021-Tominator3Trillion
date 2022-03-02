using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Levitating : MonoBehaviour
{
    private Vector3 rotationGoal;
    private Vector3 currentRotation;
    public float speed = 1f;

    void Start() {
        currentRotation = transform.rotation.eulerAngles;
    }
    
    void Update()
    {
        //if the rotation goal is zero then randomize it within 10 degrees of the current rotation
        if (rotationGoal == Vector3.zero)
        {
            rotationGoal = new Vector3(currentRotation.x + Random.Range(-10, 10), currentRotation.y + Random.Range(-10, 10), currentRotation.z + Random.Range(-10, 10));
        }
        // rotate the object to the rotation goal over time
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotationGoal), Time.deltaTime*speed);

        if(transform.rotation == Quaternion.Euler(rotationGoal))
        {
            rotationGoal = Vector3.zero;
        }

    }

    void FixedUpdate() {
        if(Random.Range(0f, 300f) > 299f) {
            rotationGoal = new Vector3(currentRotation.x + Random.Range(-10, 10), currentRotation.y + Random.Range(-10, 10), currentRotation.z + Random.Range(-10, 10));

        }
    }
}
