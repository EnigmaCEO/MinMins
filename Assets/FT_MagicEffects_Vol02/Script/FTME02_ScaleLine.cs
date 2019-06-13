using UnityEngine;
using System.Collections;

public class FTME02_ScaleLine : MonoBehaviour {

	LineRenderer myLine;
	GameObject myParent;
	float OrigStartWidth;
	float OrigEndWidth;
	public float StartWidth;
	public float EndWidth;
	public float scaleLineSize = 1f;

	public void ScaleLineOnEnable () {
		myLine = GetComponent<LineRenderer>();
		OrigStartWidth = StartWidth;
		OrigEndWidth = EndWidth;
	}

	public void ScaleLineOnDisable () {
		StartWidth = OrigStartWidth;
		EndWidth = OrigEndWidth;		
	}

	public void ScaleLine () {
		StartWidth = OrigStartWidth * scaleLineSize;
		EndWidth = OrigEndWidth * scaleLineSize;
		myLine.SetWidth (StartWidth, EndWidth);			
	}
}
