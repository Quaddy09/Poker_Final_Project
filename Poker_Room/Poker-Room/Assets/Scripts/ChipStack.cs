using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;

public class ChipStack : MonoBehaviour {

	private float deltaX;
	private float deltaZ;
	private Vector3 initialPosition;

	public string elementId;
	public string updateUrl;

	public float CHIP_HEIGHT;

	public List<GameObject> chipReferences;
	private List<GameObject> chips;

	public ChipStack chipStack;

	private List<ChipStack> subs;

	public List<int> chipValues;
	public List<float> chipRotations;

	public bool isInfinite;

	private void OnMouseDown() {
		if(Input.GetButton("Sort") && !isInfinite) {
			int len = chipValues.Count;
			for(int i = 1; i < len; i++) {
				GameObject cref = chipReferences[i];
				int val = chipValues[i];
				float rot = chipRotations[i];
				GameObject chip = chips[i];

				int j = i-1;
				while(j >= 0 && chipValues[j] > val) {
					chipReferences[j+1] = chipReferences[i];
					chipValues[j+1] = chipValues[i];
					chipRotations[j+1] = chipRotations[i];
					chips[j+1] = chips[i];

					j = j - 1;
				}
				chipReferences[j+1] = cref;
				chips[j+1] = chip;
				chipValues[j+1] = val;
				chipRotations[j+1] = rot;
			}
		}
		if(Input.GetButton("Top") && chips.Count > 1) {
			ChipStack o = Instantiate(chipStack, transform.position, transform.rotation);
			for(int i = chips.Count - 2; i >= 0; i--) {
				o.chipReferences.Insert(0, chipReferences[i]);
				o.chipValues.Insert(0, chipValues[i]);
				o.chipRotations.Insert(0, chipRotations[i]);
				chipReferences.RemoveAt(i);
				chipValues.RemoveAt(i);
				chipRotations.RemoveAt(i);
				chips.RemoveAt(i);
			}
			o.elementId = Guid.NewGuid().ToString(); 
			o.updateUrl = updateUrl;
			o.chipStack = chipStack;
			if(isInfinite) {
				isInfinite = false;
				o.isInfinite = true;
				o.chipReferences.Add(chipReferences[0]);
				o.chipValues.Add(chipValues[0]);
				o.chipRotations.Add(chipRotations[0]);
			} else {
				o.isInfinite = false;
			}
			StartCoroutine(o.SendUpdate());
			subs.Add(o);

		} else if(Input.GetButton("Split") && chips.Count > 1) {
			ChipStack o = Instantiate(chipStack, transform.position, transform.rotation);
			for(int i = (chips.Count-1) / 2; i >= 0; i--) {
				o.chipReferences.Insert(0, chipReferences[i]);
				o.chipValues.Insert(0, chipValues[i]);
				o.chipRotations.Insert(0, chipRotations[i]);
				chipReferences.RemoveAt(i);
				chipValues.RemoveAt(i);
				chipRotations.RemoveAt(i);
				chips.RemoveAt(i);
			}
			o.elementId = Guid.NewGuid().ToString(); 
			o.updateUrl = updateUrl;
			o.chipStack = chipStack;
			if(isInfinite) {
				isInfinite = false;
				o.isInfinite = true;
				for(int i = 0; i < chipReferences.Count; i++) {
					o.chipReferences.Add(chipReferences[i]);
					o.chipValues.Add(chipValues[i]);
					o.chipRotations.Add(chipRotations[i]);
				}
			} else {
				o.isInfinite = false;
			}
			StartCoroutine(o.SendUpdate());
			subs.Add(o);
		}

		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if(new Plane(Vector3.up, 0).Raycast(ray, out enter)) {
			if(isInfinite) {
				isInfinite = false;
				ChipStack o = Instantiate(chipStack, transform.position, transform.rotation);
				for(int i = 0; i < chipReferences.Count; i++) {
					o.chipReferences.Add(chipReferences[i]);
					o.chipValues.Add(chipValues[i]);
					o.chipRotations.Add(chipRotations[i]);
				}
				o.elementId = elementId;
				elementId = Guid.NewGuid().ToString();
				o.isInfinite = true;
				o.updateUrl = updateUrl;
				o.chipStack = chipStack;
				StartCoroutine(o.SendUpdate());
				subs.Add(o);
			} 

			Vector3 hitPoint = ray.GetPoint( enter );

			deltaX = hitPoint.x - transform.position.x;
			deltaZ = hitPoint.z - transform.position.z;
			
		}
	}

	private void OnMouseDrag()  {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		float enter = 0.0f;
		if(new Plane(Vector3.up, 0).Raycast(ray, out enter)) {
			Vector3 hitPoint = ray.GetPoint( enter );
			transform.position = new Vector3(hitPoint.x - deltaX, 0, hitPoint.z - deltaZ);
			foreach(GameObject chip in chips) {
				chip.transform.position = new Vector3(transform.position.x, chip.transform.position.y, transform.position.z);
			}
			StartCoroutine(SendUpdate());
			
		}
	}
	private string parseChipValues(int[] values) {
		string s = "[";
		for( int i = 0; i < values.Length - 1; i++ ) {
			s += values[i] + ",";
		}
		s += values[values.Length-1] + "]";
		return s;
	}
	private string parseChipRotations(float[] values) {
		string s = "[";
		for( int i = 0; i < values.Length - 1; i++ ) {
			s += values[i] + ",";
		}
		s += values[values.Length-1] + "]";
		return s;
	}
	public IEnumerator SendUpdate() {

		WWWForm form = new WWWForm();
		form.AddField("elementId", elementId);
		form.AddField("posX", "" + transform.position.x);
		form.AddField("posY", "" + transform.position.z);
		form.AddField("chipValues", parseChipValues(chipValues.ToArray()));
		form.AddField("chipRotations", parseChipRotations(chipRotations.ToArray()));
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

	private void OnMouseUp() {
		if( true ) {

		} else {
			transform.position = new Vector3(initialPosition.x, initialPosition.y, initialPosition.z);
			foreach(GameObject chip in chips) {
				chip.transform.position = new Vector3(transform.position.x, chip.transform.position.y, transform.position.z);
			}
		}
	}
	public void DestroySubs() {
		foreach(GameObject chip in chips) {
			Destroy(chip);
		}
		foreach(ChipStack o in subs) {
			o.DestroySubs();
			Destroy( o.gameObject );
		}
	}

    // Start is called before the first frame update
    void Start() {
    	subs = new List<ChipStack>();
    	CHIP_HEIGHT = 0.01f;
    	chips = new List<GameObject>();

        initialPosition = transform.position;
        for(int i = 0; i < chipReferences.Count; i++) {
        	chips.Add(Instantiate(
        		chipReferences[i],
        		new Vector3(transform.position.x, i*CHIP_HEIGHT, transform.position.z),
        		Quaternion.identity
        	));
        }
    }

    // Update is called once per frame
    void Update() {
        
    }
}
