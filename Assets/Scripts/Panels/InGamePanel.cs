using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class InGamePanel : BindUI
    {
        // Singleton
        private static InGamePanel _instance;
        public static InGamePanel Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("[Manager Error] InGamePanelController is null. Please check the inspector.");
                }
                
                return _instance;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            _instance = this;
        }
    }
}