using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class DrawableSymbol{
	public GUIContent content;
	public bool selected = false;
	public Rect rect;

	public DrawableSymbol(GUIContent inContent, bool sel, Rect inRect){
		content = inContent;
		rect = inRect;
		selected = sel;
	}

}

public class InstantLottoTemplate : MonoBehaviour {

	// Use this for initialization
	public virtual void Start () {
		DontDestroyOnLoad(gameObject);
		// NOTES: Tickets Contains a group of Ticket objects
		// Ticket Contains a list of Unit objects (In current game theme there are 2 units, 2nd unit is the power ball/mega)
		// Unit contains a list of Symbol objects
		// Symbol is inherently a string. But we can directly assign Symbol s1 = 7 or Symbol s2 = "a". 
		// Also int value = s1 and string value = s1 (implicit conversion from data types supported).
		BTUGameAPI.SetOnApplicationEvent(OnApplicationEvent);
		//mandatory calls end
		BTUGameAPI.SetStateChangeCallBack(OnLottoStateChange, ObservationType.Both);
		BTUGameAPI.SetStateChangeCallBack(OnWinningsRequestStateChange, ObservationType.Both);

        if (BTGameSettings.CurrentAppType==AppType.classicLotto){//additional call back
            BTUGameAPI.SetStateChangeCallBack(OnClassicLottoStateChange, ObservationType.Both);
            //the following callback is optional
            //BTUGameAPI.SetStateChangeCallBack(OnTicketRequestStateChange, ObservationType.Both);
        } else {
            BTUGameAPI.SetStateChangeCallBack(OnTicketSelectionStateChange, ObservationType.Both);
        }

		BTUGameAPI.SetPayLevelUpdateCallBack(OnPayLevelUpdate);
		BTUGameAPI.SetWalletUpdateCallBack(OnWalletUpdate);
		BTUGameAPI.SetTicketsAvailableCallBack(OnTicketsUpdate);
        BTUGameAPI.SetWinningTicketAvailableCallBack(OnWinningTicketAvailable);
        BTUGameAPI.SetIndividualResultAvailableCallBack(OnIndividualResultAvailable);
	}

	public LottoState lottoState = LottoState.idle;
    public ClassicLottoState classicLottoState = ClassicLottoState.idle;
	public WinningsRequestState winningsRequestState = WinningsRequestState.idle;
	string lottoStateMessage 		= "Lotto Game State - " + LottoState.idle.ToString();
	string winningsRequestStateMessage 		= "Lotto Game State - " + WinningsRequestState.idle.ToString();

	public LottoSettings lottoSettings;
	public decimal costPerTicket;
	public string formattedCostPerTicket;
	public string DisplayName = "Lotto Testing Template";
	public Ticket baseTicket;

	public void OnLottoStateChange(string message, params LottoState[] lottoGameStates){
		//if current Lotto State == resultsReceiveFailed or ticketSubmissionFailed, message field will contain the reason
		foreach (LottoState state in lottoGameStates){
			Debug.Log("LottoGameState:"+state);
		}
		lottoState = lottoGameStates[1];
		if (string.IsNullOrEmpty(message)){
			lottoStateMessage ="Lotto Game State - "+ lottoState.ToString();
		} else {
			lottoStateMessage = string.Format("Bonus Game State - {0}:{1}",lottoState.ToString(),message);
        }
        Debug.Log(lottoStateMessage);
	}
	
	public void OnWinningsRequestStateChange(string message, params WinningsRequestState[] winningsRequestStates){
		foreach (WinningsRequestState state in winningsRequestStates){
			Debug.Log("WinningsRequestState:"+state);
		}
		winningsRequestState = winningsRequestStates[1];
		if (string.IsNullOrEmpty(message)){
			winningsRequestStateMessage = "Winnings Request State - "+winningsRequestState.ToString();
		} else {
			winningsRequestStateMessage = string.Format("Winnings Request State - {0}:{1}",winningsRequestState.ToString(),message);
		}
        Debug.Log(winningsRequestStateMessage);
	}

