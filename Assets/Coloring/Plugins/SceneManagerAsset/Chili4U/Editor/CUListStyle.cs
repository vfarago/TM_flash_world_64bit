//
// Copyright (c) 2013 Ancient Light Studios 
// All Rights Reserved 
//  
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using UnityEditor;

public class CUListStyle {

	public GUIStyle evenBackground;
	public GUIStyle oddBackground;
	public GUIStyle item;
	public GUIStyle dropIntoHighlight;
	public GUIStyle dropBeforeHighlight;
	public GUIStyle dropAfterHighlight;

	private static CUListStyle defaultStyle;
	
	public static CUListStyle DefaultStyle {
		get {
			if (defaultStyle == null) {
				defaultStyle = new CUListStyle();
			}
			return defaultStyle;
		}
		set {
			defaultStyle = value;
		}
	}
	
	public CUListStyle() {
		GUISkin skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
		evenBackground = new GUIStyle(skin.FindStyle("OL EntryBackEven"));
		oddBackground = new GUIStyle(skin.FindStyle("OL EntryBackOdd"));
		// default style doesn't handle focus. therefore we have to swap the textures
		evenBackground.onFocused.background = evenBackground.onNormal.background;
		oddBackground.onFocused.background = oddBackground.onNormal.background;
		Texture2D texture;
		if (EditorGUIUtility.isProSkin) {
			 texture = LoadTexture("CUNotFocusedSelectionDark.png");
		} else {
			texture = LoadTexture("CUNotFocusedSelectionLight.png");
		}
		evenBackground.onNormal.background = texture;
		oddBackground.onNormal.background = texture;
		
		item = new GUIStyle(skin.FindStyle("PlayerSettingsPlatform"));
		item.alignment = TextAnchor.MiddleLeft;
		item.fixedHeight = 0;
		item.padding = new RectOffset(5, 0, 0, 0);
		item.margin = new RectOffset(); 
		
		dropIntoHighlight = new GUIStyle();
		dropIntoHighlight.normal.background = LoadTexture("CUDragIntoHighlight.png");
		dropIntoHighlight.border = new RectOffset(3, 3, 3, 3);
		dropIntoHighlight.stretchWidth = true;
		dropIntoHighlight.stretchHeight = true;
		dropIntoHighlight.imagePosition = ImagePosition.ImageOnly;
		
		dropBeforeHighlight = new GUIStyle();
		dropBeforeHighlight.normal.background = LoadTexture("CUDragBeforeHighlight.png");
		dropBeforeHighlight.border = new RectOffset(0, 0, 3, 0);
		dropBeforeHighlight.stretchWidth = true;
		dropBeforeHighlight.stretchHeight = true;	
		dropBeforeHighlight.overflow = new RectOffset(0, 0, 1, 0);
		dropBeforeHighlight.imagePosition = ImagePosition.ImageOnly;

		dropAfterHighlight = new GUIStyle();
		dropAfterHighlight.normal.background = LoadTexture("CUDragAfterHighlight.png");
		dropAfterHighlight.border = new RectOffset(0, 0, 0, 3);
		dropAfterHighlight.stretchWidth = true;
		dropAfterHighlight.stretchHeight = true;		
		dropAfterHighlight.overflow = new RectOffset(0, 0, 0, 1);
		dropAfterHighlight.imagePosition = ImagePosition.ImageOnly;
	}
	
	public static Texture2D LoadTexture(string name) { 
		return CUEditorAssetUtility.FindTextureByName("Chili4U", name);
    }

}

