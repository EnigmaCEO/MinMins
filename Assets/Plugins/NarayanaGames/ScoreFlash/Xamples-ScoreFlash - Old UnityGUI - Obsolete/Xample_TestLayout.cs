using UnityEngine;
using System.Collections;
using NarayanaGames.ScoreFlashComponent;

[RequireComponent(typeof(ScoreFlashLayout))]
public class Xample_TestLayout : MonoBehaviour {

	public string label = "Label:";

	private ScoreFlashLayout layout = null;

	void Awake() {
		layout = GetComponent<ScoreFlashLayout>();
	}

	void Start() {
	    // NOTE: When this is done in start,
	    // it may not work with forced Retina; to
	    // make it work with forced Retina, add a
	    // tiny delay (using a Coroutine)!
		layout.Push(label);
        StartCoroutine(TestRewrite());
	}

    public IEnumerator TestRewrite() {
        yield return new WaitForSeconds(1F);
        ScoreMessage msg = layout.Push(label);
        while (true) {
            yield return new WaitForSeconds(1F);
            msg.Text = "Changed:";
            yield return new WaitForSeconds(1F);
            msg.Text = label;
        }
    }

}
