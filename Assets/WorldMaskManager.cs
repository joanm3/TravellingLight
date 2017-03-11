using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class WorldMaskManager : Singleton<WorldMaskManager>
{
    public WorldMaskVariables worldMaskGlobalVariables = new WorldMaskVariables();
    public bool showHidden = true;
    public float standardHeight = 5f;

    [Header("Forest")]
    public GameObject forestPrefab;
    public List<WorldTarget> forestTargets = new List<WorldTarget>();

    [Header("City")]
    public GameObject cityPrefab;
    public List<WorldTarget> cityTargets = new List<WorldTarget>();


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

    int oldForestCount;
    int oldCityCount;
    Shader maskShader;


    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Init();
        }

        oldForestCount = forestTargets.Count;
        oldCityCount = cityTargets.Count;
        maskShader = Shader.Find("Custom/WorldSpaceNoiseMask");


        if (Application.isPlaying)
        {

            for (int i = 0; i < forestTargets.Count; i++)
            {
                forestTargets[i].maskMaterial.SetFloat("_Clip", 1f);
            }
            //maskMaterial.SetFloat("_Clip", 1f);
        }

    }

    void Update()
    {
        UpdateList(ref forestTargets, ref oldForestCount, forestPrefab);
        UpdateList(ref cityTargets, ref oldCityCount, cityPrefab);


        //if (oldForestCount < forestTargets.Count)
        //{
        //    for (int i = oldForestCount; i < forestTargets.Count; i++)
        //    {
        //        forestTargets[i].target = Instantiate(forestPrefab, transform);
        //        forestTargets[i].target.transform.position = new Vector3(0f, standardHeight, 0f);
        //        forestTargets[i].target.transform.rotation = Quaternion.identity;
        //        forestTargets[i].target.name = forestPrefab.name + "_" + (i + 1).ToString();
        //        forestTargets[i].cloak = 0f;
        //    }
        //}
        //else if (oldForestCount > forestTargets.Count)
        //{
        //    Debug.Log("reading list");

        //    List<GameObject> toDelete = FindAllInstances(forestPrefab, oldForestCount, forestTargets.Count);
        //    for (int i = 0; i < toDelete.Count; i++)
        //    {
        //        //put it to a pool later on better. 
        //        DestroyImmediate(toDelete[i]);
        //    }
        //}
        //oldForestCount = forestTargets.Count;

    }


    void UpdateList(ref List<WorldTarget> targets, ref int oldCount, GameObject prefab)
    {
        if (oldCount < targets.Count)
        {
            for (int i = oldCount; i < targets.Count; i++)
            {
                targets[i].target = (!Application.isPlaying) ? (GameObject)PrefabUtility.InstantiatePrefab(prefab) : Instantiate(prefab);
                targets[i].target.transform.parent = this.transform;
                targets[i].target.transform.position = new Vector3(0f, standardHeight, 0f);
                targets[i].target.transform.rotation = Quaternion.identity;
                targets[i].target.name = prefab.name + "_" + (i + 1).ToString();
                targets[i].cloak = 0f;
                //I THINK BETTER TO USE ONLY SHARED MATERIAL FOR ALL OF THEM NO NEED TO MULTIPLY. 
                targets[i].maskMaterial = (Application.isPlaying) ? GetComponent<MeshRenderer>().material : GetComponent<MeshRenderer>().sharedMaterial;
                if (targets[i].maskMaterial == null)
                    targets[i].maskMaterial = (Application.isPlaying) ? GetComponentInChildren<MeshRenderer>().material : GetComponentInChildren<MeshRenderer>().sharedMaterial;

                if (!targets[i].maskMaterial.shader.Equals(maskShader))
                {
                    Debug.LogError("shader is not correct for " + name + ", please assign " + maskShader.name);
                }

            }
        }
        else if (oldForestCount > targets.Count)
        {
            List<GameObject> toDelete = FindAllInstances(prefab, oldCount, targets.Count);
            for (int i = 0; i < toDelete.Count; i++)
            {
                //put it to a pool later on better. 
                DestroyImmediate(toDelete[i]);
            }
        }
        oldCount = targets.Count;
    }

    List<GameObject> FindAllInstances(GameObject myPrefab, int oldLength, int newLength)
    {
        List<GameObject> result = new List<GameObject>();
        for (int i = oldLength; i > newLength; i--)
        {
            GameObject obj = GameObject.Find(myPrefab.name + "_" + i) as GameObject;
            if (obj != null)
            {
                result.Add(obj);
            }
        }
        return result;
    }
}

[System.Serializable]
public class WorldMaskVariables
{
    public Vector3 NoiseSettings;
    public Color LineColor;
    [Range(0, 1)]
    public float CircleForce;
    [Range(0, 1)]
    public float InnerExpand;

}

[System.Serializable]
public class WorldTarget
{
    public GameObject target;
    [Range(0, 1)]
    public float cloak = 0f;
    [HideInInspector]
    public Material maskMaterial;
}
