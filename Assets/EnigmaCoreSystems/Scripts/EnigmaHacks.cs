using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnigmaHacks : SingletonMonobehaviour<EnigmaHacks>
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public bool EnjinIdNotNull;
    public bool EnjinCodeNotNull;
    public bool EnjinLinked;
    public bool ByPassIAPReceiptCheck;
    public bool ResetFileManagerAtStart;

    public ValueHack HeartBeatDelay = new ValueHack("3");
#endif

    public bool FailHeartBeat;
    public bool MuteMusic;

    public ValueHack Language = new ValueHack("Spanish");
    //public ValueHack FreezeLoadingScreenDelay = new ValueHack("5");
}
