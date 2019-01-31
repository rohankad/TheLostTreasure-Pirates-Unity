using UnityEngine;
using System.Collections;

public class BoatHighlighter : MonoBehaviour {
	public CanonControl _CanonController;
	// Use this for initialization
	void Awake(){
		//this.gameObject.SetActive (false);
	}
	void Start () {
		_CanonController = GameObject.FindGameObjectWithTag ("CanonControl").GetComponent<CanonControl> ();
	//	_CanonController = GameObject.Find ("CanonController").GetComponent<CanonControl> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown(){
		print ("Boat selected : "+this.transform.parent.gameObject.transform.parent.tag);
		_CanonController.CanonFired ();

	}

	public void StartHighlight(){
		this.gameObject.SetActive (true);
		//print ("high done");
	}

	public void StopHighlight(){
		this.gameObject.SetActive (false);
		//print ("high done");
	}



}
