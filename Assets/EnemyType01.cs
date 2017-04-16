using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(FieldOfView))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyType01 : TwoWorldsBehaviour
{
    public PathPoints Path;

}
