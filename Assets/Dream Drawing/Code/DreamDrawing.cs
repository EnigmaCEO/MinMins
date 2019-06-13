using System;
using UnityEngine;

public class DreamDrawing
{    
	#region Static Fields

    public static Texture2D texture = null;   

	#endregion

	#region Methods

	static Vector2 RotatePositionAroundPivot(Vector2 position, Vector2 pivot, float angle)
	{
		Vector2 dir = position - pivot; 
		dir = Quaternion.Euler(0,0, angle) * dir; 
		return dir + pivot; 
	}

	static Vector2 Bezier(Vector2 position0, Vector2 position1, float t)
	{
		float rt = 1-t;
		return rt * position0 + t * position1;
	}

	static Vector2 Bezier(Vector2 position0, Vector2 position1, Vector2 position2, float t)
	{
		float rt = 1-t;
		float rtt = rt * t;
		return rt * rt * position0 + 2 * rtt * position1 + t * t * position2;
	}
	
	static Vector2 Bezier(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float t)
	{
		float rt = 1-t;
		float rtt = rt * t;
		return rt * rt * rt * position0 + 3 * rt * rtt * position1 + 3 * rtt * t * position2 + t * t * t * position3;
	}

	public static void DrawPoint(Vector2 position, float diameter, Color color)
	{
		Color savedColor = GUI.color;
		
		#region Initialization
		
		if (!texture)
		{
			texture = new Texture2D(1, 1, TextureFormat.ARGB32, true);
			texture.SetPixel(0, 0, Color.white);
			texture.Apply();
		}
		
		#endregion
		
		GUI.color = color;		
		GUI.DrawTexture(new Rect(position.x - diameter / 2, position.y - diameter / 2, diameter, diameter), texture);
		GUI.color = savedColor;
	}

	public static void DrawLine(Vector2 position0, Vector2 position1, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;

		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * ((position0 - position1).magnitude / width));
		DreamDrawing.DrawPoint(position0, width, color);

