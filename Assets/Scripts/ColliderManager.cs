using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderManager : Singleton<ColliderManager>
{

    public List<ColliderController> colliderControllers;

    public enum WorldAppartenance
    {
        worldInSphere, worldOutSphere
    }
    public WorldAppartenance currentWorld = WorldAppartenance.worldInSphere;


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


    public void ChangeColliders()
    {
        if (currentWorld == WorldAppartenance.worldInSphere)
        {
            currentWorld = WorldAppartenance.worldOutSphere;
        }

        else if (currentWorld == WorldAppartenance.worldOutSphere)
        {
            currentWorld = WorldAppartenance.worldInSphere;
        }

        for (int i = 0; i < colliderControllers.Count; i++)
        {
            if (currentWorld == WorldAppartenance.worldInSphere && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldInSphere)
            {
                colliderControllers[i].SetCollider(true);
            }
            if (currentWorld == WorldAppartenance.worldInSphere && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldOutSphere)
            {
                colliderControllers[i].SetCollider(false);
            }
            if (currentWorld == WorldAppartenance.worldOutSphere && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldInSphere)
            {
                colliderControllers[i].SetCollider(false);
            }
            if (currentWorld == WorldAppartenance.worldOutSphere && colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldOutSphere)
            {
                colliderControllers[i].SetCollider(true);
            }
        }
    }

    public void SetWorldAndColliders(bool isInsideZone)
    {
        if (isInsideZone)
        {
            currentWorld = WorldAppartenance.worldInSphere;
            for (int i = 0; i < colliderControllers.Count; i++)
            {
                if (colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldInSphere)
                {
                    colliderControllers[i].SetCollider(true);
                }
                else
                {
                    colliderControllers[i].SetCollider(false);
                }
            }
        }
        else
        {
            currentWorld = WorldAppartenance.worldOutSphere;
            for (int i = 0; i < colliderControllers.Count; i++)
            {
                if (colliderControllers[i].gameObjectWorld == ColliderController.WorldAppartenance.worldInSphere)
                {
                    colliderControllers[i].SetCollider(false);
                }
                else
                {
                    colliderControllers[i].SetCollider(true);
                }
            }
        }



    }

}
