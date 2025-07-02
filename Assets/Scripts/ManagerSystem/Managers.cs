using System.Collections.Generic;
using UnityEngine;
using ManagerSystem.Base;
using ManagerSystem.UIs;
using ManagerSystem.SaveLoad;


namespace ManagerSystem
{
    public static class Managers
    {
        public static StageManager Stage { get; private set; } = new();     // 씬매니저
        public static ResourceManager Resource { get; private set; }
        public static AudioManager Audio { get; private set; } = new();
        public static UIManager UI { get; private set; } = new();
        public static EffectManager Effect { get; private set; } = new();
        public static SaveManager Save { get; private set; } = new();
        public static InGameManager InGame { get; private set; } = new();

        private static HashSet<IBaseManager> managers = new();

        public static bool DoneInitialized { get; private set; } = false;

        public static void Initialize(ResourceManager resourceManager)
        {
            Resource = resourceManager;
            managers.Add(Stage);
            managers.Add(Resource);
            managers.Add(Audio);
            managers.Add(UI);
            managers.Add(Effect);
            managers.Add(Save);
            managers.Add(InGame);

            // 이벤트 연결
            LinkedEvents();

            Debug.Log("Initialize Managers");
            foreach (var manager in managers)
            {
                manager.Initialize();
            }

            DoneInitialized = true;
        }

        public static void Start()
        {
            foreach (IBaseManager manager in managers)
            {
                manager.Start();
            }
        }

        public static void Update()
        {
            foreach (IBaseManager manager in managers)
            {
                manager.Update();
            }
        }

        public static void FixedUpdate()
        {
            foreach (IBaseManager manager in managers)
            {
                manager.FixedUpdate();
            }
        }

        private static void LinkedEvents()
        {
            Stage.AddEventAfterSceneOpened("GameScene", (_) =>
            {
                UI.FindCanvasAndGamePanel();
                InGame.InitManagers();
            });
        }
    }
}