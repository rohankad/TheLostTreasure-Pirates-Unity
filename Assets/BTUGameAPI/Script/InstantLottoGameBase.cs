using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class InstantLottoGameBase : InstantLottoTemplate {

	// Use this for initialization
	public override void Start () {
		base.Start();
        //Screen.fullScreen = true;
        //BTUGameAPI.ToggleRenderingStats(true);
		SetupDefaults();
	}

	protected void SetupDefaults(){
		if (BTGameSettings.IsStandalone()){
			UnitSetting us1 = new UnitSetting();
			us1.startSymbol = 1;
			us1.endSymbol = 30;
			us1.number = 1;
			us1.symbolsToSelect = 5;
			UnitSetting us2 = new UnitSetting();
			us2.startSymbol = 1;
			us2.endSymbol = 8;
			us2.number = 2;
			us2.symbolsToSelect = 1;
			lottoSettings = new LottoSettings();
			lottoSettings.UnitSettings.Add(us1); lottoSettings.UnitSettings.Add(us2);
			//			UnitSetting us3 = new UnitSetting();
			//			us3.startSymbol = 1;
			//			us3.endSymbol = 10;
			//			us3.number = 3;
			//			us3.symbolsToSelect = 2;
			//			lottoSettings.UnitSettings.Add(us3);
			//baseTicket = new Ticket(new Unit(1,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,26,27,28,29,30),new Unit(2,1,2,3,4));
			payLevelValues = new PayLevelValues();
			PayLevel pl0 = new PayLevel(1,100,"$100.00","4+1",0,true,100, true);
			PayLevel pl1 = new PayLevel(2,70,"$70.00","4+0",0,true,70, true);
			PayLevel pl2 = new PayLevel(3,80,"$80.00","3+1",0,true,80, false);
			PayLevel pl3 = new PayLevel(4,50,"$50.00","3+0",0,true,50, false);
			PayLevel pl4 = new PayLevel(5,60,"$60.00","2+1",0,true,60, true);
			PayLevel pl5 = new PayLevel(6,30,"$30.00","2+0",0,true,30, false);
			PayLevel pl6 = new PayLevel(7,40,"$40.00","1+1",0,true,40, true);
			PayLevel pl7 = new PayLevel(8,5,"$5.00","1+0",0,true,5, false);
			PayLevel pl8 = new PayLevel(9,0,"$0.00","0+0",0,true,0, true);
			payLevelValues.payLevel.Add(pl0);
			payLevelValues.payLevel.Add(pl1);
			payLevelValues.payLevel.Add(pl2);
			payLevelValues.payLevel.Add(pl3);
			payLevelValues.payLevel.Add(pl4);
			payLevelValues.payLevel.Add(pl5);
			payLevelValues.payLevel.Add(pl6);
			payLevelValues.payLevel.Add(pl7);
			payLevelValues.payLevel.Add(pl8);
			this.OnPayLevelUpdate(payLevelValues);
			lottoState = LottoState.open;
            classicLottoState = ClassicLottoState.openForPurchase;
		}
	}

	protected Vector2 resultsScrollPosition = Vector2.zero;
	protected Vector2 finalPayLevelsScrollPosition = Vector2.zero;
	protected Vector2 currentPayLevelsScrollPosition = Vector2.zero;
	protected int otherFontSize;
    protected Rect scrollViewRect,buyTicketRect,quickPickRect,playGameRect,displayNameRect,exitButtonRect,countdownRect,availableBalanceRect,currentPayLevelWindow,
	purchasedTicketsWindowRect,winningTicketWindow, resultsWindow, totalWinWindow, finalPayLevelsWindow, resultScrollViewRect, resultsTicketsRect,
	finalPayLevelsScrollViewRect, currentPayLevelsScrollViewRect, currentPayLevelsRect, finalPayLevelsRect, progressivePrizesSplitRect;

	protected GUIStyle contentStyle;
	protected Texture2D content;
	protected float ratio,fplBoxHeight = 31.25f;

	protected void SetupStyles(){
		GUI.skin.window.fontSize=14;
		GUI.skin.window.alignment = TextAnchor.UpperCenter;
		scrollViewRect = new Rect(0, GUI.skin.window.border.top,
		                          purchasedTicketsWindowRect.width,
		                          purchasedTicketsWindowRect.height-2*GUI.skin.window.border.bottom-10);
		ticketCartRect = new Rect(GUI.skin.window.border.left,
		                          GUI.skin.window.border.top,
		                          purchasedTicketsWindowRect.width-2*GUI.skin.window.border.right,
		                          scrollViewRect.height);
		
		resultScrollViewRect = new Rect(0, GUI.skin.window.border.top,
		                                resultsWindow.width,
		                                resultsWindow.height-2*GUI.skin.window.border.bottom-10);
		resultsTicketsRect = new Rect(GUI.skin.window.border.left,
		                              GUI.skin.window.border.top,
		                              resultsWindow.width-2*GUI.skin.window.border.right,
		                              resultScrollViewRect.height);
		
		finalPayLevelsScrollViewRect = new Rect(0, GUI.skin.window.border.top+fplBoxHeight+GUI.skin.box.margin.bottom,
		                                        finalPayLevelsWindow.width,
		                                        finalPayLevelsWindow.height-2*GUI.skin.window.border.bottom-10-fplBoxHeight-GUI.skin.box.margin.bottom);
		finalPayLevelsRect = new Rect(GUI.skin.window.border.left,
		                              finalPayLevelsScrollViewRect.y,
		                              finalPayLevelsWindow.width-2*GUI.skin.window.border.right,
		                              finalPayLevelsScrollViewRect.height);
		
		currentPayLevelsScrollViewRect = new Rect(0, GUI.skin.window.border.top,
		                                          currentPayLevelWindow.width,
		                                          currentPayLevelWindow.height-2*GUI.skin.window.border.bottom-10);
		currentPayLevelsRect = new Rect(GUI.skin.window.border.left,
		                                GUI.skin.window.border.top,
		                                currentPayLevelWindow.width-2*GUI.skin.window.border.right,
		                                currentPayLevelsScrollViewRect.height);	
	}

	public virtual void OnGUI(){
        //GUI.Box(ticketSelectionRect,"as");
        if (classicLottoState == ClassicLottoState.countDown){
            GUI.Box(countdownRect,string.Format("Buy Tickets Within: {0}hh {1}mm {2}ss",countDownTicker.Hours,countDownTicker.Minutes,countDownTicker.Seconds));//$10.00
        }
	}

    protected void DrawResultsWindow(int windowID){
        float buttonWidth = GUI.skin.button.fixedWidth;
        float buttonHeight = GUI.skin.button.fixedHeight;
        float boxHeight = GUI.skin.box.fixedHeight;
        float boxWidth = GUI.skin.box.fixedWidth;

        if (BTGameSettings.IsLVDS()){
            GUI.skin.button.fixedWidth = 26;
            GUI.skin.button.fixedHeight = 24;
            
            GUI.skin.box.fixedHeight= 23;
            GUI.skin.box.fixedWidth = 44;
        } else {
            if (Screen.width==1024)
                GUI.skin.button.fixedWidth = 34;
            else GUI.skin.button.fixedWidth = 50;
            
            GUI.skin.box.fixedHeight=30;
            GUI.skin.box.fixedWidth = 80;
        }

        GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        resultsTicketsRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),GUI.skin.box.fixedWidth)+GUI.skin.box.margin.bottom)*(tickets.Count);
        
        resultsScrollPosition = GUI.BeginScrollView(resultScrollViewRect, resultsScrollPosition, resultsTicketsRect, false, false);
        //sorted
        List<MatchTicketResult> ticketResults = drawingResult.MatchTicketResultSortedByWin(System.ComponentModel.ListSortDirection.Descending);
        for (int j=0;j<ticketResults.Count;j++){
            GUILayout.BeginHorizontal();
            GUILayout.Space(8);
            Ticket currentTicket = tickets.ticket.Find(x=>x.id == ticketResults[j].ID);
            for (int i=0;i<currentTicket.Count;i++){
                foreach (Symbol symbol in currentTicket[i].symbol){
                    GUI.enabled=false;
                    if (winningTicket[i].Contains(symbol)) GUI.enabled=true;
                    GUILayout.Button(symbol);
                    GUILayout.Space(2);
                }
                if (BTGameSettings.IsLVDS()){
                    GUILayout.Space(10);
                } else {
                    GUILayout.Space(5);
                }
            }
            GUI.enabled=true;
            GUILayout.Box(ticketResults[j].LevelDesc);
            if (ticketResults[j].Win==0) GUI.enabled=false;
            if (BTGameSettings.IsStandalone())
                GUILayout.Box(BTUGameAPI.CultureFormattedNumber(ticketResults[j].Win,"{0:C}"));
            else GUILayout.Box(BTUGameAPI.CultureFormattedNumber(ticketResults[j].Win));
            GUILayout.EndHorizontal();
        }
        //non-sorted
        //      for (int j=0;j<tickets.Count;j++){
        //          GUILayout.BeginHorizontal();
        //          GUILayout.Space(8);
        //          for (int i=0;i<tickets[j].Count;i++){
        //              foreach (Symbol symbol in tickets[j][i].symbol){
        //                  GUI.enabled=false;
        //                  if (winningTicket[i].Contains(symbol)) GUI.enabled=true;
        //                  GUILayout.Button(symbol);
        //                  GUILayout.Space(2);
        //              }
        //              //if (i!=ticket.Count-1)
        //                  GUILayout.Space(5);
        //          }
        //          GUI.enabled=true;
        //          GUILayout.Box(drawingResult[j].LevelDesc);
        //          if (drawingResult[j].Win==0) GUI.enabled=false;
        //          if (BTGameSettings.IsStandalone())
        //              GUILayout.Box(BTUGameAPI.CultureFormattedNumber(drawingResult.MatchTicketResult[j].Win,"{0:C}"));
        //          else GUILayout.Box(BTUGameAPI.CultureFormattedNumber(drawingResult.MatchTicketResult[j].Win));
        //          GUILayout.EndHorizontal();
        //      }
        GUI.EndScrollView();
        GUI.enabled=true;
        GUI.skin.button.fixedWidth = buttonWidth;
        GUI.skin.button.fixedHeight = buttonHeight;
        GUI.skin.box.fixedHeight = boxHeight;
        GUI.skin.box.fixedWidth = boxWidth;
    }

	protected void DrawExit(){
		if (lottoState==LottoState.resultsPresenting || lottoState==LottoState.ticketsSent
            || lottoState==LottoState.resultsReceived || lottoState==LottoState.resultsPresentedSent
            || classicLottoState==ClassicLottoState.locked || classicLottoState==ClassicLottoState.preShow)
			GUI.enabled=false;
        if (BTGameSettings.CurrentAppType==AppType.instantHitLotto &&  lottoState==LottoState.ticketsAcked)
            GUI.enabled=false;
		if (GUI.Button(exitButtonRect, "Exit")){
            if (BTUGameAPI.getCurrentPlayerInfo().IsCardlessPlay && ActualWalletBalance()>0){//also can use BTWallet.TotalBalance
                InvokeRepeating("InitiateTransferAndExit",0,1);
            } else {
                BTUGameAPI.SendCommand(BTCommand.actionExitGame);
            }
		}
		GUI.enabled = true;
	}

	protected void ResetGrid(int i){
		gridData[i].Where(x=> x.selected = true).ToList().ForEach(y => y.selected = false);
	}
	protected bool screenshotPending = true;
	protected bool showFinalPayLevels = false;
    protected const string NotEnoughNumbers = "NotEnoughNumbers";
    protected const string NotEnoughBalance = "NotEnoughBalance";
	protected void DrawTotalWinWindow(int windowID){
		GUILayout.BeginHorizontal();
		string totalWinString = string.Format("Total Win:\t{0}",BTUGameAPI.CultureFormattedNumber(drawingResult.TotalWin));
		GUILayout.Label(totalWinString);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (drawingResult.TotalWin==0){
			GUILayout.Box("Sorry! Try again.");
		} else {
			if (payMethod == PaymentMethod.handpay){
				GUILayout.Box("Call Attendant to Collect your Win.");
			} else {
				GUILayout.Box("Exit game to Collect your Win.");
			}
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		GUI.skin.button.stretchHeight = true;
        if (BTGameSettings.CurrentAppType==AppType.instantHitLotto){
            if (lottoState!=LottoState.resultsPresentedAcked || !BTUGameAPI.getPromotionSettings().IsActive)
                GUI.enabled = false;
    		if (GUILayout.Button("Play Again")){
    			BTUGameAPI.SendCommand(BTCommand.actionResetGame);
    			for (int i=0;i<gridData.Count;i++){
    				ResetGrid(i);
    			}
    			copyToPrevious();
    			tickets = new Tickets();
    			if (BTGameSettings.IsStandalone()){
    				lottoState = LottoState.open;
    			}
    			resultsReceived = false;
    			tickets = new Tickets();
    			ticketCartRect.height = scrollViewRect.height;
    			resultsTicketsRect.height = resultScrollViewRect.height;
    			finalPayLevelsRect.height = finalPayLevelsScrollViewRect.height;
    			currentPayLevelsRect.height = currentPayLevelsScrollViewRect.height;
    			scrollPosition = Vector2.zero;
    			resultsScrollPosition = Vector2.zero;
    			finalPayLevelsScrollPosition = Vector2.zero;
    			currentPayLevelsScrollPosition = Vector2.zero;
    			screenshotPending = true;
    			showFinalPayLevels = false;
    			//tickets
    		}
            GUI.enabled=true;
        }
        if (lottoState!=LottoState.resultsPresentedAcked)
            GUI.enabled = false;
		if (GUILayout.Button("Exit")){
            if (BTUGameAPI.getCurrentPlayerInfo().IsCardlessPlay && ActualWalletBalance()>0){//also can use BTWallet.TotalBalance
                InvokeRepeating("InitiateTransferAndExit",0,1);
            } else {
			    BTUGameAPI.SendCommand(BTCommand.actionExitGame);
            }
		}
        GUI.enabled=true;
		GUI.skin.button.stretchHeight = false;
		GUILayout.EndHorizontal();
	}

    protected int seconds = 6;
    protected void InitiateTransferAndExit(){
        BTUGameAPI.ShowDialog(string.Format(BTUGameAPI.GetStringForKey("PreparingTransfer"),BTUGameAPI.CultureFormattedNumber(BTWallet.TotalBalance),seconds),string.Empty,DialogType.StatusBox,null);
        seconds--;
        if (seconds==0){
            BTUGameAPI.SendCommand(BTCommand.actionExitGame);
            CancelInvoke("InitiateTransferAndExit");
        }
    }

    protected void WalletDialogHandler(bool result){
        if (result)
        {
            BTUGameAPI.ShowWallet();
        } else {
            //Do Nothing.
        }
    }
	protected IEnumerator TakeScreenshot(){
		screenshotPending = false;
		yield return new WaitForSeconds(0.5f);
		BTUGameAPI.CaptureScreenshot(CaptureComplete);
	}
	// Update is called once per frame
	void Update () {
	
	}
}
