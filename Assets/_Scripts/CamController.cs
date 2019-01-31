using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
	
	public GameObject _IslandCamera;
	public GameObject _BoatCamera;
	public GameObject _GameplayCamera;

	public Transform _LookPos;
	public Transform _LookPosPirate;

	public bool _RotateStatus;
	public bool _RotateStatusPirate;

	public AudioSource _IslandIntro;
	public AudioSource _PirateIntro;
	public AudioSource _GameBg;

	public GameObject[] _Boats;

	public float journeyTime;

	// Use this for initialization
	void Start () {
		journeyTime = 2f;
		_RotateStatus = false;
		_RotateStatusPirate = false;
		_Boats = GameObject.FindGameObjectsWithTag ("Boat");

		Invoke ("RotateCam",1f);
	}
	
	// Update is called once per frame
	void Update () {
		if (_RotateStatus) {
			_IslandCamera.transform.RotateAround (_LookPos.position, Vector3.up, 10* Time.deltaTime);
		}

		if (_RotateStatusPirate) {
	//	_BoatCamera.transform.RotateAround (_LookPosPirate.position, Vector3.up, 10* Time.deltaTime);
			_IslandCamera.transform.RotateAround (_LookPosPirate.position, Vector3.up, 5* Time.deltaTime);
		}
	
	}


	//Lookat island here
	public void RotateCam(){
		_IslandIntro.Play ();
		_RotateStatus = !_RotateStatus;
		Invoke ("RotateCamPirate",6f);
	}

	public void RotateCamPirate(){
		_IslandCamera.transform.position = _BoatCamera.transform.position;
		_IslandCamera.transform.rotation = _BoatCamera.transform.rotation;
		_PirateIntro.Play ();
		_RotateStatusPirate = !_RotateStatusPirate;
		_GameBg.PlayDelayed (_PirateIntro.clip.length);
		Invoke ("GamePlayView",10f);
	}


	public void GamePlayView(){
		_RotateStatusPirate = false;
		_RotateStatus = false;
		//_IslandCamera.transform.position = _GameplayCamera.transform.position;
		//_IslandCamera.transform.rotation = _GameplayCamera.transform.rotation;

		foreach (GameObject a in _Boats) {
			a.GetComponent<BlackPearl> ().StartHighlight ();
		}

		StartCoroutine("SetCam2");

	}

	public void StopHighlight(){
		foreach (GameObject a in _Boats) {
			a.GetComponent<BlackPearl> ().StopHighlight ();
		}
	}

	IEnumerator SetCam2(){
		float _DeltaTime = 0f;
		while(_DeltaTime<journeyTime){
			_IslandCamera.transform.position = Vector3.Lerp (_IslandCamera.transform.position, _GameplayCamera.transform.position, _DeltaTime/journeyTime);
			_IslandCamera.transform.rotation = Quaternion.Lerp (_IslandCamera.transform.rotation, _GameplayCamera.transform.rotation, _DeltaTime/journeyTime);
			yield return new WaitForEndOfFrame ();
			_DeltaTime += Time.deltaTime;
		}
		//print ("CAmera End Reached");
	
	}


	public void GameExit(){
		Application.Quit ();
	}



}
