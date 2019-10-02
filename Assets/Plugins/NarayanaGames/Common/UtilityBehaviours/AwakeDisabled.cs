using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class AwakeDisabled : MonoBehaviour {

    public List<Component> componentsToAwake = new List<Component>();

    public void Awake() {
        for (int i=0; i < componentsToAwake.Count; i++) {
            //Debug.LogFormat(componentsToAwake[i], "Trying to call awake on: {0}", componentsToAwake[i].name);
            System.Type componentType = componentsToAwake[i].GetType();
            MethodInfo awakeMethod = componentType.GetMethod("Awake");
            if (awakeMethod != null) {
                awakeMethod.Invoke(componentsToAwake[i], null);
            } else {
                Debug.LogErrorFormat("Could not find Awake on {0}", componentsToAwake[i].name);
            }
        }
        Destroy(this.gameObject);
    }
}
