using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
// using Newtonsoft.Json;
using SimpleJSON;

public class CasinoTable : MonoBehaviour {	  

	public string url;
	public string tableId;
	public string username;
	public string password;
	public string cookie;
	private bool isUpdating;

	public GameObject yellowChip;
	public GameObject redChip;
	public GameObject greenChip;
	public GameObject blueChip;
	public GameObject blackChip;

	private List<ChipStack> chipElements;
	private List<CardDeck> deckElements;
	private string previousState;


	public ChipStack chipStack; 
	public CardDeck cardDeck;

	private Dictionary<int, GameObject> chipLookup;

	private Dictionary<string, GameObject> cardLookup;
	public GameObject deckReference;

	public float CHIP_RADIUS;
	public float CARD_RADIUS;
	public float TABLE_RADIUS;
	
	public IEnumerator GetCookie(string url, string username, string password) {

		WWWForm form = new WWWForm();
		form.AddField("id", username);
		form.AddField("password", password);

		using( UnityWebRequest www = UnityWebRequest.Post(url, form)) {
			yield return www.SendWebRequest();

			if(www.isNetworkError) {
				Debug.Log(www.error);
			} else {
				if( www.isDone ) {
					string cookie = www.GetResponseHeader("Set-Cookie");
					this.cookie = cookie.Substring (cookie.IndexOf ("=")+1).Split (';') [0];
					// StartCoroutine(GetState(getStateUrl));


				} else {
					Debug.Log( "Couldn't get data" );
				}
			}
		}

	}

