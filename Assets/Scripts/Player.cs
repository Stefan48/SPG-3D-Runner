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
    private const float leftRightSpeed = 2.0f;
    public bool onGround = true;
    public bool isFalling = false;
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
            Debug.Log("Ground contact");
            //rb.AddForce(0.0f, 100.0f, 0.0f);
        }
        
    }

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        //rb.velocity = Vector3.forward * speed;
        //rb.AddForce(Vector3.forward * 150f);
        //rb.AddRelativeForce(Vector3.forward * 150f);

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
        // TODO - jump and slide
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
        if (Input.GetKeyDown(KeyCode.W))
        {
            onGround = false;
        }
        // TODO - when jumping, the player shouldn't exit/re-enter the trigger of the current tile

        // arrows movement (must prevent player from hitting a wall)
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Vector3 tileCenteredPoint = GetNearestPointCenteredBetweenCurrentTileEdges();
            Vector3 newPosition = transform.position - transform.right * leftRightSpeed * Time.deltaTime;
            float distance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (distance < tileWidth / 3.0f)
            {
                //transform.Translate(Vector3.left * leftRightSpeed * Time.deltaTime);
                transform.position = newPosition;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 tileCenteredPoint = GetNearestPointCenteredBetweenCurrentTileEdges();
            Vector3 newPosition = transform.position + transform.right * leftRightSpeed * Time.deltaTime;
            float distance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (distance < tileWidth / 3.0f)
            {
                //transform.Translate(Vector3.right * leftRightSpeed * Time.deltaTime);
                transform.position = newPosition;
            }
        }
        //float amountToMove = speed * Time.deltaTime;
        //transform.Translate(Vector3.forward * amountToMove);

    }

    private void FixedUpdate()
    {

        transform.Translate(Vector3.up * 0.0001f);

        //rb.AddForce(Vector3.up * 100.0f * Time.fixedDeltaTime);

        //rb.AddForce(Vector3.up * 4910.0f * Time.fixedDeltaTime);

        rb.velocity = transform.forward * 75 * Time.fixedDeltaTime;


        /*
        // Get the velocity
        Vector3 direction = rb.velocity;
        direction.y = 0.0f;
        // Calculate the approximate distance that will be traversed
        float distance = direction.magnitude * Time.fixedDeltaTime;

        direction.Normalize();
        RaycastHit hit;
        if (rb.SweepTest(direction, out hit, distance))
        {
            //aboutToCollide = true;
            //distanceToCollision = hit.distance;
            //rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            //rb.useGravity = false;
            Debug.Log("about to collide");
            //rb.AddForce(0.0f, 10.0f, 0.0f);
            //rb.velocity = rb.velocity + 100000 * transform.up * Time.fixedDeltaTime;
        }
        rb.AddForce(Vector3.forward * 150f);
        */

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
                    rb.velocity += Vector3.down * 2.0f;
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
                }
            }
            //rb.AddForce(Vector3.up * 300.0f, ForceMode.Impulse);
            //rb.velocity += Vector3.up * 100.0f;
        }
    }
}