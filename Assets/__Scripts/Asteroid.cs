using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(OffScreenWrapper))]
public class Asteroid : MonoBehaviour
{
    [Header("Set In Inspector")]
    [SerializeField] float minSpeed = 5f;
    [SerializeField] float maxSpeed = 10f;

    public int size = 3;

    Rigidbody rigid;
    OffScreenWrapper offScreenWrapper;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        offScreenWrapper = GetComponent<OffScreenWrapper>();
    }

    void Start()
    {
        //AsterX.Instance.AddAsteroid(this);
        /// <summary>
        /// Spawns an asteroid on the death boundry. Somewhere off the screen and points it roughly towards the center of the screen.
        /// </summary>
        
    }

    public void Initialize()
    {
        if (FindParent() == null)
        {
            InitAsteroidParent();
        }
        else
        {
            InitAsteroidChild();
        }

        // Spawn two child Asteroids
        if (size > 1)
        {
            Asteroid ast;
            for (int i = 0; i < 2; i++)
            {
                ast = AsterX.Instance.GetPooledAsteroid();
                ast.gameObject.SetActive(true);
                ast.gameObject.transform.position = transform.position;
                ast.size = size - 1;
                ast.transform.SetParent(transform);
                Vector3 relPos = Random.onUnitSphere / 2f;
                ast.transform.rotation = Random.rotation;
                ast.transform.localPosition = relPos;
                ast.Initialize();
            }
        }
        if (FindParent() != null)
        {
            InitAsteroidChild();
        }

        transform.localScale = Vector3.one * size * 0.60f;
    }

    public void MergeMeshs()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;

            i++;
        }
        Mesh m = new Mesh();
        m.name = gameObject.name;
        m.CombineMeshes(combine);
        GetComponent<MeshCollider>().sharedMesh = m;        
    }

    Asteroid FindParent()
    {
        if (transform.parent != null)
        {
            Asteroid parentAsteroid = transform.parent.GetComponent<Asteroid>();
            if (parentAsteroid != null)
            {
                return parentAsteroid;
            }
        }
        return null;
    }

    public void InitAsteroidParent()
    {
        transform.position = AsterX.Instance.deathBounds.ClosestPoint(Random.insideUnitCircle.normalized * 20);

        offScreenWrapper.enabled = true;
        rigid.isKinematic = false;
        // Snap this GameObject to the z=0 plane
        Vector3 pos = transform.position;
        pos.z = 0;
        transform.position = pos;
        // Initialize the velocity for this Asteroid
        InitVelocity();
        MergeMeshs();
    }

    public void InitAsteroidChild()
    {
        offScreenWrapper.enabled = false;
        rigid.isKinematic = true;

        if(size > 1)
        {
            MergeMeshs();
        }
    }

    public void InitVelocity()
    {
        Vector3 vel;

        // Point it toward a point near the center of the sceen
        vel = ((Vector3)Random.insideUnitCircle * 5) - transform.position;
        vel.Normalize();

        // Multiply the unit length of vel by the correct speed (randomized) for this size of Asteroid
        vel = vel * Random.Range(minSpeed, maxSpeed) / (float)size;

        rigid.AddForce(vel * 2, ForceMode.VelocityChange);

        rigid.angularVelocity = Random.insideUnitSphere / 2;
    }

    private void OnCollisionEnter(Collision collision)
    {
            // ensure any collision is sent to the root
            if (transform.parent == null)
            {
                BreakAsteroid(collision);
            }
            else
            {
                transform.root.gameObject.GetComponent<Asteroid>().BreakAsteroid(collision);
            }
    }

    // Deactivate any asteroids that manage to excape the off screen wrapper
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Bounds") && transform.parent == null)
        {
            AsterX.Instance.RemoveAsteroid(this);
            Debug.Log("Exit Death Bounds");
            gameObject.SetActive(false);
        }
    }

    public void BreakAsteroid(Collision coll)
    {
        ParticleSystem fx = Instantiate(AsterX.Instance.asteroid_SO.PickRandomExplosion(), transform.position, Quaternion.identity);
        foreach(Transform t in fx.GetComponentsInChildren<Transform>())
        {
            t.localScale = Vector3.one * Mathf.Sqrt(size);
        }
        

        if (coll.gameObject.CompareTag("Bullet"))
        {
            AsterX.Instance.AddScore(size);
        }

        //AsterX.Instance.RemoveAsteroid(this);

        if (size > 1)
        {
            PromoteChildren(coll);
        }

        gameObject.SetActive(false);
    }

    void PromoteChildren(Collision coll)
    {
        Asteroid[] children = ExtensionMethods.GetComponentsInDirectChildren<Asteroid>(this);
        foreach (Asteroid child in children)
        {
            child.offScreenWrapper.enabled = true;
            child.rigid.isKinematic = false;
            // Snap this GameObject to the z=0 plane
            Vector3 pos = transform.position;
            pos.z = 0;
            child.transform.position = pos;

            // inherit velocity
            child.rigid.velocity = rigid.velocity;
            child.rigid.angularVelocity = rigid.angularVelocity;

            // add small explosion force
            child.rigid.AddExplosionForce(50f, coll.transform.position, 1f);

        }
        transform.DetachChildren();
        
    }

    private void Update()
    {
        if (rigid.velocity.magnitude > maxSpeed)
        {
            rigid.velocity = rigid.velocity.normalized * maxSpeed;
        }
    }

    private void OnDisable()
    {
        //transform.SetParent(null);
        size = 3;
    }
}
