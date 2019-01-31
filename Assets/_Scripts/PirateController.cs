using UnityEngine;
using System.Collections;

public class PirateController : MonoBehaviour {

	public AudioSource _PirateSFX;
	private Animator _PirateAnimator;
	public float journeyTime;
	public GameObject chestTop;
	public GameObject ChestFireworks;


	public Animator _ChestAnim;

	// Use this for initialization
	void Start () {
		_PirateAnimator = this.GetComponent<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void PlayScreamSFX(){
		_PirateSFX.Play ();	
	}


	public void PirateDie(){
		//_PirateAnimator.SetTrigger ("Die");
		_PirateAnimator.SetBool("Die", true);
		Invoke ("OpenChest", 13f);
	}


	void OpenChest(){
		_ChestAnim.Play ("Chest");
		print ("chest opened");
	}




}
