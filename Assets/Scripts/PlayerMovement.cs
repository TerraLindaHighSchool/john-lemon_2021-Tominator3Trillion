using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviour, IPunObservable
{
    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Quaternion rotation;
    private bool isWalking;

    private bool isAlive = true;
    private bool isHunter = false;
    private bool angry = false;

    private float angryTime = 0f;
    public float angryTimeMax = 5f;
    private float angryWarmUpTime = 30f;
    public float angryTimeCooldown = 50f;
    public GameObject hunterCanvas;
    public Image angryCooldownImage;
    public GameObject angryPost;

    public Camera cam;

    private AudioSource audioSource;

    [SerializeField] private float turnSpeed = 20f;

    PhotonView view;

    public GameObject[] hideObjects;

    GameObject currentObject = null;
    public GameObject body;

    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Material GhostMaterial;
    public Material HunterMaterial;

    private int currentObjectIndex = -1;

    public GameObject[] eyes;
    private bool firingLasers = false;
    public AudioSource laserAudioSource;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if(stream.IsWriting) {
            stream.SendNext(currentObjectIndex);
            stream.SendNext(isAlive);
            
        }
        else if (stream.IsReading) {
            currentObjectIndex = (int)stream.ReceiveNext();
            bool nowAlive = (bool)stream.ReceiveNext();
            if(isAlive && !nowAlive) {
                skinnedMeshRenderer.material = GhostMaterial;
            }
            isAlive = nowAlive;

            if(isHunter) {
                skinnedMeshRenderer.material = HunterMaterial;
            }
            
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
            if(PhotonNetwork.IsMasterClient) {
                isHunter = true;
            }
        }

        if(view.Owner.IsMasterClient) {
            isHunter = true;
        }

        if(isHunter) {
            skinnedMeshRenderer.material = HunterMaterial;
            if(view.IsMine) {
                hunterCanvas.SetActive(true);
            }
        }

        if(view.IsMine) {
            cam.enabled = true;
        } else {
            cam.enabled = false;
            //disable the collider
            GetComponent<Collider>().enabled = false;
            //disable root motion on animator
            animator.applyRootMotion = false;
            GetComponent<AudioListener>().enabled = false;

            

        }

    }

    void Update() {
        if(isHunter && view.IsMine) {
            angryWarmUpTime+=Time.deltaTime;

            if(angry) {
                angryCooldownImage.color =(Color.red);
                angryCooldownImage.fillAmount = 1 - angryTime/angryTimeMax;
            } else {
                angryCooldownImage.color = (Color.grey);
                angryCooldownImage.fillAmount = angryWarmUpTime/angryTimeCooldown;
            }


            if(angryWarmUpTime >= angryTimeCooldown) {
                if(Input.GetKeyDown(KeyCode.Space)) {
                    angry = true;
                    angryPost.SetActive(true);
                    //triple animation speed
                    animator.speed = 5f;
                    //triple audio speed
                    audioSource.pitch = 5f;
                    angryWarmUpTime = 0f;
                }
            }
            if(angry) {
                angryTime += Time.deltaTime;
                if(angryTime >= angryTimeMax) {
                    angry = false;
                    angryPost.SetActive(false);
                    //reset animation speed
                    animator.speed = 1f;
                    //reset audio speed
                    audioSource.pitch = 1f;
                    angryTime = 0f;
                }
            }


            

        }

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

            // Use the input to set the movement direction based on the camera's rotation
            moveDirection.Set(horizontal, 0f, vertical);
            moveDirection = Camera.main.transform.TransformDirection(moveDirection);
            moveDirection.y = 0.0f;
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

        //on click, draw ray from cam to world mouse position, and fire lasers from eyes
            if(Input.GetMouseButton(0)) {
                
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {

                    //return if hit point is not within 90 degrees of the player vision
                    if(Vector3.Angle(transform.forward, hit.point - transform.position) > 90f) {
                        //animator.enabled = true;
                        foreach(GameObject eye in eyes) {
                            eye.SetActive(false);
                        }
                        if(laserAudioSource.isPlaying) {
                            laserAudioSource.Stop();
                        }
                        return;
                    }


                    //create a raycast from one of the eyes towards the hit point
                    Vector3 eyePos = eyes[0].transform.position;
                    Vector3 dir = hit.point - eyePos;
                    Ray eyeRay = new Ray(eyePos, dir);
                    RaycastHit eyeHit;
                    if(Physics.Raycast(eyeRay, out eyeHit)) {

                        //disble the aniamtor
                        //animator.enabled = false;

                        if(!laserAudioSource.isPlaying) {
                            laserAudioSource.Play();
                        }
                        //return if the it hit the player or within 1 unit of the player
                        if(eyeHit.collider.gameObject == gameObject) {
                            return;
                        }
                        Vector3 target = eyeHit.point;
                        //rotate to face target on the y axis
                        Vector3 direction = target - eyes[0].transform.position;
                        //only rotate the y axis
                        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                        
                        foreach(GameObject eye in eyes) {
                            eye.SetActive(true);
                            //line renderer from eye to target
                            eye.GetComponent<LineRenderer>().SetPosition(0, eye.transform.position);
                            eye.GetComponent<LineRenderer>().SetPosition(1, target);

                        }
                    }
                    
                }
            } else {
                //animator.enabled = true;
                foreach(GameObject eye in eyes) {
                    eye.SetActive(false);
                }
                if(laserAudioSource.isPlaying) {
                    laserAudioSource.Stop();
                }

            }
    }

    void OnAnimatorMove()
    {
        try {
            rb.MovePosition(rb.position + moveDirection * animator.deltaPosition.magnitude);
            rb.MoveRotation(rotation);
        } catch {
        }
    }

    void Die() {
        skinnedMeshRenderer.material = GhostMaterial;
        rb.isKinematic = true;
        
    }
}
