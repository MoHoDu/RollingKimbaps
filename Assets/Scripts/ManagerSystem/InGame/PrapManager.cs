using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using InGame;
using Panels.Base;
using UnityEngine;

namespace ManagerSystem.InGame
{
    public class PrapManager : BaseManager
    {
        // 활성화 된 프랍들
        private Dictionary<Vector3, Prap> _activePraps = new();
        
        // 생성 시 부모 지정 안 했을 때에 디폴트 부모 객체 
        private Transform _defaultParent;
        private Dictionary<EPrapType, SpawnLayer> _spawnLayers = new();
        
        // 캐시로 저장되어 있는 프랍들
        private Dictionary<PrapData, Prap> _prapCache = new();
        
        // DI
        private ResourceManager _resourceManager;
        private StageManager _stageManager;

        public override void Initialize()
        {
            base.Initialize();
            
            _defaultParent = _stageManager?.FindObjectOrCreate("PrapCache")?.transform; 
            _defaultParent ??= new GameObject("PrapCache").transform;
        }

        public override void Initialize(params object[] datas)
        {
            foreach (var data in datas)
            {
                if (data is InGameManager ingame)
                {
                    _resourceManager = ingame.resourceManager;
                    _stageManager = ingame.stageManager;
                }
                else if (data is SpawnLayer[] layers)
                {
                    foreach (var layer in layers)
                    {
                        _spawnLayers.TryAdd(layer.PrapType, layer);
                    }
                }
            }
            
            base.Initialize(datas);
        }

        public Prap CreatePrap(PrapData data, Vector3 position, Transform parent = null)
        {
            Prap prap = null;
            GameObject prapObj = null;
            if (parent is null && _spawnLayers.TryGetValue(data.Type, out SpawnLayer layer))
            {
                parent = layer.transform;
            }
            
            parent ??= _defaultParent;

            if (!_prapCache.TryGetValue(data, out prap))
            {
                prapObj = _resourceManager?.Instantiate(data.Path, _defaultParent);
                if (prapObj is not null)
                {
                    if (!prapObj.TryGetComponent<Prap>(out prap))
                    {
                        prap = prapObj.AddComponent<Prap>();
                    }
                    
                    prap.prapData = data;
                    prap.gameObject.SetActive(false);
                    _prapCache.Add(data, prap);
                }
            }
            else
            {
                prapObj = prap.gameObject;
            }
            
            if (prap is null) 
                return null;
            
            GameObject clone = GameObject.Instantiate(prapObj, parent);
            Prap clonePrap = clone.GetComponent<Prap>();
            
            Vector3 parentPos = parent.transform.position;
            Vector3 prapPostion = new Vector3(position.x, position.y, parentPos.z);
            clone.transform.position = prapPostion;
            
            clone.SetActive(true);
            if (!_activePraps.TryAdd(prapPostion, clonePrap))
            {
                DestroyPrap(clonePrap);
                return null;
            }
            
            return clonePrap;
        }

        public void DestroyPrap(Prap prapObj)
        {
            if (_prapCache.TryGetValue(prapObj.prapData, out Prap prap))
            {
                _resourceManager?.Destroy(prapObj.gameObject);
                _prapCache.Remove(prapObj.prapData);
                _activePraps.Remove(prap.transform.position);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            List<(Vector3, Prap)> movedPraps = _activePraps
                .Where(pair => pair.Value.transform.hasChanged)
                .Select(pair => (pair.Key, pair.Value))
                .ToList();

            if (movedPraps.Any())
            {
                foreach ((Vector3 pos, Prap prap) pair in movedPraps)
                {
                    pair.prap.transform.hasChanged = false;
                    _activePraps.Remove(pair.pos);
                    if (_activePraps.TryAdd(pair.pos, pair.prap))
                    {
                        Debug.LogWarning($"[Warning] 프랍({pair.prap.name})의 위치({pair.pos})가 중복이 되어 제거합니다.");
                        DestroyPrap(pair.prap);
                    }
                }
            }
        }
    }
}