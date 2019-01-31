using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerMysteryTemplate : MonoBehaviour {

	// Use this for initialization
	void Start () {
        //mandatory calls start		
        BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
        //mandatory calls end
    }

    Rect welcomeRect = new Rect(100, 200, 300, 100);
    Rect captureScreenRect = new Rect(60, 700, 120, 60);
    Rect exitGameRect = new Rect(240, 700, 120, 60);

    void OnGUI()
    {
        GUI.skin.button.fontSize = 16;

        if (GUI.Button(captureScreenRect, "Screenshot"))
        {
            BTUGameAPI.CaptureScreenshot(CaptureComplete);
        }
        if (GUI.Button(exitGameRect, "Exit Game"))
        {//user clicks the exit button
            BTUGameAPI.SendCommand(BTCommand.actionExitGame);
        }
        GUI.Label(welcomeRect, string.Format("Hello {0} {1}! {2}", 
            BTUGameAPI.getCurrentPlayerInfo().FirstName, BTUGameAPI.getCurrentPlayerInfo().LastName,
            BTUGameAPI.getPromotionSettings().TextMessage));
    }

    void CaptureComplete()
    {
        Debug.Log("Completed");
    }

    void OnApplicationEvent(BTEventType eventType, bool result)
    {
        if (eventType == BTEventType.ApplicationPause)
        {
            if (result)
            {
                Debug.Log("Game pausing");
                // TODO: Perform actions necessary to Disable Game Play. eg:Disabling Spin button to stop the user from playing
                // Other animations may continue. 
            }
            else
            {
                Debug.Log("Game resuming");
            }
        }
        else if (eventType == BTEventType.EmployeeInterrupt)
        {
            Debug.Log(string.Format("Employee Interrupt:{0}", result));
        }
        else if (eventType == BTEventType.PlayerCardPulled)
        {
            if (result) Debug.Log("perform autofinish if game mechanism dictates it");
            else Debug.Log("ignore");
        }
        else if (eventType == BTEventType.PromotionSettingsReady)
        {
            BTPromotionSettings promotion = BTUGameAPI.getPromotionSettings();
            Debug.Log("promotion settings available");
            Debug.Log("DisplayName	    :" + promotion.DisplayName);
            Debug.Log("TextMessage	    :" + promotion.TextMessage);
            Debug.Log("TextMessage	    :" + promotion.MessageKind);
            Debug.Log("Award			:" + promotion.getAward());
            Debug.Log("Award Format		:" + promotion.getAwardFormat());//currently awardFormatString is not available for this application.
            Debug.Log("Formatted Award	:" + promotion.getFormattedAward());
        }
        else if (eventType == BTEventType.DisplayBucketUpdate)
        {
            Dictionary<string, string> displayBuckets = BTUGameAPI.getCurrentBuckets();
            // Iterate through list of key-value pairs to extract any information newly added in content schedules
            // which is currently not supported in the API.
            while (displayBuckets != null)
            {
                Debug.Log(string.Format("Data Count:{0}", displayBuckets.Count));
                displayBuckets = BTUGameAPI.getCurrentBuckets();
            }
        }
    }
}
