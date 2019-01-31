using UnityEngine;
using System.Collections;

public class BlackPearl : MonoBehaviour {

	public GameObject _BoatHighlighter;
	public Material _HighlightShader;

	public Color _FirstColor;
	public Color _SecondColor;
	public float journeyTime;

	// Use this for initialization
	void Start () {
		_HighlightShader = _BoatHighlighter.GetComponent<MeshRenderer> ().material;
		journeyTime = 2f;

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void StartHighlight(){
		_BoatHighlighter.GetComponent<BoatHighlighter> ().StartHighlight ();
		//print ("high");
		StartCoroutine ("HighlightBoat");
	}

	public void StopHighlight(){
		_BoatHighlighter.GetComponent<BoatHighlighter> ().StopHighlight ();
		//print ("high");
		//StartCoroutine ("HighlightBoat");
	}

	IEnumerator HighlightBoat(){
		float _DeltaTime = 0f;
		while(_DeltaTime<journeyTime){
			_HighlightShader.color = Color.Lerp (_HighlightShader.color, _SecondColor, _DeltaTime/journeyTime);
			yield return new WaitForEndOfFrame ();
			_DeltaTime += Time.deltaTime;
		}
		//print ("Color End Reached");
		StartCoroutine ("HighlighBoatColor2");
	}

	IEnumerator HighlighBoatColor2(){
		float _DeltaTime = 0f;
		while(_DeltaTime<journeyTime){
			_HighlightShader.color = Color.Lerp (_HighlightShader.color, _FirstColor, _DeltaTime/journeyTime);
			yield return new WaitForEndOfFrame ();
			_DeltaTime += Time.deltaTime;
		}
		//print ("Color First Reached");
		StartCoroutine ("HighlightBoat");
	}
		
}


