using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using ProjectLight.Functions;

[ExecuteInEditMode]
public class WorldMaskManager : Singleton<WorldMaskManager>
{
    public GameObject parent;
    public WorldMaskVariables worldMaskGlobalVariables = new WorldMaskVariables();
    public bool showHidden = true;
    public float standardHeight = 5f;

    [Header("Forest")]
    public GameObject forestPrefab;
    public List<WorldTarget> forestTargets = new List<WorldTarget>();
    public List<Material> forestMaterials = new List<Material>();

    [Header("City")]
    public GameObject cityPrefab;
    public List<WorldTarget> cityTargets = new List<WorldTarget>();
    public List<Material> cityMaterials = new List<Material>();




    [Header("Read Only")]
    public float[] forestCloaks;
    public float[] cityCloaks;

    public Vector4[] forestTargetPositions;
    public Vector4[] cityTargetPositions;

    int oldForestCount;
    int oldCityCount;
    Shader maskShader;


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
    public Transform target5;
    [Range(0, 1)]
    public float cloak5;
    public Transform target6;
    [Range(0, 1)]
    public float cloak6;

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
    public Transform targetE;
    [Range(0, 1)]
    public float cloakE;
    public Transform targetF;
    [Range(0, 1)]
    public float cloakF;


    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            Init();
        }

        oldForestCount = forestTargets.Count;
        oldCityCount = cityTargets.Count;
        maskShader = Shader.Find("Custom/WorldSpaceNoiseMask");

        LoadMaterials();
        UpdateLengths();
        UpdateCloaks();
        //if (forestTargets != null && forestTargets.Count > 0)
        //{
        UpdateTargetPositions(forestTargets, ref forestTargetPositions);
        //}
        //if (forestTargets != null && forestTargets.Count > 0)
        //{
        UpdateTargetPositions(cityTargets, ref cityTargetPositions);
        //}

        if (Application.isPlaying)
        {
            //if (forestMaterials != null && forestMaterials.Count < 0)
            //{
            for (int i = 0; i < forestMaterials.Count; i++)
            {
                forestMaterials[i].SetFloat("_Clip", 1f);
            }
            //}
            //if (cityMaterials != null && forestMaterials.Count < 0)
            //{
            for (int i = 0; i < cityMaterials.Count; i++)
            {
                cityMaterials[i].SetFloat("_Clip", 1f);
            }
            //}
            ResetCloaks(true);
        }

    }

    void Update()
    {
        UpdateLengths();
        UpdateList(ref forestTargets, ref oldForestCount, forestPrefab);
        UpdateList(ref cityTargets, ref oldCityCount, cityPrefab);
        UpdateTargetPositions(forestTargets, ref forestTargetPositions);
        UpdateTargetPositions(cityTargets, ref cityTargetPositions);
        UpdateCloaks();
        UpdateShaderVariables(forestMaterials, forestTargets, forestCloaks, forestTargetPositions);
        UpdateShaderVariables(cityMaterials, cityTargets, cityCloaks, cityTargetPositions);

        if (!Application.isPlaying)
        {
            if (Instance.showHidden)
            {
                for (int i = 0; i < forestMaterials.Count; i++)
                {
                    forestMaterials[i].SetFloat("_Clip", 0f);
                }

                for (int i = 0; i < cityMaterials.Count; i++)
                {
                    cityMaterials[i].SetFloat("_Clip", 0f);
                }
            }
            else
            {
                for (int i = 0; i < forestMaterials.Count; i++)
                {
                    forestMaterials[i].SetFloat("_Clip", 1f);
                }

                for (int i = 0; i < cityMaterials.Count; i++)
                {
                    cityMaterials[i].SetFloat("_Clip", 1f);
                }
            }
        }
    }


    void LoadMaterials()
    {

        forestMaterials.Clear();
        cityMaterials.Clear();

        WorldType[] wt = GameObject.FindObjectsOfType<WorldType>();

        for (int i = 0; i < wt.Length; i++)
        {
            switch (wt[i].worldType)
            {
                case WorldType.InWorld.Forest:
                    if (!forestMaterials.Contains(wt[i].GetComponent<MeshRenderer>().sharedMaterial))
                        forestMaterials.Add(wt[i].GetComponent<MeshRenderer>().sharedMaterial);
                    break;
                case WorldType.InWorld.City:
                    if (!cityMaterials.Contains(wt[i].GetComponent<MeshRenderer>().sharedMaterial))
                        cityMaterials.Add(wt[i].GetComponent<MeshRenderer>().sharedMaterial);
                    break;
            }
        }
    }


    void UpdateList(ref List<WorldTarget> targets, ref int oldCount, GameObject prefab)
    {
        //if (targets.Count == 0)
        //    return;

        if (oldCount < targets.Count)
        {
            for (int i = oldCount; i < targets.Count; i++)
            {
#if UNITY_EDITOR
                targets[i].target = (!Application.isPlaying) ? (GameObject)PrefabUtility.InstantiatePrefab(prefab) : Instantiate(prefab);
#else
                targets[i].target = Instantiate(prefab);

#endif
                targets[i].target.transform.parent = (parent) ? parent.transform : null;
                targets[i].target.transform.position = new Vector3(0f, standardHeight, 0f);
                targets[i].target.transform.rotation = Quaternion.identity;
                targets[i].target.name = prefab.name + "_" + (i + 1).ToString();
                targets[i].cloak = 0f;
                targets[i].index = i;
                targets[i].colorSetter = targets[i].target.transform.GetComponentInAll<SetColorMaterial>();
                targets[i].colorSetter.index = targets[i].index;
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


    void UpdateCloaks()
    {
        //if (forestTargets.Count < 0)
        //{
        if (forestCloaks.Length != forestTargets.Count)
        {
            forestCloaks = new float[forestTargets.Count];
        }
        for (int i = 0; i < forestTargets.Count; i++)
        {
            forestCloaks[i] = forestTargets[i].cloak;
        }
        //}

        //if (cityTargets.Count < 0)
        //{
        if (cityCloaks.Length != cityTargets.Count)
        {
            cityCloaks = new float[cityTargets.Count];
        }
        for (int i = 0; i < cityTargets.Count; i++)
        {
            cityCloaks[i] = cityTargets[i].cloak;
        }
        //}

    }

    void UpdateDistances()
    {

    }

    void ResetCloaks(bool equalOne)
    {
        float value = (equalOne) ? 1f : 0f;

        //if (forestTargets != null)
        //{
        for (int i = 0; i < forestTargets.Count; i++)
        {
            forestTargets[i].cloak = value;
        }
        //}
        //if (cityTargets != null)
        //{
        for (int i = 0; i < cityTargets.Count; i++)
        {
            cityTargets[i].cloak = value;
        }
        //}
    }

    void UpdateLengths()
    {
        for (int i = 0; i < forestMaterials.Count; i++)
        {
            forestMaterials[i].SetFloat("_Length", forestTargets.Count);
        }
        for (int i = 0; i < cityMaterials.Count; i++)
        {
            cityMaterials[i].SetFloat("_Length", cityTargets.Count);
        }
    }


    void UpdateTargetPositions(List<WorldTarget> targets, ref Vector4[] targetPositions)
    {
        //if (targets == null)
        //    return;
        //if (targets.Count <= 0)
        //    return;

        if (targetPositions.Length != targets.Count)
        {
            targetPositions = new Vector4[targets.Count];
        }
        for (int i = 0; i < targetPositions.Length; i++)
        {
            targetPositions[i] = targets[i].target.GetComponentInAll<SinusMovement>().transform.position;
            if (targetPositions[i] == null)
                targetPositions[i] = targets[i].target.transform.position;
            //targetPositions[i].w = 0f;
        }
    }


    void UpdateShaderVariables(List<Material> matList, List<WorldTarget> worldTargets, float[] cloakValues, Vector4[] targetPositions)
    {
        for (int i = 0; i < matList.Count; i++)
        {
            matList[i].SetVector("_NoiseSettings2", Instance.worldMaskGlobalVariables.NoiseSettings);
            matList[i].SetVector("_LineColor", Instance.worldMaskGlobalVariables.LineColor);
            matList[i].SetFloat("_CircleForce", Instance.worldMaskGlobalVariables.CircleForce);
            matList[i].SetFloat("_LineWidth", Instance.worldMaskGlobalVariables.LineWidth);
            matList[i].SetFloat("_Expand", Instance.worldMaskGlobalVariables.InnerExpand);
            //if (worldTargets[i].useGlobalDistance)
            //{
            matList[i].SetFloat("_ChangePoint", Instance.worldMaskGlobalVariables.GlobalChangeDistance);
            //}
            //else
            //{
            //    matList[i].SetFloat("_ChangePoint", worldTargets[i].localChangeDistance);
            //}

            matList[i].SetFloat("_Length", worldTargets.Count);
            matList[i].SetFloatArray("_Cloaks", cloakValues);
            matList[i].SetVectorArray("_Positions", targetPositions);
        }
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
    [Range(0, 0.99f)]
    public float LineWidth = 0.2f;
    [Range(0, 1)]
    public float CircleForce;
    [Range(0, 1)]
    public float InnerExpand;
    public float GlobalChangeDistance = 10f;

}

[System.Serializable]
public class WorldTarget
{
    public GameObject target;
    public SetColorMaterial colorSetter;
    public int index = 0;
    [Range(0, 1)]
    public float cloak = 0f;
    //public bool useGlobalDistance = true;
    //public float localChangeDistance = 10f;
}
