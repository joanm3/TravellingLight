using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideCursor : MonoBehaviour
{

    void Start()
    {
#if !UNITY_EDITOR
        Cursor.visible = false;
#endif
    }

}
