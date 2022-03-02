﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(PhotonView))]
public class PlayerMovement : MonoBehaviour, IPunObservable
{
    private Animator animator;
    private Rigidbody rb;
    private Vector3 moveDirection;
    private Quaternion rotation;
    private bool isWalking;

    [HideInInspector]
    public float distanceWalked = 0f;
    private Vector3 lastPosition = Vector3.zero;
    public float requiredDistance = 500f;
    public Slider distanceWalkedSlider;
    public TextMeshProUGUI distanceWalkedPercentText;
    public bool reachedRequiredDistance = false;

    public GameObject allCanvas;

    public GameTimer timer;
    

    private bool upsideDown = false;
    private bool upsideDownCharged = true;

    public static bool choseHunter;

    public bool isAlive = true;
    public bool isHunter = false;
    private bool angry = false;
    private bool holdBreath = false;

    private static int hidersAlive = 0;
    private static int hidersNotSuccesful = 0;

    public float health = 100f;

    public GameObject hiderCanvas;
    public GameObject hiderFX;

    public GameObject hunterFX;

    private float angryTime = 0f;
    public float angryTimeMax = 5f;
    private float angryWarmUpTime = 30f;
    public float angryTimeCooldown = 50f;
    public GameObject hunterCanvas;
    public Image angryCooldownImage;
    public GameObject angryFX;
    public AudioSource angrySound;

    private float holdBreathTime = 0f;
    public float holdBreathTimeMax = 5f;
    private float holdBreathWarmUpTime = 10f;
    public float holdBreathTimeCooldown = 50f;
    public Image holdBreathCooldownImage;
    public GameObject holdBreathPost;
    public AudioSource breathingSource;
    public AudioClip breathingClip;
    public AudioClip inhaleClip;

    private bool wasHoldingBreath = false;
    


    public Camera cam;

    private AudioSource audioSource;

    [SerializeField] private float turnSpeed = 20f;

    PhotonView view;

    public GameObject[] hideObjects;

    GameObject currentObject = null;
    public GameObject body;

    public SkinnedMeshRenderer skinnedMeshRenderer;
    public Material GhostMaterial;
    public GameObject deadBodyPrefab;
    public Material HunterMaterial;

    private int currentObjectIndex = -1;

    public GameObject[] eyes;
    public AudioSource laserAudioSource;
    public GameObject[] laserImpacts;
    [SerializeField]public Vector3 laserImpactPosition;

    private Vector3 wantedLaserImpactPosition;

    public GameObject head;

