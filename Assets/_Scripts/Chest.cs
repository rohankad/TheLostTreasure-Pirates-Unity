using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour {

	public GameObject ChestFireworks;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void PlayFireworks(){
		ChestFireworks.SetActive (true);
		Invoke ("ReInvokeFireworks", 2f);
	}

	void ReInvokeFireworks(){
		ChestFireworks.SetActive (false);
		PlayFireworks ();
	}

}
