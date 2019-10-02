/****************************************************
 *  (c) 2014 narayana games UG (haftungsbeschränkt) *
 *  This is just an example, do with it whatever    *
 *  you like ;-)                                    *
 ****************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using NarayanaGames.Common;
using NarayanaGames.Common.UI;

namespace NarayanaGames.ScoreFlashComponent.Addons {
    /// <summary>
    ///     Attach this to a Unity UI Canvas prefab and assign the prefab to an 
    ///     instance of <see cref="ScoreFlash"/> to have that ScoreFlash
    ///     render its messages using that Canvas. This is the 3D space version
    ///     that can also be used in VR environments (e.g. Oculus Rift or Sony
    ///     Project Morpheus).
    /// </summary>
    /// <remarks>
    ///     Currently, Unity's built-in text rendering isn't really that great.
    ///     So you might consider using 
    ///     <a href="https://www.assetstore.unity3d.com/en/#!/content/17662">Text Mesh Pro</a> 
    ///     instead, which is a lot more powerful. But to prevent creating any
    ///     external dependencies this is included.
    /// </remarks>
    [ExecuteInEditMode()]
    [RequireComponent(typeof(Canvas))]
    public class ScoreFlashRendererUnityUI3D: ScoreFlashRendererBase {

        public Text text = null;
        public Transform textTransform = null;

        public bool lookAtCamera = true;

        public float positionScale = 0.01F;

        private RectTransform rt;
        private ContentSizeFitter csf;
        private Canvas canvas;
        //private RectTransform canvasRT;

        public void Awake() {
            if (text == null) {
                text = GetComponentInChildren<Text>();
            }
            if (rt == null) {
                rt = text.GetComponent<RectTransform>();
            }
            if (csf == null) {
                csf = text.GetComponent<ContentSizeFitter>();
            }
            if (canvas == null) {
                canvas = GetComponent<Canvas>();
                //canvasRT = canvas.GetComponent<RectTransform>();
                if (canvas.renderMode != RenderMode.WorldSpace) {
                    Debug.LogWarning(string.Format("To use ScoreFlash with Unity UI in 3D space, you need a canvas with render mode World Space!"), this.gameObject);
                }
            }
        }

        #region Implementation of methods required by ScoreFlashRendererBase

        /// <summary>
        ///     Returns <c>false</c> because Text Mesh does not make use of GUISkins.
        /// </summary>
        public override bool UsesGUISkin {
            get { return false; }
        }

        /// <summary>
        ///     Returns <c>false</c> because we position these absolutely.
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
        ///     Creates a new instance of this renderer that will handle rendering
        ///     for a given message. Used internally by ScoreFlash. Default
        ///     implementation simply uses 
        ///     <a href="http://docs.unity3d.com/Documentation/ScriptReference/Object.Instantiate.html">Instantiate()</a>.
        /// </summary>
        /// <param name="parent">the parent transform this should go into</param>
        /// <returns>the new instance</returns>
        public override ScoreFlashRendererBase CreateInstance(Transform parent) {
            this.gameObject.layer = parent.gameObject.layer;

            ScoreFlashRendererBase result = (ScoreFlashRendererBase)Instantiate(this);
            result.transform.SetParent(parent);
            return result;
        }


        public override void Initialize(ScoreMessage msg) {
            Vector2 sd = rt.sizeDelta;
            sd.x = msg.MaxWidth;
            rt.sizeDelta = sd;

            text.text = msg.Text;

            /*
             * Note: We simply keep the anchor, pivot and text alignment as 
             * set up in the prefab - this gives us more flexibility!
             */

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
            text.text = msg.Text;
            DoLayout();
            return new Vector2(text.preferredWidth, rt.sizeDelta.y);
        }

        /// <summary>
        ///     Update the message.
        /// </summary>
        /// <param name="msg">the current version of the message</param>
        public override void UpdateMessage(ScoreMessage msg) {
            transform.position = msg.FollowLocation.CurrentTranslatedPosition;
            if (lookAtCamera) {
                transform.LookAt(msg.Follow3D.referenceCamera.transform.position, msg.Follow3D.referenceCamera.transform.up);
            } else {
                transform.rotation = msg.FollowLocation.Target.transform.rotation;
            }

            // are we visible (we may be behind the player when rendering ScoreFlashFollow3D stuff)
            text.enabled = msg.IsVisible;

            // push text and color to Unity UI label
            text.text = msg.Text;
            text.color = msg.CurrentTextColor;

            Vector2 position = new Vector2(msg.Position.x, msg.Position.y);
            position -= msg.OriginalPosition;
            rt.anchoredPosition = new Vector2(position.x, -position.y);

            // ... ok, I guess these are kind of obvious, aren't they?
            rt.transform.localScale = msg.Scale * Vector3.one;
            rt.transform.localRotation = Quaternion.Euler(0, 180, -msg.Rotation);
        }

        #endregion Implementation of methods required by ScoreFlashRendererBase
    }
}