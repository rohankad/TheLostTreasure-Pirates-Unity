using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlexRewardTemplate : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		//mandatory calls start		
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		
		BTUGameAPI.SetStateChangeCallBack(OnBonusGameStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnSessionStateChange, ObservationType.Both);
		DontDestroyOnLoad(gameObject);
	}
	
	Rect endGameButtonRect 		= new Rect(300,680,100,60);
	Rect captureScreenRect 		= new Rect(660,680,120,60);
	Rect exitGameRect 			= new Rect(840,680,120,60);
	
	Rect welcomeRect 			= new Rect(240,600,780,60);
	Rect payMethodRect 			= new Rect(624,600,780,60);
	Rect awardAmountRect 		= new Rect(844,600,780,60);
	
	PaymentMethod paymentMethod=PaymentMethod.any;
	string awardAmount;

	Rect playerSessionStateRect 	= new Rect(20, 540, 300, 60);
	Rect bonusGameStateRect 		= new Rect(20, 580, 300, 60);	
	string sessionStateMessage 		= "Player Session State - "+SessionState.idle.ToString();
	string bonusStateMessage 		= "Bonus Game State - "+BonusGameState.idle.ToString();
	
	void OnGUI(){
		GUI.skin.button.fontSize=16;
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
		GUI.Label(payMethodRect, string.Format("Payment Method : {0}",paymentMethod));
		GUI.Label(awardAmountRect, string.Format("Award Amount : {0}",awardAmount));		
		
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
			paymentMethod = promotion.PayMethod;
			awardAmount = promotion.getFormattedAward();
			Debug.Log("Payment Method					:"+promotion.PayMethod);
			Debug.Log("Award							:"+promotion.getAward());
			Debug.Log("Award Format						:"+promotion.getAwardFormat());
			Debug.Log("Formatted Award					:"+promotion.getFormattedAward());
		} else if (eventType == BTEventType.DisplayBucketUpdate){
			Dictionary<string, string> displayBuckets = BTUGameAPI.getCurrentBuckets();			
			// Iterate through list of key-value pairs to extract any information newly added in content schedules
			// which is currently not supported in the API.
			if (displayBuckets!=null){
				Debug.Log(string.Format("Data Count:{0}",displayBuckets.Count));
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
		if (bonusGameState==BonusGameState.beginGameAcked){
			//Automatically Change to beginGameAckedAndPlaying without any user interaction as flex rewards are already awarded
			BTUGameAPI.SendCommand(BTCommand.actionBeginPlay);
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