    public void OnClassicLottoStateChange(string message, params ClassicLottoState[] lottoGameStates){
        //if current Lotto State == resultsReceiveFailed or ticketSubmissionFailed, message field will contain the reason
        foreach (ClassicLottoState state in lottoGameStates){
            Debug.Log("ClassicLottoGameState:"+state);
        }
        classicLottoState = lottoGameStates[1];
        if (string.IsNullOrEmpty(message)){
            lottoStateMessage ="Lotto Game State - "+ classicLottoState.ToString();
        } else {
            lottoStateMessage = string.Format("Bonus Game State - {0}:{1}",classicLottoState.ToString(),message);
        }           
        if (classicLottoState==ClassicLottoState.countDown){
            DateTime cra=BTUGameAPI.getPromotionSettings().CountdownReceivedAt;
            uint duration = BTUGameAPI.getPromotionSettings().CountdownDurationInSeconds;
            DateTime now = DateTime.Now;
            countDownTicker = TimeSpan.FromSeconds(duration-(now-cra).TotalSeconds);
            InvokeRepeating("performCountdown",0,1);
            Debug.Log("countdownReceivedAt:"+cra);
            Debug.Log("now:"+now);
            Debug.Log("duration:"+duration);
            Debug.Log("countDownTicker:"+countDownTicker);
        } else if (classicLottoState == ClassicLottoState.locked){
            if (IsInvoking("performCountdown")){
                CancelInvoke("performCountdown");
            }
        } else if (classicLottoState == ClassicLottoState.showResults){
            startShow();
        }
    }

    protected void startShow(){
        if (classicResultsReceived && classicLottoState == ClassicLottoState.showResults && !IsInvoking("startResults") &&!resultsReceived){
            DateTime showStartTime = BTUGameAPI.getPromotionSettings().ShowStartTime;
            DateTime now = DateTime.Now;
            Debug.Log("showStartTime:"+showStartTime);
            Debug.Log("now:"+now);
            if (showStartTime>now){
                Invoke("startResults",(float)((showStartTime-now).TotalSeconds));
            } else startResults();
        }
    }

    protected void startResults(){
        resultsReceived = true;
    }

    protected void performCountdown(){
        if (countDownTicker.Hours==0 && countDownTicker.Minutes==0 && countDownTicker.Seconds==0){
            CancelInvoke("performCountdown");
            classicLottoState = ClassicLottoState.locked;
            return;
        }
        countDownTicker=countDownTicker.Subtract(TimeSpan.FromSeconds(1));
    }

    public void OnTicketRequestStateChange(string message, params TicketRequestState[] ticketRequestStates){
    }

    public void OnTicketSelectionStateChange(string message, params TicketSelectionState[] ticketSelectionStates){
    }

	protected Tickets tickets = new Tickets();
    protected TimeSpan countDownTicker;
	protected Rect ticketCartRect,ticketSelectionRect;
    protected Vector2 scrollPosition = Vector2.zero;
	protected PayLevelValues payLevelValues;
	protected List<List<DrawableSymbol>> gridData = new List<List<DrawableSymbol>>();
	protected List<List<bool>> previousGridData = new List<List<bool>>();
	protected Rect[] selectionInstructionRect;
	decimal walletBalance;
	protected Ticket winningTicket;
	protected DrawingResult drawingResult;
	protected PaymentMethod payMethod;
	protected bool resultsReceived = false;
    protected bool classicResultsReceived = false;

    protected decimal PresentableWalletBalance(){
        if (BTGameSettings.CurrentAppType==AppType.classicLotto) return ActualWalletBalance();
        return walletBalance-(costPerTicket*tickets.Count);
    }
    protected decimal ActualWalletBalance(){
        return walletBalance;
    }

