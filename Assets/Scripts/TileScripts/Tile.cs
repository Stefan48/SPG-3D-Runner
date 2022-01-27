using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public GameObject obstacle = null;
    public ObstacleType obstacleType = ObstacleType.None;

    public int numGemPositions;
    public List<GameObject> gems = new List<GameObject>();

    public int numDecorationPositions;
    public List<GameObject> decorations = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
