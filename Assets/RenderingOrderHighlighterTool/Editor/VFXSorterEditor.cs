//Zenith Code 2014
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VFXSorter))]
public class VFXSorterEditor : Editor
{
	private VFXSorter _vfx;
	private int _index;

	private void OnEnable()
	{
		_vfx = (VFXSorter)target;
		//Get current layer
		for (int i = 0; i < VFXSorter.SortingLayers.Length; i++)
		{
			if (VFXSorter.SortingLayers[i] == _vfx.sortingLayer)
			{
				_index = i;
			}
		}
	}


	public override void OnInspectorGUI()
	{
		//Sorting layer
		if (VFXSorter.SortingLayers == null || VFXSorter.SortingLayers.Length == 0)
		{
			Debug.Log("VFX sorting layers is null");
			return;
		}
		_index = EditorGUILayout.Popup("Sorting layers", _index, VFXSorter.SortingLayers);
		_vfx.sortingLayer = VFXSorter.SortingLayers[_index];

		//Sorting order
		_vfx.sortingOrder = EditorGUILayout.IntField("Sort Order", _vfx.sortingOrder);
	}
}
