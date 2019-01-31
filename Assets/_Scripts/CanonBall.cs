using UnityEngine;
using System.Collections;

public class CanonBall : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.Rotate(new Vector3 (15f,30f,45f));
	}


	void OnCollisionEnter(Collision coll){
		print ("Collided with : "+coll.gameObject.tag);
	}

	void OnTriggerEnter(Collider other) {
	//	Destroy(other.gameObject);
		print ("Trigger with : "+other.gameObject.tag);
	}
}
