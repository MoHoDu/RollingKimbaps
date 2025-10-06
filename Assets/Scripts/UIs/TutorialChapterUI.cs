using UIs.Base;
using UnityEngine;
using System;
using System.Collections;
using Attributes;

public class TutorialChapterUI : CanvasUI
{
    [SerializeField] private GameObject _bg;

    public void ActiveOnOf(bool isOn)
    {
        _bg?.SetActive(isOn);
    }
}