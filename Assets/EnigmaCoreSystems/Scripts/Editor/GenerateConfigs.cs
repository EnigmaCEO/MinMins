using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.IO;
using SimpleJSON;

namespace Enigma.CoreSystems
{
	public class GenerateConfigs : Editor 
	{
		[MenuItem("Enigma Games/Configurations/Generate Config File")]
		public static void GenerateConfigurations()
		{
			string path = Application.dataPath + "/EnigmaCoreSystems/";
			DirectoryInfo dir = new DirectoryInfo(path);
			JSONClass totalJson = new JSONClass();
			totalJson["configs"] = new JSONArray();

			foreach( DirectoryInfo d in dir.GetDirectories() )
			{
				path = Application.dataPath + "/EnigmaCoreSystems/" + d.Name + "/defaultConfigs.txt";

				if( File.Exists ( path ) )
				{
					string contents = File.ReadAllText(path);

					JSONNode json = JSON.Parse (contents);
					totalJson["configs"].Add ( json["configs"][0].AsObject );
				}

			}
				
            //totalJson.SaveToFile( Application.dataPath + "/Resources/defaultConfigsAll.txt");
			File.WriteAllText( Application.dataPath + "/Resources/defaultConfigsAll.txt", totalJson.ToString());
			AssetDatabase.ImportAsset( "Resources/defaultConfigsAll.txt");
			AssetDatabase.Refresh();
		}
	}
}