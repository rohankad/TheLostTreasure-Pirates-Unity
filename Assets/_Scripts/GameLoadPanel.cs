using UnityEngine;
using System.Collections;

public class GameLoadPanel : MonoBehaviour {

	public GameObject _Panel;
	public CamController _camController;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void HidePanel(){
		_Panel.SetActive (false);
		_camController.RotateCam ();
	}
}
