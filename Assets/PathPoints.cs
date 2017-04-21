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
    public List<GameObject> Points;

    public int pointsLastLength;




    void OnEnable()
    {
        pointsLastLength = Points.Count;
    }

    void Update()
    {

#if UNITY_EDITOR
        if (Application.isPlaying)
            return;
#else
        return;
#endif
        if (pointsLastLength != Points.Count)
        {

            if (pointsLastLength < Points.Count)
            {
                for (int i = pointsLastLength; i < Points.Count; i++)
                {

#if UNITY_EDITOR
                    Points[i] = (!Application.isPlaying) ? (GameObject)PrefabUtility.InstantiatePrefab(pathPointPrefab) : Instantiate(pathPointPrefab);
                    Points[i].transform.parent = this.transform;
#else
                    pathPoints[i] = Instantiate(pathPointPrefab, this.transform);
#endif
                }
            }
            else if (pointsLastLength > Points.Count)
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
                    if (!Points.Contains(points[i].gameObject))
                    {
                        DestroyImmediate(points[i].gameObject);
                    }
                }
            }
            pointsLastLength = Points.Count;
        }
    }


    public int GetNextIndex(int currIndex)
    {
        if (currIndex == Points.Count - 1)
            return 0;
        else
            return currIndex + 1;
    }

    public int GetPreviousIndex(int currIndex)
    {
        if (currIndex == 0)
            return Points.Count - 1;
        else
            return currIndex - 1;
    }

    void OnDrawGizmos()
    {
        if (Points == null || Points.Count <= 0)
            return;

        Gizmos.color = gizmosColor;
        for (int i = 0; i < Points.Count - 1; i++)
        {
            Gizmos.DrawLine(Points[i].transform.position, Points[i + 1].transform.position);
        }
        if (loop)
        {
            Gizmos.DrawLine(Points[0].transform.position, Points[Points.Count - 1].transform.position);
        }
    }


}
