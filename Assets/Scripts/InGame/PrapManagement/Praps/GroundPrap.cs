using System.Collections.Generic;
using Attributes;
using ManagerSystem;
using UnityEngine;

namespace InGame.PrapManagement.Praps
{
    public class GroundPrap : Prap
    {
        // Components
        [Bind("Leftside")] GameObject _leftside;
        [Bind("Middle")] GameObject _middle;
        [Bind("Rightside")] GameObject _rightside;

        // 장애물 배치 여부
        public bool SetObstacles = false;

        // 타일 수
        public int TileCount => MiddleGroundsCount + 2;

        // 중간 그라운드 수
        public int MiddleGroundsCount { get; private set; } = 1;
        private List<GameObject> _middleGrounds = new List<GameObject>();

        // 계산용 수치들
        private readonly float _tileWidth = 2.9f;

        public override void OnSpawned(params object[] args)
        {
            int _tileCount = 2;
            foreach (var arg in args)
            {
                if (arg is int tileCount)
                {
                    _tileCount = tileCount;
                }
            }

            if (!IsBindingDone) Awake();

            Setup(_tileCount);

            base.OnSpawned(args);

            LeftPos = transform.TransformPoint(_leftside.transform.localPosition);
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