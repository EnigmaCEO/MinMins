using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSounds : SingletonPersistentPrefab<GameSounds>
{
    public void PlayUiAdvanceSound()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_ADVANCE, SoundManager.AudioTypes.Sfx);
    }

    public void PlayUiBackSound()
    {
        SoundManager.Play(GameConstants.SoundNames.UI_BACK, SoundManager.AudioTypes.Sfx);
    }
}
