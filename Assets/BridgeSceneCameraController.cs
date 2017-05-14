using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BridgeSceneCameraController : MonoBehaviour
{
    public Camera playerCam;
    public Camera firstCam;
    public Camera secondCam;
    public float changeToSecondCameraAtFollowCurvePos = 0.5f;
    public float timeToComebackToPlayer = 10f;
    public CanvasGroup canvasGroup;
    public float fadeTime = 1f;
    public BottleSphere bottleSphere;
    public FollowCurve followCurve;
    public MeshRenderer river;

    public bool firstChangeDone = false;
    public bool secondChangeDone = false;
    public bool comeBacktoPlayerDone = false;

    private float currentAlpha = 0f;
    private float timer = 0f;

    void Start()
    {
        firstCam.enabled = false;
        secondCam.enabled = false;
        river.gameObject.SetActive(false);
    }

    void Update()
    {
        if (bottleSphere.isFull && !firstChangeDone)
        {
            StartCoroutine(FadeInOutCameras(playerCam, firstCam));
            firstChangeDone = true;
        }


        if (followCurve.pos > changeToSecondCameraAtFollowCurvePos && !secondChangeDone)
        {
            StartCoroutine(FadeInOutCameras(firstCam, secondCam));
            secondChangeDone = true;
        }

        if (secondChangeDone)
        {
            timer += Time.deltaTime;

            if (timer > timeToComebackToPlayer && !comeBacktoPlayerDone)
            {
                StartCoroutine(FadeInOutCameras(secondCam, playerCam));
                comeBacktoPlayerDone = true;
                river.gameObject.SetActive(true);
            }
        }
    }


    private IEnumerator FadeInOutCameras(Camera firstCamera, Camera secondCamera)
    {
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeTime;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("arrived here");
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
