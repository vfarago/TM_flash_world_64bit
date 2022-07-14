using UnityEngine;
using System.Collections;

public class CamController : MonoBehaviour {
	public GameObject norCamera;
	public GameObject arCamera;
	public GameObject ARBackCamBtn;

	// Use this for initialization
	void Start () {
		arCamDeActive ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void norCamActive(){
		norCamera.SetActive (true);
	}

	public void norCamDeActive(){
		norCamera.SetActive (false);
	}

	public void arCamActive(){
		arCamera.SetActive (true);
		norCamDeActive ();
		ARBackCamBtn.SetActive (true);
	}
	public void arCamDeActive(){
		arCamera.SetActive (false);
		norCamActive ();
		ARBackCamBtn.SetActive (false);
	}
}
