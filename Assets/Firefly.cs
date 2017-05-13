using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firefly : MonoBehaviour
{

    [SerializeField]
    private bool isEquipped = false;
    public int index = 0;
    //[Range(0, 1)]
    //public float cloak = 0f;
    public bool useGlobalDistance = true;
    public float changeDistance = 10f;
    public SetColorMaterial colorSetter;
    public Vector3 startPosition;
    public Transform targetTransform;
    public ParticleSystem particles;
    public float goToBottleSpeed = 2f;
    public bool canActivate = true;

    //[Header("Big Firefly")]
    //public bool isBigFirefly = true;
    //public int numberOfFirefliesNeeded = 3;
    //public GameObject[] objectsToShowWhenWithEachFirefly;

    [Header("Read values")]
    public bool isInLightIntegratorZone = false;
    public bool integrateToBottle = false;
    public BottleSphere currentBottleSphere;
    public bool consumedByBottleSphere = false;
    public float startingScale;

    private SetColorMaterial colorMat;
    private GameObject player;
    private LightIntegrator lightIntegrator;

    public Renderer waterRenderer;
    public Material waterMaterial;
    public float startingTransparency;
    public float currentTransparency;

    void Start()
    {
        startingScale = transform.localScale.x;
        colorMat = GetComponentInChildren<SetColorMaterial>();
        if (!waterRenderer) waterRenderer = colorMat.GetComponentInChildren<SkinnedMeshRenderer>();
        waterMaterial = waterRenderer.material;
        startingTransparency = waterMaterial.GetFloat("_Transparency");
        currentTransparency = startingTransparency;
        if (colorMat == null)
        {
            Debug.LogError("color material null for " + this.name);
            Destroy(this);
        }
        player = GameObject.FindGameObjectWithTag("Player");
        lightIntegrator = player.GetComponentInChildren<LightIntegrator>();
    }

    void Update()
    {
        if (useGlobalDistance) changeDistance = WorldMaskManager.Instance.worldMaskGlobalVariables.GlobalChangeDistance;


        #region ASSIGN TO BIG SPHERE
        if (!isEquipped && integrateToBottle)
        {
            float step = goToBottleSpeed * Time.deltaTime;
            if (currentBottleSphere != null) transform.position = Vector3.MoveTowards(transform.position, currentBottleSphere.transform.position, step);
            float transparencyStep = Time.deltaTime * 0.2f;
            currentTransparency = Mathf.Max(0f, currentTransparency - transparencyStep);
            var emission = particles.emission;
            emission.enabled = false;
            waterMaterial.SetFloat("_Transparency", currentTransparency);
            waterMaterial.SetFloat("_RefractionAmount", currentTransparency);

            colorMat.IsActive = false;
            if (currentBottleSphere != null && Vector3.Distance(transform.position, currentBottleSphere.transform.position) < 0.1f)
            {
                transform.position = currentBottleSphere.transform.position;
                waterMaterial.SetFloat("_Transparency", 0f);
                waterMaterial.SetFloat("_RefractionAmount", 0f);
                integrateToBottle = false;
                consumedByBottleSphere = true;
                //IsEquipped = false;
            }
        }
        #endregion

    }

    public bool IsEquipped
    {
        get { return isEquipped; }
        set
        {
            isEquipped = value;
            OnIsEquippedChanged(isEquipped);
        }
    }

    void OnIsEquippedChanged(bool equiped)
    {
        if (equiped)
        {
            lightIntegrator.AssignedFireflies.Add(this);
            if (particles != null)
            {
                var emission = particles.emission;
                emission.enabled = false;
                particles.gameObject.SetActive(false);
            }
            colorMat.IsActive = false;
            isInLightIntegratorZone = false;
        }
        else
        {
            lightIntegrator.AssignedFireflies.Remove(this);
            if (particles != null)
            {
                particles.gameObject.SetActive(true);
                var emission = particles.emission;
                emission.enabled = true;
            }
            colorMat.IsActive = true;
            isInLightIntegratorZone = true;
        }
        colorMat.IsLightEnabled = !equiped;
    }





}
