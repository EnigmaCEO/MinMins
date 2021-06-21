using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnigmaHacks : SingletonMonobehaviour<EnigmaHacks>
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    public bool ByPassIAPReceiptCheck;
    public bool ResetFileManagerAtStart;

    public ValueHack EnjinIdIsNull = new ValueHack("false");  
    public ValueHack EnjinCodeIsNull = new ValueHack("false"); //If they have Id and code is null, then they are linked
    public ValueHack HeartBeatDelay = new ValueHack("3");
#endif

    public bool FailHeartBeat;
    public bool MuteMusic;
    public bool PreventAutoLogin;

    public bool LanguageHack = false;
    public SystemLanguage HackedLanguage = SystemLanguage.Spanish;
    //public ValueHack FreezeLoadingScreenDelay = new ValueHack("5");
}
