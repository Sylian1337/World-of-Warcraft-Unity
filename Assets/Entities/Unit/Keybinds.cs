using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class KeyBinds
{
    public KeyCode one;
    public KeyCode two;
    public KeyCode three;
    public KeyCode four;
    public KeyCode five;
    public KeyCode six;
    public KeyCode seven;

    public KeyBinds(KeyCode one, KeyCode two, KeyCode three, KeyCode four, KeyCode five, KeyCode six, KeyCode seven)
    {
        this.one = one; this.two = two; this.three = three; this.four = four; this.five = five; this.six = six; this.seven = seven;
    }

    public KeyBinds()
    {
        ResetToDefault();
    }

    public void ResetToDefault()
    {
        one = KeyCode.Alpha1;
        two = KeyCode.Alpha2;
        three = KeyCode.Alpha3;
        four = KeyCode.Alpha4;
        five = KeyCode.Alpha5;
        six = KeyCode.Alpha6;
        seven = KeyCode.Alpha7;
    }
}
