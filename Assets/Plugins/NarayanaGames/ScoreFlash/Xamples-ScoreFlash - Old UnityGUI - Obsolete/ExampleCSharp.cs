/****************************************************
 *  (c) 2012 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleCSharp : MonoBehaviour {

    public float posY = 10F;
    public float width = 180F;
    public float height = 20F;
    public float padding = 10F;

    public string text = "CSharp Text";

    /// <summary>
    ///     Enable the example with custom skins.
    /// </summary>
    public bool exampleUsingSkins = false;

    public GUISkin skin;
    public GUISkin skinHighDensity;

    /// <summary>
    ///     Enable the example with random colors.
    /// </summary>
    public bool exampleRandomColors = false;

    /// <summary>
    ///     Enable the example with random colors and fonts.
    /// </summary>
    public bool exampleRandomColorsAndFonts = false;

    // for a funky test with different fonts for each message
    public List<GUISkin> skinSet = new List<GUISkin>();


    public void OnGUI() {
        Rect rect = new Rect(Screen.width - (width + padding), posY, width, height);
        
        GUI.Label(rect, "C# Example");

        rect.y += rect.height + padding;
        text = GUI.TextField(rect, text);


        rect.y += rect.height + padding;
        if (GUI.Button(rect, "Push(text) from C#")) {

            ScoreFlash.Push(text); // <--- this is all there is to it!!!

        }

        if (exampleUsingSkins) {
            rect = ExampleUsingSkins(rect);
        }

        if (exampleRandomColors) {
            rect = ExampleRandomColors(rect);
        }
        if (exampleRandomColorsAndFonts) {
            rect = ExampleRandomColorsAndFonts(rect);
        }
    }


    public Rect ExampleUsingSkins(Rect rect) {
        /*
         * For advanced programmers, if you have your own skin management,
         * or, if you want to use different styles for different messages.
         * With this approach you could even control yourself each the
         * color for each message by assigning it to your style before
         * sending the message. For that to work, you need to set
         * Colors / Color Selection Mode to "UseColorFromSkin"; 
         * see: ExampleRandomColors()
         */

        // a) no mobile / retina support
        rect.y += rect.height + padding;
        if (skin != null) {
            if (GUI.Button(rect, "Push(text, style)")) {
                GUIStyle style = new GUIStyle(skin.FindStyle("ScoreFlash"));
                ScoreFlash.Push(text, style);
            }
        } else {
            GUI.Label(rect, "Please assign skin");
        }

        // b) with mobile / retina support
        rect.y += rect.height + padding;
        if (skin != null && skinHighDensity != null) {
            if (GUI.Button(rect, "Push(text, style, style)")) {
                GUIStyle style = new GUIStyle(skin.FindStyle("ScoreFlash"));
                GUIStyle styleHighDensity = new GUIStyle(skinHighDensity.FindStyle("ScoreFlash"));
                ScoreFlash.Push(text, style, styleHighDensity);
            }
        } else {
            GUI.Label(rect, "Please assign skinHighDensity");
        }

        // legacy code - just for internal testing, do not use this anymore!!!
        //if (enableLegacyButton) {
        //    rect.y += rect.height + padding;
        //    if (GUI.Button(rect, "Legacy: ScoreFlash.Instance.Show")) {
        //        ((ScoreFlash)ScoreFlash.Instance).Show(text);
        //    }
        //}
        return rect;
    }
    //public bool enableLegacyButton = false;

    // you actually have to call this from OnGUI to see an effect ;-)
    public Rect ExampleRandomColors(Rect rect) {
        rect.y += rect.height + padding;
        if (GUI.Button(rect, "Using specific color")) {
            Color color = new Color(
                Random.Range(0.3F, 1.0F), // r
                Random.Range(0.3F, 1.0F), // g
                Random.Range(0.3F, 1.0F), // b
                1.0F // alpha is controlled via alpha multiplier (under Colors on the prefab)
                );

            ScoreFlash.Instance.PushLocal("Pushed a button - random color message", color);
        }
        return rect;
    }

    // you actually have to call this from OnGUI to see an effect ;-)
    public Rect ExampleRandomColorsAndFonts(Rect rect) {
        rect.y += rect.height + padding;
        if (skinSet != null && skinSet.Count > 0) {
            if (((ScoreFlash)ScoreFlash.Instance).colorSelectionMode != ScoreFlash.ColorControl.UseColorFromSkin) {
                GUI.Label(rect, "Need to set color selection");
                rect.y += rect.height;
                GUI.Label(rect, "mode to UseColorFromSkin!");
                rect.y += rect.height + padding;
            }


            if (GUI.Button(rect, "Random Skin and Color")) {
                GUISkin randomSkin = skinSet[Random.Range(0, skinSet.Count)];

                // get the custom style "ScoreFlash" of a random skin
                GUIStyle style = new GUIStyle(randomSkin.GetStyle(((ScoreFlash)ScoreFlash.Instance).guiStyleName));

                // IMPORTANT: For the color to be used, the ScoreFlash instance needs have
                // "Colors / Color Selection Mode" set to UseColorFromSkin

                // now assign a random color
                style.normal.textColor = new Color(
                    Random.Range(0.3F, 1.0F), // r
                    Random.Range(0.3F, 1.0F), // g
                    Random.Range(0.3F, 1.0F), // b
                    1.0F // alpha is controlled via alpha multiplier (under Colors on the prefab)
                    );

                ScoreFlash.Push("Random color and font flash!", style);
            }
        } else {
            GUI.Label(rect, "Please assign skinSet!");
        }
        return rect;
    }

}
