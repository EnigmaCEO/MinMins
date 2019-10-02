/****************************************************
 *  (c) 2013 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NarayanaGames.Common;
using NarayanaGames.ScoreFlashComponent;

public class AchievementsCustomRenderer : ScoreFlashRendererBase {

    #region Public inspector "properties" ;-)
    // title of the achievement (set in inspector, or call AchievementEarned from code)
    public string title = "Top High Score";
    
    // a description for the achievement
    public string description = "You are the number one in the highscore. This is awesome! Gratz!";

    // GUIskin used for rendering the achievement
    public GUISkin mySkin;

    // the background texture for the achievement
    public Texture2D textureAchievementNormalEarned;
  
    // an audio source playing a sound when the player has received an achievement
    public AudioSource achievementEarnedSound;

    // offset from top for the name (relative to the background image)
    public float offsetTopName = 20F;

    // offset from top for the description (relative to the background image)
    public float offsetTopDescription = 50F;

    // margins on the sides to prevent text from flowing into graphical elements
    public float sideMargins = 90F;


    #endregion Public inspector "properties" ;-)

    // used to test whether a user clicked on the achievement (to make it disappear quickly)
    private bool mouseClicked = false;
    private ScoreMessage msg;

    // immediately trigger the achievement (this is useful in PlayMaker scenarios)
    public void Awake() {
        AchievementUnlocked(title, description);
    }

    // check if the user has clicked the mouse button
    public void Update() {
        if (Input.GetMouseButtonDown(0)) {
            mouseClicked = true;
        }
    }

    // set name and description, and play the sound (if it isn't playing already)
    public void AchievementUnlocked(string name, string description) {
        this.title = name;
        this.description = description;
        if (!achievementEarnedSound.isPlaying) {
            achievementEarnedSound.Play();
        }
    }

    #region Implementation of methods required by ScoreFlashRendererBase

    /// <summary>
    ///     Returns <c>false</c> because this is using its own GUISkin!
    /// </summary>
    public override bool UsesGUISkin {
        get { return false; }
    }

    /// <summary>
    ///     Returns <c>false</c> because GUISkin does not need a custom parent.
    /// </summary>
    public override bool RequiresCustomParent {
        get { return false; }
    }

    /// <summary>
    ///     Lets the developer / designer set this up.
    /// </summary>
    public bool neverAssignParent = false;
    /// <summary>
    ///     Lets the developer / designer set this up.
    /// </summary>
    public override bool NeverAssignParent {
        get { return neverAssignParent; }
    }

    /// <summary>
    ///     The size of the achievement on the screen, based on texture size of
    ///     <c>textureAchievementNormalEarned</c>.
    /// </summary>
    /// <param name="msg">ignored</param>
    public override Vector2 GetSize(ScoreMessage msg) {
        return new Vector2(textureAchievementNormalEarned.width, textureAchievementNormalEarned.height);
    }

    /// <summary>
    ///     Update the message. The implementation needs
    ///     to make sure that you update position, scale, color and outline
    ///     color as well as the text.
    /// </summary>
    /// <param name="msg">the current version of the message</param>
    public override void UpdateMessage(ScoreMessage msg) {
        this.msg = msg;
        // we don't need to do anything here, because the renderer
        // pulls the information from ScoreMessage msg while rendering
        // NGUI / EZ GUI solutions do the "interesting stuff" in this method
    }

    #endregion Implementation of methods required by ScoreFlashRendererBase

    // render the achievement
    void OnGUI() {
        if (msg == null) {
            return;
        }

        GUI.skin = mySkin;

        Matrix4x4 originalGUIMatrix = GUI.matrix;

        Vector2 alignBasedOffset = ScoreFlash.GetAlignBasedOffset(msg);

        Rect localPos = msg.Position;
        localPos.x += alignBasedOffset.x;
        localPos.y += alignBasedOffset.y;


        Vector2 pivotPoint = new Vector2(localPos.x + localPos.width * 0.5F, localPos.y + localPos.height * 0.5F);
        GUIUtility.ScaleAroundPivot(new Vector2(msg.Scale, msg.Scale), pivotPoint);
        GUIUtility.RotateAroundPivot(msg.Rotation, pivotPoint);

        // msg.CurrentTextColor; // only using alpha of this 

        // START: Here comes the custom rendering

        Texture2D bgTexture = textureAchievementNormalEarned;
        Rect posRec = localPos;

        // check if user has clicked mouse inside achievement to make it disappear quickly
        if (mouseClicked) {
            float x = Input.mousePosition.x;
            float y = Screen.height - Input.mousePosition.y;
            if (x > posRec.x && y > posRec.y
                && x < posRec.x + posRec.width && y < posRec.y + posRec.height) {
                msg.LocalTimeScale = 8; // 8 times as fast as usual ;-)
            }
            mouseClicked = false; // reset!
        }


        // the usual UnityGUI sauce ;-)
        GUIStyle styleBG = new GUIStyle(GUI.skin.label);
        GUIStyle styleName = new GUIStyle(GUI.skin.FindStyle("AchievementTitle"));
        GUIStyle styleDescription = new GUIStyle(GUI.skin.FindStyle("AchievementDescription"));

        Color colorSafe = GUI.backgroundColor;
        
        Color color = Color.white;
        color.a = msg.CurrentTextColor.a;
        styleBG.normal.background = bgTexture; // using background so alpha has an effect
        GUI.backgroundColor = color;

        color = styleName.normal.textColor;
        color.a = msg.CurrentTextColor.a;
        styleName.normal.textColor = color;

        color = styleDescription.normal.textColor;
        color.a = msg.CurrentTextColor.a;
        styleDescription.normal.textColor = color;

        GUI.Label(posRec, "", styleBG);

        GUIContent achievementName = new GUIContent(title);
        float achievementNameHeight = styleName.CalcHeight(achievementName, posRec.width);
        GUI.Label(new Rect(posRec.x, posRec.y + offsetTopName, posRec.width, achievementNameHeight), achievementName, styleName);

        GUIContent achievementDescription = new GUIContent(description);
        float achievementDescriptionHeight = styleName.CalcHeight(achievementDescription, posRec.width - 2 * sideMargins);
        GUI.Label(new Rect(posRec.x + sideMargins, posRec.y + offsetTopDescription, posRec.width - 2 * sideMargins, achievementDescriptionHeight), achievementDescription, styleDescription);

        GUI.backgroundColor = colorSafe;


        // END: Here ends the custom rendering

        GUI.matrix = originalGUIMatrix;
    }




}
