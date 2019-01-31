using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class InstantLottoGameLVDS : InstantLottoGameBase {

	// Use this for initialization
	public override void Start ()
	{
		base.Start();
		float baseWidth=Screen.width;
		float baseHeight=Screen.height;
		content = new Texture2D(1,1);
		Color32[] colors = new Color32[content.width * content.height];
		for (int i = 0; i < colors.Length; i++)
		{
			colors[i] = Color.clear;
		}
		content.SetPixels32(colors);
		content.Apply();
		quickPickRect = new Rect(baseWidth * 0.00f, baseHeight * 0.85f, baseWidth * 0.15f, baseHeight * 0.15f);
		buyTicketRect = new Rect(baseWidth * 0.175f, baseHeight * 0.85f, baseWidth * 0.15f, baseHeight * 0.15f);
		playGameRect = new Rect(baseWidth * 0.35f, baseHeight * 0.85f, baseWidth * 0.15f, baseHeight * 0.15f);
		viewPurchasedTicketsRect = new Rect(baseWidth * 0.525f, baseHeight * 0.85f, baseWidth * 0.25f, baseHeight * 0.15f);
		ticketSelectionRect = new Rect(baseWidth * 0.04f, baseHeight * 0.15f, baseWidth * 0.92f, baseHeight * 0.65f);
		displayNameRect = new Rect(ticketSelectionRect.x, baseHeight * 0.01f, ticketSelectionRect.width, baseHeight * 0.08f);
		exitButtonRect = new Rect(baseWidth * 0.9f, baseHeight * 0.0f, baseWidth * 0.1f, baseHeight * 0.15f);
		availableBalanceRect = new Rect(baseWidth*0.80f, baseHeight*0.85f, baseWidth*0.2f, baseHeight*0.15f);

		currentPayLevelWindow = new Rect(baseWidth*0.65f, baseHeight*0.15f, baseWidth*0.25f, baseHeight*0.65f);
		
		purchasedTicketsWindowRect = new Rect(baseWidth*0.05f, baseHeight*0.15f, baseWidth*0.45f, baseHeight*0.65f);
		winningTicketWindow = new Rect(baseWidth*0.59f,baseHeight * 0.1f, baseWidth * 0.40f, baseHeight * 0.2f);
		resultsWindow = new Rect(baseWidth*0.01f,baseHeight * 0.1f, baseWidth * 0.52f, baseHeight * 0.9f);
		totalWinWindow = new Rect(baseWidth*0.59f,baseHeight * 0.5f, baseWidth * 0.40f, baseHeight * 0.5f);

		bottomButtonPanelWindow = new Rect(baseWidth * 0.04f, baseHeight * 0.82f, baseWidth * 0.92f, baseHeight * 0.18f);
		finalPayLevelsWindow = new Rect(baseWidth*0.01f, baseHeight*0.2f, baseWidth*0.52f, baseHeight*0.8f);
		progressivePrizesSplitRect  = new Rect(baseWidth*0.01f, baseHeight*0.09f, baseWidth*0.53f, baseHeight*0.09f);

		paytableViewRect = new Rect(baseWidth*0.64f,baseHeight * 0.325f, baseWidth * 0.30f, baseHeight * 0.15f);

		float scrollBarButtonWidth = 34;
		float scrollBarButtonHeight = 30;

		ticketsUpButton = new Rect(purchasedTicketsWindowRect.x+purchasedTicketsWindowRect.width+Screen.width*0.01f,purchasedTicketsWindowRect.y,scrollBarButtonWidth,scrollBarButtonHeight);
		ticketsDownButton = new Rect(ticketsUpButton.x,purchasedTicketsWindowRect.y+purchasedTicketsWindowRect.height-scrollBarButtonHeight,scrollBarButtonWidth,scrollBarButtonHeight);

		cplUpButton = new Rect(currentPayLevelWindow.x+currentPayLevelWindow.width+Screen.width*0.01f,currentPayLevelWindow.y,scrollBarButtonWidth,scrollBarButtonHeight);
		cplDownButton = new Rect(cplUpButton.x,currentPayLevelWindow.y+currentPayLevelWindow.height-scrollBarButtonHeight,scrollBarButtonWidth,scrollBarButtonHeight);

		resultsUpButton = new Rect(resultsWindow.x+resultsWindow.width+Screen.width*0.005f,resultsWindow.y,scrollBarButtonWidth,scrollBarButtonHeight);
		resultsDownButton = new Rect(resultsUpButton.x,resultsWindow.y+resultsWindow.height-scrollBarButtonHeight,scrollBarButtonWidth,scrollBarButtonHeight);

		fplUpButton = new Rect(finalPayLevelsWindow.x+finalPayLevelsWindow.width+Screen.width*0.005f,finalPayLevelsWindow.y,scrollBarButtonWidth,scrollBarButtonHeight);
		fplDownButton = new Rect(fplUpButton.x,finalPayLevelsWindow.y+finalPayLevelsWindow.height-scrollBarButtonHeight,scrollBarButtonWidth,scrollBarButtonHeight);

		RectOffset padding = new RectOffset(4,4,0,0);
		contentStyle = new GUIStyle();
		contentStyle.normal.background = content;
		contentStyle.alignment = TextAnchor.UpperLeft;
		contentStyle.normal.textColor = Color.white;
		contentStyle.wordWrap = true;
		contentStyle.padding = padding;
		contentStyle.padding.top = 4;
		contentStyle.padding.left = 20;
		contentStyle.fontSize = (20);
		otherFontSize = (15);

		fplBoxHeight=31.25f;

		if (BTGameSettings.IsStandalone())
		ConfigureGrid();
	}

	protected Rect bottomButtonPanelWindow, viewPurchasedTicketsRect, ticketsUpButton, ticketsDownButton, cplUpButton, cplDownButton,
	paytableViewRect, resultsUpButton, resultsDownButton, fplUpButton, fplDownButton;
	bool showPurchasedTicketsAndPayLevels = false;

	float scrollDistance = 20;
	string upSymbol = "▲",downSymbol = "▼";
	GUISkin customSkin;
	void checkSelectionLimit(int unitNum, int symbolPosition){
		int counter = previousGridData[unitNum].Where(x => x==true).Count();
		if (previousGridData[unitNum][symbolPosition]) {
			previousGridData[unitNum][symbolPosition] = false;
		} 
		if (!previousGridData[unitNum][symbolPosition] && counter < maximumAllowedSelection[unitNum]) {
			previousGridData[unitNum][symbolPosition] = true;
		}
		if (!previousGridData[unitNum][symbolPosition] && counter == maximumAllowedSelection[unitNum]) {
			gridData[unitNum][symbolPosition].selected = false;
		}
		
	}
	
	bool IsEnoughSymbolsSelected(){
		for (int i=0;i<gridData.Count;i++){
			if (gridData[i].Where(x => x.selected==true).Count() != maximumAllowedSelection[i]){
				return false;
			}
		}
		return true;
	}
	
	void DrawSelectionGrid(){
		for (int i=0;i<gridData.Count;i++){
			for (int j=0;j<gridData[i].Count;j++){
				DrawableSymbol symbol = gridData[i][j];
				symbol.selected = GUI.Toggle(symbol.rect,symbol.selected,symbol.content,"Button");
				if (symbol.selected!=previousGridData[i][j])
					checkSelectionLimit(i,j);
			}
		}
	}
	
	void DrawLabels(){
		//GUI.Label(availableBalanceRect,"Available Balance:\n"+BTUGameAPI.CultureFormattedNumber(walletBalance,"{0:C}"));
		GUI.skin.box.wordWrap = true;
		GUI.skin.box.alignment = TextAnchor.MiddleCenter;
        GUI.Box(availableBalanceRect,"Available Balance:\n"+BTUGameAPI.CultureFormattedNumber(PresentableWalletBalance(),"{0:C}"));//$10.00
		GUI.skin.box.wordWrap = false;
		int tmpFontSize = GUI.skin.label.fontSize;
		GUI.skin.label.fontSize=12;
		for (int i=0;i<selectionInstructionRect.Length;i++){
			GUI.Label(selectionInstructionRect[i], string.Format("Pick {0} total.",maximumAllowedSelection[i]));
		}
		GUI.skin.label.fontSize = tmpFontSize;
	}
	
    ulong ticketID = 1;

	void DrawGamePlayButtons(){
        if ((BTGameSettings.CurrentAppType==AppType.instantHitLotto &&
             lottoState!=LottoState.open) || 
            (BTGameSettings.CurrentAppType==AppType.classicLotto && 
         (classicLottoState!=ClassicLottoState.openForPurchase && 
         classicLottoState!=ClassicLottoState.countDown) || lottoState==LottoState.ticketsSent))
			GUI.enabled = false;
		if (GUI.Button(quickPickRect,"Pick Random")){
			for (int i=0;i<gridData.Count;i++){
				ResetGrid(i);
				int[] indexes=new int[maximumAllowedSelection[i]];
				for (int j=0;j<maximumAllowedSelection[i];j++){
					int random = Random.Range (0,gridData[i].Count);
					while (indexes.Contains(random)){
						random = Random.Range (0,gridData[i].Count);
					}
					indexes[j]=random;
					gridData[i][indexes[j]].selected = true;
				}
			}
			copyToPrevious();
		}
		GUI.enabled = true;
        if (BTGameSettings.CurrentAppType==AppType.instantHitLotto){
		if (lottoState!=LottoState.open || tickets.Count==0)
			GUI.enabled=false;
		if (GUI.Button(playGameRect, "Play Game")){
			// SUBMIT Ticket to Framework.
			BTUGameAPI.SubmitTickets(tickets);
			if (BTGameSettings.IsStandalone()) { 
				lottoState=LottoState.resultsReceived;
				resultsReceived=true;
				winningTicket = new Ticket();
				for (int i=0;i<gridData.Count;i++){
					int[] indexes=new int[maximumAllowedSelection[i]];
					Unit unit = new Unit();unit.number = baseTicket.unit[i].number;
					for (int j=0;j<maximumAllowedSelection[i];j++){
						int random = Random.Range (0,gridData[i].Count);
						while (indexes.Contains(random)){
							random = Random.Range (0,gridData[i].Count);
						}
						indexes[j]=random;
						unit+=gridData[i][indexes[j]].content.text;
					}
					winningTicket+=unit;
				}
				drawingResult = tickets.Winnings(winningTicket,payLevelValues);
			}
		}
        }
		GUI.enabled = true;
		GUI.skin.button.wordWrap = true;
        if ((BTGameSettings.CurrentAppType==AppType.instantHitLotto &&
             lottoState!=LottoState.open) || 
            (BTGameSettings.CurrentAppType==AppType.classicLotto && 
         (classicLottoState!=ClassicLottoState.openForPurchase && 
         classicLottoState!=ClassicLottoState.countDown) || lottoState==LottoState.ticketsSent))
			GUI.enabled = false;
        if (GUI.Button(buyTicketRect, "Buy Ticket for "+formattedCostPerTicket)){//$5.00
//          TODO:
            if (costPerTicket>PresentableWalletBalance()){
                BTUGameAPI.ShowDialog(BTUGameAPI.GetStringForKey(NotEnoughBalance), string.Empty, DialogType.ConfirmationBox,WalletDialogHandler);
                return;
            }
			if (IsEnoughSymbolsSelected()){
				// Creating a Unit 
				List<Unit> units = new List<Unit>();
				foreach (Unit unit in baseTicket.unit){
					units.Add(new Unit(unit.number,gridData[unit.number-1].Where(x=>x.selected==true).Select(x=>(Symbol)x.content.text).ToArray()));
				}
				Ticket ticket = new Ticket(units);
                if (BTGameSettings.IsStandalone()) {
                    ticket.id = ticketID++;
                }
                tickets += ticket;//Adding a ticket to a Tickets object. Also supported tickets.AddTicket
                if (BTGameSettings.CurrentAppType == AppType.instantHitLotto || BTGameSettings.IsStandalone()){
                    if (BTGameSettings.CurrentAppType == AppType.classicLotto)
                        lottoState = LottoState.ticketsSent;
                    tickets += ticket;//Adding a ticket to a Tickets object. Also supported tickets.AddTicket
                    OnTicketsUpdate(tickets);
                } else {
                    Tickets puchasedTicket = new Tickets(ticket);
                    BTUGameAPI.SubmitTickets(puchasedTicket);
                }
				for (int i=0;i<gridData.Count;i++){
					gridData[i].Where(x=> x.selected = true).ToList().ForEach(y => y.selected = false);
				}
			} else {
                BTUGameAPI.ShowDialog(BTUGameAPI.GetStringForKey(NotEnoughNumbers), string.Empty, DialogType.InformationBox,null);
			}
		}
		GUI.enabled = true;
        if ((BTGameSettings.CurrentAppType==AppType.instantHitLotto &&
             lottoState!=LottoState.open) || 
            (BTGameSettings.CurrentAppType==AppType.classicLotto && 
         classicLottoState!=ClassicLottoState.openForPurchase && 
         classicLottoState!=ClassicLottoState.countDown))
			GUI.enabled = false;
        if (GUI.Button(viewPurchasedTicketsRect,string.Format("Purchased Tickets({0}) & Current Pay Levels",tickets.Count))){
			showPurchasedTicketsAndPayLevels = true;
		}
		GUI.skin.button.wordWrap = false;
		GUI.enabled = true;
	}
	
	void DrawPurchasedTicketsWindow(int windowID) {
        ticketCartRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),40)+GUI.skin.box.margin.bottom)*(tickets.Count);
        scrollPosition.y = ticketCartRect.height;

		float boxWidth = GUI.skin.box.fixedWidth;

		//scrollPosition.y = GUI.VerticalScrollbar(scrollViewRect,scrollPosition.y,2,1,10);

		scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, ticketCartRect, false, false);

		GUI.skin.box.fixedWidth = 30;

		foreach (Ticket ticket in tickets.ticket){
			GUILayout.BeginHorizontal();
			GUILayout.Space(8);
			for (int i=0;i<ticket.Count;i++){
				foreach (Symbol symbol in ticket[i].symbol){
					GUILayout.Box(symbol);//"▲▼"
					GUILayout.Space(2);
				}
				if (i!=ticket.Count-1)
					GUILayout.Space(18);
			}
			GUILayout.EndHorizontal();
		}
		GUI.EndScrollView();

		GUI.skin.box.fixedWidth = boxWidth;
	}
	
	void DrawCurrentPayLevelWindow(int windowID) {
		float boxHeight = GUI.skin.box.fixedHeight;
		currentPayLevelsRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),GUI.skin.box.fixedWidth)+GUI.skin.box.margin.bottom)*(payLevelValues.payLevel.Count);
		currentPayLevelsScrollPosition = GUI.BeginScrollView(currentPayLevelsScrollViewRect, currentPayLevelsScrollPosition, currentPayLevelsRect, false, false);
		
		foreach (PayLevel payLevel in payLevelValues.payLevel){
            if (payLevel.isPaying){
    			GUILayout.BeginHorizontal();
    			GUILayout.Box(string.Format("{0}\t=\t{1}",(payLevel.isProgressive)?payLevel.LevelDesc+"*":payLevel.LevelDesc,payLevel.formattedValue));
    			GUILayout.EndHorizontal();
            }
		}
		GUI.EndScrollView();
		GUI.skin.box.fixedHeight = boxHeight;
	}
	
	void DrawWinningTicketWindow(int windowID){
		GUILayout.BeginHorizontal();
		for (int i=0;i<winningTicket.Count;i++){
			foreach (Symbol symbol in winningTicket[i].symbol){
				GUILayout.Button(symbol);
				GUILayout.Space(5);
			}
			if (i!=winningTicket.Count-1)
				GUILayout.Space(25);
		}
		GUILayout.EndHorizontal();
	}
	
	void DrawFinalPayLevelsWindow(int windowID){
		float boxHeight = GUI.skin.box.fixedHeight;
		float boxWidth = GUI.skin.box.fixedWidth;
		int boxFontSize = GUI.skin.box.fontSize;
		//if (Screen.width==640)
			GUI.skin.box.fixedWidth = 57.5f;
		//else GUI.skin.box.fixedWidth = 100;
		
		finalPayLevelsRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),GUI.skin.box.fixedWidth)+GUI.skin.box.margin.bottom)*(payLevelValues.payLevel.Count);
		
		GUI.skin.box.fixedHeight=fplBoxHeight;
		GUILayout.BeginHorizontal();
		//GUI.skin.box.stretchHeight = true;
		GUILayout.Box("MATCH");
		GUILayout.Box("PRIZES");
		GUI.skin.box.fontSize=11;
		GUILayout.Box("MY PRIZE");
		GUI.skin.box.fontSize=8;
		GUI.skin.box.wordWrap = true;
		GUILayout.Box("MY WINNING TICKETS");
		GUILayout.Box("TOTAL WINNING TICKETS");
		GUI.skin.box.wordWrap = false;
		//GUI.skin.box.fixedWidth=100;
		GUI.skin.box.fontSize = boxFontSize;
		GUILayout.EndHorizontal();
		
		GUI.skin.box.fixedHeight = boxHeight;
		
		finalPayLevelsScrollPosition = GUI.BeginScrollView(finalPayLevelsScrollViewRect, finalPayLevelsScrollPosition, finalPayLevelsRect, false, false);
		
		foreach (PayLevel payLevel in payLevelValues.payLevel){
            if (payLevel.isPaying){
    			GUILayout.BeginHorizontal();
    			GUILayout.Space(8);
    			GUILayout.Box((payLevel.isProgressive)?payLevel.LevelDesc+"*":payLevel.LevelDesc);
    			GUILayout.Box(payLevel.formattedValue);
    			int myWinningTickets = drawingResult.MatchTicketResult.Where(x=>x.LevelDesc==payLevel.LevelDesc).Count();
    			GUILayout.Box(BTUGameAPI.CultureFormattedNumber(payLevel.prizePerTicket*myWinningTickets));
    			if (BTGameSettings.IsStandalone()){
    				GUILayout.Box(myWinningTickets.ToString());
    				GUILayout.Box(myWinningTickets.ToString());
    			} else {
    				GUILayout.Box(myWinningTickets.ToString());
    				GUILayout.Box(payLevel.noOfWinningTickets.ToString());
    			}
    			GUILayout.EndHorizontal();
            }
		}
		GUI.EndScrollView();
		GUI.skin.box.fixedHeight = boxHeight;
		GUI.skin.box.fixedWidth = boxWidth;
	}

	void DrawPurchasedTickets(){
		if (GUI.Button(ticketsUpButton,upSymbol)){
			scrollPosition.y-=scrollDistance;
			Mathf.Clamp(scrollPosition.y,0,ticketCartRect.height);
		}
		if (GUI.Button(ticketsDownButton,downSymbol)){
			scrollPosition.y+=scrollDistance;
			Mathf.Clamp(scrollPosition.y,0,ticketCartRect.height);
		}
		GUI.Window(1,purchasedTicketsWindowRect, DrawPurchasedTicketsWindow, string.Format("Currently Purchased Tickets ({0})",tickets.Count));
	}
	
	void DrawCurrentPayLevels(){
		if (GUI.Button(cplUpButton,upSymbol)){
			currentPayLevelsScrollPosition.y-=scrollDistance;
			Mathf.Clamp(currentPayLevelsScrollPosition.y,0,currentPayLevelsRect.height);
		}
		if (GUI.Button(cplDownButton,downSymbol)){
			currentPayLevelsScrollPosition.y+=scrollDistance;
			Mathf.Clamp(currentPayLevelsScrollPosition.y,0,currentPayLevelsRect.height);
		}
		
		GUI.Window(0,currentPayLevelWindow, DrawCurrentPayLevelWindow, "Current Pay Levels");
	}

	void DrawFinalPayLevels(){
		if (GUI.Button(fplUpButton,upSymbol)){
			finalPayLevelsScrollPosition.y-=scrollDistance;
			Mathf.Clamp(finalPayLevelsScrollPosition.y,0,finalPayLevelsRect.height);
		}
		if (GUI.Button(fplDownButton,downSymbol)){
			finalPayLevelsScrollPosition.y+=scrollDistance;
			Mathf.Clamp(finalPayLevelsScrollPosition.y,0,finalPayLevelsRect.height);
		}
		GUI.Window(4,finalPayLevelsWindow, DrawFinalPayLevelsWindow, "Final Pay Levels");
	}

	void DrawResults(){
		if (GUI.Button(resultsUpButton,upSymbol)){
			resultsScrollPosition.y-=scrollDistance;
			Mathf.Clamp(resultsScrollPosition.y,0,resultsTicketsRect.height);
		}
		if (GUI.Button(resultsDownButton,downSymbol)){
			resultsScrollPosition.y+=scrollDistance;
			Mathf.Clamp(resultsScrollPosition.y,0,resultsTicketsRect.height);
		}

        GUI.Window(3,resultsWindow, DrawResultsWindow, string.Format("Your Results ({0} Tickets)",tickets.Count));
	}

	bool justOnce = true;

	public override void OnGUI(){
        base.OnGUI();
		if (justOnce){
			SetupStyles();
			justOnce = false;
		}
		GUI.Label(displayNameRect,DisplayName);
		if (dataReady){
			GUI.skin.button.fontSize = otherFontSize;
			GUI.skin.box.fontSize = otherFontSize;
			GUI.skin.button.clipping = TextClipping.Overflow;
			GUI.skin.label.fontSize = contentStyle.fontSize;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.textArea.fontSize = contentStyle.fontSize;
			//GUI.skin.button.alignment = TextAnchor.MiddleCenter;
			
			if (resultsReceived){
				// Show the Drawing
				GUI.Window(2,winningTicketWindow, DrawWinningTicketWindow, "Winning Ticket");
				if (showFinalPayLevels){
					int fontSize = GUI.skin.box.fontSize;
					if (Screen.width == 640) GUI.skin.box.fontSize = 12;
					GUI.Box(progressivePrizesSplitRect, "*PROGRESSIVE PRIZES SPLIT AMONG WINNING TICKETS");
					GUI.skin.box.fontSize = fontSize;
					DrawFinalPayLevels();
					if (GUI.Button(paytableViewRect, "TICKET VIEW")){
						showFinalPayLevels = false;
					}
				} else {
					DrawResults();

					if (GUI.Button(paytableViewRect, "PAYTABLE VIEW")){
						showFinalPayLevels = true;
					}
				}
				GUI.Window(5,totalWinWindow, DrawTotalWinWindow, "Total Win");
				if (screenshotPending){
					StartCoroutine(TakeScreenshot());
				}
			} else if (showPurchasedTicketsAndPayLevels){
				DrawPurchasedTickets();
				DrawCurrentPayLevels();
				if (GUI.Button(viewPurchasedTicketsRect,"Back to Ticket Selection")){
					showPurchasedTicketsAndPayLevels = false;
				}
				//DrawExit();
			} else {
				DrawSelectionGrid();
				DrawLabels();
				DrawGamePlayButtons();
				DrawExit();
			}
		}
	}
	// Update is called once per frame
	void Update ()
	{
		
	}
}
