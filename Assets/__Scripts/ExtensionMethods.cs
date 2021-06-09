using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Not mine. Found online.
public static class ExtensionMethods
{
    public static T[] GetComponentsInDirectChildren<T>(this Component parent) where T : Component
    {
        List<T> tmpList = new List<T>();

        foreach (Transform transform in parent.transform)
        {
            T component;
            if ((component = transform.GetComponent<T>()) != null)
            {
                tmpList.Add(component);
            }
        }

        return tmpList.ToArray();
    }
}
