//
// Copyright (c) 2013 Ancient Light Studios
// All Rights Reserved
// 
// http://www.ancientlightstudios.com
//

using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Transition implementation that shows "tetris" blocks.
/// </summary>
[AddComponentMenu("Scripts/SceneManager/Tetris Transition")]
public class SMTetrisTransition : SMPostRenderTransition {
	
	/// <summary>
	/// The size of a block in pixels (if geater than 1) or relative to the screen (otherwise).
	/// </summary>
	public Vector2 preferredTileSize = new Vector2(100, 100);
	
	/// <summary>
	/// time for a single block to fall down
	/// </summary>
	public float tileFallTime = 1f;
	
	/// <summary>
	/// The minimum delay between a tile and its tile below
	/// </summary>
	public float minDelayBetweenTiles = .01f;

	/// <summary>
	/// The maximum delay between a tile and its tile below
	/// </summary>
	public float maxDelayBetweenTiles = .5f;
	
	private Material material;
	
	private Vector2 actualTileSize;		// relative screen size: 0 <= size <= 1
	private int columns;
	private int rows;

	private float duration;
	private List<Tile> tiles;
	private float effectTime;
	
	protected override void Prepare() {
		if (material == null) {
			material = new Material(Shader.Find("Scene Manager/Tetris Effect"));
			material.SetTexture("_Background", holdMaterial.mainTexture);
		}
		
		duration = 0;
		columns = Mathf.FloorToInt(Screen.width / SMTransitionUtils.ToAbsoluteSize(preferredTileSize.x, Screen.width));
		rows = Mathf.FloorToInt(Screen.height / SMTransitionUtils.ToAbsoluteSize(preferredTileSize.y, Screen.height));		
		actualTileSize = new Vector2(1f / columns, 1f / rows);
			
		tiles = new List<Tile>(columns * rows);
		for(int x = 0; x < columns; x++) {
			float startTime = 0;	
			for(int y = 0; y < rows; y++) {
				startTime += UnityEngine.Random.Range(minDelayBetweenTiles, maxDelayBetweenTiles);
				tiles.Add(new Tile(x, y, new Vector2(x * actualTileSize.x, y * actualTileSize.y + (state == SMTransitionState.In ? 1 : 0)), startTime));
				duration = Mathf.Max(duration, startTime);
			}
		}
		
		duration += tileFallTime;
	}
		
	protected override bool Process(float elapsedTime) {
		effectTime = elapsedTime;		
		return elapsedTime < duration;		
	}
	
	protected override void OnRender() {
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.LoadIdentity();

		DrawBackground();
		DrawTiles();
		
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
			GL.Vertex3(0, 1, 0);
			GL.TexCoord3(1, 1, 0);
			GL.Vertex3(1, 1, 0);
			GL.TexCoord3(1, 0, 0);
			GL.Vertex3(1, 0, 0);			
			GL.End();
		}				
	}

	private void DrawTiles() {
		material.SetFloat("_BlendMode", 1);
		for (var i = 0; i < material.passCount; ++i) {
			material.SetPass(i);
			GL.Begin(GL.QUADS);
			
			foreach(Tile tile in tiles) {
				float tileProgress = SMTransitionUtils.SmoothProgress(tile.startTime, tileFallTime, effectTime);
				Vector2 position = tile.position - Vector2.up * tileProgress; // move the tile down
				
				if (position.y < 1 && position.y >= (-actualTileSize.y)) {
					GL.TexCoord3(tile.column * actualTileSize.x, tile.row * actualTileSize.y, 0);
					GL.Vertex3(position.x, position.y, 0);
					GL.TexCoord3(tile.column * actualTileSize.x, (tile.row + 1) * actualTileSize.y, 0);
					GL.Vertex3(position.x, position.y + actualTileSize.y, 0);
					GL.TexCoord3((tile.column + 1) * actualTileSize.x, (tile.row + 1) * actualTileSize.y, 0);
					GL.Vertex3(position.x + actualTileSize.x, position.y + actualTileSize.y, 0);
					GL.TexCoord3((tile.column + 1) * actualTileSize.x, tile.row * actualTileSize.y, 0);
					GL.Vertex3(position.x + actualTileSize.x, position.y, 0);			
				}
			}
			GL.End();					
		}				
	}
	
	private struct Tile {
	
		public int column;
		public int row;
		public Vector2 position;
		public float startTime; 
		
		public Tile(int column, int row, Vector2 position, float startTime) {
			this.column = column;
			this.row = row;
			this.position = position;
			this.startTime = startTime;
		}
	}

}

