using System;
using System.Collections.Generic;
using Attributes;
using ManagerSystem;
using Obstacles;
using UIs.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UIs
{
    public class GroundUI : BindUI
    {
        [Bind("Obstacles")] Transform _obstacleParent;
        [Bind("Persons")] Transform _personParent;
        
        [Bind("Leftside")] GameObject _leftside;
        [Bind("Middle")] GameObject _middle;
        [Bind("Rightside")] GameObject _rightside;
        
        private List<GameObject> _middleGrounds = new List<GameObject>();
        private List<Obstacle> _obstacles = new List<Obstacle>();
        public int MiddleGroundsCount { get; private set; } = 1;
        public int TileCount => MiddleGroundsCount + 2;
        public float GroundWidth => TileCount * _tileWidth;
        
        // 위치 값 
        public float StartPos => transform.position.x;
        public float EndPos => _rightside.transform.position.x;
        

        private readonly float _tileWidth = 2.9f;
        
        private readonly int _obstacleMinSpacing = 7;
        private readonly int _obstacleMaxSpacing = 15;
        
        private readonly string[] _obstacleNames =
        {
            "Prefabs/Obstacles/rock_big", 
            "Prefabs/Obstacles/rock_small", 
            "Prefabs/Obstacles/tree_big",
            "Prefabs/Obstacles/tree_small"
        };
        
        private readonly string[] _personPrefabNames =
        {
            "Prefabs/Persons/person_01", 
            "Prefabs/Persons/person_02", 
            "Prefabs/Persons/person_03",
            "Prefabs/Persons/person_04",
            "Prefabs/Persons/person_05",
            "Prefabs/Persons/person_06"
        };

        protected override void Initialize()
        {
            base.Initialize();
            
            _middleGrounds.Add(_middle);
        }

        public OrdererPrap SetPerson(float posX)
        {
            string prefabPath = _personPrefabNames[Random.Range(0, _personPrefabNames.Length)];
            GameObject go = Managers.Resource.Instantiate(prefabPath, _personParent);

            if (go.TryGetComponent<OrdererPrap>(out OrdererPrap orderer))
            {
                go.transform.position = new Vector3(posX, 0, 0);
                return orderer;
            }
            
            Managers.Resource.Destroy(go);
            return null;
        }

        public List<Vector3> SetObstacles()
        {
            List<Vector3> result = new List<Vector3>();
            if (_obstacleParent == null) return result;
            
            int maxTryCount = 3;
            int currentTryCount = 0;
            
            Vector3 lastObstaclePos = new Vector3(_leftside.transform.localPosition.x, 0, 0);
            while (currentTryCount < maxTryCount)
            {
                currentTryCount++;              
                
                int obstacleSpace = Random.Range(_obstacleMinSpacing, _obstacleMaxSpacing);
                float space = obstacleSpace * _tileWidth;
                
                Vector3 obstaclePos = lastObstaclePos + (Vector3.right * space);
                if (obstaclePos.x > _rightside.transform.localPosition.x)
                    break;
                
                string obstacleName = _obstacleNames[Random.Range(0, _obstacleNames.Length)];
                GameObject go = Managers.Resource.Instantiate(obstacleName, _obstacleParent);
                if (go.TryGetComponent(out Obstacle obstacle))
                {
                    go.transform.localPosition = obstaclePos;
                    // 장애물의 끝 지점이 지형물의 밖으로 넘어가면 안됨.
                    float obstacleRightPos = obstaclePos.x + (obstacle.Width / 2f);
                    if (obstacleRightPos >= _rightside.transform.localPosition.x)
                    {
                        Managers.Resource.Destroy(go);
                        break;
                    }

                    go.name = "obstacle";
                    _obstacles.Add(obstacle);
                    
                    Vector3 currentPos = new Vector3(obstacleRightPos, 0, 0);
                    result.Add(currentPos);
                    
                    lastObstaclePos = currentPos;
                }
                else
                {
                    Managers.Resource.Destroy(go);
                }
            }

            return result;
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
            
            SetObstacles();
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

        private void OnDestroy()
        {
            foreach (var obstacle in _obstacles)
            {
                if (obstacle?.gameObject != null) 
                    Managers.Resource.Destroy(obstacle.gameObject);
            }
        }
    }
}