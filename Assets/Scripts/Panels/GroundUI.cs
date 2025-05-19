using System.Collections.Generic;
using Attributes;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class GroundUI : BindUI
    {
        [Bind("Leftside")] GameObject _leftside;
        [Bind("Middle")] GameObject _middle;
        [Bind("Rightside")] GameObject _rightside;
        
        private List<GameObject> _middleGrounds = new List<GameObject>();
        public int MiddleGroundsCount { get; private set; } = 1;
        public int TileCount => MiddleGroundsCount + 2;
        public float GroundWidth => TileCount * _tileWidth;

        private readonly float _tileWidth = 2.9f;

        protected override void Initialize()
        {
            base.Initialize();
            
            _middleGrounds.Add(_middle);
        }

        public void Setup(int tileCount = 3)
        {
            ClearGrounds();
            
            tileCount = tileCount < 3 ? 3 : tileCount;
            MiddleGroundsCount = tileCount - 2;

            for (int i = 1; i < MiddleGroundsCount; i++)
            {
                GameObject ground = GameObject.Instantiate(_middle, transform);
                ground.transform.localPosition = Vector3.right * (_tileWidth * i);
            }
            
            _rightside.transform.localPosition = Vector3.right * (_tileWidth * MiddleGroundsCount);
        }

        private void ClearGrounds()
        {
            foreach (var ground in _middleGrounds)
            {
                if (_middle == ground) continue;
                
                Managers.Resource.Destroy(ground);
            }
            
            _middleGrounds.Clear();
            _middleGrounds.Add(_middle);
        }
    }
}