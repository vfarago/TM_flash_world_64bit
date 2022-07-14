//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using System.Collections;

[AddComponentMenu("Scripts/SceneManager/Newspaper Transition")]
public class SMNewspaperTransition : SMPostRenderTransition {
	
	/// <summary>
	/// rotation angle
	/// </summary>
	public float angle = 360 * 3;
	
	/// <summary>
	/// duration of the effect
	/// </summary>
	public float duration = 2;
	
	private Material material;
	private float progress;
	
	protected override void Prepare() {
		if (material == null) {
			material = new Material(Shader.Find("Scene Manager/Newspaper Effect"));
			material.SetTexture("_Background", holdMaterial.mainTexture);
		}
	}
		
	protected override bool Process(float elapsedTime) {
		float effectTime = elapsedTime;
		// invert direction 
		if (state == SMTransitionState.In) {
			effectTime = duration - effectTime;
		}
		
		progress = SMTransitionUtils.SmoothProgress(0, duration, effectTime);
		
		return elapsedTime < duration;		
	}
	
	protected override void OnRender() {
		GL.PushMatrix();
		// pixel matrix instead of orthogonal to maintain aspect ratio during rotation
		GL.LoadPixelMatrix();
		GL.LoadIdentity();
		
		DrawBackground();
		DrawImage();
		
		GL.PopMatrix(); 
	}
	
	private void DrawBackground() {
		material.SetFloat("_BlendMode", 0);
		for (var i = 0; i < material.passCount; ++i) {
			material.SetPass(i);
			GL.Begin(GL.QUADS);
			GL.TexCoord3(0, 0, 0);
			GL.Vertex3(0, 0, 0);
			GL.TexCoord3(0, 1, 0);
			GL.Vertex3(0, Screen.height, 0);
			GL.TexCoord3(1, 1, 0);
			GL.Vertex3(Screen.width, Screen.height, 0);
			GL.TexCoord3(1, 0, 0);
			GL.Vertex3(Screen.width, 0, 0);
			GL.End();
		}
	}	
	
	private void DrawImage() {
		material.SetFloat("_BlendMode", 1);
		
		float dx = Screen.width / 2f;
		float dy = Screen.height / 2f;
		
		Quaternion rotation = Quaternion.AngleAxis(progress * angle, Vector3.forward);
		GL.MultMatrix(Matrix4x4.TRS(new Vector3(dx, dy, 0), rotation, Vector3.one * (1 - progress)));
		for (var i = 0; i < material.passCount; ++i) {
			material.SetPass(i);
			GL.Begin(GL.QUADS);
			GL.TexCoord3(0, 0, 0);
			GL.Vertex3(-dx, -dy, 0);
			GL.TexCoord3(0, 1, 0);
			GL.Vertex3(-dx, dy, 0);
			GL.TexCoord3(1, 1, 0);
			GL.Vertex3(dx, dy, 0);
			GL.TexCoord3(1, 0, 0);
			GL.Vertex3(dx, -dy, 0);
			GL.End();
		}				
	}
}

