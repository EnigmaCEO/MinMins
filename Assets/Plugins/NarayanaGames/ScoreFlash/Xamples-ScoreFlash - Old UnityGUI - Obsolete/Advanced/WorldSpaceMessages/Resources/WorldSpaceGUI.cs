using UnityEngine;
using System.Collections;

public class WorldSpaceGUI : MonoBehaviour {

    public GameObject cubeUpOffset;
    public GameObject shooter;
    public GameObject sphere2DOffset;
    public GameObject sphere3DOffset;
    public GameObject sphereLeaveBehind;
    public GameObject sphereNoFollow3D;

    public void OnGUI() {
        Rect rect = new Rect(10F, Screen.height - 60F, 140F, 20F);
        PushActive(sphereLeaveBehind, GUI.Toggle(rect, PullActive(sphereLeaveBehind), "Leave Behind"));
        ToggleControlColors(sphereLeaveBehind, rect);
        rect.x += rect.width + 10F;
        
        PushActive(sphere3DOffset, GUI.Toggle(rect, PullActive(sphere3DOffset), "3D Offset"));
        ToggleControlColors(sphere3DOffset, rect);
        rect.x += rect.width + 10F;
        
        PushActive(sphere2DOffset, GUI.Toggle(rect, PullActive(sphere2DOffset), "2D Offset"));
        ToggleControlColors(sphere2DOffset, rect);
        rect.x += rect.width + 10F;
        
        PushActive(sphereNoFollow3D, GUI.Toggle(rect, PullActive(sphereNoFollow3D), "Using Vector3"));
        ToggleControlColors(sphereNoFollow3D, rect);
        rect.x += rect.width + 10F;
        
        PushActive(cubeUpOffset, GUI.Toggle(rect, PullActive(cubeUpOffset), "Cube, Up Offset"));
        ToggleControlColors(cubeUpOffset, rect);
        rect.x += rect.width + 10F;
        
        PushActive(shooter, GUI.Toggle(rect, PullActive(shooter), "Using Momentum"));
        // can't toggle colors because shooter doesn't support that (would be easy to add, though)
    }

    private bool PullActive(GameObject obj) {
        return obj.activeInHierarchy;
    }

    private void PushActive(GameObject obj, bool val) {
        obj.SetActive(val);
    }

    private void ToggleControlColors(GameObject obj, Rect rect) {
        WorldSpaceMessagesCSharp wsmcs = obj.GetComponent<WorldSpaceMessagesCSharp>();
        if (wsmcs != null) {
            rect.y += 30F;
            wsmcs.controlColors = GUI.Toggle(rect, wsmcs.controlColors, "Control Colors");
        }
    }
}
