using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
            {
                // Singleton pattern
                instance = FindObjectOfType<Player>();
            }
            return instance;
        }
    }

    private Rigidbody rb;
    private BoxCollider bc;
    // TODO - player's speed should affect the animation speed
    public float speed;
    private const float leftRightVelocity = 2.0f;
    private const float uplift = 0.1f;
    
    private bool onGround = true;
    private bool isFalling = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool isRolling = false;
    private float rollTime = 0.0f;
    private const float rollMaxTime = 1.0f;

    private Transform frontSideTransform;
    private Transform backSideTransform;
    public GameObject CurrentTile { get; private set; }
    // used for the ray length when checking if there's a wall on either side of the player
    private float tileWidth;
    // player shouldn't be able to change direction more than once on the same tile
    private bool changedDirectionOnCurrentTile = false;
    // used when the player chooses the direction on a split tile
    Vector3 chosenDirection = Vector3.forward;
    // object used by the camera (follows the player except for jumps/slides and left/right movements)
    private GameObject cameraParams;

    private Transform animationObj;
    private Animator anim;
    

    private bool SideWallFound(Vector3 direction, float distance)
    {
        RaycastHit hit;
        Vector3 rayStartFront = frontSideTransform.position;
        Ray rayFront = new Ray(rayStartFront, direction);
        Debug.DrawRay(rayFront.origin, rayFront.direction * 0.5f, Color.red, 7);
        Vector3 rayStartBack = backSideTransform.position;
        Ray rayBack = new Ray(rayStartBack, direction);
        Debug.DrawRay(rayBack.origin, rayBack.direction * 0.5f, Color.red, 7);
        // raycast using default layer mask (~0 == hit anything); ignore triggers
        if (Physics.Raycast(rayFront, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("front ray hit " + hit.collider.name);
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        else if (Physics.Raycast(rayBack, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            Debug.Log("back ray hit " + hit.collider.name);
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        else Debug.Log("no ray hit");
        return false;
    }

    public Vector3 GetNearestPointCenteredBetweenCurrentTileEdges()
    {
        Vector3 currentTilePosition = CurrentTile.transform.position;
        return Vector3.Project(transform.position - currentTilePosition, transform.forward) + currentTilePosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Contains("Ground"))
        {
            CurrentTile = other.transform.parent.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        
        if (other.tag.Contains("Ground"))
        {
            Road.Instance.IncrementTilesBehind();
            GameObject tile = other.gameObject.transform.parent.gameObject;
            if (tile.tag.Contains("TileSplit"))
            {
                // when the player leaves a split tile, merge the chosen branch into the main road and recycle the other branch(es)
                if (changedDirectionOnCurrentTile)
                {
                    StartCoroutine(tile.GetComponent<SplitTile>().MergeIntoRoad(chosenDirection));
                }
                else
                {
                    StartCoroutine(tile.GetComponent<SplitTile>().MergeIntoRoad(Vector3.forward));
                }
            }
            changedDirectionOnCurrentTile = false;
            Road.Instance.SpawnTile();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            speed = 0;
            Debug.Log("Wall collision");
            // TODO - crash animation
        }
        else
        {
            //Debug.Log("Ground contact");
        }
        
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        frontSideTransform = gameObject.transform.Find("FrontSide").transform;
        backSideTransform = gameObject.transform.Find("BackSide").transform;
        CurrentTile = GameObject.Find("TileStart");
        tileWidth = CurrentTile.transform.Find("Ground").localScale.x;
        cameraParams = GameObject.Find("CameraParams");
        animationObj = transform.Find("Animation");
        anim = animationObj.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRolling)
        {
            rollTime += Time.deltaTime;
            if (rollTime >= rollMaxTime)
            {
                isRolling = false;
                bc.size = new Vector3(bc.size.x, bc.size.y * 2.0f, bc.size.z);
                bc.center = new Vector3(bc.center.x, bc.center.y * 2.0f, bc.center.z);
            }
        }

        // WASD movement
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (!changedDirectionOnCurrentTile)
            {
                if (!SideWallFound(-transform.right, tileWidth))
                {
                    changedDirectionOnCurrentTile = true;
                    chosenDirection = Vector3.left;
                    transform.Rotate(0, -90, 0);
                    cameraParams.transform.Rotate(0, -90, 0);
                }
                else
                {
                    // TODO - player stumbles
                    anim.SetTrigger("Stumble Trigger");
                }
            }            
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {        
            if (!changedDirectionOnCurrentTile)
            {
                if (!SideWallFound(transform.right, tileWidth))
                {
                    changedDirectionOnCurrentTile = true;
                    chosenDirection = Vector3.right;
                    transform.Rotate(0, 90, 0);
                    cameraParams.transform.Rotate(0, 90, 0);
                }
                else
                {
                    // TODO - player stumbles
                    anim.SetTrigger("Stumble Trigger");
                }
            }
        }
        // TODO - jump and roll/slide animations
        // when jumping, the player shouldn't exit/re-enter the trigger of the current tile => larger colliders
        if (onGround && Input.GetKeyDown(KeyCode.W))
        {
            onGround = false;
            anim.SetTrigger("Jump Trigger");
        }
        else if (onGround && !isRolling && Input.GetKeyDown(KeyCode.S))
        {
            isRolling = true;
            rollTime = 0.0f;
            bc.size = new Vector3(bc.size.x, bc.size.y / 2.0f, bc.size.z);
            bc.center = new Vector3(bc.center.x, bc.center.y / 2.0f, bc.center.z);
            if (Random.Range(0, 100) < 50)
            {
                anim.SetTrigger("Roll Trigger");
            }
            else
            {
                anim.SetTrigger("Crawl Trigger");
            }
        }
        
        // arrows movement (must prevent player from hitting a wall)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 tileCenteredPoint = GetNearestPointCenteredBetweenCurrentTileEdges();
            Vector3 newPosition = transform.position - transform.right * leftRightVelocity * Time.fixedDeltaTime;
            float distance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (distance < tileWidth / 3.0f)
            {
                movingLeft = true;
                movingRight = false;
            }
            else
            {
                movingLeft = movingRight = false;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 tileCenteredPoint = GetNearestPointCenteredBetweenCurrentTileEdges();
            Vector3 newPosition = transform.position + transform.right * leftRightVelocity * Time.fixedDeltaTime;
            float distance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (distance < tileWidth / 3.0f)
            {                
                movingLeft = false;
                movingRight = true;
            }
            else
            {
                movingLeft = movingRight = false;
            }
        }
        else
        {
            movingLeft = movingRight = false;
        }
        //float amountToMove = speed * Time.deltaTime;
        //transform.Translate(Vector3.forward * amountToMove);
    }

    private void FixedUpdate()
    {

        // small translation so that the player doesn't get stuck when entering a new tile (even though the ground is flat)
        transform.Translate(Vector3.up * uplift * Time.fixedDeltaTime);
        rb.velocity = transform.forward * speed;

        if (!onGround)
        {
            if (isFalling)
            {
                if (transform.position.y <= 0.51f)
                {
                    onGround = true;
                    isFalling = false;
                    transform.position = new Vector3(transform.position.x, 0.5f, transform.position.z);
                }
                else
                {
                    rb.velocity += Vector3.down * 5.0f;
                }
            }
            else
            {
                if (transform.position.y >= 1.5f)
                {
                    isFalling = true;
                }
                else
                {
                    rb.velocity += Vector3.up * 5.0f;
                    //rb.AddForce(Vector3.up * 100.0f, ForceMode.Impulse);
                }                
            }
        }
        if (movingLeft)
        {
            rb.velocity += -transform.right * leftRightVelocity;
        }
        else if (movingRight)
        {
            rb.velocity += transform.right * leftRightVelocity;
        }
    }
}