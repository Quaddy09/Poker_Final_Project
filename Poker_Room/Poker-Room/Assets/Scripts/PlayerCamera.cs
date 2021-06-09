using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	public float speed;
	public float rotateSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("MoveCameraRight")) {
        	transform.RotateAround(Vector3.zero, Vector3.up, -1*speed);
    	} else if(Input.GetButton("MoveCameraLeft")) {
    		transform.RotateAround(Vector3.zero, Vector3.up,  speed);
    	}
    	if(Input.GetButton("PanCameraUp")) {
    		transform.Rotate(-1*rotateSpeed, 0, 0);
		} else if(Input.GetButton("PanCameraDown")) {
			transform.Rotate(rotateSpeed, 0,0 );
		}
    }
}
