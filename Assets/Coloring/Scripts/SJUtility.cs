using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public static class SJUtility  {

	public delegate void ShowUIDelegate();

	//To use this method, UI should have the canvasGroup component
	static public void ShowUI( MonoBehaviour monoBehaviour, float delayTime, float durationTime, GameObject UI, bool show, ShowUIDelegate showUIComplete = null ) {
		monoBehaviour.StartCoroutine(UITransition(delayTime, durationTime, show, UI, showUIComplete));
	}


	static private IEnumerator UITransition(float delayTime, float durationTime, bool show, GameObject UI, ShowUIDelegate showUIComplete ) {

		yield return new WaitForSeconds(delayTime);

		CanvasGroup cg = UI.GetComponent<CanvasGroup> ();
		float elapsedTime = 0;
		float start, end;
		UI.SetActive (true);
		if (show) {
			start = 0f;
			end = 1f;
		} else {
			start = 1f;
			end = 0f;
		}

		while (elapsedTime < durationTime) {
			cg.alpha = Mathf.Lerp(start, end, elapsedTime / durationTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		//cg.alpha = end;
		cg.alpha = 1f; //whether the UI gets activated or not, cg.alpha should always be 1f.
		UI.SetActive (show);

		if (showUIComplete != null)
			showUIComplete ();
	}


}
