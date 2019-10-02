using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using NarayanaGames.Common;
using NarayanaGames.UnityEditor.Common;
using System.Xml;

namespace NarayanaGames.UnityEditor.Common {
    /// <summary>
    ///     Base class for editors / inspectors by narayana games.
    /// </summary>
    public class NGEditorBase : Editor {

        /// <summary>
        ///     Shall the API documentation be shown?
        /// </summary>
        protected bool showFullAPIDocumentation = false;

        /// <summary>
        ///     A property field that shows the correct tooltip (coming from XML) 
        ///     and always shows children (if there are any).
        /// </summary>
        /// <param name="property">the property this field is for</param>
        /// <param name="label">the label displayed next to the field</param>
        protected void PropertyField(SerializedProperty property, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUILayout.PropertyField(property, content);
            DrawFullDocumentation(property);
        }

        protected bool PropertyField(SerializedProperty property, string label, bool includeChildren) {
            GUIContent content = ContentWithTooltip(property, label);
            bool result = EditorGUILayout.PropertyField(property, content, includeChildren);
            DrawFullDocumentation(property);
            return result;
        }

        protected void NiceFloatSlider(SerializedProperty property, float min, float max, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUIExtensions.NiceFloatSlider(property, min, max, content);
            DrawFullDocumentation(property);
        }

        protected void NiceIntSlider(SerializedProperty property, int min, int max, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUIExtensions.NiceIntSlider(property, min, max, content);
            DrawFullDocumentation(property);
        }

        protected void NiceVector2Field(SerializedProperty property, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUIExtensions.NiceVector2Field(property, content);
            DrawFullDocumentation(property);
        }

        protected void NiceVector3Field(SerializedProperty property, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUIExtensions.NiceVector3Field(property, content);
            DrawFullDocumentation(property);
        }

        protected void NiceArrayWithFoldout(SerializedProperty property, string label) {
            GUIContent content = ContentWithTooltip(property, label);
            EditorGUIExtensions.ArrayWithFoldout(serializedObject, property, content);
            DrawFullDocumentation(property);
        }

        protected GUIContent ContentWithTooltip(SerializedProperty property, string label) {
            if (!Tooltips.ContainsKey(property.name)) {
                Debug.LogWarning(string.Format("Could not find documentation for {0} on {1}", property.name, serializedObject.targetObject.GetType().Name));
                return new GUIContent(label);
            }
            return new GUIContent(label, string.Format("{0} (API-Name: {1})", 
                Tooltips[property.name].tooltip, 
                property.name));
        }

        protected void DrawFullDocumentation(SerializedProperty property) {
            DrawFullDocumentation(property.name, 50);
        }

        protected void DrawFullDocumentation(string itemName, float minPaddingLeft) {
            if (!showFullAPIDocumentation) {
                return;
            }

            if (!Tooltips.ContainsKey(itemName)) {
                Debug.LogWarning(string.Format("Could not find documentation for {0} on {1}", itemName, serializedObject.targetObject.GetType().Name));
                return;
            }

            APIDocEntry fieldDoc = Tooltips[itemName];
            ButtonDocs(fieldDoc.mainDoc, minPaddingLeft);
            EditorGUIExtensions.LabelDocs(string.Format("{0} (API-Name: {1})",
                fieldDoc.tooltip.Replace("[", "<b>").Replace("]", "</b>"), 
                itemName));
            foreach (Link link in fieldDoc.links) {
                ButtonDocs(link, minPaddingLeft + 20);
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        private void ButtonDocs(Link link, float leftSpace) {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(leftSpace);
                if (GUILayout.Button(new GUIContent(link.title, "Open Link in Browser"))) {
                    Help.BrowseURL(link.url);
                }
                GUILayout.Space(20);
            }
            EditorGUILayout.EndHorizontal();
        }

        private Dictionary<string, APIDocEntry> tooltips;
        internal Dictionary<string, APIDocEntry> Tooltips {
            get {
                if (tooltips == null) {
                    ParseTooltips();
                }
                return tooltips;
            }
        }

        /// <summary>
        ///     Builds a dictionary of tooltips to be shown in fancy inspector GUIs.
        /// </summary>
        /// <remarks>
        ///     This requires the right XML file (I'm doing quite some processing
        ///     on the original XML files to make this work properly in the editor;
        ///     if you're interested in using this: 
        ///     <a href="http://narayana-games.net/Contact.aspx">contact me!</a>
        /// </remarks>
        internal void ParseTooltips() {
            string className = serializedObject.targetObject.GetType().Name;
            TextAsset tooltipsXML = ((IHasTooltips)serializedObject.targetObject).Tooltips;

            tooltips = new Dictionary<string, APIDocEntry>();

            // unity doesn't seem to easily support Link ... so let's keep that for the future
            //XDocument doc = XDocument.Parse(tooltips.text);
            //foreach (XElement field in doc.Element("fields").Elements("fields")) {
            //    result[field.Attribute("name").Value] = field.Element("summary").Value;
            //}

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(tooltipsXML.text);

            ParseEntries(xmlDocument, className, "/tooltips/fields", "field", "class", "field");
            ParseEntries(xmlDocument, className, "/tooltips/classes", "class", "name", "name");
        }

        private void ParseEntries(XmlDocument xmlDocument, string className, string path, string elementName, string classAttributeName, string entryNameAttributeName) {
            XmlNode fields = xmlDocument.SelectSingleNode(path);

            XmlNodeList fieldList = fields.SelectNodes(elementName);
            foreach (XmlElement field in fieldList) {
                if (field.GetAttribute(classAttributeName).Equals(className)) {
                    XmlNode summaryNode = field.SelectSingleNode("tooltip");
                    string summary = summaryNode.InnerText;
                    string fieldName = field.GetAttribute(entryNameAttributeName);
                    APIDocEntry apiDocEntry = new APIDocEntry();
                    apiDocEntry.field = fieldName;
                    apiDocEntry.tooltip = summary;
                    apiDocEntry.mainDoc = new Link();
                    apiDocEntry.mainDoc.title = string.Format("API docs for: {0}", fieldName);
                    apiDocEntry.mainDoc.url = field.GetAttribute("url");
                    tooltips[fieldName] = apiDocEntry;

                    XmlNode linksNode = field.SelectSingleNode("links");
                    XmlNodeList linkList = linksNode.SelectNodes("link");
                    List<Link> links = new List<Link>();

                    foreach (XmlElement linkElem in linkList) {
                        Link link = new Link();
                        link.title = linkElem.GetAttribute("title");
                        link.url = linkElem.GetAttribute("url");
                        links.Add(link);
                    }
                    apiDocEntry.links = links;
                }
            }
        }


        internal class APIDocEntry {
            internal string field;
            internal string tooltip;
            internal Link mainDoc;
            internal List<Link> links;
        }

        internal struct Link {
            internal string title;
            internal string url;
        }
    }
}