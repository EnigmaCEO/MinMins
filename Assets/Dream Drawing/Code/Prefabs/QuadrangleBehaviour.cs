using UnityEngine;
using System.Collections;

public class QuadrangleBehaviour : MonoBehaviour
{
	#region Fields

	public Vector2 leftTop;
	public Vector2 rightTop;
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
		DreamDrawing.DrawQuadrangle(leftTop, rightTop, rightBottom, leftBottom, intensity, width, color, pivot, angle);
	}

	#endregion
}
