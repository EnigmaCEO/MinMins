using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(FTME02_ParticleController))]
public class FTME02_ParticleControllerEditor : Editor {



	private SerializedProperty scaleProperty;
	private SerializedProperty scaleLifeProperty;
	private SerializedProperty scaleOnlyLineProperty;

	void OnEnable(){
		scaleProperty = serializedObject.FindProperty("scale");
		scaleLifeProperty = serializedObject.FindProperty("scaleLife");
		scaleOnlyLineProperty = serializedObject.FindProperty("scaleOnlyLine");
	}
	
	public override void OnInspectorGUI()
	{
		//DrawDefaultInspector();
		FTME02_ParticleController myScript = (FTME02_ParticleController)target;
		serializedObject.Update();

		var scaleValue = EditorGUILayout.Slider( "Scaling Particle", scaleProperty.floatValue, 0.1f, 10.0f );

		if (scaleValue != scaleProperty.floatValue)
		{
			scaleProperty.floatValue = scaleValue;
		}

		var scaleLifeValue = EditorGUILayout.Slider( "Scaling Lifetime", scaleLifeProperty.floatValue, 0.1f, 10.0f );
		
		if (scaleLifeValue != scaleLifeProperty.floatValue)
		{
			scaleLifeProperty.floatValue = scaleLifeValue;
		}
		if (myScript.hasLineRenderer) {
			myScript.onlyLineRendererScaling = EditorGUILayout.Toggle ("Only LineRenderer Scaling", myScript.onlyLineRendererScaling);
		}
		if (myScript.onlyLineRendererScaling) {
			var scaleOnlyLineValue = EditorGUILayout.Slider ("Scaling Only LineRenderer", scaleOnlyLineProperty.floatValue, 0.1f, 10.0f);		
			if (scaleOnlyLineValue != scaleOnlyLineProperty.floatValue) {
				scaleOnlyLineProperty.floatValue = scaleOnlyLineValue;
			}
		}

		EditorGUILayout.LabelField ("Color Editor");
		EditorGUILayout.LabelField ("------------------------------------------------------------------------------------------------------------------------------");
		EditorGUILayout.PropertyField(serializedObject.FindProperty("particleSystems"),true);

		EditorGUILayout.PropertyField(serializedObject.FindProperty("particleColor"),true);

		EditorGUILayout.BeginHorizontal();
		
		if(GUILayout.Button("Get Particle Color"))
		{
			myScript.GetColor();
		}
		if(GUILayout.Button("Clear"))
		{
			myScript.ClearColor();
		}
		EditorGUILayout.EndHorizontal();




		
		serializedObject.ApplyModifiedProperties();
	}
}
