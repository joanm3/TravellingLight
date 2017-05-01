using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PathPoints : MonoBehaviour
{

    public GameObject pathPointPrefab;

    [Tooltip("for the moment everything loops")]
    public bool loop = false;
    public Color gizmosColor = Color.red;
    public List<GameObject> points;

    public int pointsLastLength;




    void Update()
    {

#if UNITY_EDITOR
        if (Application.isPlaying)
            return;
#else
        return; 
#endif


        if (pointsLastLength != points.Count)
        {

            if (pointsLastLength < points.Count)
            {
                for (int i = pointsLastLength; i < points.Count; i++)
                {

#if UNITY_EDITOR
                    points[i] = (GameObject)PrefabUtility.InstantiatePrefab(pathPointPrefab);
                    points[i].transform.parent = this.transform;
#endif

                }
            }
            else if (pointsLastLength > points.Count)
            {
                PathPoint[] points = GameObject.FindObjectsOfType<PathPoint>();
                for (int i = 0; i < points.Length; i++)
                {
                    if (!this.points.Contains(points[i].gameObject))
                    {
                        DestroyImmediate(points[i].gameObject);
                    }
                }
            }
            pointsLastLength = points.Count;
        }
    }

    internal int GetNextIndex(int destIndex)
    {
        throw new NotImplementedException();
    }

    void OnDrawGizmos()
    {
        if (points == null || points.Count <= 0)
            return;

        Gizmos.color = gizmosColor;
        for (int i = 0; i < points.Count - 1; i++)
        {
            Gizmos.DrawLine(points[i].transform.position, points[i + 1].transform.position);
        }
        if (loop)
        {
            Gizmos.DrawLine(points[0].transform.position, points[points.Count - 1].transform.position);
        }
    }


}
