using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : Singleton<ColliderManager>
{


    public List<ColliderController> colliderControllers; 


    void Start()
    {
        ColliderController[] _col = FindObjectsOfType<ColliderController>();
        for (int i = 0; i < _col.Length; i++)
        {
            colliderControllers.Add(_col[i]); 
        }
    }

    public void ChangeColliders()
    {
        for (int i = 0; i < colliderControllers.Count; i++)
        {
            colliderControllers[i].DoSomething(); 
        }
    }
	
}
