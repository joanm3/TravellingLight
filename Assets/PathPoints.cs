using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class PathPoints : MonoBehaviour
{

    public GameObject pathPointPrefab;
    public bool loop = false;
    public Color gizmosColor = Color.red;
    public List<GameObject> pathPoints;

    public int pointsLastLength;




    void OnEnable()
    {
        pointsLastLength = pathPoints.Count;
    }

    void Update()
    {
        if (pointsLastLength != pathPoints.Count)
        {

            if (pointsLastLength < pathPoints.Count)
            {
                for (int i = pointsLastLength; i < pathPoints.Count; i++)
                {

#if UNITY_EDITOR
                    pathPoints[i] = (!Application.isPlaying) ? (GameObject)PrefabUtility.InstantiatePrefab(pathPointPrefab) : Instantiate(pathPointPrefab);
                    pathPoints[i].transform.parent = this.transform;
#else
                    pathPoints[i] = Instantiate(pathPointPrefab, this.transform);
#endif
                }
            }
            else if (pointsLastLength > pathPoints.Count)
            {

                //foreach (Transform child in transform)
                //{
                //    if (!pathPoints.Contains(child.gameObject))
                //    {
                //        DestroyImmediate(child.gameObject);
                //    }
                //}

                PathPoint[] points = GameObject.FindObjectsOfType<PathPoint>();
                for (int i = 0; i < points.Length; i++)
                {
                    if (!pathPoints.Contains(points[i].gameObject))
                    {
                        DestroyImmediate(points[i].gameObject);
                    }
                }
            }
            pointsLastLength = pathPoints.Count;
        }
    }


    void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count <= 0)
            return;

        Gizmos.color = gizmosColor;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathPoints[i].transform.position, pathPoints[i + 1].transform.position);
        }
        if (loop)
        {
            Gizmos.DrawLine(pathPoints[0].transform.position, pathPoints[pathPoints.Count - 1].transform.position);
        }
    }

    
}
