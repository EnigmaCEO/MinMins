using UnityEngine;
using System.Collections;

public class CurvePathBehaviour : MonoBehaviour 
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
		DreamDrawing.DrawCurvePath(positions, intensity, width, color, size, space);
	}
	
	#endregion
}