	public IEnumerator GetState(string url) {
		this.isUpdating = true;
		using( UnityWebRequest www = UnityWebRequest.Post(url, new WWWForm()) ) {
			// Debug.Log(this.cookie);
			www.SetRequestHeader("Cookie", this.cookie);
			yield return www.SendWebRequest();
			if(www.isNetworkError) {
				Debug.Log(www.error);
			} else {
				if( www.isDone ) {
					var result = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);
					JSONNode table = JSON.Parse(result);
					if(!previousState.Equals(result)) {
						UpdateScene( table );
					}
					previousState = result;

				} else {
					Debug.Log( "Couldn't get data" );
				}
			}
		}
		this.isUpdating = false;
	}
	public static void DumpToConsole(object obj)
    {
        var output = JsonUtility.ToJson(obj, true);
        Debug.Log(output);  
    }
	public void UpdateScene( JSONNode table ) {
		// Use table object element values to create game pieces in the scene
		
		// clear scene
		for(int i = 0; i < chipElements.Count; ++i) {
			chipElements[i].DestroySubs();
			Destroy( chipElements[i].gameObject );
		}
		chipElements.Clear();
		for(int i = 0; i < deckElements.Count; ++i) {
			deckElements[i].DestroySubs();
			Destroy( deckElements[i].gameObject );
		}
		deckElements.Clear();

		// add new stacks to scene
		foreach(JSONNode element in table["stacks"]) {
			ChipStack o = Instantiate(chipStack, new Vector3(
				element["pos"]["x"], 
				0,
				element["pos"]["y"]
			), Quaternion.identity);
			// DumpToConsole(o); 

			foreach(JSONNode chip in element["stack"]) {
				o.chipReferences.Add(chipLookup[chip["value"]]);
				o.chipValues.Add(chip["value"]);
				o.chipRotations.Add(chip["rotation"]);
			}
			o.elementId = element["id"];
			o.updateUrl = url + "/table/" + tableId + "/modifyelement/chipstack";
			o.chipStack = chipStack;
			o.isInfinite = element["isInfinite"];
			chipElements.Add(o);	
		}

		
		// combine overlapping stacks
		List<int> stackIndecesToDestroy = new List<int>();
		List<int> stackIndecesToShuffle = new List<int>();
		for(int i = 0; i < chipElements.Count; i++) {
			for(int j = 0; j < i; j++) {
				if(Vector3.Distance(chipElements[i].transform.position, chipElements[j].transform.position) < 2 * CHIP_RADIUS) {
					foreach(GameObject chip in chipElements[j].chipReferences) {
						chipElements[i].chipReferences.Add(chip);
					} foreach(int _value in chipElements[j].chipValues) {
						chipElements[i].chipValues.Add(_value);
					} foreach(int rotation in chipElements[j].chipRotations) {
						chipElements[i].chipRotations.Add(rotation);
					}
					chipElements[j].chipReferences.Clear();
					chipElements[j].chipValues.Clear();
					chipElements[j].chipRotations.Clear();
					stackIndecesToDestroy.Add(j);
					stackIndecesToShuffle.Add(i);
					StartCoroutine(chipElements[i].SendUpdate());
				}
			}
		}
		
		// shuffle combined stacks if key held
		if(Input.GetButton("Shuffle")) {
			foreach(int index in stackIndecesToShuffle) {
				for(int i = chipElements[index].chipReferences.Count-1; i > 0; i--) {
					int k = UnityEngine.Random.Range(0, i);

					GameObject refTemp = chipElements[index].chipReferences[i];
					int valTemp = chipElements[index].chipValues[i];
					float rotTemp = chipElements[index].chipRotations[i];

					chipElements[index].chipReferences[i] = chipElements[index].chipReferences[k];
					chipElements[index].chipValues[i] = chipElements[index].chipValues[k];
					chipElements[index].chipRotations[i] = chipElements[index].chipRotations[k];

					chipElements[index].chipReferences[k] = refTemp;
					chipElements[index].chipValues[k] = valTemp;
					chipElements[index].chipRotations[k] = rotTemp;
				}
				StartCoroutine(chipElements[index].SendUpdate());
			}
		}
		foreach(int i in stackIndecesToDestroy) {
			StartCoroutine(SendDelete(chipElements[i].elementId));
			// chipElements[i].DestroySubs();
			Destroy(chipElements[i].gameObject);
			chipElements.RemoveAt(i);
		}

		// add new decks to scene
		foreach(JSONNode element in table["decks"]) {
			CardDeck o = Instantiate(cardDeck, new Vector3(
				element["pos"]["x"],
				0,
				element["pos"]["y"]
			), Quaternion.identity);

			foreach(JSONNode card in element["deck"]) {
				o.cardReferences.Add(cardLookup[card["name"]]);
				o.cardNames.Add(card["name"]);
				o.cardSuits.Add(card["suit"]);
				o.cardSymbols.Add(card["symbol"]);
				o.cardValues.Add(card["value"]);
				o.cardIsUps.Add(card["isUp"]);
			}
			o.elementId = element["id"];
			o.updateUrl = url + "/table/" + tableId + "/modifyelement/carddeck";
			o.rotation = element["rotation"];
			o.isInfinite = element["isInfinite"];
			o.deckReference = deckReference;
			o.cardDeck = cardDeck;
			deckElements.Add(o);

		}

		// combine overlapping decks
		List<int> deckIndecesToDestroy = new List<int>();
		List<int> deckIndecesToShuffle = new List<int>();
		for(int i = 0; i < deckElements.Count; i++) {
			for(int j = 0; j < i; j++) {
				if(Vector3.Distance(deckElements[i].transform.position, deckElements[j].transform.position) < CARD_RADIUS) {
					foreach(GameObject card in deckElements[j].cardReferences) {
						deckElements[i].cardReferences.Add(card);
					} foreach(string name in deckElements[j].cardNames) {
						deckElements[i].cardNames.Add(name);
					} foreach(string suit in deckElements[j].cardSuits) {
						deckElements[i].cardSuits.Add(suit);
					} foreach(string symbol in deckElements[j].cardSymbols) {
						deckElements[i].cardSymbols.Add(symbol);
					} foreach(string _value in deckElements[j].cardValues) {
						deckElements[i].cardValues.Add(_value);
					} foreach(bool isUp in deckElements[j].cardIsUps) {
						deckElements[i].cardIsUps.Add(isUp);
					}
					deckElements[j].cardReferences.Clear();
					deckElements[j].cardNames.Clear();
					deckElements[j].cardSuits.Clear();
					deckElements[j].cardSymbols.Clear();
					deckElements[j].cardValues.Clear();
					deckElements[j].cardIsUps.Clear();
					deckIndecesToDestroy.Add(j);
					deckIndecesToShuffle.Add(i);
					
					StartCoroutine(deckElements[i].SendUpdate());
				}
			}
		}
		
		// shuffle combined decks if key held
		if(Input.GetButton("Shuffle")) {
			foreach(int index in deckIndecesToShuffle) {
				for(int i = deckElements[index].cardNames.Count-1; i > 0; i--) {
					int k = UnityEngine.Random.Range(0, i);

					GameObject refTemp = deckElements[index].cardReferences[i];
					string nameTemp = deckElements[index].cardNames[i];
					string suitTemp = deckElements[index].cardSuits[i];
					string symTemp = deckElements[index].cardSymbols[i];
					string valTemp = deckElements[index].cardValues[i];
					bool isUpTemp = deckElements[index].cardIsUps[i];

					deckElements[index].cardReferences[i] = deckElements[index].cardReferences[k];
					deckElements[index].cardNames[i] = deckElements[index].cardNames[k];
					deckElements[index].cardSuits[i] = deckElements[index].cardSuits[k];
					deckElements[index].cardSymbols[i] = deckElements[index].cardSymbols[k];
					deckElements[index].cardValues[i] = deckElements[index].cardValues[k];
					deckElements[index].cardIsUps[i] = deckElements[index].cardIsUps[k];

					deckElements[index].cardReferences[k] = refTemp;
					deckElements[index].cardNames[k] = nameTemp;
					deckElements[index].cardSuits[k] = suitTemp;
					deckElements[index].cardSymbols[k] = symTemp;
					deckElements[index].cardValues[k] = valTemp;
					deckElements[index].cardIsUps[k] = isUpTemp;

				}
				StartCoroutine(deckElements[index].SendUpdate());
			}
		}
		foreach(int i in deckIndecesToDestroy) {
			StartCoroutine(SendDelete(deckElements[i].elementId));
			// deckElements[i].DestroySubs();
			Destroy(deckElements[i].gameObject);
			deckElements.RemoveAt(i);
		}

		// Destroy elements outside of table
		for(int i = 0; i < chipElements.Count; i++) {
			if(Vector3.Distance(Vector3.zero, chipElements[i].transform.position) > TABLE_RADIUS) {
				Destroy(chipElements[i]);
				StartCoroutine(SendDelete(chipElements[i].elementId));
				chipElements.RemoveAt(i);
			}
		}
		for(int i = 0; i < deckElements.Count; i++) {
			if(Vector3.Distance(Vector3.zero, deckElements[i].transform.position) > TABLE_RADIUS) {
				Destroy(deckElements[i]);
				StartCoroutine(SendDelete(deckElements[i].elementId));
				deckElements.RemoveAt(i);
			}
		}

	}

	private IEnumerator SendDelete(string id) {
		WWWForm form = new WWWForm();
		form.AddField("elementId", id);
		using(UnityWebRequest www = UnityWebRequest.Post(url + "/table/" + tableId + "/deleteelement", form)) {
			yield return  www.SendWebRequest();
			if(www.isNetworkError) {
				Debug.Log(www.error);
			} else {
				if(www.isDone) {

				}  else {
					Debug.Log( "Couldn't get data" );
				}
			}
		}
	}

    // Start is called before the first frame update
    void Start() {
    	var loginUrl = url + "/login";
    	chipElements = new List<ChipStack>();
    	deckElements = new List<CardDeck>();
    	previousState = "";

    	chipLookup = new Dictionary<int, GameObject>();
    	chipLookup.Add(5, yellowChip);
    	chipLookup.Add(10, redChip);
    	chipLookup.Add(25, greenChip);
    	chipLookup.Add(50, blueChip);
    	chipLookup.Add(100, blackChip);

    	cardLookup = new Dictionary<string, GameObject>();
    	InitializeCards();

    	this.isUpdating = false;
        StartCoroutine(GetCookie(loginUrl, username, password));
    }

    // Update is called once per frame
    void Update() {
    	var getStateUrl = url + "/table/" + tableId + "/getstate";
    	if(this.cookie != "" && !this.isUpdating && !Input.GetMouseButton(0)) {
        	StartCoroutine(GetState(getStateUrl));
    	}
    }
    private void InitializeCards() {
    	cardLookup.Add("Ac", cardAc);
    	cardLookup.Add("2c", card2c);
    	cardLookup.Add("3c", card3c);
    	cardLookup.Add("4c", card4c);
    	cardLookup.Add("5c", card5c);
    	cardLookup.Add("6c", card6c);
    	cardLookup.Add("7c", card7c);
    	cardLookup.Add("8c", card8c);
    	cardLookup.Add("9c", card9c);
    	cardLookup.Add("Tc", cardTc);
    	cardLookup.Add("Jc", cardJc);
    	cardLookup.Add("Qc", cardQc);
    	cardLookup.Add("Kc", cardKc);

    	cardLookup.Add("Ad", cardAd);
    	cardLookup.Add("2d", card2d);
    	cardLookup.Add("3d", card3d);
    	cardLookup.Add("4d", card4d);
    	cardLookup.Add("5d", card5d);
    	cardLookup.Add("6d", card6d);
    	cardLookup.Add("7d", card7d);
    	cardLookup.Add("8d", card8d);
    	cardLookup.Add("9d", card9d);
    	cardLookup.Add("Td", cardTd);
    	cardLookup.Add("Jd", cardJd);
    	cardLookup.Add("Qd", cardQd);
    	cardLookup.Add("Kd", cardKd);

    	cardLookup.Add("Ah", cardAh);
    	cardLookup.Add("2h", card2h);
    	cardLookup.Add("3h", card3h);
    	cardLookup.Add("4h", card4h);
    	cardLookup.Add("5h", card5h);
    	cardLookup.Add("6h", card6h);
    	cardLookup.Add("7h", card7h);
    	cardLookup.Add("8h", card8h);
    	cardLookup.Add("9h", card9h);
    	cardLookup.Add("Th", cardTh);
    	cardLookup.Add("Jh", cardJh);
    	cardLookup.Add("Qh", cardQh);
    	cardLookup.Add("Kh", cardKh);

    	cardLookup.Add("As", cardAs);
    	cardLookup.Add("2s", card2s);
    	cardLookup.Add("3s", card3s);
    	cardLookup.Add("4s", card4s);
    	cardLookup.Add("5s", card5s);
    	cardLookup.Add("6s", card6s);
    	cardLookup.Add("7s", card7s);
    	cardLookup.Add("8s", card8s);
    	cardLookup.Add("9s", card9s);
    	cardLookup.Add("Ts", cardTs);
    	cardLookup.Add("Js", cardJs);
    	cardLookup.Add("Qs", cardQs);
    	cardLookup.Add("Ks", cardKs);

    	cardLookup.Add("J1", cardJoker1);
    	cardLookup.Add("J2", cardJoker2);
    }

    public GameObject cardAc;
    public GameObject card2c;
    public GameObject card3c;
    public GameObject card4c;
    public GameObject card5c;
    public GameObject card6c;
    public GameObject card7c;
    public GameObject card8c;
    public GameObject card9c;
    public GameObject cardTc;
    public GameObject cardJc;
    public GameObject cardQc;
    public GameObject cardKc;

    public GameObject cardAd;
    public GameObject card2d;
    public GameObject card3d;
    public GameObject card4d;
    public GameObject card5d;
    public GameObject card6d;
    public GameObject card7d;
    public GameObject card8d;
    public GameObject card9d;
    public GameObject cardTd;
    public GameObject cardJd;
    public GameObject cardQd;
    public GameObject cardKd;

    public GameObject cardAh;
    public GameObject card2h;
    public GameObject card3h;
    public GameObject card4h;
    public GameObject card5h;
    public GameObject card6h;
    public GameObject card7h;
    public GameObject card8h;
    public GameObject card9h;
    public GameObject cardTh;
    public GameObject cardJh;
    public GameObject cardQh;
    public GameObject cardKh;

    public GameObject cardAs;
    public GameObject card2s;
    public GameObject card3s;
    public GameObject card4s;
    public GameObject card5s;
    public GameObject card6s;
    public GameObject card7s;
    public GameObject card8s;
    public GameObject card9s;
    public GameObject cardTs;
    public GameObject cardJs;
    public GameObject cardQs;
    public GameObject cardKs;

    public GameObject cardJoker1;
    public GameObject cardJoker2;
}
