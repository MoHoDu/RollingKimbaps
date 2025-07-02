using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Attributes;
using EnumFiles;


namespace UIs.Panels.Infos
{
    public class UIInfo
    {
        public EButtonType buttonType;
        public bool placeQuitButton;
        public bool pauseGame;

        public Action[] buttonActions;

        public UIInfo(EButtonType type,
            bool hasQuitButton, bool setPause,
            params Action[] buttonClickEvents)
        {
            buttonType = type;
            placeQuitButton = hasQuitButton;
            pauseGame = setPause;
            buttonActions = buttonClickEvents;
        }

        // 만약 pauseGame이 true라면,
        // UI on 시에 정지, 
        // off 시에 3초 카운트 및 재개 로직을
        // onOpened와 onClosed에 (이벤트)추가 해야 함
    }
}