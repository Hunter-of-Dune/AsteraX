using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerShip : MonoBehaviour {

    public delegate void TIMER(float time);
    public static TIMER Timer;

    private Transform playerTransform;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform turretTransform;
    [SerializeField] private ParticleSystem explosion;

    Rigidbody rigid;
    private float speed = 10f;
    private float ammo = 100;
    float shootTimer = 0;
    
    // Use this for initialization
	void Start () {
        playerTransform = GetComponent<Transform>();
        rigid = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        shootTimer += Time.deltaTime;
        
        // Player movement
        float horizontalInput = CrossPlatformInputManager.GetAxis("Horizontal");
        float verticalInput = CrossPlatformInputManager.GetAxis("Vertical");
        Vector3 velocity = new Vector3(horizontalInput, verticalInput);

        if (velocity.magnitude > 1)
        {
            // Avoid multiping by 1.4 when moving diagonal
            velocity.Normalize();
        }

        rigid.velocity = velocity * speed;

        if (CrossPlatformInputManager.GetButtonDown("Fire1") && ammo >= 5f)
        {
            GameObject g = Instantiate(bullet, playerTransform.position, turretTransform.rotation);
            Bullet b = g.GetComponent<Bullet>();
            b.rigid.velocity = turretTransform.forward * b.speed;
            ammo -= 5;
            shootTimer = 0;
        }

        ammo = Mathf.Clamp(ammo += 10 * Time.deltaTime, 0, 100);
        AsterX.Instance.UpdateAmmoSlider(ammo);

        // Not shooting achievement
        if(Timer != null)
        {
            Timer.Invoke(shootTimer);
        }
	}

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(explosion, transform.position, Quaternion.identity);

        AsterX.Instance.PlayerDied();
            
        Destroy(gameObject);
    }
}
