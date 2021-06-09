using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add to objects that need to wrap around the screen.
/// </summary>
public class OffScreenWrapper : MonoBehaviour {

    private void Update()
    {
        return;
    }

    private void OnTriggerExit(Collider other)
    {
        // only trigger when it is the parent
        if (transform.root == transform)
        {
            Vector3 relativeLocation = other.bounds.ClosestPoint(transform.position);

            if (Mathf.Abs(relativeLocation.x) >= other.bounds.extents.x - 1)
            {
                transform.position = new Vector3(-transform.position.x, transform.position.y);
            }
            if (Mathf.Abs(relativeLocation.y) >= other.bounds.extents.y - 1)
            {
                transform.position = new Vector3(transform.position.x, -transform.position.y);
            }
         }

    }

}
