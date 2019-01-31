using UnityEngine;
using System.Collections;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

public class FrameworkTesterBehaviour : MonoBehaviour {
	
	
	void Awake () {
		//mandatory calls start		
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		BTUGameAPI.ToggleRenderingStats(true);

		Debug.Log(BTGameSettings.IsDisplayOnPC());
		Debug.Log(BTRandom.Next());
		Debug.Log(BTRandom.NextDouble());
		
		BTUGameAPI.SwitchLanguage("en");
		
//		((tk2dCamera)GameObject.FindObjectOfType(typeof(tk2dCamera))).mainCamera.);
//		Debug.Log(BTUGameAPI.getPromotionSettings().getFormattedWinAmount
		
		BTUGameAPI.SetStateChangeCallBack(OnBonusGameStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnSessionStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnPlayerEligibilityStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnTournamentCodeValidationStateStateChange, ObservationType.Both);
		
		BTUGameAPI.SetLeaderboardUpdateCallBack(LeaderboardUpdate);
		DontDestroyOnLoad(gameObject);
		BTUGameAPI.SendCommand(BTCommand.actionEndGame);
	}

	// Use this for initialization
	void Start(){
	}

	void OnGUI(){
	}

	void CaptureComplete(){
		Debug.Log("Completed");
	}
		
	void LeaderboardUpdate(List<BTLeaderboardEntity> leaderboard, bool final, uint playerCurrentRank, uint playerBestRank, uint playerBestScore){
		if (playerCurrentRank>0)
			Debug.Log("Player\t Current Rank:"+playerCurrentRank);
		if (final && playerBestRank>0)
			Debug.Log("Player Best Rank:"+playerBestRank);
		if (leaderboard!=null){
			//map to rendering target
		}
	}
	
	void OnApplicationEvent(BTEventType eventType, bool result){
		if (eventType == BTEventType.ApplicationPause){
			if (result){
				Debug.Log("Game pausing");
			} else {
				Debug.Log("Game resuming");
			}
		} else if (eventType == BTEventType.EmployeeInterrupt){
			Debug.Log(string.Format("Employee Interrupt:{0}",result));
		} else if (eventType == BTEventType.PlayerCardPulled){
			if (result) Debug.Log("perform autofinish if game mechanic dictates it");
			else Debug.Log("ignore");
		}
	}

	public void OnBonusGameStateChange(string message, params BonusGameState[] bonusGameStates){
		//if current bonus game state == BeginGameFailed, message field will contain the reason
		Debug.Log(message);
		foreach (BonusGameState state in bonusGameStates){
			Debug.Log("BonusGameState:"+state);
		}
		if (!BTGameSettings.IsDisplayOnPC())
			Screen.fullScreen = true;//TODO
	}
	
	public void OnSessionStateChange(string message, params SessionState[] sessionStates){
		//if current session state == startSessionFailed, message field will contain the reason
		Debug.Log(message);
		foreach (SessionState state in sessionStates){
			Debug.Log("SessionState:"+state);
		}
	}
	
	public void OnPlayerEligibilityStateChange(string message, params PlayerEligibilityState[] playerEligibilityStates){
		//if current session state == notEligible, message field will contain the reason
		Debug.Log(message);
		foreach (PlayerEligibilityState state in playerEligibilityStates){
			Debug.Log("PlayerEligibilityState:"+state);
		}
	}

	public void OnTournamentCodeValidationStateStateChange(string message, params TournamentCodeValidationState[] tournamentCodeValidationStates){
		//if current tournament code validation state == codeValidationFailed, message field will contain the reason
		Debug.Log(message);
		foreach (TournamentCodeValidationState state in tournamentCodeValidationStates){
			Debug.Log("TournamentCodeValidationState:"+state);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.S)){
			BTUGameAPI.CaptureScreenshot(CaptureComplete);			
		}
		if (Input.GetKeyUp(KeyCode.U)){
			object sender=null;
			sender.ToString();
		}
		if (Input.GetKeyUp(KeyCode.E)){
			BTUGameAPI.ShowDialog("Communications down. Please call attendant.", string.Empty, DialogType.Error, null, true);
		}
		if (Input.GetKeyUp(KeyCode.C)){
            BTUGameAPI.ShowDialog("Welcome! Are you Damodaran Ramani?", string.Empty, DialogType.ConfirmationBox, Result);
		}		
		if (Input.GetKeyUp(KeyCode.I)){
            BTUGameAPI.ShowDialog("Welcome to your Bonus Game. Good Luck!", string.Empty, DialogType.InformationBox, Result);
		}		
		if (Input.GetKeyUp(KeyCode.A)){
            BTUGameAPI.ShowDialog("Slot Door Open!", string.Empty, DialogType.StatusBox, null);
		}
		if (Input.GetKeyUp(KeyCode.N)){
			BTUGameAPI.SwitchLanguage("en");
		}
		if (Input.GetKeyUp(KeyCode.H)){
			BTUGameAPI.SwitchLanguage("hi");
		}
		if (Input.GetKeyUp(KeyCode.F)){
			BTUGameAPI.SwitchLanguage("fr");
		}
	}

    void Result(bool result){
        Debug.Log(result);
    }
}
