using UnityEngine;
using System.Collections;

public class CameraScale : MonoBehaviour {

	// Use this for initialization
	void Start () {
		this.GetComponent<Camera> ().projectionMatrix = Matrix4x4.Perspective (60, 1.777f, 0.3f, 1000f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}



}
