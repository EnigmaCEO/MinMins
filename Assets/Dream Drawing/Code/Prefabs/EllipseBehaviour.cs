using UnityEngine;
using System.Collections;

public class EllipseBehaviour : MonoBehaviour
{
	#region Fields

	public Vector2 middleTop;
	public Vector2 middleRight;
	public Vector2 middleBottom;
	public Vector2 middleLeft;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;

	#endregion

	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawEllipse(middleTop, middleRight, middleBottom, middleLeft, intensity, width, color, pivot, angle);
	}

	#endregion
}
