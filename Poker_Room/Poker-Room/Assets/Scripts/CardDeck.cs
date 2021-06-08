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

	private void OnMouseDown() {

		if(Input.GetButton("Flip") && Input.GetButton("Top")) {
			cardIsUps[cardIsUps.Count - 1] = !cardIsUps[cardIsUps.Count - 1];
			topCard.transform.localScale = new Vector3(
				topCard.transform.localScale.x,
				-1 * topCard.transform.localScale.y,
				topCard.transform.localScale.z
			);
		} else if(Input.GetButton("Flip") && Input.GetButton("Split")) {
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
		} else if(Input.GetButton("Flip")) {
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
			for(int i = cardReferences.Count - 2; i >= 0; i--) {
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
			o.rotation = rotation;
			o.deckReference = deckReference;
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
			o.rotation = rotation;
			o.deckReference = deckReference;
			StartCoroutine(o.SendUpdate());
			subs.Add( o );
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if(new Plane(Vector3.up, 0).Raycast(ray, out enter)) {
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

			StartCoroutine(SendUpdate());
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
	private IEnumerator SendUpdate() {
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
        topCard.transform.localScale = new Vector3(
        	defaultScale,
        	cardIsUps[cardIsUps.Count-1] ? defaultScale : -1 * defaultScale,
        	defaultScale
        );
    }

    // Update is called once per frame
    void Update() {
        
    }
}
