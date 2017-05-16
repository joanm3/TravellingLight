using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class BridgeSceneCameraController : MonoBehaviour
{
    public Camera playerCam;
    public Camera firstCam;
    public Camera secondCam;
    public Camera thirdCam;
    public float changeToSecondCameraAtFollowCurvePos = 0.5f;
    public float timeToChangeToThirdCam = 10f;
    public float timeToChangeToPlayer = 10f;

    public CanvasGroup canvasGroup;
    public float fadeTime = 1f;
    public BottleSphere bottleSphere;
    public FollowCurve followCurve;
    public MeshRenderer river;
    public float riverFadeSpeed = 0.2f;
    public GameObject[] flocks;
    public StudioEventEmitter backgroundMusic;

    public bool firstChangeDone = false;
    public bool secondChangeDone = false;
    public bool thirdChangeDone = false;
    public bool toPlayerChangeDone = false;
    public float volumeFadeInSpeed = 1f;


    private float currentAlpha = 0f;
    private float timer = 0f;

    private Material riverMat;
    private float riverRimSize = 2f;
    private float riverTransp = 0.435f;
    private float riverRefraction = 0.1f;
    private float riverValue;

    [SerializeField]
    private float sRiverRimSize = 2f;
    [SerializeField]
    private float sRiverTransp = 0.435f;
    [SerializeField]
    private float sRiverRefraction = 0.1f;

    private float currentVolume;

    void Start()
    {
        firstCam.enabled = false;
        secondCam.enabled = false;
        river.gameObject.SetActive(false);
        riverMat = river.material;
        sRiverRimSize = riverMat.GetFloat("_RimSize");
        sRiverTransp = riverMat.GetFloat("_Transparency");
        sRiverRefraction = riverMat.GetFloat("_RefractionAmount");
        river.gameObject.GetComponent<BoxCollider>().enabled = false;

        riverMat.SetFloat("_RimSize", 0f);
        riverMat.SetFloat("_Transparency", 0f);
        riverMat.SetFloat("_RefractionAmount", 0f);

        //if (flocks.Length < 0) return;
        //foreach (var flock in flocks)
        //{
        //    flock.SetActive(false);
        //}

    }

    void Update()
    {
        if (bottleSphere.isFull && !firstChangeDone)
        {
            StartCoroutine(FadeInOutCameras(playerCam, firstCam));
            StartCoroutine(ChangeVolume());
            firstChangeDone = true;
        }


        if (followCurve.pos > changeToSecondCameraAtFollowCurvePos && !secondChangeDone)
        {
            StartCoroutine(FadeInOutCameras(firstCam, secondCam));
            secondChangeDone = true;
        }

        if (secondChangeDone && followCurve.pos >= 1f && !thirdChangeDone)
        {
            if (!thirdChangeDone) timer += Time.deltaTime;

            if (timer > timeToChangeToThirdCam && !thirdChangeDone)
            {
                StartCoroutine(FadeInOutCameras(secondCam, thirdCam));
                thirdChangeDone = true;
                river.gameObject.SetActive(true);
                river.gameObject.GetComponent<BoxCollider>().enabled = true;
                StartCoroutine(FadeRiverIn());
                timer = 0f;
                if (flocks.Length > 0)
                {
                    foreach (var flock in flocks)
                    {
                        flock.SetActive(true);
                    }
                }
            }
        }



        if (thirdChangeDone)
        {
            timer += Time.deltaTime;
            if (timer > timeToChangeToPlayer && !toPlayerChangeDone)
            {
                StartCoroutine(FadeInOutCameras(thirdCam, playerCam));
                toPlayerChangeDone = true;
            }
        }
    }



    private IEnumerator ChangeVolume()
    {


        while (currentVolume < 1f)
        {
            currentVolume += Time.deltaTime * volumeFadeInSpeed;
            backgroundMusic.SetParameter("LevelEnded", currentVolume);
            yield return new WaitForEndOfFrame();
        }
        currentVolume = 1f;
        backgroundMusic.SetParameter("LevelEnded", currentVolume);

    }

    private IEnumerator FadeRiverIn()
    {
        while (riverValue < sRiverRimSize)
        {
            riverValue += Time.deltaTime * riverFadeSpeed;

            riverMat.SetFloat("_RimSize", Mathf.Min(sRiverRimSize, riverValue));
            riverMat.SetFloat("_Transparency", Mathf.Min(sRiverTransp, riverValue));
            riverMat.SetFloat("_RefractionAmount", Mathf.Min(sRiverRefraction, riverValue));
            yield return new WaitForEndOfFrame();
        }

        riverMat.SetFloat("_RimSize", sRiverRimSize);
        riverMat.SetFloat("_Transparency", sRiverTransp);
        riverMat.SetFloat("_RefractionAmount", sRiverRefraction);
    }


    private IEnumerator FadeInOutCameras(Camera firstCamera, Camera secondCamera)
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeTime;
            yield return new WaitForEndOfFrame();
        }
        canvasGroup.alpha = 1f;
        secondCamera.enabled = true;
        firstCamera.enabled = false;
        //StartCoroutine(FadeOutCanvas());

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeTime;
            yield return new WaitForEndOfFrame();
        }
        canvasGroup.alpha = 0f;

    }


    private IEnumerator FadeOutCanvas()
    {
        Debug.Log("enterd second coroutine");
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha += Time.deltaTime * fadeTime;
            yield return new WaitForEndOfFrame();
        }
        canvasGroup.alpha = 0f;
    }
}
