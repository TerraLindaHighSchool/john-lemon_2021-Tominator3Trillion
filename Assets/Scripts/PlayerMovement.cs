using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviour, IPunObservable
{
    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Quaternion rotation;
    private bool isWalking;

    public Camera cam;

    private AudioSource audioSource;

    [SerializeField] private float turnSpeed = 20f;

    PhotonView view;

    public GameObject[] hideObjects;

    GameObject currentObject = null;
    public GameObject body;

    private int currentObjectIndex = -1;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        Debug.Log("OnPhotonSerializeView");
        if(stream.IsWriting) {
            stream.SendNext(currentObjectIndex);
        }
        else if (stream.IsReading) {
            currentObjectIndex = (int)stream.ReceiveNext();
        }
    }
    


    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rotation = Quaternion.identity;
        view = GetComponent<PhotonView>();

        if(view.IsMine) {
            cam.enabled = true;
        } else {
            cam.enabled = false;
            Destroy(rb);
            //disable the collider
            GetComponent<Collider>().enabled = false;
            //disable root motion on animator
            animator.applyRootMotion = false;

            

        }

    }

    void Update() {
        if(view.IsMine) {
            //set object index based on scroll wheel
            if(Input.GetAxis("Mouse ScrollWheel") > 0) {
                currentObjectIndex++;
                if(currentObjectIndex >= hideObjects.Length) {
                    currentObjectIndex = -1;
                }
            } else if(Input.GetAxis("Mouse ScrollWheel") < 0) {
                currentObjectIndex--;
                if(currentObjectIndex < -1) {
                    currentObjectIndex = hideObjects.Length - 1;
                }
            }
            
        }
    }

    void FixedUpdate()
    {
        if(currentObjectIndex == -1) {
            if(currentObject!=null){
                Destroy(currentObject);
                currentObject = null;
            }
            
            body.SetActive(true);
        }
        else if(hideObjects[currentObjectIndex] != currentObject) {
            if(currentObject!=null){
                Destroy(currentObject);
                currentObject = null;
            }
            body.SetActive(false);
            currentObject = Instantiate(hideObjects[currentObjectIndex], transform.position, Quaternion.identity);
        }


        if(view.IsMine)  {
            

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
    }

    void OnAnimatorMove()
    {
        rb.MovePosition(rb.position + moveDirection * animator.deltaPosition.magnitude);
        rb.MoveRotation(rotation);
    }
}
