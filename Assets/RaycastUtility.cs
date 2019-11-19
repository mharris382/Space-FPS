using System;
using UnityEngine;

public static class RaycastUtility
{
    public static bool RaycastForHit(Transform origin, out RaycastHit hit, Action<RaycastHit> onHit = null)
    {
        Ray ray = new Ray(origin.position, origin.forward);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            onHit?.Invoke(hit);
            return true;
        }
        return false;
    }
    public static bool RaycastForHit<T>(Transform origin, out RaycastHit hit, Action<RaycastHit, T> onHit = null)
    {
        Ray ray = new Ray(origin.position, origin.forward);
        if (Physics.Raycast(ray, out hit, 1000))
        {
            var t = hit.collider.GetComponent<T>();
            if (t == null) return false;
            onHit?.Invoke(hit, t);
            return true;
        }
        return false;
    }
}