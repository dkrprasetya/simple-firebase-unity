using UnityEngine;
using System.Collections;

public class AutoRotate : MonoBehaviour {

	Vector3 rotation;

	// Use this for initialization
	void Start () {
		rotation = Random.onUnitSphere * 50f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate (rotation * Time.deltaTime);
	}
}
