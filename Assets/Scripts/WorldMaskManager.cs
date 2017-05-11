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

    [Header("Read Only")]
    public float[] forestCloaks;
    public float[] cityCloaks;

    public Vector4[] forestTargetPositions;
    public Vector4[] cityTargetPositions;

    int oldForestCount;
    int oldCityCount;


    void OnEnable()
    {
        Init();

        oldForestCount = forestTargets.Count;

        LoadMaterials();
        UpdateLengths();
        UpdateCloaks();
        UpdateTargetPositions(forestTargets, ref forestTargetPositions);

        if (Application.isPlaying)
        {
            for (int i = 0; i < forestMaterials.Count; i++)
            {
                forestMaterials[i].SetFloat("_Clip", 1f);
            }
            ResetCloaks(true);
            for (int i = 0; i < forestTargets.Count; i++)
            {
                forestTargets[i].startPosition =
                    new Vector3(forestTargets[i].target.transform.position.x, forestTargets[i].target.transform.position.y, forestTargets[i].target.transform.position.z);
            }
        }
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            UpdateLengths();
            UpdateList(ref forestTargets, ref oldForestCount, forestPrefab);
        }
        if (forestMaterials.Count <= 0 || forestTargets.Count <= 0 || forestPrefab == null)
            return;

        UpdateTargetPositions(forestTargets, ref forestTargetPositions);
        UpdateCloaks();
        UpdateShaderVariables(forestMaterials, forestTargets, forestCloaks, forestTargetPositions);

        if (!Application.isPlaying)
        {
            if (Instance.showHidden)
            {
                for (int i = 0; i < forestMaterials.Count; i++)
                {
                    forestMaterials[i].SetFloat("_Clip", 0f);
                }
            }
            else
            {
                for (int i = 0; i < forestMaterials.Count; i++)
                {
                    forestMaterials[i].SetFloat("_Clip", 1f);
                }
            }
        }
    }


    void LoadMaterials()
    {

        forestMaterials.Clear();
        //cityMaterials.Clear();

        WorldType[] wt = GameObject.FindObjectsOfType<WorldType>();

        for (int i = 0; i < wt.Length; i++)
        {
            if (!forestMaterials.Contains(wt[i].GetComponent<Renderer>().sharedMaterial))
                forestMaterials.Add(wt[i].GetComponent<Renderer>().sharedMaterial);
        }
    }


    void UpdateList(ref List<WorldTarget> targets, ref int oldCount, GameObject prefab)
    {
        //this should only be possible in edit mode, change later. 
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

                targets[i].firefly = targets[i].target.GetComponent<Firefly>();
                if (targets[i].firefly == null) Debug.LogError("no firefly component to the instantiated prefab");
                targets[i].targetChildTransform = targets[i].firefly.targetTransform;
                Debug.Log(targets[i].targetChildTransform);
                targets[i].cloak = 0f;
                targets[i].index = i;
                targets[i].firefly.index = i;
                targets[i].colorSetter = targets[i].firefly.colorSetter;
                targets[i].colorSetter.index = targets[i].index;
                targets[i].particles = targets[i].firefly.particles;
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

        if (forestCloaks.Length != forestTargets.Count)
        {
            forestCloaks = new float[forestTargets.Count];
        }
        for (int i = 0; i < forestTargets.Count; i++)
        {
            forestCloaks[i] = forestTargets[i].cloak;
            forestTargets[i].firefly.cloak = forestTargets[i].cloak;
        }
    }

    void ResetCloaks(bool equalOne)
    {
        float value = (equalOne) ? 1f : 0f;

        for (int i = 0; i < forestTargets.Count; i++)
        {
            forestTargets[i].cloak = value;
        }

    }

    void UpdateLengths()
    {
        for (int i = 0; i < forestMaterials.Count; i++)
        {
            forestMaterials[i].SetFloat("_Length", forestTargets.Count);
        }

    }


    void UpdateTargetPositions(List<WorldTarget> targets, ref Vector4[] targetPositions)
    {
        if (targetPositions.Length != targets.Count)
        {
            targetPositions = new Vector4[targets.Count];
        }
        for (int i = 0; i < targetPositions.Length; i++)
        {
            targetPositions[i] = targets[i].target.GetComponentInAll<SinusMovement>().transform.position;
            if (targetPositions[i] == null)
                targetPositions[i] = targets[i].target.transform.position;
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

            if (worldTargets[i].firefly.useGlobalDistance)
            {
                matList[i].SetFloat("_ChangePoint", Instance.worldMaskGlobalVariables.GlobalChangeDistance);
                //IMPORTANT
                //TODO TODO TODO TODO TODO 
                //matList[i].SetFloatArray("_ChangePoints",)
                //Debug.Log("is there");
            }
            else
            {
                Debug.Log("is here");
                matList[i].SetFloat("_ChangePoint", worldTargets[i].firefly.changeDistance);
            }

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
    public Firefly firefly;
    public GameObject target;
    public bool isEquipped = false;
    public SetColorMaterial colorSetter;
    public int index = 0;
    [Range(0, 1)]
    public float cloak = 0f;
    public Vector3 startPosition;
    public Transform targetChildTransform;
    public ParticleSystem particles;
    public bool useGlobalDistance = true;
    public float changeDistance = 10f;
    //public bool useGlobalDistance = true;
    //public float localChangeDistance = 10f;
}
