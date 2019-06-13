using UnityEngine;
using System.Collections;

public class LinePathBehaviour : MonoBehaviour 
{
	#region Fields
	
	public Vector2[] positions;
	public float intensity;
	public float width;
	public Color color;
	public float size;
	public float space;
	
	#endregion
	
	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawLinePath(positions, intensity, width, color, size, space);
	}
	
	#endregion
}
