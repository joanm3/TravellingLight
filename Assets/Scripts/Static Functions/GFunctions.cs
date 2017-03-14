using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectLight.Functions
{

    public static class Functions
    {

        public static T GetComponentInAll<T>(this Transform transform)
        {
            T t = transform.GetComponent<T>();
            if (t == null)
                t = transform.GetComponentInChildren<T>();
            if (t == null)
                t = transform.GetComponentInParent<T>();
            return t;
        }

        public static T GetComponentInAll<T>(this GameObject gameObject)
        {
            T t = gameObject.GetComponent<T>();
            if (t == null)
                t = gameObject.GetComponentInChildren<T>();
            if (t == null)
                t = gameObject.GetComponentInParent<T>();
            return t;
        }

        public static Renderer GetRendererFromCollision(RaycastHit hit)
        {
            Renderer _colliderRend = (Renderer)hit.collider.GetComponent<MeshRenderer>();
            if (_colliderRend == null)
                _colliderRend = (Renderer)hit.collider.GetComponent<MeshRenderer>();
            if (_colliderRend == null)
                _colliderRend = (Renderer)hit.collider.GetComponent<SkinnedMeshRenderer>();
            return _colliderRend;
        }

        public static int SumLayers(LayerMask first, LayerMask second)
        {
            return first.value + second.value;
        }

        public static int SumLayers(LayerMask first, LayerMask second, LayerMask third)
        {
            return first.value + second.value + third.value;
        }

        /// <summary>
        /// Transforms an old range to a new one and then returns the valueToTransform in the new range
        /// </summary>
        /// <param name="valueToTransform"> value to transform in new range and then interpolate</param>
        /// <param name="oldMin"> old min will be transformed to new min</param>
        /// <param name="oldMax">old max will be transformed to new max</param>
        /// <param name="newMin"></param>
        /// <param name="newMax"></param>
        /// <returns></returns>
        public static float MapRange(float valueToTransform, float oldMin, float oldMax, float newMin, float newMax)
        {
            float oldRange = oldMax - oldMin;
            float newRange = newMax - newMin;
            return (((valueToTransform - oldMin) * newRange) / oldRange) + newMin;
        }

        /// <summary>
        /// Transforms a value in a range to a normalized range from 0 to 1. 
        /// </summary>
        /// <param name="valueToTransform">the value to interpolate</param>
        /// <param name="oldMin">old min = 0 in normalized range</param>
        /// <param name="oldMax">old max = 1 in normalized range</param>
        /// <returns></returns>
        public static float NormalizeRange(float valueToTransform, float oldMin, float oldMax)
        {
            float oldRange = oldMax - oldMin;
            return ((valueToTransform - oldMin) / oldRange);
        }

    }


}