using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleHint : MonoBehaviour
{

    public RectTransform hintTransform;

    void Start()
    {
        if (hintTransform == null)
            Destroy(gameObject);



        hintTransform.gameObject.SetActive(false);
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            hintTransform.gameObject.SetActive(true);

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            hintTransform.gameObject.SetActive(false);

        }
    }
}
