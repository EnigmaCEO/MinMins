using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnigmaHacks : SingletonMonobehaviour<EnigmaHacks>
{
    public bool EnjinIdNotNull;
    public bool EnjinCodeNotNull;
    public bool EnjinLinked;

    public bool ResetFileManagerAtStart;

    public bool ByPassIAPReceiptCheck;

    public ValueHack Language = new ValueHack("Spanish");
}
