using UnityEngine;
using System.Collections;

public class CanonControl : MonoBehaviour {

	public AudioSource _FireSFX;

	public GameObject CanonBallPrefab;
	public GameObject OriginOfCanon;
	public GameObject CanonTarget;

	private float startTime;
	public float journeyTime;

	public GameObject CanonBall;
	public GameObject _CanonExplosion;

	public CompleteCameraController _CompleteCamController;
	public CamController _CamController;
	public GameObject _BoatExplosion;
	public GameObject _boatSmoke;
	public GameObject _LastCamera;
	public GameObject _MainCamera;


	public AudioSource _BoatExplodeSource;
	public AudioClip _BoatExplodeSFX;

	public PirateController _pirateController;

	public Animator _boatAnim;

	private bool _CamAroundDeadPirate;

	private bool _Slerp;
	// Use this for initialization
	void Start () {
		_CamAroundDeadPirate = false;
		_Slerp = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (_CamAroundDeadPirate) {
			_MainCamera.transform.RotateAround (CanonTarget.transform.position, Vector3.up, 10* Time.deltaTime);
		}

		if(_Slerp){
			MoveCanonBallSlerp();
		}

	}


	public void CanonFired(){
		_CamController.StopHighlight ();
		CanonBall = Instantiate (CanonBallPrefab, OriginOfCanon.transform.position,Quaternion.identity) as GameObject;
		//CanonBall = Instantiate (CanonBallPrefab, OriginOfCanon.transform.position,Quaternion.Euler(-46.7f,-131.5f,-18.13f)) as GameObject;
		//CanonBall.GetComponent<Rigidbody> ().AddForce (new Vector3(0f,5f,0f));

		//Vector3 shoot = (CanonTarget.transform.position - CanonBall.transform.position).normalized;
		//CanonBall.GetComponent<Rigidbody> ().AddForce (shoot * 2000.0f);

		_CanonExplosion.SetActive (true);
		startTime = Time.time;
		_FireSFX.Play ();

		//Time.timeScale = 0.1f;
		Invoke ("ChangeTimeScale", 0.5f);
	}

	public void AdjustCanon(){
	
		
	}

	void ChangeTimeScale(){
		
		StartCoroutine(MoveCanonBall());
		//_Slerp=true;


	//	Time.timeScale = 0.1f;
		_CompleteCamController.enabled = true;
	}


/*	IEnumerator MoveCanonBall(){
		float fracComplete = (Time.time - startTime) / journeyTime;
		CanonBall.transform.position  = Vector3.Lerp(CanonBall.transform.position, CanonTarget.transform.position,fracComplete);

		//float _y = CanonBall.transform.position.y +  CanonBall.transform.position.y;
		//CanonBall.transform.position = new Vector3 (CanonBall.transform.position.x, _y, CanonBall.transform.position.z);

		//_Dart.transform.position  = Vector3.Lerp(_Dart.transform.position, _DartTarget.transform.position,fracComplete);
		yield return new WaitForEndOfFrame();
		//yield return new WaitForSeconds (0.01f);
		StartCoroutine("MoveCanonBall");

		//print ("fracComplete : "+fracComplete);
	} */

	IEnumerator MoveCanonBall(){
		float _DeltaTime = 0f;
		Vector3 v = CanonBall.transform.position;
		while(_DeltaTime<journeyTime){

			CanonBall.transform.position  = Vector3.Lerp(v, CanonTarget.transform.position,_DeltaTime/journeyTime);
			yield return new WaitForEndOfFrame();
			_DeltaTime += Time.deltaTime;
		}
		print ("Canon Reached");
		Destroy (CanonBall);
		GameEndScene ();
	}

	void MoveCanonBallSlerp(){
		if (CanonBall != null) {
		//	float startTime = Time.deltaTime;
			Vector3 v = CanonBall.transform.position;
			Vector3 center = (v + CanonTarget.transform.position) * 0.5f;
			center -= new Vector3 (0f, 0.5f, 0f); 
			Vector3 riseRelCenter = v - center;
			Vector3 setRelCenter = CanonTarget.transform.position - center;
			float fracComplete = (Time.time - startTime) / journeyTime;

			CanonBall.transform.position = Vector3.Slerp (riseRelCenter, setRelCenter, fracComplete);
			CanonBall.transform.position += center;

			print ("Canon Reached");
		}
		//GameEndScene ();
	}


	void GameEndScene(){
		StopAllCoroutines ();
		_MainCamera.GetComponent<CompleteCameraController> ().enabled = false;
		_BoatExplodeSource.clip = _BoatExplodeSFX;

		_MainCamera.transform.position = _LastCamera.transform.position;
		_MainCamera.transform.rotation = _LastCamera.transform.rotation;

		_pirateController.PirateDie ();

		_BoatExplodeSource.Play ();
		_BoatExplosion.SetActive (true);
		_boatSmoke.SetActive (true);
		_boatAnim.SetBool ("explode", true);
		_CamAroundDeadPirate = true;
		//Invoke ("ShowChest", 1f);
	}

	void ShowChest(){
		_boatAnim.Play("Chest");
	}
}
