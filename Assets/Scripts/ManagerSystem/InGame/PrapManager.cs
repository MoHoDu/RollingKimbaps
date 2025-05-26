using System.Collections.Generic;
using InGame.Combination;
using UnityEngine;

namespace ManagerSystem.InGame
{
    public class PrapManager : BaseManager
    {
        // 매니저
        private ResourceManager _resourceManager;
        
        // 생성 시 부모 지정 안 했을 때에 디폴트 부모 객체 
        private Transform _defaultParent;

        public override void Initialize(params object[] datas)
        {
            base.Initialize(datas);
            
            foreach (object data in datas)
            {
                if (data is Transform inParent)
                    _defaultParent = inParent;
                
                if (data is ResourceManager resource)
                    _resourceManager = resource;
            }
        }
    }
}