// (C) 2013 Ancient Light Studios. All rights reserved.
using System;
using UnityEngine;

public class SMEditorResources
{
	private static Texture _SMLevelMarker;
	private static Texture _SMScreenMarker;
	
	public static Texture SMLevelMarker {
		get {
			if (_SMLevelMarker == null) {
				_SMLevelMarker = CUEditorAssetUtility.FindTextureByName("SceneManager", "SMLevelMarker.png");
			}
			return _SMLevelMarker;
		}
	}
	
	public static Texture SMScreenMarker {
		get {
			if (_SMScreenMarker == null) {
				_SMScreenMarker = CUEditorAssetUtility.FindTextureByName("SceneManager", "SMScreenMarker.png");
			}
			return _SMScreenMarker;
		}
	}
	
}

