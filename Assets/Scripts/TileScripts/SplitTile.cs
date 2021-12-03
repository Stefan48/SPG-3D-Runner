﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SplitTile : MonoBehaviour {

    // make sure split tiles' branches get merged before the actual split tiles get recycled due to being the oldest of the road
    // TODO - delay time as a function of player's speed
    protected const float mergeUnusedBranchDelay = 0.5f;

    // used to spawn a tile on each branch
    public abstract void Spawn();

    // used the merge the chosen branch into the main road and recycle the other branch(es)
    public abstract IEnumerator MergeIntoRoad(Vector3 chosenDirection);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
