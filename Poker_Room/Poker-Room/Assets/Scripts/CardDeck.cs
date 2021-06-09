using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;


public class CardDeck : MonoBehaviour {

	private float deltaX;
	private float deltaZ;
	private Vector3 initialPosition;

	public float CARD_HEIGHT;

	public List<GameObject> cardReferences;
	public GameObject deckReference;

	public List<string> cardNames;
	public List<string> cardSuits;
	public List<string> cardSymbols;
	public List<string> cardValues;
	public List<bool> cardIsUps;
	private GameObject baseDeck;
	private GameObject topCard;

	private List<CardDeck> subs;
	public CardDeck cardDeck;

	public string elementId;
	public string updateUrl;
	public float rotation;

	public float defaultScale;
	public float defaultHeight;

	public bool isInfinite;

	private Dictionary<string, int> cardValueMap;

	private void OnMouseDown() {
		if(Input.GetButton("Sort") && !isInfinite) {
			// sort deck
			int len = cardNames.Count;
			for(int i = 1; i < len; i++) {
				int key = cardValueMap[cardNames[i]];
				GameObject cref = cardReferences[i];
				string name = cardNames[i];
				string suit = cardSuits[i];
				string sym = cardSymbols[i];
				string val = cardValues[i];
				bool isUp = cardIsUps[i];

				int j = i-1;
				while(j >= 0 && cardValueMap[cardNames[j]] > key) {
					cardReferences[j+1] = cardReferences[j];
					cardNames[j+1] = cardNames[j];
					cardSuits[j+1] = cardSuits[j];
					cardSymbols[j+1] = cardSymbols[j];
					cardValues[j+1] = cardValues[j];
					cardIsUps[j+1] = cardIsUps[j];

					j = j - 1;
				}
				cardReferences[j+1] = cref;
				cardNames[j+1] = name;
				cardSuits[j+1] = suit;
				cardSymbols[j+1] = sym;
				cardValues[j+1] = val;
				cardIsUps[j+1] = isUp;
			}

		} if(Input.GetButton("Flip") && Input.GetButton("Top") && !isInfinite) {
			cardIsUps[cardIsUps.Count - 1] = !cardIsUps[cardIsUps.Count - 1];
			topCard.transform.localScale = new Vector3(
				topCard.transform.localScale.x,
				-1 * topCard.transform.localScale.y,
				topCard.transform.localScale.z
			);
		} else if(/*Input.GetButton("PrivateFlip")*/false) {

		} else if(Input.GetButton("Flip") && Input.GetButton("Split") && !isInfinite) {
			for(int i = cardIsUps.Count / 2; i < cardIsUps.Count; i++) {
				cardIsUps[i] = !cardIsUps[i];
			}
			cardIsUps.Reverse(cardIsUps.Count/2, cardIsUps.Count-(cardIsUps.Count/2));
			cardReferences.Reverse(cardReferences.Count/2, cardReferences.Count-(cardReferences.Count/2));
			cardNames.Reverse(cardNames.Count/2, cardNames.Count-(cardNames.Count/2));
			cardSuits.Reverse(cardSuits.Count/2, cardSuits.Count-(cardSuits.Count/2));
			cardSymbols.Reverse(cardSymbols.Count/2, cardSymbols.Count-(cardSymbols.Count/2));
			cardValues.Reverse(cardValues.Count/2, cardValues.Count-(cardValues.Count/2));
			Destroy( topCard );
			topCard = Instantiate(
	        	cardReferences[cardReferences.Count-1],
	        	new Vector3(
	        		transform.position.x,
	        		cardReferences.Count*CARD_HEIGHT,
	        		transform.position.z
	        	), transform.rotation
	        );
	        topCard.transform.localScale = new Vector3(
	        	defaultScale,
	        	cardIsUps[cardIsUps.Count-1] ? defaultScale : -1 * defaultScale,
	        	defaultScale
	        );
		} else if(Input.GetButton("Flip") && !isInfinite) {
			for(int i = 0; i < cardIsUps.Count; i++) {
				cardIsUps[i] = !cardIsUps[i];
			}
			cardIsUps.Reverse(0, cardIsUps.Count);
			cardReferences.Reverse(0, cardReferences.Count);
			cardNames.Reverse(0, cardNames.Count);
			cardSuits.Reverse(0, cardSuits.Count);
			cardSymbols.Reverse(0, cardSymbols.Count);
			cardValues.Reverse(0, cardValues.Count);
			Destroy( topCard );
			topCard = Instantiate(
	        	cardReferences[cardReferences.Count-1],
	        	new Vector3(
	        		transform.position.x,
	        		cardReferences.Count*CARD_HEIGHT,
	        		transform.position.z
	        	), transform.rotation
	        );
	        topCard.transform.localScale = new Vector3(
	        	defaultScale,
	        	cardIsUps[cardIsUps.Count-1] ? defaultScale : -1 * defaultScale,
	        	defaultScale
	        );
		} else if(Input.GetButton("Top") && cardReferences.Count > 1) {
			CardDeck o = Instantiate(cardDeck, transform.position, transform.rotation);
			for(int i = cardReferences.Count-2; i>= 0; i--) {
				o.cardReferences.Add(cardReferences[i]);
				o.cardNames.Add(cardNames[i]);
				o.cardSuits.Add(cardSuits[i]);
				o.cardSymbols.Add(cardSymbols[i]);
				o.cardValues.Add(cardValues[i]);
				o.cardIsUps.Add(cardIsUps[i]);
				cardReferences.RemoveAt(i);
				cardNames.RemoveAt(i);
				cardSuits.RemoveAt(i);
				cardSymbols.RemoveAt(i);
				cardValues.RemoveAt(i);
				cardIsUps.RemoveAt(i);
			}
			o.elementId = Guid.NewGuid().ToString(); 
			o.updateUrl = updateUrl;
			o.rotation = rotation;
			o.deckReference = deckReference;
			o.cardDeck = cardDeck;
			if(isInfinite) {
				isInfinite = false;
				o.isInfinite = true;
				o.cardReferences.Add(cardReferences[0]);
				o.cardNames.Add(cardNames[0]);
				o.cardSuits.Add(cardSuits[0]);
				o.cardSymbols.Add(cardSymbols[0]);
				o.cardValues.Add(cardValues[0]);
				o.cardIsUps.Add(cardIsUps[0]);
			} else {
				o.isInfinite = false;
			}
			StartCoroutine(o.SendUpdate());
			subs.Add( o );
		} else if(Input.GetButton("Split") && cardReferences.Count > 1) {
			CardDeck o = Instantiate(cardDeck, transform.position, transform.rotation);
			for(int i = (cardReferences.Count-1)/2; i >= 0; i--) {
				o.cardReferences.Insert(0, cardReferences[i]);
				o.cardNames.Insert(0, cardNames[i]);
				o.cardSuits.Insert(0, cardSuits[i]);
				o.cardSymbols.Insert(0, cardSymbols[i]);
				o.cardValues.Insert(0, cardValues[i]);
				o.cardIsUps.Insert(0, cardIsUps[i]);
				cardReferences.RemoveAt(i);
				cardNames.RemoveAt(i);
				cardSuits.RemoveAt(i);
				cardSymbols.RemoveAt(i);
				cardValues.RemoveAt(i);
				cardIsUps.RemoveAt(i);

			}
			o.elementId = Guid.NewGuid().ToString(); 
			o.updateUrl = updateUrl;
			o.cardDeck = cardDeck;
			o.rotation = rotation;
			o.deckReference = deckReference;
			if(isInfinite) {
				isInfinite = false;
				o.isInfinite = true;
				for(int i = 0; i < cardReferences.Count; i++) {
					o.cardReferences.Add(cardReferences[i]);
					o.cardNames.Add(cardNames[i]);
					o.cardSuits.Add(cardSuits[i]);
					o.cardSymbols.Add(cardSymbols[i]);
					o.cardValues.Add(cardValues[i]);
					o.cardIsUps.Add(cardIsUps[i]);
				}
			} else {
				o.isInfinite = false;
			}
			StartCoroutine(o.SendUpdate());
			subs.Add( o );
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if(new Plane(Vector3.up, 0).Raycast(ray, out enter)) {
			if(isInfinite) {
				isInfinite = false;
				CardDeck o = Instantiate(cardDeck, transform.position, transform.rotation);
				for(int i = 0; i < cardReferences.Count; i++) {
					o.cardReferences.Add(cardReferences[i]);
					o.cardNames.Add(cardNames[i]);
					o.cardSuits.Add(cardSuits[i]);
					o.cardSymbols.Add(cardSymbols[i]);
					o.cardValues.Add(cardValues[i]);
					o.cardIsUps.Add(cardIsUps[i]);
				}
				o.elementId = elementId;
				elementId = Guid.NewGuid().ToString();
				o.isInfinite = true;
				o.updateUrl = updateUrl;
				o.rotation = rotation;
				o.cardDeck = cardDeck;
				o.deckReference = deckReference;
				StartCoroutine(o.SendUpdate());
				subs.Add(o);
			}
			Vector3 hitPoint = ray.GetPoint( enter );

			deltaX = hitPoint.x - transform.position.x;
			deltaZ = hitPoint.z - transform.position.z;
		}
	}

	private void OnMouseDrag() {
		int rotationSpeed = 1;
		if(Input.GetButton("RotateRight")) {
			baseDeck.transform.Rotate(0, rotationSpeed, 0);
			topCard.transform.Rotate(0,rotationSpeed,0);
		} else if(Input.GetButton("RotateLeft")) {
			baseDeck.transform.Rotate(0, -1*rotationSpeed, 0);
			topCard.transform.Rotate(0,-1*rotationSpeed,0);
		}
		rotation = baseDeck.transform.localRotation.eulerAngles.y;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if(new Plane(Vector3.up, 0).Raycast(ray, out enter)) {
			Vector3 hitPoint = ray.GetPoint( enter );
			transform.position = new Vector3(hitPoint.x - deltaX, 0, hitPoint.z - deltaZ);
			baseDeck.transform.position = new Vector3(
				transform.position.x, 
				0, 
				transform.position.z
			);
			topCard.transform.position = new Vector3(
				transform.position.x, 
				CARD_HEIGHT*cardReferences.Count, 
				transform.position.z
			);
			if(!Input.GetButton("PrivateFlip")) {
				StartCoroutine(SendUpdate());
			}
		}
	}
	private string ParseStringArray(string[] arr) {
		string s = "[";
		for( int i = 0; i < arr.Length - 1; i++ ) {
			s += "\""+arr[i] + "\",";
		}
		s += "\"" + arr[arr.Length-1] + "\"]";
		return s;
	}
	private string ParseBoolArray(bool[] arr) {
		string s = "[";
		for( int i = 0; i < arr.Length - 1; i++ ) {
			s += arr[i] + ",";
		}
		s += arr[arr.Length-1] + "]";
		return s.ToLower();
	}
	public IEnumerator SendUpdate() {
		WWWForm form = new WWWForm();
		form.AddField("elementId", elementId);
		form.AddField("posX", "" + transform.position.x);
		form.AddField("posY", "" + transform.position.z);
		form.AddField("rotation", "" + rotation);
		form.AddField("cardNames", ParseStringArray(cardNames.ToArray()));
		form.AddField("cardSuits", ParseStringArray(cardSuits.ToArray()));
		form.AddField("cardSymbols", ParseStringArray(cardSymbols.ToArray()));
		form.AddField("cardValues", ParseStringArray(cardValues.ToArray()));
		form.AddField("cardIsUps", ParseBoolArray(cardIsUps.ToArray()));
		form.AddField("isInfinite", isInfinite.ToString().ToLower());
		using(UnityWebRequest www = UnityWebRequest.Post(updateUrl, form)) {
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
	public void DestroySubs() {
		Destroy( baseDeck );
		Destroy( topCard );
		foreach(CardDeck o in subs) {
			o.DestroySubs();
			Destroy( o.gameObject );
		}
	}

	private void OnMouseUp() {
		if( true ) {

		} else {
			transform.position = new Vector3(
				initialPosition.x, 
				initialPosition.y, 
				initialPosition.z
			);
			baseDeck.transform.position = new Vector3(
				transform.position.x, 
				0, 
				transform.position.z
			);
			topCard.transform.position = new Vector3(
				transform.position.x, 
				CARD_HEIGHT*cardReferences.Count, 
				transform.position.z
			);
		}
	}

    // Start is called before the first frame update
    void Start() {
    	subs = new List<CardDeck>();
    	baseDeck = Instantiate(deckReference,
    		new Vector3(
    			transform.position.x,
    			0,
    			transform.position.z
    		), Quaternion.identity
    	);
    	baseDeck.transform.Rotate(0,rotation,0);
    	baseDeck.transform.localScale = new Vector3(
    		defaultScale, 
    		defaultScale*CARD_HEIGHT*cardReferences.Count/defaultHeight,
    		defaultScale
    	);
        initialPosition = transform.position;
        topCard = Instantiate(
        	cardReferences[cardReferences.Count-1],
        	new Vector3(
        		transform.position.x,
        		cardReferences.Count*CARD_HEIGHT,
        		transform.position.z
        	), Quaternion.identity
        );
        topCard.transform.Rotate(0,rotation,0);
        topCard.transform.localScale = new Vector3(
        	defaultScale,
        	cardIsUps[cardIsUps.Count-1] ? defaultScale : -1 * defaultScale,
        	defaultScale
        );
        cardValueMap = new Dictionary<string,int>();
        cardValueMap.Add("J1", 0);
    	cardValueMap.Add("J2", 0);

        cardValueMap.Add("Ac", 1);
    	cardValueMap.Add("2c", 2);
    	cardValueMap.Add("3c", 3);
    	cardValueMap.Add("4c", 4);
    	cardValueMap.Add("5c", 5);
    	cardValueMap.Add("6c", 6);
    	cardValueMap.Add("7c", 7);
    	cardValueMap.Add("8c", 8);
    	cardValueMap.Add("9c", 9);
    	cardValueMap.Add("Tc", 10);
    	cardValueMap.Add("Jc", 11);
    	cardValueMap.Add("Qc", 12);
    	cardValueMap.Add("Kc", 13);

    	cardValueMap.Add("Ad", 14);
    	cardValueMap.Add("2d", 15);
    	cardValueMap.Add("3d", 16);
    	cardValueMap.Add("4d", 17);
    	cardValueMap.Add("5d", 18);
    	cardValueMap.Add("6d", 19);
    	cardValueMap.Add("7d", 20);
    	cardValueMap.Add("8d", 21);
    	cardValueMap.Add("9d", 22);
    	cardValueMap.Add("Td", 23);
    	cardValueMap.Add("Jd", 24);
    	cardValueMap.Add("Qd", 25);
    	cardValueMap.Add("Kd", 26);

    	cardValueMap.Add("Ah", 27);
    	cardValueMap.Add("2h", 28);
    	cardValueMap.Add("3h", 29);
    	cardValueMap.Add("4h", 30);
    	cardValueMap.Add("5h", 31);
    	cardValueMap.Add("6h", 32);
    	cardValueMap.Add("7h", 33);
    	cardValueMap.Add("8h", 34);
    	cardValueMap.Add("9h", 35);
    	cardValueMap.Add("Th", 36);
    	cardValueMap.Add("Jh", 37);
    	cardValueMap.Add("Qh", 38);
    	cardValueMap.Add("Kh", 39);

    	cardValueMap.Add("As", 40);
    	cardValueMap.Add("2s", 41);
    	cardValueMap.Add("3s", 42);
    	cardValueMap.Add("4s", 43);
    	cardValueMap.Add("5s", 44);
    	cardValueMap.Add("6s", 45);
    	cardValueMap.Add("7s", 46);
    	cardValueMap.Add("8s", 47);
    	cardValueMap.Add("9s", 48);
    	cardValueMap.Add("Ts", 49);
    	cardValueMap.Add("Js", 50);
    	cardValueMap.Add("Qs", 51);
    	cardValueMap.Add("Ks", 52);


    }

    // Update is called once per frame
    void Update() {
        
    }
}
