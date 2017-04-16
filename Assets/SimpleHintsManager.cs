using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleHintsManager : MonoBehaviour {

    [SerializeField]
    public struct Hint
    {
        public Canvas hintCanvas;
        public BoxCollider boxTrigger;
        public float timeToShow; 
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
