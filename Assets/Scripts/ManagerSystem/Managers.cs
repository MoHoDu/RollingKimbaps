using System.Collections.Generic;
using UnityEngine;

namespace ManagerSystem
{
    public static class Managers
    {
        public static ResourceManager Resource { get; private set; }

        private static HashSet<IBaseManager> managers = new();

        public static void Initialize(ResourceManager resourceManager)
        {
            Resource = resourceManager;
            managers.Add(Resource);

            Debug.Log("Initialize Managers");
            foreach (var manager in managers)
            {
                manager.Initialize();
            }
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