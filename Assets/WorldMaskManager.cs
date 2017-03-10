using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WorldMaskManager : Singleton<WorldMaskManager>
{
    //public List<GameObject> targets = new List<GameObject>();

    public WorldMaskVariables worldMaskGlobalVariables = new WorldMaskVariables();
    public bool showHidden = true;

    [Header("Forest")]
    public Transform target1;
    [Range(0, 1)]
    public float cloak1;
    public Transform target2;
    [Range(0, 1)]
    public float cloak2;
    public Transform target3;
    [Range(0, 1)]
    public float cloak3;
    public Transform target4;
    [Range(0, 1)]
    public float cloak4;

    [Header("City")]
    public Transform targetA;
    [Range(0, 1)]
    public float cloakA;
    public Transform targetB;
    [Range(0, 1)]
    public float cloakB;
    public Transform targetC;
    [Range(0, 1)]
    public float cloakC;
    public Transform targetD;
    [Range(0, 1)]
    public float cloakD;


    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Init();
        }
    }

    //// Use this for initialization
    //override protected void OnAwake()
    //{
    //    GameObject[] _targets = GameObject.FindGameObjectsWithTag("WorldTarget");
    //    for (int i = 0; i < _targets.Length; i++)
    //    {
    //        targets.Add(_targets[i]);
    //    }
    //}

    //// Update is called once per frame
    //void Update()
    //{

    //}
}

[System.Serializable]
public class WorldMaskVariables
{
    public Vector3 NoiseSettings;
    public Color LineColor;
    [Range(0,1)]
    public float CircleForce;
    [Range(0, 1)]
    public float InnerExpand;

}
