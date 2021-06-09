using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static int hitInARowCounter = 0;
    public float speed = 20f;
    [SerializeField] private float lifeTime = 2f;
    public Rigidbody rigid;

    private ParticleSystem.EmissionModule emitter;
    private bool isLuckyShot = false;

    public delegate void BulletHit(bool lucky, int hitsInRow);
    public static BulletHit HIT;

    public delegate void BulletFired();
    public static BulletFired FIRED;

    // Use this for initialization
    void Start()
    {
        emitter = GetComponentInChildren<ParticleSystem>().emission;

        StartCoroutine(DestroyAfterTime());

        if(FIRED != null)
        {
            FIRED.Invoke();
        }
    }

    // LateUpdate is called once per frame, after all Updates have been called.
    void LateUpdate()
    {
        if (ScreenBounds.OOB(transform.position))
        {
            emitter.enabled = false;
            isLuckyShot = true;
        }
        else
        {
            emitter.enabled = true;
        }
    }

    private IEnumerator DestroyAfterTime()
    {
        yield return new WaitForSeconds(lifeTime);
        hitInARowCounter = 0;
        Destroy(gameObject);
    }

    // hitInARow and HIT.Invoke are for multiple achievements
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 9)
        {
            hitInARowCounter++;
            if (HIT != null)
            {
                HIT.Invoke(isLuckyShot, hitInARowCounter);
            }
            Destroy(gameObject);
        }
    }
}
