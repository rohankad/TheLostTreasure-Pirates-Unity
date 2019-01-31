using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BonusTournamentTemplate : MonoBehaviour {
	
	void Awake () {
		//mandatory calls start		
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		
		BTUGameAPI.SetStateChangeCallBack(OnBonusGameStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnSessionStateChange, ObservationType.Both);
		BTUGameAPI.SetLeaderboardUpdateCallBack(LeaderboardUpdate);
		DontDestroyOnLoad(gameObject);
	}
	
	Rect beginPlayRect 			= new Rect(100,680,200,60);
	Rect spinButtonRect 		= new Rect(240,680,200,60);
	Rect endGameButtonRect 		= new Rect(500,680,100,60);
	Rect captureScreenRect 		= new Rect(660,680,120,60);
	Rect exitGameRect 			= new Rect(840,680,120,60);
	
	Rect welcomeRect 			= new Rect(240,600,400,60);
	Rect scoreRect 				= new Rect(624,600,784,60);

	Rect playerSessionStateRect 	= new Rect(20, 540, 300, 60);
	Rect bonusGameStateRect 		= new Rect(20, 580, 300, 60);	
	string sessionStateMessage 		= "Player Session State - "+SessionState.idle.ToString();
	string bonusStateMessage 		= "Bonus Game State - "+BonusGameState.idle.ToString();	
	// Use this for initialization
	void Start () {

	}
	
	uint currentScore=0;
	uint totalScore=0;
	bool firstTime = true;
	bool firstSpin = true;
	
	void PostScore(){
		currentScore=(uint)BTRandom.Next(50,300);
		totalScore+=currentScore;
		BTUGameAPI.PostIntermediateScore(currentScore,totalScore,"Bonus Tournament Template");
		// 	NOTES:
		//	or all reel symbols on the screen for this particular spin
		//	BTUGameAPI.PostIntermediateScore(currentScore,totalScore,"BLAZING7,BELL,DOLLAR");
		//	alternate command
		//	BTUGameAPI.SendCommand(BTCommand.actionPostIntermediateScoreWithOutcome,currentScore,totalScore,"Royal Flush");
	}
	
	void OnGUI(){
		GUI.skin.button.fontSize=16;
		if (firstTime && Application.platform!=RuntimePlatform.WindowsEditor && !BTGameSettings.IsStandalone()){
			// NOTES: This is to be automatically fired once game is loaded and the first frame has rendered
			// and the game is ready to play for the user - ONLY FOR BONUS TOURNAMENTS
			BTUGameAPI.SendCommand(BTCommand.actionReadyToPlay);
			firstTime = false;
		}
		if (bonusGameState == BonusGameState.beginGameAcked){
			if (firstSpin && GUI.Button(beginPlayRect, new GUIContent("Begin Play (Spin)","First Spin of the Game"))){
				BTUGameAPI.SendCommand(BTCommand.actionBeginPlay);
				PostScore();
				firstSpin = false;
			}
		}
		if (!firstSpin && bonusGameState == BonusGameState.beginGameAckedAndPlaying){
			if (GUI.Button(spinButtonRect,"Spin Reel/Post Score")){
				PostScore();
			}
		}
		if (bonusGameState == BonusGameState.beginGameAckedAndPlaying){
			if (GUI.Button(endGameButtonRect,"End Game")){
				BTUGameAPI.SendCommand(BTCommand.actionEndGame, totalScore);
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
		GUI.Label(scoreRect, string.Format("Score:{0}",totalScore));
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
	
	public void OnBonusGameStateChange(string message, params BonusGameState[] bonusGameStates){
		//if current bonus game state == BeginGameFailed, message field will contain the reason
		//Debug.Log(message);
		foreach (BonusGameState state in bonusGameStates){
			Debug.Log("BonusGameState:"+state);
		}
		bonusGameState = bonusGameStates[1];
		if (bonusGameState==BonusGameState.beginGameAcked){
			if (!BTGameSettings.IsDisplayOnPC())
				Screen.fullScreen = true;//TODO
			// NOTES: Enable Game Play only now
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
