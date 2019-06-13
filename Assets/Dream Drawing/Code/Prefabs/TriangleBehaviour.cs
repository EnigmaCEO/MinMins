using UnityEngine;
using System.Collections;

public class TriangleBehaviour : MonoBehaviour 
{
	#region Fields

	public Vector2 leftTop;
	public Vector2 rightBottom;
	public Vector2 leftBottom;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;

	#endregion

	#region Methods

	void OnGUI()
	{
		DreamDrawing.DrawTriangle(leftTop, rightBottom, leftBottom, intensity, width, color, pivot, angle);
	}

	#endregion
}
