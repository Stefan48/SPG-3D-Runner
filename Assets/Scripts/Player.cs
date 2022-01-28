using UnityEngine;
using UnityEngine.UI;

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

    public AudioSource jumpSound;
    public AudioSource stumbleSound;
    public AudioSource hitSound;
    public AudioSource crashSound;
    public AudioSource fallSound;
    public AudioSource woohooSound;
    public AudioSource clinkSound;

    private Rigidbody rb;
    private BoxCollider bc;
    private Vector3 bcInitialSize = new Vector3(0.4f, 1.8f, 0.4f);
    private Vector3 bcInitialCenter = new Vector3(0.0f, 0.9f, 0.0f);
    // TODO - player's speed should affect the animation speed
    private float speed;
    private const float leftRightVelocity = 2.0f;
    private const float uplift = 0.1f;

    public float score = 0;
    private int nextTarget = 50;
    private float scoreIncreaseAmount;
    private const float minScoreBetweenStumbles = 10.0f;
    private float scoreOnLastStumble = -15.0f;
    public int numGemsCollected = 0;

    public Text scoreText;
    public Text numGemsText;
    public GameObject optionsMenu;

    private bool isRunning = true;
    private bool onGround = true;
    private bool isFalling = false;
    private bool movingLeft = false;
    private bool movingRight = false;
    private bool isRolling = false;
    private const float maxJumpHeight = 2.0f;
    private float rollTime = 0.0f;
    private const float rollMaxTime = 1.0f;

    private Transform frontSideTransform;
    private Transform backSideTransform;
    private const float rayStartHeight = 0.6f;
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

    private GameObject land;

    public void UpdateScore(int playerScore)
    {
        scoreText.text = "Score: " + playerScore.ToString();
    }

    public void UpdateGems(int playerCollected)
    {
        numGemsText.text = "Gems: " + playerCollected.ToString();
    }

    private bool SideWallFound(Vector3 direction, float distance)
    {
        RaycastHit hit;
        Vector3 rayStartFront = new Vector3(frontSideTransform.position.x, rayStartHeight, frontSideTransform.position.z);
        Ray rayFront = new Ray(rayStartFront, direction);
        //Debug.DrawRay(rayFront.origin, rayFront.direction * 0.5f, Color.red, 7);
        Vector3 rayStartBack = new Vector3(backSideTransform.position.x, rayStartHeight, backSideTransform.position.z);
        Ray rayBack = new Ray(rayStartBack, direction);
        //Debug.DrawRay(rayBack.origin, rayBack.direction * 0.5f, Color.red, 7);
        // raycast using default layer mask (~0 == hit anything), ignoring triggers
        if (Physics.Raycast(rayFront, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.gameObject.tag == "Wall")
            {
                return true;
            }
        }
        else if (Physics.Raycast(rayBack, out hit, distance, ~0, QueryTriggerInteraction.Ignore))
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
        else if (other.tag == "FallInto")
        {
            // game end
            isRunning = false;
            rb.velocity = Vector3.zero;
            bc.size = bcInitialSize;
            bc.center = bcInitialCenter;
            // disable current tile's ground's non-trigger collider (which must be the first collider of the ground object)
            CurrentTile.transform.Find("Ground").GetComponent<Collider>().enabled = false;
            anim.SetTrigger("Fall Trigger");
            fallSound.Play();
            optionsMenu.SetActive(true);

        }
        else if (other.tag == "Gem")
        {
            other.gameObject.SetActive(false);
            numGemsCollected++;
            UpdateGems(numGemsCollected);
            clinkSound.Play();
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
                SplitTile splitTileComponent = tile.GetComponent<SplitTile>();
                Road.Instance.spawningGems = splitTileComponent.spawningGems;
                Road.Instance.numTilesWithGemsRemaining = splitTileComponent.numTilesWithGemsRemaining;
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
            // game end
            isRunning = false;
            rb.velocity = Vector3.zero;
            bc.size = bcInitialSize;
            bc.center = bcInitialCenter;
            anim.SetTrigger("Fall Back Trigger");
            crashSound.Play();
            optionsMenu.SetActive(true);
        }
        else if (collision.gameObject.tag == "Obstacle")
        {
            // if (transform.position.y <= 0.55f) => player stepped on obstacle
            // game end
            isRunning = false;
            rb.velocity = Vector3.zero;
            bc.size = bcInitialSize;
            bc.center = bcInitialCenter;
            anim.SetTrigger("Fall Back Trigger");
            crashSound.Play();
            optionsMenu.SetActive(true);
        }
        else
        {
            //Debug.Log("Ground contact");
        }
    }

    void Start()
    {
        speed = 6.0f;
        scoreIncreaseAmount = speed / 3.0f;
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        frontSideTransform = gameObject.transform.Find("FrontSide").transform;
        backSideTransform = gameObject.transform.Find("BackSide").transform;
        CurrentTile = GameObject.Find("TileStart");
        tileWidth = CurrentTile.transform.Find("Ground").localScale.x;
        cameraParams = GameObject.Find("CameraParams");
        animationObj = transform.Find("Animation");
        anim = animationObj.GetComponentInChildren<Animator>();
        land = GameObject.Find("Land");
    }

    void Update()
    {        
        if(!isRunning)
        {
            return;
        }

        score += scoreIncreaseAmount * Time.deltaTime;
        UpdateScore((int)score);
        if ((int)score != 0 && (int)score % nextTarget == 0)
        {
            // speed increases based on the score
            speed += speed / 10.0f;
            scoreIncreaseAmount = speed / 3.0f;
            nextTarget += 50;
            woohooSound.Play();
        }

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
                    // player stumbles
                    if (score - scoreOnLastStumble < minScoreBetweenStumbles)
                    {
                        // game ends at two close consecutive stumbles
                        isRunning = false;
                        rb.velocity = Vector3.zero;
                        bc.size = bcInitialSize;
                        bc.center = bcInitialCenter;
                        anim.SetTrigger("Fall Back Trigger");
                        hitSound.Play();
                        optionsMenu.SetActive(true);
                    }
                    else
                    {
                        scoreOnLastStumble = score;
                        anim.SetTrigger("Stumble Trigger");
                        stumbleSound.Play();
                    }
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
                    // player stumbles
                    if (score - scoreOnLastStumble < minScoreBetweenStumbles)
                    {
                        // game ends at two close consecutive stumbles
                        isRunning = false;
                        rb.velocity = Vector3.zero;
                        bc.size = bcInitialSize;
                        bc.center = bcInitialCenter;
                        anim.SetTrigger("Fall Back Trigger");
                        hitSound.Play();
                        optionsMenu.SetActive(true);
                    }
                    else
                    {
                        scoreOnLastStumble = score;
                        anim.SetTrigger("Stumble Trigger");
                        stumbleSound.Play();
                    }
                }
            }
        }
        // when jumping, the player shouldn't exit/re-enter the trigger of the current tile => larger colliders
        if (onGround && Input.GetKeyDown(KeyCode.W))
        {
            onGround = false;
            bc.size = new Vector3(bc.size.x, bc.size.y / 2.0f, bc.size.z);
            bc.center = new Vector3(bc.center.x, bc.center.y * 1.5f, bc.center.z);
            anim.SetTrigger("Jump Trigger");
            jumpSound.Play();

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
            float oldDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            float newDistance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (newDistance > oldDistance && newDistance >= tileWidth / 3.0f)
            {
                movingLeft = movingRight = false;
            }
            else
            {
                movingLeft = true;
                movingRight = false;
            }
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            Vector3 tileCenteredPoint = GetNearestPointCenteredBetweenCurrentTileEdges();
            Vector3 newPosition = transform.position + transform.right * leftRightVelocity * Time.fixedDeltaTime;
            float oldDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            float newDistance = Vector3.Distance(new Vector3(newPosition.x, 0, newPosition.z), new Vector3(tileCenteredPoint.x, 0, tileCenteredPoint.z));
            if (newDistance > oldDistance && newDistance >= tileWidth / 3.0f)
            {
                movingLeft = movingRight = false;
            }
            else
            {
                movingLeft = false;
                movingRight = true;
            }
        }
        else
        {
            movingLeft = movingRight = false;
        }
        //float amountToMove = speed * Time.deltaTime;
        //transform.Translate(Vector3.forward * amountToMove);
    }

    void FixedUpdate()
    {
        if(!isRunning)
        {
            return;
        }
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
                    bc.size = new Vector3(bc.size.x, bc.size.y * 2.0f, bc.size.z);
                    bc.center = new Vector3(bc.center.x, bc.center.y * 2.0f / 3.0f, bc.center.z);
                }
                else
                {
                    rb.velocity += Vector3.down * 5.0f;
                }
            }
            else
            {
                if (transform.position.y >= maxJumpHeight)
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