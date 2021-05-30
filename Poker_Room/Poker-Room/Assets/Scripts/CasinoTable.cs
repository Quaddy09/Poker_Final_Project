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
	public bool isUpdating;

	public List<GameObject> gameElements;
	public string previousState;

	public GameObject Chip;

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

	public void UpdateScene( JSONNode table ) {
		// Use table object element values to create game pieces in the scene
		
		// clear scene
		foreach(GameObject o in gameElements) {
			Destroy( o );
		}

		// add new elements to scene
		foreach(JSONNode element in table["elements"]) {
			GameObject o = Instantiate(Chip, new Vector3(
				element["pos"]["x"], 
				element["pos"]["y"],
				1
			), Quaternion.identity);

			gameElements.Add(o);

		}
	}

    // Start is called before the first frame update
    void Start() {
    	var loginUrl = url + "/login";
    	gameElements = new List<GameObject>();
    	previousState = "";

    	this.isUpdating = false;
        StartCoroutine(GetCookie(loginUrl, username, password));
    }

    // Update is called once per frame
    void Update() {
    	var getStateUrl = url + "/table/" + tableId + "/getstate";
    	if(this.cookie != "" && !this.isUpdating) {
        	StartCoroutine(GetState(getStateUrl));
    	}
    }
}
