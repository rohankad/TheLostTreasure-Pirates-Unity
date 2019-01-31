using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class USpinTemplate : MonoBehaviour {
	
	void Awake () {
		//mandatory calls start		
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		
		BTUGameAPI.SetStateChangeCallBack(OnBonusGameStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnSessionStateChange, ObservationType.Both);
		DontDestroyOnLoad(gameObject);
	}
	
	// Use this for initialization
	void Start () {
	
	}
	
	Rect startGameButtonRect 	= new Rect(80,600,140,60);
	Rect endGameButtonRect 		= new Rect(240,600,140,60);	
	Rect captureScreenRect 		= new Rect(80,670,140,60);
	Rect exitGameRect 			= new Rect(240,670,140,60);
	
	Rect welcomeRect 			= new Rect(60,500,400,60);
	
	Rect playerSessionStateRect 	= new Rect(20, 540, 300, 60);
	Rect bonusGameStateRect 		= new Rect(20, 580, 300, 60);	
	string sessionStateMessage 		= "Player Session State - "+SessionState.idle.ToString();
	string bonusStateMessage 		= "Bonus Game State - "+BonusGameState.idle.ToString();
	
	void OnGUI(){
		GUI.skin.button.fontSize=16;
		if (bonusGameState == BonusGameState.beginGameAcked){
			if (GUI.Button(startGameButtonRect,"Start Game\n(Spin Wheel)")){
				BTUGameAPI.SendCommand(BTCommand.actionBeginPlay);//user started spinning the wheel
			}
		}
		if (bonusGameState == BonusGameState.beginGameAckedAndPlaying){
			if (GUI.Button(endGameButtonRect,"End Game")){//game finished
				BTUGameAPI.SendCommand(BTCommand.actionEndGame);
			}
		}
		if (GUI.Button(captureScreenRect,"Screenshot")){
			BTUGameAPI.CaptureScreenshot(CaptureComplete);
		}
		if (GUI.Button(exitGameRect,"Exit Game")){//user clicks the exit button
			BTUGameAPI.SendCommand(BTCommand.actionExitGame);			
		}
		if (sessionState == SessionState.startSessionAcked)
			GUI.Label(welcomeRect, string.Format("Hello {0} {1}!",BTUGameAPI.getCurrentPlayerInfo().FirstName,BTUGameAPI.getCurrentPlayerInfo().LastName));
		
		GUI.Label(playerSessionStateRect, sessionStateMessage);
		GUI.Label(bonusGameStateRect, bonusStateMessage);
	}
	
	void CaptureComplete(){
		Debug.Log("Completed");
	}
	
	void OnApplicationEvent(BTEventType eventType, bool result){
		if (eventType == BTEventType.ApplicationPause){
			if (result){
				Debug.Log("Game pausing");
				// TODO: Perform actions necessary to Disable Game Play. eg:Disabling Spin button to stop the user from playing
				// Other animations may continue. 
			} else {
				Debug.Log("Game resuming");
			}
		} else if (eventType == BTEventType.EmployeeInterrupt){
			Debug.Log(string.Format("Employee Interrupt:{0}",result));
		} else if (eventType == BTEventType.PlayerCardPulled){
			if (result) Debug.Log("perform autofinish if game mechanism dictates it");
			else Debug.Log("ignore");
		} else if (eventType == BTEventType.PromotionSettingsReady){
			BTPromotionSettings promotion = BTUGameAPI.getPromotionSettings();
			Debug.Log("promotion settings available");
			//map payline to wheel or some render target.
			foreach (PayLine payline in promotion.PayTable.PayLines){
				Debug.Log(string.Format("payline.Description:{0},PrizeType:{1},PayMethod:{2}",payline.Description,payline.PrizeType,payline.payMethod));
			}
		} else if (eventType == BTEventType.DisplayBucketUpdate){
			Dictionary<string, string> displayBuckets = BTUGameAPI.getCurrentBuckets();			
			// Iterate through list of key-value pairs to extract any information newly added in content schedules
			// which is currently not supported in the API.
			while (displayBuckets!=null){
				Debug.Log(string.Format("Data Count:{0}",displayBuckets.Count));
				displayBuckets = BTUGameAPI.getCurrentBuckets();
			}
		}
	}

	public BonusGameState bonusGameState = BonusGameState.idle;
	public SessionState sessionState = SessionState.idle;
	
	public void OnBonusGameStateChange(string message, params BonusGameState[] bonusGameStates){
		//if current bonus game state == BeginGameFailed, message field will contain the reason
		//Debug.Log(message);
		foreach (BonusGameState state in bonusGameStates){
			Debug.Log("BonusGameState:"+state);
		}
		bonusGameState = bonusGameStates[1];
		if (string.IsNullOrEmpty(message)){
			bonusStateMessage ="Bonus Game State - "+ bonusGameState.ToString();
		} else {
			bonusStateMessage = string.Format("Bonus Game State - {0}:{1}",bonusGameState.ToString(),message);
		}			
	}
	
	public void OnSessionStateChange(string message, params SessionState[] sessionStates){
		//if current session state == startSessionFailed, message field will contain the reason
		//Debug.Log(message);
		foreach (SessionState state in sessionStates){
			Debug.Log("SessionState:"+state);
		}
		sessionState = sessionStates[1];
		if (string.IsNullOrEmpty(message)){
			sessionStateMessage = "Player Session State - "+sessionState.ToString();
		} else {
			sessionStateMessage = string.Format("Player Session State - {0}:{1}",sessionState.ToString(),message);
		}		
	}	
}
