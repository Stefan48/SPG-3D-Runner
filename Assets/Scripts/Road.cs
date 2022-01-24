using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Road : MonoBehaviour {

    // Singleton Pattern
    private static Road instance;
    public static Road Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<Road>();
            }
            return instance;
        }
    }

    public string theme;

    public GameObject RegularTilePrefab { get; private set; }
    public GameObject CornerRightTilePrefab { get; private set; }
    public GameObject CornerLeftTilePrefab { get; private set; }
    public GameObject SplitSidesTilePrefab { get; private set; }
    public GameObject SplitRightTilePrefab { get; private set; }
    public GameObject SplitLeftTilePrefab { get; private set; }
    public GameObject SplitTriTilePrefab { get; private set; }


    // TODO - find the right storage max count value to minimize memory overhead
    // TODO - there's shouldn't be the same amount of each tile type in storage
    private const int tileStorageMaxCount = 30;
    private List<GameObject> regularTilesStorage = new List<GameObject>();
    private List<GameObject> cornerRightTilesStorage = new List<GameObject>();
    private List<GameObject> cornerLeftTilesStorage = new List<GameObject>();
    private List<GameObject> splitSidesTilesStorage = new List<GameObject>();
    private List<GameObject> splitRightTilesStorage = new List<GameObject>();
    private List<GameObject> splitLeftTilesStorage = new List<GameObject>();
    private List<GameObject> splitTriTilesStorage = new List<GameObject>();

    private const int maxTilesAhead = 4;
    private const int maxTilesBehind = 2;
    private int tilesBehind = 0;
    private List<GameObject> road = new List<GameObject>();

    public void IncrementTilesBehind()
    {
        tilesBehind++;
    }
    
    public GameObject GetRegularTile()
    {
        GameObject tile = regularTilesStorage.Last();
        regularTilesStorage.RemoveAt(regularTilesStorage.Count - 1);
        return tile;
    }

    public GameObject GetRandomTile()
    {
        GameObject tile;
        int type = Random.Range(0, 100);
        if (type < 20)
        {
            tile = cornerRightTilesStorage.Last();
            cornerRightTilesStorage.RemoveAt(cornerRightTilesStorage.Count - 1);
        }
        else if (type < 40)
        {
            tile = cornerLeftTilesStorage.Last();
            cornerLeftTilesStorage.RemoveAt(cornerLeftTilesStorage.Count - 1);
        }
        else if (type < 44)
        {
            tile = splitSidesTilesStorage.Last();
            splitSidesTilesStorage.RemoveAt(splitSidesTilesStorage.Count - 1);
        }
        else if (type < 48)
        {
            tile = splitRightTilesStorage.Last();
            splitRightTilesStorage.RemoveAt(splitRightTilesStorage.Count - 1);
        }
        else if (type < 52)
        {
            tile = splitLeftTilesStorage.Last();
            splitLeftTilesStorage.RemoveAt(splitLeftTilesStorage.Count - 1);
        }
        else if (type < 55)
        {
            tile = splitTriTilesStorage.Last();
            splitTriTilesStorage.RemoveAt(splitTriTilesStorage.Count - 1);
        }
        else
        {
            tile = regularTilesStorage.Last();
            regularTilesStorage.RemoveAt(regularTilesStorage.Count - 1);
        }
        return tile;
    }

    public void RecycleTile(GameObject tile)
    {
        tile.SetActive(false);
        if (tile.tag == "TileCornerRight")
        {
            cornerRightTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileCornerLeft")
        {
            cornerLeftTilesStorage.Add(tile);
        }
        // recycle split tiles recursively
        else if (tile.tag == "TileSplitSides")
        {
            List<GameObject> leftRoad = tile.GetComponent<SplitSidesTile>().leftRoad;
            List<GameObject> rightRoad = tile.GetComponent<SplitSidesTile>().rightRoad;
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            leftRoad.Clear();
            rightRoad.Clear();
            splitSidesTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitRight")
        {
            List<GameObject> frontRoad = tile.GetComponent<SplitRightTile>().frontRoad;
            List<GameObject> rightRoad = tile.GetComponent<SplitRightTile>().rightRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            rightRoad.Clear();
            splitRightTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitLeft")
        {
            List<GameObject> frontRoad = tile.GetComponent<SplitLeftTile>().frontRoad;
            List<GameObject> leftRoad = tile.GetComponent<SplitLeftTile>().leftRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            leftRoad.Clear();
            splitLeftTilesStorage.Add(tile);
        }
        else if (tile.tag == "TileSplitTri")
        {
            List<GameObject> frontRoad = tile.GetComponent<SplitTriTile>().frontRoad;
            List<GameObject> leftRoad = tile.GetComponent<SplitTriTile>().leftRoad;
            List<GameObject> rightRoad = tile.GetComponent<SplitTriTile>().rightRoad;
            foreach (GameObject t in frontRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in leftRoad)
            {
                RecycleTile(t);
            }
            foreach (GameObject t in rightRoad)
            {
                RecycleTile(t);
            }
            frontRoad.Clear();
            leftRoad.Clear();
            rightRoad.Clear();
            splitTriTilesStorage.Add(tile);
        }
        else
        {
            regularTilesStorage.Add(tile);
        }
    }

    public static void SetTilePositionAndRotation(GameObject spawnedTile, GameObject lastTile)
    {
        float spawnedTileRotation;
        if (lastTile.tag == "TileCornerRight")
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y + 90.0f;
        }
        else if (lastTile.tag == "TileCornerLeft")
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y - 90.0f;
        }
        else
        {
            spawnedTileRotation = lastTile.transform.eulerAngles.y;
        }
        spawnedTile.transform.position = lastTile.transform.Find("AttachPoint").transform.position;
        spawnedTile.transform.eulerAngles = new Vector3(0, spawnedTileRotation, 0);
    }

    public void SpawnTile()
    {
        if (tilesBehind > maxTilesBehind)
        {
            // erase road's oldest tile
            GameObject tile = road[0];
            RecycleTile(tile);
            road.RemoveAt(0);
        }

        GameObject lastTile = road.Last();
        if (lastTile.tag.Contains("TileSplit"))
        {
            lastTile.GetComponent<SplitTile>().Spawn();
        }
        else
        {
            GameObject spawnedTile;
            if (lastTile.tag.Contains("TileCorner"))
            {
                // there can't be consecutive corner tiles (must avoid overlapping)
                spawnedTile = GetRegularTile();
            }
            else
            {
                spawnedTile = GetRandomTile();
            }
            // compute tile's position and rotation based on previous tile
            SetTilePositionAndRotation(spawnedTile, lastTile);
            // set tile's name according to its index
            if (lastTile.name == "TileStart")
            {
                spawnedTile.name = "Tile1";
            }
            else
            {
                spawnedTile.name = "Tile" + (int.Parse(lastTile.name.Substring(4)) + 1).ToString();
            }
            spawnedTile.SetActive(true);
            road.Add(spawnedTile);
        }
    }

    public void MergeTilesIntoRoad(List<GameObject> tiles)
    {
        road.AddRange(tiles);
    }

	// Use this for initialization
	void Start ()
    {
        string prefabsPath = "Prefabs/" + theme + "/";
        RegularTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileRegular");
        CornerRightTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileCornerRight");
        CornerLeftTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileCornerLeft");
        SplitSidesTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileSplitSides");
        SplitRightTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileSplitRight");
        SplitLeftTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileSplitLeft");
        SplitTriTilePrefab = Resources.Load<GameObject>(prefabsPath + "TileSplitTri");

        // instantiate tiles
        for (int i = 0; i < tileStorageMaxCount; ++i)
        {
            // regular tile
            GameObject newTile = Instantiate(RegularTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            regularTilesStorage.Add(newTile);
            // corner right tile
            newTile = Instantiate(CornerRightTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            cornerRightTilesStorage.Add(newTile);
            // corner left tile
            newTile = Instantiate(CornerLeftTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            cornerLeftTilesStorage.Add(newTile);
            // split sides tile
            newTile = Instantiate(SplitSidesTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            splitSidesTilesStorage.Add(newTile);
            // split right tile
            newTile = Instantiate(SplitRightTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            splitRightTilesStorage.Add(newTile);
            // split left tile
            newTile = Instantiate(SplitLeftTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            splitLeftTilesStorage.Add(newTile);
            // split tri tile
            newTile = Instantiate(SplitTriTilePrefab, new Vector3(), Quaternion.identity);
            newTile.SetActive(false);
            splitTriTilesStorage.Add(newTile);
        }

        // initialize road
        road.Add(GameObject.Find("TileStart"));
        for (int i = 0; i < maxTilesAhead; ++i)
        {
            SpawnTile();
        }
        // TODO - fog

    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}
}