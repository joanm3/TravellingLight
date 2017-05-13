using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeWhenEnabling : MonoBehaviour
{

    public Renderer[] renderers;
    public float[] startingTranparencies;
    public float[] currentTransparencies;

    public bool finishedFading = false;

    void Awake()
    {
        if (renderers.Length <= 0) renderers = GetComponentsInChildren<Renderer>(true);
        startingTranparencies = new float[renderers.Length];
        currentTransparencies = new float[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            startingTranparencies[i] = renderers[i].material.GetFloat("_Transparency");
            currentTransparencies[i] = startingTranparencies[i];
        }
    }



    public IEnumerator FadeIn(float speed)
    {
        while (!finishedFading)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                currentTransparencies[i] += Time.deltaTime * speed;
                renderers[i].material.SetFloat("_Transparency", currentTransparencies[i]);
                renderers[i].material.SetFloat("_RefractionAmount", currentTransparencies[i]);
            }

            if (renderers[renderers.Length - 1].material.GetFloat("_Transparency") > startingTranparencies[renderers.Length - 1])
            {
                finishedFading = true;
            }
            yield return new WaitForEndOfFrame();
        }

        if (finishedFading)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_Transparency", startingTranparencies[i]);
                renderers[i].material.SetFloat("_RefractionAmount", startingTranparencies[i]);

                currentTransparencies[i] = startingTranparencies[i];
            }
            yield return new WaitForEndOfFrame();
        }


        yield return null;
    }

    public IEnumerator FadeOut(float speed)
    {

        while (!finishedFading)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                currentTransparencies[i] -= Time.deltaTime * speed;
                renderers[i].material.SetFloat("_Transparency", currentTransparencies[i]);
                renderers[i].material.SetFloat("_RefractionAmount", currentTransparencies[i]);
            }

            if (renderers[renderers.Length - 1].material.GetFloat("_Transparency") <= 0f)
            {
                finishedFading = true;
            }
            yield return new WaitForEndOfFrame();

        }

        if (finishedFading)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.SetFloat("_Transparency", 0f);
                renderers[i].material.SetFloat("_RefractionAmount", 0f);
                currentTransparencies[i] = 0f;
            }
            //yield return new WaitForEndOfFrame();
            gameObject.SetActive(false);
        }

        yield return null;
    }
}
