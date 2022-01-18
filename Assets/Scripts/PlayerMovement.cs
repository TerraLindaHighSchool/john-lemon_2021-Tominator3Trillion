using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Quaternion rotation;
    private bool isWalking;

    private AudioSource audioSource;

    [SerializeField] private float turnSpeed = 20f;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rotation = Quaternion.identity;

    }

    void FixedUpdate()
    {
        // Get user input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Use the input to set the movement direction
        moveDirection.Set(horizontal, 0f, vertical);
        moveDirection.Normalize();

        // Set the animator to walking or idle depending on whether the player is moving
        isWalking = !(Mathf.Approximately(horizontal, 0f) && Mathf.Approximately(vertical, 0f));
        animator.SetBool("IsWalking", isWalking);
        if(isWalking && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
        else if(!isWalking && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        
        // Assign rotation towards the direction
        Vector3 desiredDirection = Vector3.RotateTowards(transform.forward, moveDirection, turnSpeed * Time.deltaTime, 0f);
        rotation = Quaternion.LookRotation(desiredDirection);
    }

    void OnAnimatorMove()
    {
        rb.MovePosition(rb.position + moveDirection * animator.deltaPosition.magnitude);
        rb.MoveRotation(rotation);
    }
}