	protected void OnTicketsUpdate(Tickets inTickets){
		tickets = inTickets;
		Debug.Log("Updated Tickets:"+tickets);
        if (BTGameSettings.CurrentAppType == AppType.classicLotto && BTGameSettings.IsStandalone())
            lottoState = LottoState.ticketsAcked;
	}

    void OnIndividualResultAvailable(DrawingResult result){
        this.drawingResult = result;
        Debug.Log(result);
    }

	void OnWinningTicketAvailable(Ticket winningTicket, PayLevelValues finalPayLevels){
		Debug.Log("Winning Ticket:"+winningTicket);
		this.winningTicket = winningTicket;
		this.payMethod = BTUGameAPI.getPromotionSettings().PayMethod;
		this.payLevelValues = finalPayLevels;
		Debug.Log(finalPayLevels);
		Debug.Log(payMethod);
        filterPayLevels();
        if (BTGameSettings.CurrentAppType == AppType.instantHitLotto){
		    resultsReceived = true;
        } else classicResultsReceived = true;
        startShow();
		//NOTES: Make sure Screenshot is taken after result is presented as below and fire the actionResultsPresented in Capturecomplete callback.
		//BTUGameAPI.CaptureScreenshot(CaptureComplete);
	}

	protected void CaptureComplete(){
		Debug.Log("Capture Complete");
		BTUGameAPI.SendCommand(BTCommand.actionResultsPresented);
	}

	void OnWalletUpdate(decimal walletBalance, decimal totalBet){
		Debug.Log("New Balance:"+walletBalance);
        this.walletBalance = walletBalance;
	}

    void filterPayLevels(){
        if (payLevelValues!=null){
            payLevelValues.payLevel = payLevelValues.payLevel.Where(x=>x.isPaying == true).ToList<PayLevel>();
        }
    }
       
	public void OnPayLevelUpdate(PayLevelValues payLevelValues){
		this.payLevelValues = payLevelValues;
        filterPayLevels();
		//Debug.Log(payLevelValues);
	}

