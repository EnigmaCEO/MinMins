using UnityEngine;
using System.Collections;

public class Arrow0Behaviour : MonoBehaviour
{
	#region Fields
	
	public Vector2 start;
	public Vector2 end;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;
	public float size;
	
	#endregion
	
	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawArrow(start, end, intensity, width, color, pivot, angle, size);
	}
	
	#endregion
}
