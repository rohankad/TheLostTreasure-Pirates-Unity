using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RopedOffTournamentTemplate : MonoBehaviour {
	
	public virtual void Awake () {
		//mandatory calls start		
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		
		BTUGameAPI.SetStateChangeCallBack(OnBonusGameStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnSessionStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnPlayerEligibilityStateChange, ObservationType.Both);
		
		BTUGameAPI.SetLeaderboardUpdateCallBack(LeaderboardUpdate);
		DontDestroyOnLoad(gameObject);
	}
	
	Rect startGameButtonRect 	= new Rect(100,680,140,60);
	Rect enableDisableLight		= new Rect(300,680,140,60);	
	Rect spinButtonRect 		= new Rect(500,680,140,60);	
	Rect endGameButtonRect 		= new Rect(700,680,100,60);
	Rect captureScreenRect 		= new Rect(860,680,120,60);
	
	Rect welcomeRect 			= new Rect(240,640,400,60);
	Rect scoreRect 				= new Rect(624,640,784,60);
	
	Rect playerSessionStateRect 	= new Rect(20, 540, 300, 60);
	Rect playerEligibilityStateRect = new Rect(20, 580, 300, 60);
	Rect bonusGameStateRect 		= new Rect(20, 620, 300, 60);
	
	string sessionStateMessage 		= "Player Session State - "+SessionState.idle.ToString();
	string eligiblityStateMessage 	= "Player Eligibility State - "+PlayerEligibilityState.idle.ToString();
	string bonusStateMessage 		= "Bonus Game State - "+BonusGameState.idle.ToString();
	
	// Use this for initialization
	public virtual void Start () {

	}
	
	string message;
	uint currentScore=0;
	uint totalScore=0;
	bool lightState = true;
	string playerFirstName="";
	string playerLastName="";
	bool showStartButton = false;
	public virtual void OnGUI(){
		GUI.skin.button.fontSize=16;
		if (showStartButton && BTUGameAPI.getPromotionSettings().StartMode.Equals(StartMode.player)){
			if (GUI.Button(startGameButtonRect,"Start Game\n(Player Start)")){
				BTUGameAPI.SendCommand(BTCommand.actionBeginGame);
				BTUGameAPI.SendCommand(BTCommand.actionSetClick, 570, 630);
				BTUGameAPI.SendCommand(BTCommand.actionEGMButtonLight, true);
				showStartButton = false;
			}
		}
		if (bonusGameState == BonusGameState.beginGameAckedAndPlaying){
			if (GUI.Button(spinButtonRect,"Spin/Post Score")){
				currentScore=(uint)BTRandom.Next(50,300);
				totalScore+=currentScore;
				BTUGameAPI.PostIntermediateScore(currentScore,totalScore,"Roped Off Tournament Template");
				// 	NOTES
				//	or all reel symbols on the screen for this particular spin
				//	BTUGameAPI.PostIntermediateScore(currentScore,totalScore,"BLAZING7,BELL,DOLLAR");
				//	alternate command
				//	BTUGameAPI.SendCommand(BTCommand.actionPostIntermediateScoreWithOutcome,currentScore,totalScore,"Royal Flush");
			}
			if (GUI.Button(endGameButtonRect,"End Game")){
				BTUGameAPI.SendCommand(BTCommand.actionEndGame, totalScore);
				BTUGameAPI.SendCommand(BTCommand.actionEGMButtonLight, false);			
			}
		}
		if (GUI.Button(captureScreenRect,"Screenshot")){
			BTUGameAPI.CaptureScreenshot(CaptureComplete);
		}
		if (GUI.Button(enableDisableLight,"Enable/\nDisable Light")){
			BTUGameAPI.SendCommand(BTCommand.actionEGMButtonLight, lightState);
			lightState^=true;
		}
		if (sessionState == SessionState.startSessionAcked)
			GUI.Label(welcomeRect, string.Format("Hello {0} {1}!",playerFirstName,playerLastName));
		GUI.Label(scoreRect, string.Format("Score:{0}",totalScore));
		
		GUI.Label(playerSessionStateRect, sessionStateMessage);
		GUI.Label(playerEligibilityStateRect, eligiblityStateMessage);
		GUI.Label(bonusGameStateRect, bonusStateMessage);
	}
	
	// Update is called once per frame
	public virtual void Update () {
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
			//read the tournament settings and configure the game.
			Debug.Log("Tournament promotion name:\t"+promotion.DisplayName);			
			Debug.Log("Game Duration In Seconds:\t"	+promotion.TournamentSettings.GameDurationInSeconds);
			Debug.Log("In Game Leaderboard:\t"		+promotion.TournamentSettings.ShowInGameLeaderBoard);
			Debug.Log("Post Game Leaderboard:\t"	+promotion.TournamentSettings.ShowPostGameLeaderBoard);
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
	
	void LeaderboardUpdate(List<BTLeaderboardEntity> leaderboard, bool final, uint playerCurrentRank, uint playerBestRank, uint playerBestScore){
		Debug.Log("Final leaderboard:"+final);
		if (playerCurrentRank>0)
			Debug.Log("Player Current Rank:"+playerCurrentRank);
		if (final && playerBestRank>0)
			Debug.Log("Player Best Rank:"+playerBestRank);
		if (leaderboard!=null){
			//map to rendering target
			foreach (BTLeaderboardEntity leaderboardEntity in leaderboard){
				Debug.Log(string.Format("Rank:{0},Score:{1}",leaderboardEntity.tournamentRank,leaderboardEntity.score));
			}
			if (BTUGameAPI.getPromotionSettings().TournamentSettings.ShowInGameLeaderBoard && (bonusGameState==BonusGameState.beginGameAckedAndPlaying || bonusGameState==BonusGameState.endGameSent)){
				// treat leaderboard as in-game leaderboard.
			}
			if (BTUGameAPI.getPromotionSettings().TournamentSettings.ShowPostGameLeaderBoard && bonusGameState==BonusGameState.endGameAcked){
				// treat leaderboard as post-game leaderboard.
			}
		}

	}
	
	public BonusGameState bonusGameState = BonusGameState.idle;
	public SessionState sessionState = SessionState.idle;
	public PlayerEligibilityState eligibilityState = PlayerEligibilityState.idle;
		
	public void OnBonusGameStateChange(string message, params BonusGameState[] bonusGameStates){
		//if current bonus game state == BeginGameFailed, message field will contain the reason
		//Debug.Log(message);
		
		// NOTES: bonusGameStates will be of length 2 only if ObservationType is Both. All other cases the length will be 1.
		foreach (BonusGameState state in bonusGameStates){
			Debug.Log("BonusGameState:"+state);
		}
		bonusGameState = bonusGameStates[1];
		if (bonusGameState==BonusGameState.beginGameAcked){
			if (!BTGameSettings.IsDisplayOnPC())
				Screen.fullScreen = true;//TODO
			showStartButton = true;
		} else if (bonusGameState==BonusGameState.idle){
			// NOTES: RESET Game To Initial State
			totalScore = currentScore=0;
			showStartButton = false;
		}
		if (string.IsNullOrEmpty(message)){
			bonusStateMessage ="Bonus Game State - "+ bonusGameState.ToString();
		} else {
			bonusStateMessage = string.Format("Bonus Game State - {0}:{1}",bonusGameState.ToString(),message);
		}		
	}
	
	public void OnSessionStateChange(string message, params SessionState[] sessionStates){
		//if current session state == startSessionFailed, message field will contain the reason
		//Debug.Log(message);
		
		// NOTES: sessionStates will be of length 2 only if ObservationType is Both. All other cases the length will be 1.
		foreach (SessionState state in sessionStates){
			Debug.Log("SessionState:"+state);
		}
		sessionState = sessionStates[1];
		if (sessionState==SessionState.startSessionAcked){
			playerFirstName = BTUGameAPI.getCurrentPlayerInfo().FirstName;
			playerLastName = BTUGameAPI.getCurrentPlayerInfo().LastName;
		} else if (sessionState==SessionState.idle || sessionState==SessionState.startSessionSent || sessionState==SessionState.startSessionFailed){
			playerFirstName="";
			playerLastName="";
		}
		if (string.IsNullOrEmpty(message)){
			sessionStateMessage = "Player Session State - "+sessionState.ToString();
		} else {
			sessionStateMessage = string.Format("Player Session State - {0}:{1}",sessionState.ToString(),message);
		}
	}
	
	public void OnPlayerEligibilityStateChange(string message, params PlayerEligibilityState[] playerEligibilityStates){
		//if current player eligibility state == notEligible, message field will contain the reason
//		Debug.Log(message);
		foreach (PlayerEligibilityState state in playerEligibilityStates){
			Debug.Log("PlayerEligibilityState:"+state);
		}
		eligibilityState = playerEligibilityStates[1];
		if (string.IsNullOrEmpty(message)){
			eligiblityStateMessage = "Player Eligibility State  - "+eligibilityState.ToString();
		} else {
			eligiblityStateMessage = string.Format("Player Eligibility State - {0}:{1}",eligibilityState.ToString(),message);
		}
	}

}
