using UnityEngine;
using System.Collections;

public class Curve1Behaviour : MonoBehaviour 
{
	#region Fields

	public Vector2 start;
	public Vector2 tangent0;
	public Vector2 tangent1;
	public Vector2 end;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;

	#endregion

	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawCurve(start, tangent0, tangent1, end, intensity, width, color, pivot, angle);
	}

	#endregion
}
