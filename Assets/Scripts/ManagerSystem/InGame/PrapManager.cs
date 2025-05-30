using System.Collections.Generic;
using System.Linq;
using EnumFiles;
using GameDatas;
using InGame;
using InGame.PrapManagement;
using Panels;
using Panels.Base;
using Panels.Spawn;
using UnityEngine;

namespace ManagerSystem.InGame
{
    public class PrapManager : BaseManager
    {
        // 활성화 된 프랍들
        private Dictionary<Vector3, Prap> _activePraps = new();
        
        // 생성 시 부모 지정 안 했을 때에 디폴트 부모 객체 
        private Transform _defaultParent;
        // 디폴트 스폰 레이어
        private Dictionary<EPrapType, SpawnLayer> _defaultLayers = new();
        // 모든 스폰 레이어
        private Dictionary<EPrapType, HashSet<SpawnLayer>> _spawnLayers = new();
        // 자동으로 생성할 필요가 있는 레이어와 담당 스포너
        private Dictionary<SpawnLayer, PrapSpawner> _autoSpawners = new();
        
        // 캐시로 저장되어 있는 프랍들
        private Dictionary<PrapData, Prap> _prapCache = new();
        
        // DI
        private ResourceManager _resourceManager;
        private StageManager _stageManager;
        private RaceStatus _raceStatus;

        public override void Initialize(params object[] datas)
        {
            _defaultParent = _stageManager?.FindObjectOrCreate("PrapCache")?.transform; 
            _defaultParent ??= new GameObject("PrapCache").transform;
            
            foreach (var data in datas)
            {
                if (data is InGameManager ingame)
                {
                    _resourceManager = ingame.resourceManager;
                    _stageManager = ingame.stageManager;
                    _raceStatus = ingame.Status.RaceStatus;
                }
                else if (data is SpawnLayer[] layers)
                {
                    foreach (var layer in layers)
                    {
                        if (layer.isDefaultLayer)
                        {
                            if (_defaultLayers.TryGetValue(layer.PrapType, out var _))
                            {
                                Debug.LogWarning($"[WARNING] Default layer {layer.PrapType} is already set.");
                            }
                            else
                            {
                                _defaultLayers.TryAdd(layer.PrapType, layer);
                            }
                        }
                        
                        if (_spawnLayers.TryGetValue(layer.PrapType, out var targetLayers))
                        {
                            targetLayers.Add(layer);
                        }
                        else
                        {
                            HashSet<SpawnLayer> newLayers = new();
                            newLayers.Add(layer);
                            _spawnLayers.TryAdd(layer.PrapType, newLayers);
                        }
                        
                        if (layer.autoGenerate)
                        {
                            PrapSpawner targetSpawner;
                            if (layer is GroundSpawnLayer groundSpawnLayer)
                            {
                                targetSpawner = new GroundSpawner(this, groundSpawnLayer, _raceStatus);
                            }
                            else if (layer is ObstacleSpawnLayer obstacleSpawnLayer)
                            {
                                targetSpawner = new ObstacleSpawner(this, obstacleSpawnLayer, _raceStatus);
                            }
                            else
                            {
                                targetSpawner = new PrapSpawner(this, layer, _raceStatus);
                            }
                            
                            _autoSpawners.TryAdd(layer, targetSpawner);
                        }
                    }
                }
            }
            
            base.Initialize(datas);
        }

        public SpawnLayer GetDefaultLayer(EPrapType type)
        {
            if (_defaultLayers.TryGetValue(type, out SpawnLayer target))
            {
                return target;
            }

            return null;
        }
        
        public Prap CreatePrap(PrapData data, Vector3 position, Transform parent = null)
        {
            if (data is null) return null;
            
            Prap prap = null;
            GameObject prapObj = null;
            if (parent is null)
            {
                parent = GetDefaultLayer(data.Type)?.transform;
            }
            
            parent ??= _defaultParent;

            _prapCache.TryGetValue(data, out prap);
            if (prap is null)
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
            clone.transform.SetParent(parent);
            clone.name = data.displayName;
            
            clonePrap.PrevPosition = prapPostion;
            clone.SetActive(true);
            
            if (clonePrap is not Character && !_activePraps.TryAdd(prapPostion, clonePrap))
            {
                if (_activePraps.TryGetValue(prapPostion, out Prap origin))
                {
                    if (origin is null || origin == clonePrap)
                    {
                        _activePraps[prapPostion] = clonePrap;
                        return clonePrap;
                    }
                }
                
                Debug.LogWarning($"[Warning] 프랍({clonePrap.name})의 위치({prapPostion})가 중복이 되어 제거합니다.");
                DestroyPrap(clonePrap);
                return null;
            }
            
            return clonePrap;
        }

        public void DestroyPrap(Prap prapObj)
        {
            if (prapObj is null || prapObj.prapData is null) return;
            if (_prapCache.TryGetValue(prapObj.prapData, out Prap _))
            {
                _resourceManager?.Destroy(prapObj.gameObject);
                _activePraps?.Remove(prapObj.transform.position);
            }
        }

        public override void Tick()
        {
            foreach (PrapSpawner spawner in _autoSpawners.Values)
            {
                spawner.Tick(_raceStatus.TravelDistance);
            }
        }

        public void FixedPrapPosition(Prap target, Vector3 prevPosition)
        {
            if (_activePraps.TryGetValue(prevPosition, out Prap prap) && prap == target)
            {
                _activePraps.Remove(prevPosition);
                if (!_activePraps.TryAdd(target.transform.position, target))
                {
                    if (_activePraps.TryGetValue(target.transform.position, out Prap origin))
                    {
                        if (origin is null || origin == target)
                        {
                            _activePraps[target.transform.position] = target;
                            return;
                        }
                    }
                    
                    Debug.LogWarning($"[Warning] 프랍({target.name})의 위치({target.transform.position})가 중복이 되어 제거합니다.");
                    DestroyPrap(target);
                }
            }
        }

        public void TestSpawnLayer()
        {
            foreach (PrapSpawner spawner in _autoSpawners.Values)
            {
                spawner.Tick(_raceStatus.TravelDistance);
            }
        }
    }
}