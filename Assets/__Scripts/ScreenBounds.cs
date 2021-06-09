//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// NOTE: If Camera.main is going to move or rotate at all, then it will need to
//  have a Rigidbody attached so that the physics engine properly updates the 
//  position and rotation of this BoxCollider.

/// <summary>
/// This class should be attached to a child of Camera.main. It triggers various
///  behaviors to happen when a GameObject exits the screen.<para/>
/// NOTE: Camera.main must be orthographic.<para/>
/// NOTE: This GameObject must have a BoxCollider attached.
/// </summary>
[RequireComponent(typeof(BoxCollider))]
public class ScreenBounds : MonoBehaviour
{
    static private ScreenBounds S; // Private but unprotected Singleton.

    BoxCollider boxColl;


    void Awake()
    {
        S = this;

        // No need to check whether boxColl is null because of RequireComponent above.
        boxColl = GetComponent<BoxCollider>();
    }



    static public Bounds BOUNDS
    {
        get
        {
            if (S == null)
            {
                Debug.LogError("ScreenBounds.BOUNDS - ScreenBounds.S is null!");
                return new Bounds();
            }
            if (S.boxColl == null)
            {
                Debug.LogError("ScreenBounds.BOUNDS - ScreenBounds.S.boxColl is null!");
                return new Bounds();
            }
            return S.boxColl.bounds;
        }
    }


    static public bool OOB(Vector3 worldPos)
    {
        Vector3 locPos = S.transform.InverseTransformPoint(worldPos);
        // Find in which dimension the locPos is furthest from the origin
        float maxDist = Mathf.Max( Mathf.Abs(locPos.x), Mathf.Abs(locPos.y), Mathf.Abs(locPos.z) );
        // If that furthest distance is >0.5f, then worldPos is out of bounds
        return (maxDist > 0.5f);
    }


    static public int OOB_X(Vector3 worldPos)
    {
        Vector3 locPos = S.transform.InverseTransformPoint(worldPos);
        return OOB_(locPos.x);
    }
    static public int OOB_Y(Vector3 worldPos)
    {
        Vector3 locPos = S.transform.InverseTransformPoint(worldPos);
        return OOB_(locPos.y);
    }
    static public int OOB_Z(Vector3 worldPos)
    {
        Vector3 locPos = S.transform.InverseTransformPoint(worldPos);
        return OOB_(locPos.z);
    }


    static private int OOB_(float num)
    {
        if (num > 0.5f) return 1;
        if (num < -0.5f) return -1;
        return 0;
    }
}


