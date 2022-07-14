using UnityEngine;
using System.Collections;

public class BackKeyHandler : MonoBehaviour {


	void Update () {

		#if UNITY_ANDROID
		if (Input.GetKeyUp (KeyCode.Escape)) {
			Application.Quit();
		}
		#endif

	}
}
