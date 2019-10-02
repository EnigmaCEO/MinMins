using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[ExecuteInEditMode()]
public class XamplesGenericGUI : MonoBehaviour {

    public Rect labelLocation;
    public string labelText = "This scene is cool";

    public Rect buttonLocation;
    public string buttonText = "Next Scene";

    /// <summary>
    ///     How long are the samples to determine the framerate?
    /// </summary>
    public float updateInterval = 1F;

    public string performanceTextFormat = "{1:000} FPS{5} (Avg: {2:000}, Min: {3:000}, Max: {4:000})"; // {0} - 

    private float currentFPS = 0F;

    private string performanceString = "";

    private float accum = 0.0F; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    private int accumFPSCount = 0;
    private float accumFPS = 0F;
    private float maxFPS = 0F;
    private float minFPS = 999F;
    public float AvgFPS {
        get {
            if (accumFPSCount == 0) {
                return 0;
            }
            return accumFPS / (float)accumFPSCount;
        }
    }

    public void Start() {
        Application.targetFrameRate = 250; // high value to avoid limiting
    }

    /// <summary>
    ///     Calculates the current framerate.
    ///     <seealso cref="updateInterval"/>
    /// </summary>
    public void Update() {
        this.timeleft -= Time.deltaTime;
        this.accum += Time.timeScale / Time.deltaTime;
        this.frames++;

        // Interval ended - update GUI text and start new interval
        if (this.timeleft <= 0.0) {
            this.currentFPS = this.accum / this.frames;

            this.maxFPS = Mathf.Max(this.maxFPS, currentFPS);
            this.minFPS = Mathf.Min(this.minFPS, currentFPS);
            this.accumFPS += currentFPS;
            this.accumFPSCount++;

            this.timeleft = this.updateInterval;
            this.accum = 0.0F;
            this.frames = 0;

            UpdatePerformanceText();
        }
    }

    private void UpdatePerformanceText() {
        string max = "";
        if (currentFPS >= Application.targetFrameRate - 1) {
            max = " (max)";
        }

        this.performanceString = string.Format(performanceTextFormat,
            QualitySettings.GetQualityLevel().ToString(), // {0}
            this.currentFPS, // {1}
            this.AvgFPS,     // {2}
            this.minFPS,     // {3}
            this.maxFPS,     // {4}
            max);            // {5}
    }

    public void OnGUI() {
        GUI.Label(labelLocation, string.Format(labelText, performanceString));

        if (!Application.isEditor) {
            if (GUI.Button(buttonLocation, buttonText)) {
                
                int nextLevel = (SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCount;
                SceneManager.LoadScene(nextLevel);
            }
        }

    }
}
