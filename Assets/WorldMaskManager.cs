using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMaskManager : Singleton<WorldMaskManager>
{
    public List<GameObject> targets = new List<GameObject>();

    // Use this for initialization
    override protected void OnAwake()
    {
        GameObject[] _targets = GameObject.FindGameObjectsWithTag("WorldTarget");
        for (int i = 0; i < _targets.Length; i++)
        {
            targets.Add(_targets[i]);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
