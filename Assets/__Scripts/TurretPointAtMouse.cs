using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class TurretPointAtMouse : MonoBehaviour {

    private Transform turretTransform;
    
    // Use this for initialization
	void Start () {
        turretTransform = GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {       
        turretTransform.LookAt(Camera.main.ScreenToWorldPoint(CrossPlatformInputManager.mousePosition) + new Vector3(0, 0, 10), Vector3.back);
    }
}
