/****************************************************
 *  (c) 2014 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

//#define SERVER //I
#define MASTERCLIENT //A
#define TESTCLIENT //A
#define NETWORK //A
//#define REGEVAL //I
//#define EVALUATION //I

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using NarayanaGames.Common;
using NarayanaGames.Common.UI;
using NarayanaGames.ScoreFlashComponent;

namespace NarayanaGames.ScoreFlashComponent.Addons {
    public class AchievementsCustomRendererUnityUI : ScoreFlashRendererBase {

        // we need a reference to this so we can set the text and change the color
        public Image background;
        public Text title;
        public Text description;

        private Color origColorBackground;
        private Color origColorName;
        private Color origColorDescription;

        public AudioSource awardEarnedSound;

        public void AwardEarned(string name, string description) {
            this.title.text = name;
            this.description.text = description;
            if (!awardEarnedSound.isPlaying) {
                awardEarnedSound.Play();
            }
        }

        private RectTransform rt;
        private Canvas canvas;
        private RectTransform canvasRT;


        void Awake() {
            origColorBackground = background.color;
            origColorName = title.color;
            origColorDescription = description.color;

            if (rt == null) {
                rt = GetComponent<RectTransform>();
            }
        }

        public void Start() {
            // we need the root canvas
            Transform t = transform;
            while (canvas == null && t != null || !canvas.isRootCanvas) {
                canvas = t.GetComponent<Canvas>();
                t = t.parent;
            }
            canvasRT = canvas.GetComponent<RectTransform>();
        }

        #region Implementation of methods required by ScoreFlashRendererBase

        /// <summary>
        ///     Returns <c>false</c> because Unity UI does not make use of GUISkins.
        /// </summary>
        public override bool UsesGUISkin {
            get { return false; }
        }

        /// <summary>
        ///     Returns <c>true</c> because you need to attach Unity UI stuff correctly
        ///     in the scene hierarchy. Pull a Canvas into
        ///     ScoreFlash / Main Layout / Custom Parent for Renderer Instances.
        /// </summary>
        public override bool RequiresCustomParent {
            get { return true; }
        }

        /// <summary>
        ///     Return false because a custom parent is required.
        /// </summary>
        public override bool NeverAssignParent {
            get { return false; }
        }

        /// <summary>
        ///     Creates a new instance of this renderer that will handle rendering
        ///     for a given message. Used internally by ScoreFlash. Default
        ///     implementation simply uses 
        ///     <a href="http://docs.unity3d.com/Documentation/ScriptReference/Object.Instantiate.html">Instantiate()</a>.
        /// </summary>
        /// <param name="parent">the parent transform this should go into</param>
        /// <returns>the new instance</returns>
        public override ScoreFlashRendererBase CreateInstance(Transform parent) {
            // make sure we're on the right layer
            this.gameObject.layer = parent.gameObject.layer;
            if (background != null && background.gameObject != null) {
                this.background.gameObject.layer = parent.gameObject.layer;
            }
            if (title != null && title.gameObject != null) {
                this.title.gameObject.layer = parent.gameObject.layer;
            }
            if (description != null && description.gameObject != null) {
                this.description.gameObject.layer = parent.gameObject.layer;
            }

            ScoreFlashRendererBase result
                = (ScoreFlashRendererBase)((GameObject)Instantiate(this.gameObject)).GetComponent<AchievementsCustomRendererUnityUI>();

            result.transform.SetParent(parent, false);

            return result;
        }

        public override void Initialize(ScoreMessage msg) {
            // for ScoreFlash, the anchor always needs to be "top-left"!
            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);

            // set the anchor and pivot correctly depending on ScreenAlign
            Vector2 pivot = Vector2.zero; // min and max are the same, so is Pivot
            switch (NGAlignment.Horizontal(msg.InnerAnchor)) {
                case NGAlignment.HorizontalAlign.Left:
                    pivot.x = 0;
                    break;
                case NGAlignment.HorizontalAlign.Center:
                    pivot.x = 0.5F;
                    break;
                case NGAlignment.HorizontalAlign.Right:
                    pivot.x = 1;
                    break;
            }
            switch (NGAlignment.Vertical(msg.InnerAnchor)) {
                case NGAlignment.VerticalAlign.Bottom:
                    pivot.y = 0;
                    break;
                case NGAlignment.VerticalAlign.Middle:
                    pivot.y = 0.5F;
                    break;
                case NGAlignment.VerticalAlign.Top:
                    pivot.y = 1;
                    break;
            }
            rt.pivot = pivot;
        }

        /// <summary>
        ///     The size of the message on screen. This is using RectTransform.sizeDelta.
        /// </summary>
        /// <param name="msg">the current version of the message</param>
        public override Vector2 GetSize(ScoreMessage msg) {
            return rt.sizeDelta;
        }

        /// <summary>
        ///     Update the message.
        /// </summary>
        /// <param name="msg">the current version of the message</param>
        public override void UpdateMessage(ScoreMessage msg) {
            // push only the alpha channel of the color
            background.color = AlphaMultiplyColor(origColorBackground, msg.CurrentTextColor);
            title.color = AlphaMultiplyColor(origColorName, msg.CurrentTextColor);
            description.color = AlphaMultiplyColor(origColorDescription, msg.CurrentTextColor);

            Vector2 position = new Vector2(msg.Position.x, msg.Position.y);
            if (canvas != null && canvasRT != null) {
                Vector2 screenSize = new Vector2(canvas.pixelRect.width, canvas.pixelRect.height);
                Vector2 viewPortSize = canvasRT.sizeDelta;
                position = new Vector2(
                    position.x * viewPortSize.x / screenSize.x,
                    position.y * viewPortSize.y / screenSize.y);
            }

            // convert our coordinate system into Unity UI's coordinate system
            //Vector2 pos = NGAlignment.GetAlignBasedOffset(msg.InnerAnchor, msg.Position);
            rt.anchoredPosition = new Vector2(position.x, -position.y);

            // ... ok, I guess there's are kind of obvious, aren't they?
            transform.localScale = msg.Scale * Vector3.one;
            transform.localRotation = Quaternion.Euler(0, 0, -msg.Rotation);
        }

        #endregion Implementation of methods required by ScoreFlashRendererBase


    }
}