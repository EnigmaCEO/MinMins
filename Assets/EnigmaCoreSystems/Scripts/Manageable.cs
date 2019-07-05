using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Enigma.CoreSystems
{
	public class Manageable<T> : MonoBehaviour where T: MonoBehaviour
	{
		private static T m_Instance;
		private static object _lock = new object();
		public static T Instance
		{
			get
			{
				lock(_lock)
				{
					if( m_Instance == null )
					{
						m_Instance = (T)FindObjectOfType (typeof(T));
						if( FindObjectsOfType (typeof(T)).Length > 1 )
						{
							Debug.LogError ("There are more than one instances of {0}, there should never be more than one singleton!");
							return m_Instance;
						}
					}

					return m_Instance;
				}
			}
		}

		//protected T()
		//{
		//
		//}

		// Use this for initialization
		protected virtual void Awake()
		{

		}

		protected virtual void Start ()
		{

		}

		// Update is called once per frame
		protected virtual void Update ()
		{

		}

		public static void Initialize()
		{
			GameObject singleton = new GameObject();
			m_Instance = singleton.AddComponent<T>();
			//singleton.name = "Enigma.CoreSystems." + typeof(T).ToString ();
			//singleton.transform.parent = GameObject.Find ("Enigma.CoreSystems").transform;
			ConfigureDefaultValues(m_Instance);
			DontDestroyOnLoad (singleton);
		}

		public static void ConfigureDefaultValues(object objToConfigure)
		{
			string selfName = typeof(T).Name;
			BindingFlags bFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			//string filePath = Application.dataPath + "/EnigmaCoreSystems/defaultConfigsAll.txt";
			TextAsset file = Resources.Load("defaultConfigsAll") as TextAsset;
			if( file.text != null )
			{

				JSONNode values = JSONNode.Parse( file.text );
				JSONClass compNode = new JSONClass();
				int index = -1;
				for( int i = 0; i < values["configs"].Count; i++ )
				{
					string component = values["configs"][i]["component"].ToString ().Trim ('"');
					if( component == "Enigma.CoreSystems." + selfName )
					{
						index = i;
						compNode = values["configs"][index]["values"] as JSONClass;
						break;
					}
				}

				if( index == -1 )
				{
					return;
				}

				foreach( KeyValuePair<string, JSONNode> pair in compNode.Dict )
				{
					string key = pair.Key;
					JSONNode value = pair.Value;
					try
					{
						Type objectType = objToConfigure.GetType ();
						FieldInfo field = objectType.GetField ( key, bFlags );
						if( field == null )
						{
							throw new TargetException();
						}

						Type fieldType = field.FieldType;
						object configValue = value;
						Debug.Log(fieldType.ToString() + " " + value.ToString());
						if( fieldType.ToString () == "System.Boolean" )
						{
							configValue = value.AsBool;
						}
						else if( fieldType.ToString () == "System.Int32" )
						{
							configValue = value.AsInt;
						}
						else if( fieldType.ToString () == "System.Single" )
						{
							configValue = value.AsFloat;
						}
						else if( fieldType.ToString () == "System.Double" )
						{
							configValue = value.AsDouble;
						}
						else if( fieldType.ToString () == "System.String" )
						{
							configValue = value.ToString ().Trim ('"');
						}
						else if( fieldType.ToString() == "System.String[]" )
						{
							JSONArray jsonArray = value.AsArray;
							List<string> elements = new List<string>();
							for( int i = 0; i < jsonArray.Count; i++ )
							{
								elements.Add( jsonArray[i] );
							}

							string[] arrayValue = elements.ToArray();
							configValue = arrayValue;

							elements = null;
						}

						//Debug.Log (fieldType.ToString ());

						field.SetValue ( objToConfigure, Convert.ChangeType ( configValue, fieldType ));
					}
					catch( TargetException )
					{
						Debug.LogError ("Configuration failed for class; " + objToConfigure.GetType().ToString () + " cannot set field " +  key );
					}
				}
			}
		}

		public static string md5(string strToEncrypt)
		{
			System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
			byte[] bytes = ue.GetBytes(strToEncrypt);

			// encrypt bytes
			#if UNITY_WP8
			byte[] hashBytes = UnityEngine.Windows.Crypto.ComputeMD5Hash(bytes);
			#else
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] hashBytes = md5.ComputeHash(bytes);
			#endif

			// Convert the encrypted bytes back to a string (base 16)
			string hashString = "";

			for (int i = 0; i < hashBytes.Length; i++)
			{
				hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
			}

			return hashString.PadLeft(32, '0');
		}
	}
}
