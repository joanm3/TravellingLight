using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

public class MenuScript : MonoBehaviour {

    public WorldMaskManager bubbleControler;
    public float openSpeed;
    bool actived = false;
    public int sceneNumber;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            print("ON IT ");
            actived = true;           
        }

        if (actived && bubbleControler.forestTargets[0].cloak >= 0)
        {
            bubbleControler.forestTargets[0].cloak -= openSpeed * Time.deltaTime;
        }
        else
        {
            //SceneManager.LoadScene(sceneNumber);
            //Application.LoadLevel(sceneNumber);
        }
    }
}
