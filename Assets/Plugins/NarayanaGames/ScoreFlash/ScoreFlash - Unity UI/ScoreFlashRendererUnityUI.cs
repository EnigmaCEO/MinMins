/****************************************************
 *  (c) 2014 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using NarayanaGames.Common.UI;
using NarayanaGames.ScoreFlashComponent;
using System;

namespace NarayanaGames.ScoreFlashComponent.Addons {
    /// <summary>
    ///     ScoreFlashRenderer adaptor for the new Unity UI.
    /// </summary>
    public class ScoreFlashRendererUnityUI : ScoreFlashRendererBase {

        /// <summary>
        ///     Reference to the object rendering the label that contains the
        ///     text that was pushed.
        /// </summary>
        public Text text;

        /// <summary>
        ///     When using layered images you can define a separate scale for 
        ///     each image for some fancy wobbling effects.
        /// </summary>
        public List<RectTransformScale> scaleOffsets = new List<RectTransformScale>();

        /// <summary>
        ///     If you're using a more complex prefab that also has some sort
        ///     of background, you can add the relevant Images (or other texts,
        ///     or any kind of Graphic) here so that ScoreFlash automatically
        ///     sets the alpha level according to the settings in ScoreFlash.
        /// </summary>
        public List<Graphic> coloredGraphics = new List<Graphic>();
        private List<Color> coloredGraphicsOrigColor = new List<Color>();

        /// <summary>
        ///     Shall we fix the pivot according to the layout settings in
        ///     ScoreFlash or ScoreFlashLayout? You can disable this on a 
        ///     prefab that you want to ignore the settings in ScoreFlash
        ///     or ScoreFlashLayout. This is useful, for example, for real
        ///     3D text (which you'll usually always want to have centered).
        /// </summary>
        public bool fixPivot = true;

        /// <summary>
        ///     Shall we fix the text alignment according to the layout 
        ///     settings in ScoreFlash or ScoreFlashLayout? You can disable 
        ///     this on a prefab that you want to ignore the settings in 
        ///     ScoreFlash or ScoreFlashLayout. This is useful, for example, 
        ///     for real 3D text (which you'll usually always want to have 
        ///     centered).
        /// </summary>
        public bool fixAlignment = true;

        private RectTransform rt;
        private ContentSizeFitter csf;
        private Canvas canvas;
        private RectTransform canvasRT;

        public void Awake() {
            if (text == null) {
                text = GetComponent<Text>();
            }
            if (rt == null) {
                rt = GetComponent<RectTransform>();
            }
            if (csf == null) {
                csf = GetComponent<ContentSizeFitter>();
            }

            coloredGraphicsOrigColor.Clear();
            foreach (Graphic graphic in coloredGraphics) {
                coloredGraphicsOrigColor.Add(graphic.color);
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
        ///     Returns <c>false</c> because DF GUI does not make use of GUISkins.
        /// </summary>
        public override bool UsesGUISkin {
            get { return false; }
        }

        /// <summary>
        ///     Returns <c>true</c> because you need to attach Unity UI stuff correctly
        ///     in the scene hierarchy. This is usually the Canvas that you want to use
        ///     for ScoreFlash.
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
            this.gameObject.layer = parent.gameObject.layer;
            if (text != null && text.gameObject != null) {
                this.text.gameObject.layer = parent.gameObject.layer;
            }

            ScoreFlashRendererBase result
                = (ScoreFlashRendererBase)((GameObject)Instantiate(this.gameObject)).GetComponent<ScoreFlashRendererUnityUI>();

            result.transform.SetParent(parent);

            result.transform.localPosition = Vector2.zero;

            return result;
        }


        public override void Initialize(ScoreMessage msg) {
            Vector2 sd = rt.sizeDelta;
            sd.x = msg.MaxWidth;
            rt.sizeDelta = sd;

            text.text = msg.Text;

            rt.anchorMin = rt.anchorMax = new Vector2(0, 1);

            // set the alignment correctly 
            if (fixAlignment) {
                text.alignment = (TextAnchor)NGAlignment.ConvertAlignment(msg.InnerAnchor, NGAlignment.AlignmentType.TextAnchor);
            }

            // set the anchor and pivot correctly depending on ScreenAlign
            if (fixPivot) {
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

            DoLayout();
        }

        private void DoLayout() {
            if (csf != null) {
                csf.SetLayoutHorizontal();
                csf.SetLayoutVertical();
            }
        }

        /// <summary>
        ///     The size of the message on screen. This is using 
        ///     text.preferredWith and text.preferredHeight.
        /// </summary>
        /// <param name="msg">the current version of the message</param>
        public override Vector2 GetSize(ScoreMessage msg) {
            if (csf != null) {
                text.text = msg.Text;
                DoLayout();
                return new Vector2(text.preferredWidth, rt.sizeDelta.y);
            } else {
                return rt.sizeDelta;
            }
        }

        /// <summary>
        ///     Update the message.
        /// </summary>
        /// <param name="msg">the current version of the message</param>
        public override void UpdateMessage(ScoreMessage msg) {
            // are we visible (we may be behind the player when rendering ScoreFlashFollow3D stuff)
            text.enabled= msg.IsVisible;

            // if the text has changed, we might have to re-layout
            if (!msg.Text.Equals(text.text)) {
                DoLayout();
                Rect pos = msg.Position;
                pos.height = rt.sizeDelta.y;
                msg.Position = pos;
            }

            // push text and color to Unity UI
            text.text = msg.Text;
            text.color = msg.CurrentTextColor;

            // if we have background images registered, push color to those
            for (int i = 0; i < coloredGraphics.Count; i++) {
                coloredGraphics[i].color = AlphaMultiplyColor(coloredGraphicsOrigColor[i], msg.CurrentTextColor);
            }

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

        [Serializable]
        public class RectTransformScale {
            public RectTransform rectTransform;
            public Vector2 scale = Vector3.one;
        }

    }
}