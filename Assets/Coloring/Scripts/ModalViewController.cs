using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;

namespace SJ.VAR
{
	public class ModalViewController : MonoBehaviour, IPointerClickHandler
	{
	
		#region IPointerClickHandler implementation

		public virtual void OnPointerClick (PointerEventData eventData)
		{
			print ("OnPointerClick");
			SJUtility.ShowUI (this, 0f, 0.2f, gameObject, false);
		}

		#endregion


	}
}