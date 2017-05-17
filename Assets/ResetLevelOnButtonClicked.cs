using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetLevelOnButtonClicked : MonoBehaviour
{

    public KeyCode keyToPress = KeyCode.R;


    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
