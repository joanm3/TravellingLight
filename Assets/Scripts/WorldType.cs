using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WorldType : MonoBehaviour
{

    public enum InWorld { Forest, City };
    public InWorld worldType = InWorld.Forest;
}
