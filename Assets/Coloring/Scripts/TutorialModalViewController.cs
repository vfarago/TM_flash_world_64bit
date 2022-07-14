using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace SJ.VAR
{
	public class TutorialModalViewController : ModalViewController
	{


		#region IPointerClickHandler implementation

		public override void OnPointerClick (PointerEventData eventData)
		{
			SJUtility.ShowUI (this, 0f, 0.2f, gameObject, false);
		}

		#endregion

	}
}