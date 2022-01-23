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
    // TODO - player's speed should affect the animation speed
    public float speed;
    private const float leftRightVelocity = 2.0f;
    private const float uplift = 0.1f;
    
    private bool onGround = true;
    private bool isFalling = false;
    private bool movingLeft = false;
    private bool movingRight = false;
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
    

    private bool SideWallFound(Vector3 direction, float distance)
    {
        RaycastHit hit;
        Vector3 rayStartFront = frontSideTransform.position;
        Ray rayFront = new Ray(rayStartFront, direction);
        Debug.DrawRay(rayFront.origin, rayFront.direction * 0.5f, Color.red, 7);
        Vector3 rayStartBack = backSideTransform.position;
        Ray rayBack = new Ray(rayStartBack, direction);
        Debug.DrawRay(rayBack.origin, rayBack.direction * 0.5f, Color.red, 7);
        if (Physics.Raycast(rayFront, out hit, distance))
        {
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        else if (Physics.Raycast(rayBack, out hit, distance))
        {
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true;
            }
        }
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
        frontSideTransform = gameObject.transform.Find("FrontSide").transform;
        backSideTransform = gameObject.transform.Find("BackSide").transform;
        CurrentTile = GameObject.Find("TileStart");
        tileWidth = Resources.Load<GameObject>("Prefabs/TileRegular").transform.Find("Ground").localScale.x;
        cameraParams = GameObject.Find("CameraParams");
    }

    // Update is called once per frame
    void Update()
    {
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
                }
            }
        }
        // TODO - jump and roll/slide animations
        // when jumping, the player shouldn't exit/re-enter the trigger of the current tile => larger colliders
        if (Input.GetKeyDown(KeyCode.W))
        {
            onGround = false;
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
        rb.velocity = transform.forward * speed * Time.fixedDeltaTime;

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
        else
        {
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
}