		for (int i = 1; i < points; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, i / (float)points), width, color);

		DreamDrawing.DrawPoint(position1, width, color);
	}

	public static void DrawCurve(Vector2 position0, Vector2 position1, Vector2 position2, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
		}

		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude) / width));
		DreamDrawing.DrawPoint(position0, width, color);

		for (int i = 1; i < points; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, i/(float)points), width, color);

		DreamDrawing.DrawPoint(position2, width, color);
    }

	public static void DrawCurve(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
			position3 = RotatePositionAroundPivot(position3, pivot, angle);
		}

		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude + (position2 - position3).magnitude) / width));
		DreamDrawing.DrawPoint(position0, width, color);

		for (int i = 1; i < points; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, position3, i/(float)points), width, color);

		DreamDrawing.DrawPoint(position3, width, color);
	}

	public static void DrawArrow(Vector2 position0, Vector2 position1, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * ((position0 - position1).magnitude / width));		
		DreamDrawing.DrawPoint(position0, width, color);
		
		for (int i = 1; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, i/(float)points), width, color);
		
		Vector2 arrowPosition = Bezier(position0, position1, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position1, width, color);
		Vector2 arrowDirection = (position1 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position1, intensity, width, color, new Vector2(), 0);
	}

	public static void DrawArrow(Vector2 position0, Vector2 position1, Vector2 position2, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude) / width));		
		DreamDrawing.DrawPoint(position0, width, color);
		
		for (int i = 1; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, i/(float)points), width, color);

		Vector2 arrowPosition = Bezier(position0, position1, position2, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position2, width, color);
		Vector2 arrowDirection = (position2 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position2, intensity, width, color, new Vector2(), 0);
	}

	public static void DrawArrow(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
			position3 = RotatePositionAroundPivot(position3, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude + (position2 - position3).magnitude) / width));
		DreamDrawing.DrawPoint(position0, width, color);
		
		for (int i = 1; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, position3, i/(float)points), width, color);
		
		Vector2 arrowPosition = Bezier(position0, position1, position2, position3, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position3, width, color);
		Vector2 arrowDirection = (position3 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position3, intensity, width, color, new Vector2(), 0);
	}

	public static void DrawDoubleArrow(Vector2 position0, Vector2 position1, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * ((position0 - position1).magnitude / width));
		Vector2 arrowPosition = Bezier(position0, position1, 1 /(float)points);
		DreamDrawing.DrawPoint(position0, width, color);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		Vector2 arrowDirection = (position0 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position0, intensity, width, color, new Vector2(), 0);
		
		for (int i = 2; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, i/(float)points), width, color);
		
		arrowPosition = Bezier(position0, position1, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position1, width, color);
		arrowDirection = (position1 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position1, intensity, width, color, new Vector2(), 0);
	}

	public static void DrawDoubleArrow(Vector2 position0, Vector2 position1, Vector2 position2, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
		}

		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude) / width));
		Vector2 arrowPosition = Bezier(position0, position1, position2, 1 /(float)points);
		DreamDrawing.DrawPoint(position0, width, color);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		Vector2 arrowDirection = (position0 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position0, intensity, width, color, new Vector2(), 0);

		for (int i = 2; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, i/(float)points), width, color);
		
		arrowPosition = Bezier(position0, position1, position2, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position2, width, color);
		arrowDirection = (position2 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position2, intensity, width, color, new Vector2(), 0);
	}
	
	public static void DrawDoubleArrow(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float intensity, float width, Color color, Vector2 pivot, float angle, float size)
	{
		if (width == 0 || !Event.current.type.Equals(EventType.Repaint)) return;
		
		if(angle != 0)
		{
			position0 = RotatePositionAroundPivot(position0, pivot, angle);
			position1 = RotatePositionAroundPivot(position1, pivot, angle);
			position2 = RotatePositionAroundPivot(position2, pivot, angle);
			position3 = RotatePositionAroundPivot(position3, pivot, angle);
		}
		
		int points = Mathf.CeilToInt(intensity * (((position0 - position1).magnitude + (position1 - position2).magnitude + (position2 - position3).magnitude) / width));
		Vector2 arrowPosition = Bezier(position0, position1, position2, position3, 1 /(float)points);
		DreamDrawing.DrawPoint(position0, width, color);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		Vector2 arrowDirection = (position0 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position0, intensity, width, color, new Vector2(), 0);
				
		for (int i = 2; i < points - 1; i++) DreamDrawing.DrawPoint(Bezier(position0, position1, position2, position3, i/(float)points), width, color);
		
		arrowPosition = Bezier(position0, position1, position2, position3, (points - 1)/(float)points);
		DreamDrawing.DrawPoint(arrowPosition, width, color);
		DreamDrawing.DrawPoint(position3, width, color);
		arrowDirection = (position3 - arrowPosition) * size;
		arrowPosition -= arrowDirection;
		DrawTriangle(RotatePositionAroundPivot(arrowDirection, new Vector2(), 90) + arrowPosition, RotatePositionAroundPivot(arrowDirection, new Vector2(), -90) + arrowPosition, position3, intensity, width, color, new Vector2(), 0);
	}


	public static void DrawTriangle(Vector2 position0, Vector2 position1, Vector2 position2, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		DrawLine(position0, position1, intensity, width, color, pivot, angle);
		DrawLine(position1, position2, intensity, width, color, pivot, angle);
		DrawLine(position2, position0, intensity, width, color, pivot, angle);
	}

	public static void DrawQuadrangle(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		DrawLine(position0, position1, intensity, width, color, pivot, angle);
		DrawLine(position1, position2, intensity, width, color, pivot, angle);
		DrawLine(position2, position3, intensity, width, color, pivot, angle);
		DrawLine(position3, position0, intensity, width, color, pivot, angle);
	}

	public static void DrawEllipse(Vector2 position0, Vector2 position1, Vector2 position2, Vector2 position3, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		DrawCurve(position0, new Vector2(position1.x, position0.y), position1, intensity, width, color, pivot, angle);
		DrawCurve(position1, new Vector2(position1.x, position2.y), position2, intensity, width, color, pivot, angle);
		DrawCurve(position2, new Vector2(position3.x, position2.y), position3, intensity, width, color, pivot, angle);
		DrawCurve(position3, new Vector2(position3.x, position0.y), position0, intensity, width, color, pivot, angle);
	}

	public static void DrawCross(Vector2 position0, float size, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		DrawLine(new Vector2(position0.x - 1 * size, position0.y), new Vector2(position0.x + 1 * size, position0.y), intensity, width, color, pivot, angle);
		DrawLine(new Vector2(position0.x, position0.y - 1 * size), new Vector2(position0.x, position0.y + 1 * size), intensity, width, color, pivot, angle);
	}

	public static void DrawLinePath(Vector2[] positions, float intensity, float width, Color color, float size, float space)
	{
		DrawArrow(positions[0], positions[1], intensity, width, color, new Vector2(), 0f, size);

		for (int i = 1; i < positions.Length - 1; i++)
		{
			Vector2 Direction = (positions[i + 1] - positions[i]);
			Direction.Normalize();
			Direction *= space;
			DrawArrow(positions[i] + Direction, positions[i + 1], intensity, width, color, new Vector2(), 0f, size);
		}
	}

	public static void DrawCurvePath(Vector2[] positions, float intensity, float width, Color color, float size, float space)
	{
		DrawArrow(positions[0], new Vector2(positions[0].x, positions[1].y), positions[1], intensity, width, color, new Vector2(), 0f, size);

		for (int i = 1; i < positions.Length - 1; i++)
		{
			Vector2 Direction = (positions[i + 1] - positions[i]);
			Direction.Normalize();
			Direction *= space;
			DrawArrow(positions[i] + Direction, new Vector2(positions[i].x + Direction.x, positions[i + 1].y), positions[i + 1], intensity, width, color, new Vector2(), 0f, size);
		}
	}

	public static void DrawGrid(Vector2 position0, Vector2 position1, float square0, float square1, float intensity, float width, Color color, Vector2 pivot, float angle)
	{
		float step0 = square0 + width;
		float step1 = square1 + width;

		for (int i = 0; i <= (int)(Math.Abs(position0.x - position1.x) / step0); i++) DrawLine(new Vector2(position0.x + i * step0, position0.y), new Vector2(position0.x + i * step0, position1.y), intensity, width, color, pivot, angle); 

		for (int i = 0; i <= (int)(Math.Abs(position0.y - position1.y) / step1); i++) DrawLine(new Vector2(position0.x, position0.y  + i * step1), new Vector2(position1.x, position0.y  + i * step1), intensity, width, color, pivot, angle); 
	}

	#endregion
}
