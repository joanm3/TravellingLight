using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMeshManagement : MonoBehaviour {

    public GameObject player;
    public GameObject sphere;
    public float detectDist;

	// Use this for initialization
	void Start () {
        GetComponent<UnityEngine.AI.NavMeshAgent>().destination = sphere.transform.position;
    }
	
	// Update is called once per frame
	void Update () {
		if (Vector3.Distance(player.transform.position,gameObject.transform.position) <= detectDist)
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().destination = player.transform.position;
        }
        else
        {
            GetComponent<UnityEngine.AI.NavMeshAgent>().destination = sphere.transform.position;
        }

	}
}