    public GameObject map;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if(stream.IsWriting && view.IsMine) {
            stream.SendNext(currentObjectIndex);
            stream.SendNext(angry);
            stream.SendNext(holdBreath);
            stream.SendNext(laserImpactPosition);
            stream.SendNext(distanceWalked);
            
        }
        else if (stream.IsReading) {
            currentObjectIndex = (int)stream.ReceiveNext();

            // if(isHunter) {
            //     skinnedMeshRenderer.material = HunterMaterial;
            // }

            angry = (bool)stream.ReceiveNext();
            holdBreath = (bool)stream.ReceiveNext();
            wantedLaserImpactPosition = (Vector3)stream.ReceiveNext();
            distanceWalked = (float)stream.ReceiveNext();
            
        }
    }
    

    [PunRPC]
    public void RPC_SetHunter(bool isHunter) {
        this.isHunter = isHunter;
        if(isHunter) {
            hidersAlive--;
            hidersNotSuccesful--;
            skinnedMeshRenderer.material = HunterMaterial;
            foreach(GameObject eye in eyes) {
                eye.GetComponent<LineRenderer>().enabled = true;
                eye.transform.GetChild(0).gameObject.SetActive(true);
            }
        }
    }

    [PunRPC]
    public void RPC_SetAlive(bool isAlive) {
        this.isAlive = isAlive;
        if(!this.isAlive && !view.IsMine) {
            Die();
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

        map.SetActive(false);

        distanceWalkedSlider.maxValue = requiredDistance;
        distanceWalkedPercentText.text = "0%";
        distanceWalked = 0f;

        hidersAlive++;
        hidersNotSuccesful++;

        Debug.Log("Hiders alive: " + hidersAlive);
        

        if(view.IsMine) {
            allCanvas.SetActive(true);
            isHunter = choseHunter;
            view.RPC("RPC_SetHunter", RpcTarget.AllBuffered, isHunter);
        } else {
            allCanvas.SetActive(false);
        }

        if(!isHunter && view.IsMine) {
            hiderCanvas.SetActive(true);
        }
        else if(isHunter && view.IsMine) {
            skinnedMeshRenderer.material = HunterMaterial;
            if(view.IsMine) {
                hunterCanvas.SetActive(true);
            }
        } else {
            //loop through eyes and remove line renderers
            foreach(GameObject eye in eyes) {
                eye.GetComponent<LineRenderer>().enabled = false;
                eye.transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        if(view.IsMine) {
            cam.enabled = true;
        } else {
            Destroy(cam);
            //disable the collider
            //GetComponent<Collider>().enabled = false;
            //disable root motion on animator
            animator.applyRootMotion = false;
            GetComponent<AudioListener>().enabled = false;

            

        }

        

    }



    void Update() {
        


        //if D is pressed die
        // if(view.IsMine && Input.GetKeyDown(KeyCode.J)) {
        //     Die();
        // }

        if(view.IsMine && Input.GetKeyDown(KeyCode.U) && !upsideDown && upsideDownCharged && (!isHunter||!isAlive)) {
             StartCoroutine(UpsideDown());
        }

        // if touching another non hunter upsideDownCharged = true, check every 10 seconds
        if(Time.timeSinceLevelLoad % 10 == 0 && view.IsMine && !upsideDown && !upsideDownCharged && !isHunter && isAlive) {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
            foreach(Collider col in hitColliders) {
                if(!col.gameObject.GetComponent<PlayerMovement>().isHunter) {
                    upsideDownCharged = true;
                }
            }
        } else if(!isAlive) {
            upsideDownCharged = true;
        }

        


        if(!view.IsMine) {
            //lerp laser impact position to wanted position
            laserImpactPosition = Vector3.Lerp(laserImpactPosition, wantedLaserImpactPosition, Time.deltaTime * 15f);
            //if close enough to wanted position, set it to wanted position
            if(Vector3.Distance(laserImpactPosition, wantedLaserImpactPosition) < 0.1f) {
                laserImpactPosition = wantedLaserImpactPosition;
            }
        }


        if(isHunter && view.IsMine) {
            if(!angry) {
                angryWarmUpTime+=Time.deltaTime;
            }

            if(angry) {
                angryCooldownImage.color =(Color.red);
                angryCooldownImage.fillAmount = 1 - angryTime/angryTimeMax;
            } else {
                if(angryWarmUpTime/angryTimeCooldown >= 1f) {
                    angryCooldownImage.color = (Color.white);
                } else {
                    angryCooldownImage.color = (Color.grey);
                }
                
                angryCooldownImage.fillAmount = angryWarmUpTime/angryTimeCooldown;
            }


            if(angryWarmUpTime >= angryTimeCooldown) {
                if(Input.GetKeyDown(KeyCode.Space)) {
                    angry = true;
                    angryFX.SetActive(true);
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
                    angryFX.SetActive(false);
                    //reset animation speed
                    animator.speed = 1f;
                    //reset audio speed
                    audioSource.pitch = 1f;
                    angryTime = 0f;
                }
            }
        } else if (view.IsMine) {
            if(!holdBreath) {
                holdBreathWarmUpTime+=Time.deltaTime;
            }

            if(holdBreath) {
                holdBreathCooldownImage.color =(Color.red);
                holdBreathCooldownImage.fillAmount = 1 - holdBreathTime/holdBreathTimeMax;
            } else {
                if(holdBreathWarmUpTime/holdBreathTimeCooldown >= 1f) {
                    holdBreathCooldownImage.color = (Color.white);
                } else {
                    holdBreathCooldownImage.color = (Color.grey);
                }
                
                holdBreathCooldownImage.fillAmount = holdBreathWarmUpTime/holdBreathTimeCooldown;
            }


            if(holdBreathWarmUpTime >= holdBreathTimeCooldown) {
                if(Input.GetKeyDown(KeyCode.Space)) {
                    holdBreath = true;
                    holdBreathPost.SetActive(true);
                    holdBreathWarmUpTime = 0f;
                    breathingSource.Stop();
                    breathingSource.PlayOneShot(inhaleClip);
            }}

            if(holdBreath) {
                holdBreathTime += Time.deltaTime;
                if(holdBreathTime >= holdBreathTimeMax) {
                    holdBreath = false;
                    holdBreathPost.SetActive(false);
                    holdBreathTime = 0f;
                    breathingSource.clip = breathingClip;
                    breathingSource.Play();
                }
            }
            
            

            if(holdBreath) {
                holdBreathCooldownImage.color =(Color.red);
                holdBreathCooldownImage.fillAmount = 1 - holdBreathTime/holdBreathTimeMax;
            } else {
                if(holdBreathWarmUpTime/holdBreathTimeCooldown >= 1f) {
                    holdBreathCooldownImage.color = (Color.white);
                } else {
                    holdBreathCooldownImage.color = (Color.grey);
                }
                
                holdBreathCooldownImage.fillAmount = holdBreathWarmUpTime/holdBreathTimeCooldown;
            }
            
        }


        
        if(!view.IsMine && isHunter) {
            if(angry) {
                animator.speed = 5f;
                audioSource.pitch = 5f;
            } else {
                animator.speed = 1f;
                audioSource.pitch = 1f;
            }
            if(laserImpactPosition != Vector3.zero) {
                if(!laserAudioSource.isPlaying) {
                    laserAudioSource.Play();
                }
                // //return if the it hit the player or within 1 unit of the player
                // if(eyeHit.collider.gameObject == gameObject) {
                //     return;
                // }
                Vector3 target = laserImpactPosition;
                //rotate to face target on the y axis
                Vector3 direction = target - eyes[0].transform.position;
                //only rotate the y axis
                Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                        
                foreach(GameObject eye in eyes) {
                    eye.GetComponent<LineRenderer>().enabled = true;
                    eye.transform.GetChild(0).gameObject.SetActive(true);
                     //line renderer from eye to target
                    eye.GetComponent<LineRenderer>().SetPosition(0, eye.transform.position);
                    eye.GetComponent<LineRenderer>().SetPosition(1, target);

                }

                foreach(GameObject impact in laserImpacts) {
                    impact.SetActive(true);
                    impact.transform.position = target;
                    //rotate lookrotation 180 degrees
                    impact.transform.rotation = lookRotation * Quaternion.Euler(0, 180f, 0);     
                }
            } else {
                //animator.enabled = true;
                foreach(GameObject eye in eyes) {
                    eye.GetComponent<LineRenderer>().enabled = false;
                    eye.transform.GetChild(0).gameObject.SetActive(false);
                }
                if(laserAudioSource.isPlaying) {
                    laserAudioSource.Stop();
                }
                foreach(GameObject impacts in laserImpacts) {
                    impacts.SetActive(false);
                }
            }

            
        } else if(!view.IsMine) {
            if(holdBreath && !wasHoldingBreath) {
                //play one shot
                breathingSource.PlayOneShot(inhaleClip);
                wasHoldingBreath = true;
                
            } else if(!holdBreath && wasHoldingBreath) {
                breathingSource.clip = breathingClip;
                breathingSource.Play();
            }
        }

        if(view.IsMine && isAlive) {
            if(health <= 0f) {
                Die();
            }
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

        if(view.IsMine) {
            if(Input.GetKeyDown(KeyCode.M)) {
                map.SetActive(!map.activeSelf);
            }
        }
    }

    public IEnumerator UpsideDown() {
        upsideDownCharged=false;
        upsideDown = true;
        transform.position = new Vector3(transform.position.x, transform.position.y + 300f, transform.position.z);
        yield return new WaitForSeconds(10);
        transform.position = new Vector3(transform.position.x, transform.position.y - 300f, transform.position.z);
        upsideDown = false;
    }

    void FixedUpdate()
    {
        if(!view.IsMine && !isHunter && distanceWalked > requiredDistance && !reachedRequiredDistance) {
            hidersNotSuccesful--;
            reachedRequiredDistance = true;
            if(hidersNotSuccesful <= 0) {
                StartCoroutine(timer.EndGame());
            }
        }
        if(view.IsMine && !isHunter) {
            //update distance walked
            if( Vector3.Distance(transform.position, lastPosition) < 1f && Vector3.Distance(transform.position, lastPosition) > 0.002f) {
                distanceWalked += Vector3.Distance(transform.position, lastPosition);
            }
            lastPosition = transform.position;
            
            //update slider
            distanceWalkedSlider.value = distanceWalked;
            distanceWalkedPercentText.text = (distanceWalked / requiredDistance * 100f).ToString("F0") + "%";
            Debug.Log(distanceWalked);
            


            if(distanceWalked >= requiredDistance && !reachedRequiredDistance) {
                distanceWalked = requiredDistance;
                reachedRequiredDistance = true;
                hidersNotSuccesful--;
                if(hidersNotSuccesful <= 0) {
                    StartCoroutine(timer.EndGame());
                }
            }
            if(distanceWalked > requiredDistance) {
                distanceWalked = requiredDistance;
            }
        }


        if(!view.IsMine) {
            if(animator.GetBool("IsWalking")) {
                if(!audioSource.isPlaying) {
                    audioSource.Play();
                }
            } else {
                if(audioSource.isPlaying) {
                    audioSource.Stop();
                }
            }
        }


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
            moveDirection = cam.transform.TransformDirection(moveDirection);
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
        

        //on click, draw ray from cam to world mouse position, and fire lasers from eyes
            if(Input.GetMouseButton(0) && isHunter && view.IsMine && currentObjectIndex == -1) {
                
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if(Physics.Raycast(ray, out hit)) {

                    // //return if hit point is not within 90 degrees of the player vision
                    // if(Vector3.Angle(transform.forward, hit.point - transform.position) > 90f) {
                    //     //animator.enabled = true;
                    //     foreach(GameObject eye in eyes) {
                    //         eye.SetActive(false);
                    //     }
                    //     if(laserAudioSource.isPlaying) {
                    //         laserAudioSource.Stop();
                    //     }
                    //     return;
                    // }


                    //create a raycast from one of the eyes towards the hit point
                    Vector3 eyePos = eyes[0].transform.position;
                    Vector3 dir = hit.point - eyePos;
                    Ray eyeRay = new Ray(eyePos, dir);
                    RaycastHit eyeHit;
                    if(Physics.Raycast(eyeRay, out eyeHit)) {

                        //disble the animator
                        //animator.enabled = false;

                        if(!laserAudioSource.isPlaying) {
                            laserAudioSource.Play();
                        }
                        // //return if the it hit the player or within 1 unit of the player
                        // if(eyeHit.collider.gameObject == gameObject) {
                        //     return;
                        // }
                        Vector3 target = eyeHit.point;
                        laserImpactPosition = target;
                        //rotate to face target on the y axis
                        Vector3 direction = target - eyes[0].transform.position;
                        //only rotate the y axis
                        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
                        
                        foreach(GameObject eye in eyes) {
                            eye.GetComponent<LineRenderer>().enabled = true;
                            eye.transform.GetChild(0).gameObject.SetActive(true);
                            //line renderer from eye to target
                            eye.GetComponent<LineRenderer>().SetPosition(0, eye.transform.position);
                            eye.GetComponent<LineRenderer>().SetPosition(1, target);

                        }

                        foreach(GameObject impact in laserImpacts) {
                            impact.SetActive(true);
                            impact.transform.position = target;
                            //rotate lookrotation 180 degrees
                            impact.transform.rotation = lookRotation * Quaternion.Euler(0, 180f, 0);

                        }

                        //create a radius of 0.1f around the laser impact position 
                        Collider[] hitColliders = Physics.OverlapSphere(laserImpactPosition, 0.1f);
                        //loop through all the colliders
                        foreach(Collider hitCollider in hitColliders) {
                            //if the collider is this player
                            if(hitCollider.gameObject != this.gameObject && hitCollider.gameObject.tag == "Player") {
                                //call the det health rpc
                                hitCollider.gameObject.GetComponent<PlayerMovement>().LoseHealth(Time.deltaTime * 50f);
                                //break out of the loop
                                break;
                            }
                        }
                    }
                    
                }
            } else {
                laserImpactPosition = Vector3.zero;
                //animator.enabled = true;
                foreach(GameObject eye in eyes) {
                    eye.GetComponent<LineRenderer>().enabled = false;
                    eye.transform.GetChild(0).gameObject.SetActive(false);
                }
                if(laserAudioSource.isPlaying) {
                    laserAudioSource.Stop();
                }
                foreach(GameObject impacts in laserImpacts) {
                    impacts.SetActive(false);
                }

            }
        }
    }


    [PunRPC]
    private void RPC_LoseHealth(float damage) {
        health -= damage;
    }

    public void LoseHealth(float damage) {
        if(view.IsMine) {
            view.RPC("RPC_LoseHealth", RpcTarget.AllBuffered, damage);
        }
    }

    void LateUpdate() {
        if(laserImpactPosition != Vector3.zero) {
            foreach(GameObject eye in eyes) {
                //rotate the eye to look at the laser impact
                eye.transform.rotation = Quaternion.LookRotation(laserImpactPosition - eye.transform.position, Vector3.up);
            }
            //rotate the head to look at the laser impact

            head.transform.rotation = Quaternion.LookRotation(laserImpactPosition - head.transform.position, Vector3.up);
            //set the head z rotation to 0
            head.transform.rotation = Quaternion.Euler(0f, head.transform.rotation.eulerAngles.y, -90f);
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
        GameTimer.timeMultiplier += 0.5f;
        isAlive = false;
        //call the Die trigger on the animator
        animator.SetTrigger("Die");
        if(view.IsMine) {
            view.RPC("RPC_SetAlive", RpcTarget.AllBuffered, isAlive);
        }
        
        //call deathanimation enumerator
        StartCoroutine(DeathAnimation());

        rb.isKinematic = true;
        GetComponent<Collider>().enabled = false;
        currentObjectIndex = -1;

        //check if all players are dead
        hidersAlive--;
        if(hidersAlive <= 0) {
            StartCoroutine(timer.EndGame());
        }


        
        
        
    }

    //death animation 
    IEnumerator DeathAnimation() {
        yield return new WaitForSeconds(3f);
        //instantiate a dead body at the player's position
        GameObject deadBody = Instantiate(deadBodyPrefab, transform.position, transform.rotation);
        
        
        animator.SetTrigger("GetUp");

        

        yield return new WaitForSeconds(0.25f);
        transform.position += transform.forward * 0.5f;
        skinnedMeshRenderer.material = GhostMaterial;
        yield return new WaitForSeconds(1.5f);

        animator.SetTrigger("GetUp");

         yield return new WaitForSeconds(2f);

        animator.speed = 1.2f;
    }
}
