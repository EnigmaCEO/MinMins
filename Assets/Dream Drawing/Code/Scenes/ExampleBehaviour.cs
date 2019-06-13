using UnityEngine;
using System.Collections;

public class ExampleBehaviour : MonoBehaviour 
{
	#region Fields

	Vector2 scroll;

	#endregion

	#region Methods

	void FixedUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
	}

	void OnGUI()
	{
		scroll = GUI.BeginScrollView(new Rect(0, 0, Screen.width, Screen.height), scroll, new Rect(0, 0, 3000, Screen.height - 50));

		DreamDrawing.DrawLine(new Vector2(25, 75), new Vector2(125, 75), 1f, 2f, Color.black, new Vector2(), 0f);
		DreamDrawing.DrawLine(new Vector2(25, 175), new Vector2(125, 175), 0.5f, 2f, Color.blue, new Vector2(), 0f);
		DreamDrawing.DrawLine(new Vector2(25, 275), new Vector2(125, 275), 3f, 4f, Color.cyan, new Vector2(75, 275), 45);
		DreamDrawing.DrawLine(new Vector2(25, 375), new Vector2(125, 375), 0.5f, 4f, Color.gray, new Vector2(75, 375), -45);

		DreamDrawing.DrawCurve(new Vector2(225, 25), new Vector2(325, 25), new Vector2(325, 125), 2f, 2f, Color.green, new Vector2(), 0f);		
		DreamDrawing.DrawCurve(new Vector2(225, 125), new Vector2(325, 125), new Vector2(325, 225), 0.5f, 2f, Color.grey, new Vector2(), 0f);		
		DreamDrawing.DrawCurve(new Vector2(225, 225), new Vector2(325, 225), new Vector2(325, 325), 3f, 4f, Color.magenta, new Vector2(275, 275), 45);		
		DreamDrawing.DrawCurve(new Vector2(225, 325), new Vector2(325, 325), new Vector2(325, 425), 0.5f, 4f, Color.red, new Vector2(275, 375), 90);

		DreamDrawing.DrawCurve(new Vector2(425, 25), new Vector2(525, 25), new Vector2(525, 100), new Vector2(425, 100), 2f, 2f, Color.black, new Vector2(), 0f);		
		DreamDrawing.DrawCurve(new Vector2(425, 125), new Vector2(525, 125), new Vector2(525, 200), new Vector2(425, 200), 0.5f, 2f, Color.yellow, new Vector2(), 0f);		
		DreamDrawing.DrawCurve(new Vector2(425, 225), new Vector2(525, 225), new Vector2(525, 300), new Vector2(425, 300), 3f, 4f, Color.black, new Vector2(475, 262.5f), -45);		
		DreamDrawing.DrawCurve(new Vector2(425, 325), new Vector2(525, 325), new Vector2(525, 400), new Vector2(425, 400), 0.5f, 4f, Color.blue, new Vector2(475, 362.5f), 90);

		DreamDrawing.DrawArrow(new Vector2(625, 25), new Vector2(725, 25), new Vector2(725, 125), 2f, 2f, Color.cyan, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawArrow(new Vector2(625, 125), new Vector2(725, 125), new Vector2(725, 225), 0.5f, 2f, Color.gray, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawArrow(new Vector2(625, 225), new Vector2(725, 225), new Vector2(725, 325), 3f, 4f, Color.green, new Vector2(675, 275), 45, 4f);		
		DreamDrawing.DrawArrow(new Vector2(625, 325), new Vector2(725, 325), new Vector2(725, 425), 0.5f, 4f, Color.grey, new Vector2(675, 375), 90, 4f);
		
		DreamDrawing.DrawArrow(new Vector2(825, 25), new Vector2(925, 25), new Vector2(925, 100), new Vector2(825, 100), 2f, 2f, Color.magenta, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawArrow(new Vector2(825, 125), new Vector2(925, 125), new Vector2(925, 200), new Vector2(825, 200), 0.5f, 2f, Color.red, new Vector2(), 0f, 4f);		
		DreamDrawing.DrawArrow(new Vector2(825, 225), new Vector2(925, 225), new Vector2(925, 300), new Vector2(825, 300), 3f, 4f, Color.black, new Vector2(875, 262.5f), -45, 4f);		
		DreamDrawing.DrawArrow(new Vector2(825, 325), new Vector2(925, 325), new Vector2(925, 400), new Vector2(825, 400), 0.5f, 4f, Color.yellow, new Vector2(875, 362.5f), 90, 4f);

		DreamDrawing.DrawDoubleArrow(new Vector2(1025, 25), new Vector2(1125, 25), new Vector2(1125, 125), 2f, 2f, Color.black, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1025, 125), new Vector2(1125, 125), new Vector2(1125, 225), 0.5f, 2f, Color.blue, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1025, 225), new Vector2(1125, 225), new Vector2(1125, 325), 3f, 4f, Color.cyan, new Vector2(1075, 275), 45, 4f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1025, 325), new Vector2(1125, 325), new Vector2(1125, 425), 0.5f, 4f, Color.gray, new Vector2(1075, 375), 90, 4f);
		
		DreamDrawing.DrawDoubleArrow(new Vector2(1225, 25), new Vector2(1325, 25), new Vector2(1325, 100), new Vector2(1225, 100), 2f, 2f, Color.green, new Vector2(), 0f, 2f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1225, 125), new Vector2(1325, 125), new Vector2(1325, 200), new Vector2(1225, 200), 0.5f, 2f, Color.grey, new Vector2(), 0f, 4f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1225, 225), new Vector2(1325, 225), new Vector2(1325, 300), new Vector2(1225, 300), 3f, 4f, Color.magenta, new Vector2(1275, 262.5f), -45, 4f);		
		DreamDrawing.DrawDoubleArrow(new Vector2(1225, 325), new Vector2(1325, 325), new Vector2(1325, 400), new Vector2(1225, 400), 0.5f, 4f, Color.red, new Vector2(1275, 362.5f), 90, 4f);

		DreamDrawing.DrawQuadrangle(new Vector2(1425, 25), new Vector2(1525, 25), new Vector2(1525, 100), new Vector2(1425, 100), 1f, 2f, Color.black, new Vector2(), 0f);		
		DreamDrawing.DrawQuadrangle(new Vector2(1425, 125), new Vector2(1525, 125), new Vector2(1475, 200), new Vector2(1425, 200), 0.5f, 2f, Color.yellow, new Vector2(), 0f);		
		DreamDrawing.DrawQuadrangle(new Vector2(1425, 225), new Vector2(1500, 225), new Vector2(1500, 300), new Vector2(1425, 300), 3f, 4f, Color.black, new Vector2(1475, 262.5f), -45);		
		DreamDrawing.DrawQuadrangle(new Vector2(1425, 325), new Vector2(1525, 375), new Vector2(1525, 400), new Vector2(1425, 350), 0.5f, 4f, Color.blue, new Vector2(1475, 362.5f), 90);

		DreamDrawing.DrawTriangle(new Vector2(1625, 25), new Vector2(1725, 25), new Vector2(1725, 100), 2f, 2f, Color.cyan, new Vector2(), 0f);		
		DreamDrawing.DrawTriangle(new Vector2(1625, 125), new Vector2(1725, 125), new Vector2(1675, 200), 0.5f, 2f, Color.gray, new Vector2(), 0f);		
		DreamDrawing.DrawTriangle(new Vector2(1625, 225), new Vector2(1700, 225), new Vector2(1700, 300), 3f, 4f, Color.green, new Vector2(1675, 262.5f), -45);		
		DreamDrawing.DrawTriangle(new Vector2(1625, 325), new Vector2(1725, 375), new Vector2(1725, 400), 0.5f, 4f, Color.grey, new Vector2(1675, 362.5f), 90);

		DreamDrawing.DrawEllipse(new Vector2(1875, 25), new Vector2(1925, 62.5f), new Vector2(1875, 100), new Vector2(1825, 62.5f), 1f, 2f, Color.magenta, new Vector2(), 0f);		
		DreamDrawing.DrawEllipse(new Vector2(1825, 125), new Vector2(1925, 125), new Vector2(1875, 200), new Vector2(1925, 200), 0.5f, 2f, Color.red, new Vector2(), 0f);		
		DreamDrawing.DrawEllipse(new Vector2(1825, 225), new Vector2(1900, 225), new Vector2(1900, 300), new Vector2(1925, 300), 3f, 4f, Color.black, new Vector2(1875, 262.5f), -45);		
		DreamDrawing.DrawEllipse(new Vector2(1825, 325), new Vector2(1925, 375), new Vector2(1925, 400), new Vector2(1925, 400), 0.5f, 4f, Color.yellow, new Vector2(1875, 362.5f), 90);

		DreamDrawing.DrawArrow(new Vector2(2025, 75), new Vector2(2125, 75), 2f, 2f, Color.black, new Vector2(), 0f, 2f);
		DreamDrawing.DrawArrow(new Vector2(2025, 175), new Vector2(2125, 175), 0.5f, 2f, Color.blue, new Vector2(), 0f, 4f);
		DreamDrawing.DrawArrow(new Vector2(2025, 275), new Vector2(2125, 275), 3f, 4f, Color.cyan, new Vector2(2075, 275), 45, 4f);
		DreamDrawing.DrawArrow(new Vector2(2025, 375), new Vector2(2125, 375), 0.5f, 4f, Color.gray, new Vector2(2075, 375), -45, 4f);

		DreamDrawing.DrawDoubleArrow(new Vector2(2225, 75), new Vector2(2325, 75), 2f, 2f, Color.green, new Vector2(), 0f, 2f);
		DreamDrawing.DrawDoubleArrow(new Vector2(2225, 175), new Vector2(2325, 175), 0.5f, 2f, Color.grey, new Vector2(), 0f, 4f);
		DreamDrawing.DrawDoubleArrow(new Vector2(2225, 275), new Vector2(2325, 275), 3f, 4f, Color.magenta, new Vector2(2275, 275), 45, 4f);
		DreamDrawing.DrawDoubleArrow(new Vector2(2225, 375), new Vector2(2325, 375), 0.5f, 4f, Color.red, new Vector2(2275, 375), -45, 4f);

		DreamDrawing.DrawCross(new Vector2(2425, 75), 25f, 2f, 2f, Color.yellow, new Vector2(), 0f);
		DreamDrawing.DrawCross(new Vector2(2425, 175), 25f, 0.5f, 2f, Color.black, new Vector2(), 0f);
		DreamDrawing.DrawCross(new Vector2(2425, 300), 50f, 2f, 25f, Color.blue, new Vector2(2475, 275), 45);
		DreamDrawing.DrawCross(new Vector2(2425, 375), 50f, 0.5f, 4f, Color.cyan, new Vector2(2475, 375), -45);

		DreamDrawing.DrawGrid(new Vector2(2625, 25), new Vector2(2725, 125), 8f, 8f, 2f, 2f, Color.gray, new Vector2(), 0f);
		DreamDrawing.DrawGrid(new Vector2(2625, 150), new Vector2(2725, 250), 18f, 8f, 2f, 2f, Color.green, new Vector2(), 0f);
		DreamDrawing.DrawGrid(new Vector2(2625, 300), new Vector2(2725, 400), 18f, 18f, 0.5f, 2f, Color.grey, new Vector2(2675, 350), 45f);

		DreamDrawing.DrawLinePath(new Vector2[]{ new Vector2(2825, 25), new Vector2(2875, 50), new Vector2(2900, 100), new Vector2(2925, 125)}, 4f, 2f, Color.magenta, 4f, 5f);
		DreamDrawing.DrawCurvePath(new Vector2[]{ new Vector2(2825, 150), new Vector2(2875, 175), new Vector2(2900, 225), new Vector2(2925, 250)}, 0.5f, 2f, Color.red, 1f, 10f);

		GUI.EndScrollView();
	}

	#endregion
}
