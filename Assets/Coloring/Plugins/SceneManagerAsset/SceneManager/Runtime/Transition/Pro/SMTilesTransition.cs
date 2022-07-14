//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Transition implementation that shows 3d tiles.
/// </summary>
[AddComponentMenu("Scripts/SceneManager/Tiles Transition")]
public class SMTilesTransition : SMPostRenderTransition {
	
	/// <summary>
	/// The background texture.
	/// </summary>
	public Texture backgroundTexture;
	
	/// <summary>
	/// The size of the tiles in pixels (if geater than 1) or relative to the screen (otherwise).
	/// </summary>
	public Vector2 preferredTileSize = new Vector2(100, 100);
	
	/// <summary>
	/// The duration of the effect.
	/// </summary>
	public float duration = 2f;
	
	/// <summary>
	/// The time to flip a single tile.
	/// </summary>
	public float tilesFlipTime = .5f;
	
	private Material material;
	
	private float distance = 10f;
	private Vector2 actualTileSize;
	private int columns;
	private int rows;
	
	private float tileStartOffset;	
	
	private Vector3 topLeft;
	private Vector3 bottomRight;
	private float width;
	private float height;

	private float effectTime;

	protected override void Prepare() {
		if (material == null) {
			material = new Material(Shader.Find("Scene Manager/Tiles Effect"));
			material.SetTexture("_Backface", holdMaterial.mainTexture);
			if (backgroundTexture) {
				material.SetTexture("_Background", backgroundTexture);
			}
		}
		
		topLeft = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(0, Screen.height, distance));
		bottomRight = gameObject.GetComponent<Camera>().ScreenToWorldPoint(new Vector3(Screen.width, 0, distance));
		
		width = bottomRight.x - topLeft.x;
		height = topLeft.y - bottomRight.y;
		
		columns = Mathf.FloorToInt(Screen.width / SMTransitionUtils.ToAbsoluteSize(preferredTileSize.x, Screen.width));
		rows = Mathf.FloorToInt(Screen.height / SMTransitionUtils.ToAbsoluteSize(preferredTileSize.y, Screen.height));	
		                        
		// recalculate size to avoid clipped tiles
		actualTileSize = new Vector2(width / columns, height / rows);

		tileStartOffset = (duration - tilesFlipTime) / (columns + rows);	
	}

	protected override bool Process(float elapsedTime) {
		effectTime = elapsedTime;
		return elapsedTime < duration;
	}

	protected override void OnRender() {
		GL.PushMatrix();
		DrawBackground();
		GL.Clear(true, false, Color.black);
		
		for(int x = 0; x < columns; x++) {
			for(int y = 0; y < rows; y++) {
				float tileProgress = SMTransitionUtils.SmoothProgress((x+y) * tileStartOffset, tilesFlipTime, effectTime);
				DrawTile(x, y, tileProgress * 180);
			}
		}
		
		GL.PopMatrix(); 
	}
	
	private void DrawBackground() {
		material.SetFloat("_BlendMode", 0);
		GL.LoadOrtho();
		GL.LoadIdentity();
		
		for (var i = 0; i < material.passCount; ++i) {
			material.SetPass(i);
			GL.Begin(GL.QUADS);
			GL.TexCoord3(0, 0, 0);
			GL.Vertex3(0, 0, 0);
			GL.TexCoord3(0, 1, 0);
			GL.Vertex3(0, 1, 0);
			GL.TexCoord3(1, 1, 0);
			GL.Vertex3(1, 1, 0);
			GL.TexCoord3(1, 0, 0);
			GL.Vertex3(1, 0, 0);
			GL.End();
		}				
	}
	
	private void DrawTile(int xIndex, int yIndex, float progress) {
		material.SetFloat("_BlendMode", 1);
		float halfWidth = actualTileSize.x / 2f;
		float halfHeight = actualTileSize.y / 2f;
		
		float xOffset = actualTileSize.x * xIndex;
		float yOffset = actualTileSize.y * yIndex;
		float umin = xOffset / width;
		float umax = (xOffset + actualTileSize.x) / width;
		float vmin = (height - yOffset - actualTileSize.y) / height;
		float vmax = (height - yOffset) / height;
		
		GL.LoadProjectionMatrix(gameObject.GetComponent<Camera>().projectionMatrix);
		GL.LoadIdentity();
		
		Vector3 translation = new Vector3(topLeft.x + xOffset + halfWidth, topLeft.y - yOffset - halfHeight, -distance);
		Quaternion rotation = Quaternion.AngleAxis(progress + (state == SMTransitionState.In ? 180 : 0), Vector3.up);
		GL.MultMatrix(Matrix4x4.TRS(translation, rotation, Vector3.one));
						
		for (var i = 0; i < material.passCount; ++i) {
			material.SetPass(i);
			GL.Begin(GL.QUADS);
			GL.TexCoord3(i == 1 ? umin : umax, vmin, 0);
			GL.Vertex3(-halfWidth, -halfHeight, 0);
			GL.TexCoord3(i == 1 ? umin : umax, vmax, 0);
			GL.Vertex3(-halfWidth, halfHeight, 0);
			GL.TexCoord3(i == 1 ? umax : umin, vmax, 0);
			GL.Vertex3(halfWidth, halfHeight, 0);
			GL.TexCoord3(i == 1 ? umax : umin, vmin, 0);
			GL.Vertex3(halfWidth, -halfHeight, 0);
			GL.End();
		}				
	}
	
}

