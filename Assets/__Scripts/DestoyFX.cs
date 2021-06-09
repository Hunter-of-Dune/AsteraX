using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestoyFX : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(DestroyAfterTime());
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
