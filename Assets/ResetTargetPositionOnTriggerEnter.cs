using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetTargetPositionOnTriggerEnter : MonoBehaviour
{



    void OnTriggerEnter(Collider other)
    {
        if (WorldMaskManager.Instance == null)
            return;

        SetColorMaterial scm = other.transform.GetComponent<SetColorMaterial>();
        if (scm != null)
        {
            Firefly atc = other.transform.GetComponentInParent<Firefly>();
            atc.transform.position = WorldMaskManager.Instance.forestTargets[scm.index].startPosition;
            atc.IsEquipped = false;
            scm.IsActive = false;
            //WorldMaskManager.Instance.forestTargets[scm.index].cloak = 1f;
        }
    }
}