	int[] symbolsAcross=new int[]{5,2,2,1,1};
	protected int[] maximumAllowedSelection;
	protected bool dataReady = false;
	protected void ConfigureGrid(){
		baseTicket= new Ticket();
		int noOfUnits = lottoSettings.UnitSettings.Count;
		Debug.Log(lottoSettings);
		float unitSpacing = (ticketSelectionRect.width-(ticketSelectionRect.width/((noOfUnits>1)?(noOfUnits+0.2f):int.MaxValue))*noOfUnits);
		float unitWidth=ticketSelectionRect.width/(noOfUnits)-(unitSpacing-(unitSpacing)/noOfUnits);
		maximumAllowedSelection = new int[lottoSettings.UnitSettings.Count];
		selectionInstructionRect = new Rect[lottoSettings.UnitSettings.Count];
		foreach (UnitSetting unitSetting in lottoSettings.UnitSettings){
			Unit unit=new Unit();
			unit.number = unitSetting.number;
			maximumAllowedSelection[unitSetting.number-1]=unitSetting.symbolsToSelect;
			if (BTGameSettings.IsLVDS()){
				selectionInstructionRect[unitSetting.number-1]= new Rect(ticketSelectionRect.x+(unitWidth+unitSpacing)*(unit.number-1),ticketSelectionRect.y-0.09f*Screen.height,unitWidth,0.08f*Screen.height);
			} else {
				selectionInstructionRect[unitSetting.number-1]= new Rect(ticketSelectionRect.x+(unitWidth+unitSpacing)*(unit.number-1),ticketSelectionRect.y-0.06f*Screen.height,unitWidth,0.05f*Screen.height);
			}
			List<DrawableSymbol> symbolsToDraw = new List<DrawableSymbol>();
			int totalSymbolsInUnit = ((unitSetting.endSymbol-unitSetting.startSymbol)+1);
			int symbolCount = (totalSymbolsInUnit<symbolsAcross[unit.number-1])?totalSymbolsInUnit:symbolsAcross[unit.number-1];//((unitSetting.endSymbol-unitSetting.startSymbol)+1)/symbolsAcross[unit.number-1];
			float symbolHorizontalSpacing = unitWidth-(unitWidth/((symbolCount>1)?(symbolCount+0.1f):int.MaxValue))*symbolCount;
			int totalRows = (totalSymbolsInUnit/symbolCount);
            if (totalSymbolsInUnit%symbolCount!=0)totalRows++;
			float symbolVerticalSpacing = ticketSelectionRect.height-(ticketSelectionRect.height/((totalRows>1)?(totalRows+0.1f):int.MaxValue))*totalRows;
			float symbolWidth = unitWidth/(symbolCount)-(symbolHorizontalSpacing-(symbolHorizontalSpacing)/symbolCount);
			float symbolHeight = ticketSelectionRect.height/(totalRows)-(symbolVerticalSpacing-(symbolVerticalSpacing)/totalRows);
			int symbolsAcrossCounter=0;
			float xPos = ticketSelectionRect.x+(unitWidth+unitSpacing)*(unit.number-1);
			float initialXPos = xPos;
			float yPos = ticketSelectionRect.y;
			for (int i=unitSetting.startSymbol;i<=unitSetting.endSymbol;i++){
				unit+=i;
				DrawableSymbol symbol = new DrawableSymbol(new GUIContent(i.ToString()),false, new Rect(xPos,yPos,symbolWidth,symbolHeight));
				symbolsAcrossCounter++;
				if (symbolsAcrossCounter>=symbolsAcross[unit.number-1]){
					yPos=yPos+symbolHeight+symbolVerticalSpacing;
					xPos = initialXPos;
					symbolsAcrossCounter=0;
				} else {
					xPos=xPos+symbolWidth+symbolHorizontalSpacing;
				}
				symbolsToDraw.Add(symbol);
			}
			gridData.Add(symbolsToDraw);
			previousGridData.Add(Enumerable.Repeat(false,symbolsToDraw.Count).ToList());
			baseTicket+=unit;
		}
		dataReady = true;
		Debug.Log(baseTicket);
	}

	protected void copyToPrevious(){
		for (int i=0;i<gridData.Count;i++){
			for (int j=0;j<gridData[i].Count;j++){
				previousGridData[i][j]=gridData[i][j].selected;
			}
		}
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

		} else if (eventType == BTEventType.PromotionSettingsReady){
			BTPromotionSettings promotion = BTUGameAPI.getPromotionSettings();
			Debug.Log("promotion settings available");
			//Settings to determine the numbers on the ticket and selection count permitted.
			lottoSettings = promotion.LottoSettings;
			DisplayName = promotion.DisplayName;
			costPerTicket = promotion.CostPerTicket;
			formattedCostPerTicket = BTUGameAPI.getPromotionSettings().FormattedCostPerTicket;
			Debug.Log(promotion.ThemeName);
			ConfigureGrid();
		} else if (eventType == BTEventType.DisplayBucketUpdate){
			Dictionary<string, string> displayBuckets = BTUGameAPI.getCurrentBuckets();			
			// OPTIONAL: Iterate through list of key-value pairs to extract any information newly added in content schedules
			// which is currently not supported in the API.
			while (displayBuckets!=null){
				Debug.Log(string.Format("Data Count:{0}",displayBuckets.Count));
				displayBuckets = BTUGameAPI.getCurrentBuckets();
			}

		} else if (eventType == BTEventType.EmployeeInterrupt){
			Debug.Log(string.Format("Employee Interrupt:{0}",result));

		} else if (eventType == BTEventType.PlayerCardPulled){
			if (result) Debug.Log("perform autofinish if game mechanism dictates it");
			else Debug.Log("ignore");

		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
