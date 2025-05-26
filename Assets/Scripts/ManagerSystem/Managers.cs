using System.Collections.Generic;
using UnityEngine;

namespace ManagerSystem
{
    public static class Managers
    {
        // 씬매니저
        public static ResourceManager Resource { get; private set; }
        // 오디오 매니저
        // UI매니저
        // 이펙트 매니저
        public static SaveManager Save { get; private set; } = new();
        public static InGameManager InGame { get; private set; } = new();

        private static HashSet<IBaseManager> managers = new();

        public static bool DoneInitialized { get; private set; } = false;

        public static void Initialize(ResourceManager resourceManager)
        {
            Resource = resourceManager;
            managers.Add(Resource);
            managers.Add(Save);
            managers.Add(InGame);

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
    }
}