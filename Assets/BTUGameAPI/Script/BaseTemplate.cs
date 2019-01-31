using UnityEngine;
using System.Collections;

public class BaseTemplate : MonoBehaviour {

    public AppType appType = AppType.unknown;
	void Awake(){
        BTGameSettings.SimulateAppType(appType);
		AssociateScripts();
	}
	// Use this for initialization
	void Start () {
	}
	
	void AssociateScripts(){
		Debug.Log("CurrentAppType:"+BTGameSettings.CurrentAppType);
		if (BTGameSettings.CurrentAppType.Equals(AppType.uSpin)){
			gameObject.AddComponent<USpinTemplate>();
		} else if (BTGameSettings.CurrentAppType.Equals(AppType.tournament)){
			if (BTGameSettings.GameMode.Equals(GameMode.exclusive)){
				if (BTGameSettings.IsStandaloneTournamentSetup()){
					gameObject.AddComponent<StandaloneTournamentTemplate>();
				}else {
					gameObject.AddComponent<RopedOffTournamentTemplate>();
				}
			} else {
				gameObject.AddComponent<BonusTournamentTemplate>();
			}
		} else if (BTGameSettings.CurrentAppType.Equals(AppType.powerWinners)){
			gameObject.AddComponent<PowerWinnerTemplate>();
        }
        else if (BTGameSettings.CurrentAppType.Equals(AppType.powerMystery))
        {
            gameObject.AddComponent<PowerMysteryTemplate>();
        }
        else if (BTGameSettings.CurrentAppType.Equals(AppType.flexRewards)){
			gameObject.AddComponent<FlexRewardTemplate>();
        } else if (BTGameSettings.CurrentAppType.Equals(AppType.instantHitLotto) || BTGameSettings.CurrentAppType.Equals(AppType.classicLotto)){
			if (BTGameSettings.IsV32() || BTGameSettings.IsLVDS()){
				gameObject.AddComponent<InstantLottoGameLVDS>();
			} else {
				gameObject.AddComponent<InstantLottoGameDM>();
			}
		} else if (BTGameSettings.CurrentAppType.Equals(AppType.unknown)){
			if (BTGameSettings.IsV32() || BTGameSettings.IsLVDS()){
				gameObject.AddComponent<InstantLottoGameLVDS>();
			} else {
				gameObject.AddComponent<InstantLottoGameDM>();
			}
			//gameObject.AddComponent<StandaloneTournamentTemplate>();
		} 
		Destroy(this);
	}

}
