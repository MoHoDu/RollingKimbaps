using System;
using System.Collections.Generic;
using System.IO;
using EnumFiles;
using JsonData;
using UnityEngine;
using Utils;
using ManagerSystem.Base;

namespace ManagerSystem.SaveLoad
{
    public class SaveManager : BaseManager
    {
        public PlayerSettingsController PlayerSettings { get; private set; } = new PlayerSettingsController();

        public override void Initialize(params object[] args)
        {
            base.Initialize(args);

            PlayerSettings.LoadSettings();
        }
    }
}