using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class InstantLottoGameDM : InstantLottoGameBase
{

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
		quickPickRect = new Rect(baseWidth * 0.085f, baseHeight * 0.8f, baseWidth * 0.1f, baseHeight * 0.1f);
		buyTicketRect = new Rect(baseWidth * 0.2125f, baseHeight * 0.8f, baseWidth * 0.1f, baseHeight * 0.1f);
		playGameRect = new Rect(baseWidth * 0.34f, baseHeight * 0.8f, baseWidth * 0.1f, baseHeight * 0.1f);
		ticketSelectionRect = new Rect(baseWidth * 0.04f, baseHeight * 0.2f, baseWidth * 0.45f, baseHeight * 0.4f);
		displayNameRect = new Rect(ticketSelectionRect.x, baseHeight * 0.06f, ticketSelectionRect.width, baseHeight * 0.07f);
		exitButtonRect = new Rect(baseWidth * 0.87f, baseHeight * 0.05f, baseWidth * 0.1f, baseHeight * 0.1f);
        countdownRect = new Rect(ticketSelectionRect.x, baseHeight * 0.625f, ticketSelectionRect.width, baseHeight * 0.05f);
		availableBalanceRect = new Rect(ticketSelectionRect.x, baseHeight * 0.70f, ticketSelectionRect.width, baseHeight * 0.05f);
		currentPayLevelWindow = new Rect(baseWidth*0.80f, baseHeight*0.25f, baseWidth*0.175f, baseHeight*0.2f);

		purchasedTicketsWindowRect = new Rect(baseWidth*0.55f, baseHeight*0.1f, baseWidth*0.23f, baseHeight*0.35f);
		winningTicketWindow = new Rect(baseWidth*0.065f,baseHeight * 0.2f, baseWidth * 0.40f, baseHeight * 0.07f);
		resultsWindow = new Rect(baseWidth*0.04f,baseHeight * 0.3f, baseWidth * 0.45f, baseHeight * 0.45f);
		totalWinWindow = new Rect(baseWidth*0.065f,baseHeight * 0.8f, baseWidth * 0.40f, baseHeight * 0.17f);

		finalPayLevelsWindow = new Rect(baseWidth*0.515f, baseHeight*0.1f, baseWidth*0.465f, baseHeight*0.375f);

		progressivePrizesSplitRect  = new Rect(baseWidth*0.515f, baseHeight*0.04f, baseWidth*0.465f, baseHeight*0.03f);
		ratio = purchasedTicketsWindowRect.height / 240;
		RectOffset padding = new RectOffset(4,4,0,0);
		contentStyle = new GUIStyle();
		contentStyle.normal.background = content;
		contentStyle.alignment = TextAnchor.UpperLeft;
		contentStyle.normal.textColor = Color.white;
		contentStyle.wordWrap = true;
		contentStyle.padding = padding;
		contentStyle.padding.top = 4;
		contentStyle.padding.left = 20;
		contentStyle.fontSize = (int)(21 * ratio);
		otherFontSize = (int)(17 * ratio);
		fplBoxHeight = 50;
		if (BTGameSettings.IsStandalone())
		ConfigureGrid();
	}

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
		//GUI.Label(availableBalanceRect,"Available Balance:\t"+BTUGameAPI.CultureFormattedNumber(walletBalance,"{0:C}"));
        GUI.Box(availableBalanceRect,"Available Balance:\t"+BTUGameAPI.CultureFormattedNumber(PresentableWalletBalance(),"{0:C}"));//$10.00
		for (int i=0;i<selectionInstructionRect.Length;i++){
			GUI.Label(selectionInstructionRect[i], string.Format("Pick {0} total.",maximumAllowedSelection[i]));
		}
	}

    ulong ticketID = 1;
	void DrawGamePlayButtons(){
        if ((BTGameSettings.CurrentAppType==AppType.instantHitLotto &&
             lottoState!=LottoState.open) || 
            (BTGameSettings.CurrentAppType==AppType.classicLotto && 
         (classicLottoState!=ClassicLottoState.openForPurchase && 
         classicLottoState!=ClassicLottoState.countDown) || lottoState==LottoState.ticketsSent))
			GUI.enabled = false;
        GUI.skin.button.wordWrap = true;
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
        GUI.skin.button.wordWrap = false;
        if (BTGameSettings.CurrentAppType== AppType.instantHitLotto){
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
        if ((BTGameSettings.CurrentAppType==AppType.instantHitLotto &&
             lottoState!=LottoState.open) || 
            (BTGameSettings.CurrentAppType==AppType.classicLotto && 
         (classicLottoState!=ClassicLottoState.openForPurchase && 
         classicLottoState!=ClassicLottoState.countDown) || lottoState==LottoState.ticketsSent))
			GUI.enabled = false;
		if (GUI.Button(buyTicketRect, "Buy Ticket \nfor "+formattedCostPerTicket)){//$5.00
//			TODO:
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
	}

	void DrawPurchasedTicketsWindow(int windowID) {

        ticketCartRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),40)+GUI.skin.box.margin.bottom)*(tickets.Count);
        scrollPosition.y = ticketCartRect.height;

		float boxWidth = GUI.skin.box.fixedWidth;
		
		if (Screen.width==1024)
			GUI.skin.box.fixedWidth = 26;
		else GUI.skin.box.fixedWidth = 30;

        if (!BTGameSettings.IsStandalone()){
            GUI.skin.box.fontSize = otherFontSize-2;
        }
        scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, ticketCartRect, false, false);
        foreach (Ticket ticket in tickets.ticket){
			GUILayout.BeginHorizontal();
			GUILayout.Space(8);
			for (int i=0;i<ticket.Count;i++){
				foreach (Symbol symbol in ticket[i].symbol){
					GUILayout.Box(symbol);
					GUILayout.Space(2);
				}
				if (i!=ticket.Count-1)
					GUILayout.Space(18);
			}
			GUILayout.EndHorizontal();
			//ticketDetail=ticketDetail+string.Format("{0}\t\t-\t{1}\n",((tickets[index].id==0)?(ulong)(index+1):tickets[index].id), tickets[index]);
		}
		GUI.EndScrollView();
        if (!BTGameSettings.IsStandalone()){
            GUI.skin.box.fontSize = otherFontSize-2;
        }

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
		if (Screen.width==1024)
			GUI.skin.box.fixedWidth = 84;
		else GUI.skin.box.fixedWidth = 100;

		finalPayLevelsRect.height = (GUI.skin.box.CalcHeight(new GUIContent("10"),GUI.skin.box.fixedWidth)+GUI.skin.box.margin.bottom)*(payLevelValues.payLevel.Count);

		GUI.skin.box.fixedHeight=fplBoxHeight;
		GUILayout.BeginHorizontal();
		//GUI.skin.box.stretchHeight = true;
		GUILayout.Box("MATCH");
		GUILayout.Box("PRIZES");
        GUI.skin.box.wordWrap = true;
        GUILayout.Box("MY PRIZE");
		//GUI.skin.box.fixedWidth=160;
		GUI.skin.box.fontSize/=2;
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

	bool justOnce = true;

	public override void OnGUI(){
        base.OnGUI();
		if (justOnce){
			SetupStyles();
			justOnce = false;
		}
        if (BTGameSettings.CurrentAppType==AppType.classicLotto)
            GUI.Box(displayNameRect,string.Format("{0}\nClassicLottoState: {1}",DisplayName,classicLottoState));
        else 
            GUI.Box(displayNameRect,string.Format("{0}\nLottoState: {1}",DisplayName,lottoState));

		if (dataReady){
			GUI.skin.button.fontSize = otherFontSize;
			GUI.skin.box.fontSize = otherFontSize;
			GUI.skin.button.clipping = TextClipping.Overflow;
			GUI.skin.label.fontSize = contentStyle.fontSize;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.textArea.fontSize = contentStyle.fontSize;


			if (resultsReceived){
				// Show the Drawing
                if (!BTGameSettings.IsStandalone()){
                    GUI.skin.box.fontSize = otherFontSize-2;
                }
				GUI.Box(progressivePrizesSplitRect, "*PROGRESSIVE PRIZES SPLIT AMONG WINNING TICKETS");
                if (!BTGameSettings.IsStandalone()){
                    GUI.skin.box.fontSize = otherFontSize;
                }
				GUI.Window(2,winningTicketWindow, DrawWinningTicketWindow, "Winning Ticket");
                GUI.Window(3,resultsWindow, DrawResultsWindow, string.Format("Your Results ({0} Tickets)",tickets.Count));
				GUI.Window(4,finalPayLevelsWindow, DrawFinalPayLevelsWindow, "Final Pay Levels");
				GUI.Window(5,totalWinWindow, DrawTotalWinWindow, "Total Win");
				if (screenshotPending){
					StartCoroutine(TakeScreenshot());
				}
			} else {
				DrawSelectionGrid();
				DrawLabels();
				GUI.Window(1,purchasedTicketsWindowRect, DrawPurchasedTicketsWindow, string.Format("Currently Purchased Tickets ({0})",tickets.Count));
				DrawGamePlayButtons();
				GUI.Window(0,currentPayLevelWindow, DrawCurrentPayLevelWindow, "Current Pay Levels");
				DrawExit();
			}
		}
	}
	// Update is called once per frame
	void Update ()
	{

	}
}

