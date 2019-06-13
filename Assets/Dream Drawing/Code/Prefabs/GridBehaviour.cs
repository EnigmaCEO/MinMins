using UnityEngine;
using System.Collections;

public class GridBehaviour : MonoBehaviour
{
	#region Fields
	
	public Vector2 topLeft;
	public Vector2 bottomRight;
	public float squareWidth;
	public float squareHeight;
	public float intensity;
	public float width;
	public Color color;
	public Vector2 pivot;
	public float angle;
	
	#endregion
	
	#region Methods
	
	void OnGUI()
	{
		DreamDrawing.DrawGrid(topLeft, bottomRight, squareWidth, squareHeight, intensity, width, color, pivot, angle);
	}
	
	#endregion
}
