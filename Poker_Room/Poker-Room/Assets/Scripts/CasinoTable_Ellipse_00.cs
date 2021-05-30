using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class CasinoTable_Ellipse_00 : MonoBehaviour {	  

	public string loginUrl;
	public string getStateUrl;
	public string username;
	public string password;
	public string cookie;
	public bool isUpdating;

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
					var table = JsonConvert.DeserializeObject(result);
					Debug.Log(table.name);

				} else {
					Debug.Log( "Couldn't get data" );
				}
			}
		}
		this.isUpdating = false;
	}

    // Start is called before the first frame update
    void Start() {
    	this.isUpdating = false;
        StartCoroutine(GetCookie(loginUrl, username, password));
    }

    // Update is called once per frame
    void Update() {
    	if(this.cookie != "" && !this.isUpdating) {
        	StartCoroutine(GetState(getStateUrl));
    	}
    }
}
