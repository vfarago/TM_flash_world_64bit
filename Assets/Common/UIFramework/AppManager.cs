/*============================================================================== 
 * Copyright (c) 2012-2014 Qualcomm Connected Experiences, Inc. All Rights Reserved. 
 * ==============================================================================*/
using UnityEngine;
using System.Collections;
using Vuforia;

/// <summary>
/// This class manages different views in the scene like AboutPage, SplashPage and ARCameraView.
/// All of its Init, Update and Draw calls take place via SceneManager's Monobehaviour calls to ensure proper sync across all updates
/// </summary>
public class AppManager : MonoBehaviour
{
	
	#region PUBLIC_MEMBER_VARIABLES
	public ISampleAppUIEventHandler m_UIEventHandler;
	#endregion PUBLIC_MEMBER_VARIABLES
	
	#region PROTECTED_MEMBER_VARIABLES
	public static ViewType mActiveViewType;
	public enum ViewType { SPLASHVIEW, ABOUTVIEW, UIVIEW, ARCAMERAVIEW };
	
	#endregion PROTECTED_MEMBER_VARIABLES
	

	//This gets called from SceneManager's Start() 
	public virtual void InitManager() {
		InputController.SingleTapped += OnSingleTapped;		
		m_UIEventHandler.Bind();
	}
	
	public virtual void DeInitManager()
	{
		InputController.SingleTapped -= OnSingleTapped;		
		m_UIEventHandler.UnBind();
	}
	
	public virtual void UpdateManager()
	{
		//Does nothing but anyone extending AppManager can run their update calls here
	}
	
	public virtual void Draw()
	{
	}

	
	#region PRIVATE_METHODS
	
	private void OnSingleTapped() {

		m_UIEventHandler.TriggerAutoFocus();
	}

	#endregion PRIVATE_METHODS
	
}

