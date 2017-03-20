using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : Singleton<ColliderManager>
{


    public List<ColliderController> colliderControllers;

    public enum WorldAppartenance
    {
        world1, world2
    }
    public WorldAppartenance currentWorld = WorldAppartenance.world1;


    void Start()
    {
        ReloadColliderControllers();
    }

    void ReloadColliderControllers()
    {
        ColliderController[] _col = FindObjectsOfType<ColliderController>();
        for (int i = 0; i < _col.Length; i++)
        {
            colliderControllers.Add(_col[i]);
        }
        ChangeColliders();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ChangeColliders();
        }
    }

    public void ChangeColliders()
    {
        if (currentWorld == WorldAppartenance.world1)
        {
            currentWorld = WorldAppartenance.world2;
        }

        else if (currentWorld == WorldAppartenance.world2)
        {
            currentWorld = WorldAppartenance.world1;
        }

        for (int i = 0; i < colliderControllers.Count; i++)
        {
            if (currentWorld == WorldAppartenance.world1 && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.world1)
            {
                colliderControllers[i].SetCollider(true);              
            }
            if (currentWorld == WorldAppartenance.world1 && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.world2)
            {
                colliderControllers[i].SetCollider(false);
            }
            if (currentWorld == WorldAppartenance.world2 && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.world1)
            {
                colliderControllers[i].SetCollider(false);
            }
            if (currentWorld == WorldAppartenance.world2 && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.world2)
            {
                colliderControllers[i].SetCollider(true);
            }
        }
    }
	
}
