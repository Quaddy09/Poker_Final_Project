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

	public GameObject cardAs;

	private List<ChipStack> chipElements;
	private List<CardDeck> deckElements;
	private string previousState;


	public ChipStack chipStack; 
	public CardDeck cardDeck;

	private Dictionary<int, GameObject> chipLookup;

	private Dictionary<string, GameObject> cardLookup;
	public GameObject deckReference;

	
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
			chipElements.Add(o);
			
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
			o.deckReference = deckReference;
			deckElements.Add(o);

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
    	cardLookup.Add("As", cardAs);

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
}
