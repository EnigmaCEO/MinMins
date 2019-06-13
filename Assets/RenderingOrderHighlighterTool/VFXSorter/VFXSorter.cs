//Zenith Code 2014
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif
using UnityEngine;

[ExecuteInEditMode]
public class VFXSorter : MonoBehaviour
{
	public int sortingOrder;
	public string sortingLayer = "Default";
    private readonly List<Renderer> _allRenderers = new List<Renderer>();

#if UNITY_EDITOR
    private static string[] _sortingLayers;
	public static string[] SortingLayers
	{
		get { return _sortingLayers ?? (_sortingLayers = GetSortingLayerNames()); }
	}

	
	void Awake()
	{
		GetAllRenderers();
		_sortingLayers = GetSortingLayerNames();
	}

	void Update()
	{
		SetRenderingLayerProperties();
	}

	#region GetSortingLayers
	//Thanks to: http://answers.unity3d.com/questions/585108/how-do-you-access-sorting-layers-via-scripting.html
	public static string[] GetSortingLayerNames()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
	#endregion
#else
    void Awake()
    {
        GetAllRenderers();
        SetRenderingLayerProperties();
    }
#endif


	private void SetRenderingLayerProperties()
	{
		foreach (Renderer allRenderer in _allRenderers)
		{
			allRenderer.sortingLayerName = sortingLayer;
			allRenderer.sortingOrder = sortingOrder;
		}
	}

	private void GetAllRenderers()
	{
		var thisVFX = GetComponent<ParticleSystem>();
		var allVFX = GetComponentsInChildren<ParticleSystem>();

		if (thisVFX != null)
		{
			_allRenderers.Add(gameObject.GetComponent<Renderer>());
		}

		foreach (ParticleSystem system in allVFX)
		{
			_allRenderers.Add(system.GetComponent<Renderer>());
		}
	}
}