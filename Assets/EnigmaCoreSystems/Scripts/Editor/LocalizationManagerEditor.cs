using I2.Loc;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LocalizationManagerEditor : MonoBehaviour
{
    /*
    public static void ChangeSpreadSheetByProductName()
    {
        GameObject languagesObject = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/EnigmaCoreSystems/Resources/I2Languages.prefab", typeof(GameObject));
        LanguageSource source = languagesObject.GetComponent<LanguageSource>();
        source.Google_SpreadsheetKey = getSpreadSheetKeyByProductName();
        source.Google_SpreadsheetName = "I2Loc " + PlayerSettings.productName + " Localization";
        source.Event_OnSourceUpdateFromGoogle += onGoogleUpdate;
        source.Import_Google(true, false, true);
    }

    private static string getSpreadSheetKeyByProductName()
    {
        string key = "";
        string productName = PlayerSettings.productName;

        if (productName == "Greek Quiz")
            key = "13P8l0vsUISVS74f1hgIW5lu2d2LS2Sm0bn9U4YNdmzs";
        else if (productName == "Egypt Quiz")
            key = "13B7rGOya_j_1zGNHOPpMiGNunx2GFI-6xhSFR3H88I4";
        else if (productName == "Norse Quiz")
            key = "1IkrOcBVp_Htl3WFdI_bYrmmDhLc9P4horeFjJQxi-jU";
        else if (productName == "Celtic Quiz")
            key = "1rc2PjR2-EUOayZzh5mhrXaj3J5dkTHZRJS4W2BPfG7Y";
        else if (productName == "Irish Quiz")
            key = "1nd5anzqNFwTX_CVgneM7BeZsUPIuR9mi2Wdm90lWGWE";
        else if (productName == "Jewish Quiz")
            key = "1n85m_GfHqhXDNqHs0SN7x1vlQYunRwitDIc4xti3Ias";

        if (key == "")
            Debug.LogError("There is no spreadsheet key for product: " + productName + ". Please add at DailyQuizMenuItems::GetSpreadSheetKeyByProductName().");

        return key;
    }

    private static void onGoogleUpdate(LanguageSource source, bool successful, string error)
    {
        source.Event_OnSourceUpdateFromGoogle -= onGoogleUpdate;

        if (successful)
        {
            Debug.Log("New SpreadsheetName: " + source.Google_SpreadsheetName);
            Debug.Log("New SpredSheetKey: " + source.Google_SpreadsheetKey);

            EditorUtility.SetDirty(source);
        }
        else
            Debug.LogError("onGoogleUpdate not completed: " + error);
    }
    */
}
