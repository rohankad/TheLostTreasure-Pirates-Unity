using UnityEngine;
using System.Collections;

public class StandaloneTournamentTemplate : RopedOffTournamentTemplate {
	
	public override void Awake () {
		base.Awake();
		BTUGameAPI.SetStateChangeCallBack(OnTournamentCodeValidationStateChange, ObservationType.Both);		
	}
	
	// Use this for initialization
	public override void Start () {
		base.Start();
	}
	
	Rect codeValidationStateRect 	= new Rect(20, 500, 300, 60);
	Rect enterCodeInsRect 			= new Rect(350,600,70,20);
	Rect enterRegCoreRect 			= new Rect(440,600,70,20);
	Rect validateCodeRect 			= new Rect(530,600,70,20);
	Rect closeSessionRect 			= new Rect(860,600,120,60);
	
	string code = string.Empty;
	string codeValidationMessage	= "Tournament Code Validation State - "+TournamentCodeValidationState.idle.ToString();	
	
	public override void OnGUI() {
		base.OnGUI();
		GUI.skin.button.fontSize=16;
		if (bonusGameState==BonusGameState.idle){//not a comprehensive check, just for demo.
			if (codeValidationState==TournamentCodeValidationState.codeValidated && sessionState == SessionState.idle) {
				GUI.Label(new Rect(350,600,280,20), string.Format("Is Player Name {0} {1}?", BTUGameAPI.getCurrentPlayerInfo().FirstName, BTUGameAPI.getCurrentPlayerInfo().LastName));
				if (GUI.Button(new Rect(640,600,40,30), "Yes")){
					BTUGameAPI.SendCommand(BTCommand.actionConfirmName,true);
				}
				if (GUI.Button(new Rect(700,600,40,30), "No")){
					BTUGameAPI.SendCommand(BTCommand.actionConfirmName,false);
				}
			} else {
				code = GUI.TextField(enterRegCoreRect, code);
				if (code.Length>7) code = code.Substring(0,7);
				
				if (GUI.Button(validateCodeRect, "Validate")){
					BTUGameAPI.SendCommand(BTCommand.actionValidateCode,code);
				}
				GUI.Label(enterCodeInsRect, "Enter Code:");
			}
		}
		if (bonusGameState != BonusGameState.beginGameAckedAndPlaying && sessionState > SessionState.startSessionSent){
			//equivalent to card-out for a machine without card reader (only standalone tournam	ent)
			if (GUI.Button(closeSessionRect, "Cancel/Close Session")){
				BTUGameAPI.SendCommand(BTCommand.actionCloseSession);
			}
		}
		GUI.Label(codeValidationStateRect, codeValidationMessage);
	}
	
	// Update is called once per frame
	public override void Update () {
		base.Update();
	}

	public TournamentCodeValidationState codeValidationState = TournamentCodeValidationState.idle;
	
	public void OnTournamentCodeValidationStateChange(string message, params TournamentCodeValidationState[] tournamentCodeValidationStates){
		//if current tournament code validation state == codeValidationFailed, message field will contain the reason
//		Debug.Log(message);
		foreach (TournamentCodeValidationState state in tournamentCodeValidationStates){
			Debug.Log("TournamentCodeValidationState:"+state);
		}
		codeValidationState = tournamentCodeValidationStates[1];
		if (string.IsNullOrEmpty(message)){
			codeValidationMessage ="Code Validation State-"+ codeValidationState.ToString();
		} else {
			codeValidationMessage = string.Format("Code Validation State - {0}:{1}",codeValidationState.ToString(),message);
		}		
	}	
}
