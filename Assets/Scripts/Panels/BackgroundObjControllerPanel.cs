using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using Panels.Base;
using UnityEngine;

namespace Panels
{
    public class BackgroundObjControllerPanel<T> : BindUI where T : BindUI
    {
        protected List<T> _objects = new();

        protected string _prefabPath = "";
        protected string _hierarchyName = "";
        protected int _instantiateCount = 8;
        protected float _onDestroyedPosX = -50f;
        protected float _instantiateSpace = 0f;
        protected float _instantiateSpaceGap = 0f;
        
        protected bool _playGame = true;
        protected Coroutine _coroutine;

        protected override void Initialize()
        {
            base.Initialize();
            _objects = transform.GetComponentsInChildren<T>(true)
                .OrderBy(b => b.transform.localPosition.x)
                .ToList();

            _coroutine = StartCoroutine(DestroyAndInstantiateTerrains());
        }

        protected virtual IEnumerator DestroyAndInstantiateTerrains()
        {
            while (_playGame)
            {
                T[] destroyedTerrains =
                    _objects.Where(t => t.transform.position.x < _onDestroyedPosX).ToArray();

                foreach (var terrain in destroyedTerrains)
                {
                    _objects.Remove(terrain);
                    Managers.Resource.Destroy(terrain.gameObject);
                }

                float? _postX = _objects
                    .OrderBy(x => x.transform.localPosition.x)
                    .LastOrDefault()?
                    .transform.localPosition.x;
                while (_objects.Count < _instantiateCount)
                {
                    if (!_postX.HasValue) _postX = 0f;
                    
                    GameObject terrain = Managers.Resource.Instantiate(_prefabPath, transform);
                    if (terrain == null)
                    {
                        Debug.LogWarning($"{_prefabPath} could not be instantiated.");
                        continue;
                    }
                    
                    float space = Random.Range(_instantiateSpace, _instantiateSpace + _instantiateSpaceGap);
                    Vector3 intantiatePos = Vector3.right * (_postX.Value + space);
                    terrain.transform.localPosition = intantiatePos;
                    terrain.name = _hierarchyName;
                    
                    _objects.Add(terrain.GetComponent<T>());
                    
                    _postX = intantiatePos.x;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
        
        protected void OnDestroy()
        {
            if (_coroutine != null) StopCoroutine(_coroutine);
        }
    }
}