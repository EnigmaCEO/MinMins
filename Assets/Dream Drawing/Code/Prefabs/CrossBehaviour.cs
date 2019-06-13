using UnityEngine;
using System.Collections;

public class CrossBehaviour : MonoBehaviour 
{
	#region Fields

	public Vector2 center;
	public float size;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;
	
	#endregion
	
	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawCross(center, size, intensity, width, color, pivot, angle);
	}
	
	#endregion
}
