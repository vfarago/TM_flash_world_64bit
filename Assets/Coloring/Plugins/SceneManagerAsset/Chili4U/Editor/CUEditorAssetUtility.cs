using System.IO;
using UnityEngine;
using System;
using UnityEditor;

public class CUEditorAssetUtility
{
	
	/// <summary>
	/// Gets the project root.
	/// </summary>
	public static DirectoryInfo ProjectRoot {
		get {
			return new DirectoryInfo (Application.dataPath).Parent;
		}
	}
	
	/// <summary>
	/// Finds a texture with a unique name within the project.
	/// </summary>
	/// <returns>
	/// The texture or null if no such texture could be found.
	/// </returns>
	public static Texture2D FindTextureByName (string withinDirectory, string name)
	{
		var root = ProjectRoot;
		FileInfo foundFile = null;
		
		var files = root.GetFiles (name, SearchOption.AllDirectories);
		foreach (var file in files) {
			if (file.FullName.Replace("\\","/").Contains ("/"+withinDirectory+"/")) {
				foundFile = file;
				break;
			}
		}
		
		if (foundFile == null) {
			return null;
		}
		
		var rootName = root.FullName;
		var fullName = foundFile.FullName;
		
		rootName = rootName.Replace ("\\", "/");
		fullName = fullName.Replace ("\\", "/");
		var path = fullName.Substring (rootName.Length + 1);
		return AssetDatabase.LoadMainAssetAtPath (path) as Texture2D;
	} 
	
}