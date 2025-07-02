using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Attributes;
using ManagerSystem;
using UnityEngine;

namespace UIs
{
    public class BackTreesPanel : BackgroundObjControllerPanel<TreeUI>
    {
        [Bind("FrontLine")] Transform _frontLine;
        [Bind("MiddleLine")] Transform _middleLine;
        [Bind("BackLine")] Transform _backLine;

        private Transform[] _treeParents => new[]
        {
            _frontLine,
            _middleLine,
            _backLine
        };
        private List<List<TreeUI>> _trees = new List<List<TreeUI>>();
        private string[] _prefabPostFix = new[]
        {
            "_Front",
            "_Middle",
            "_Back"
        };
        
        protected override void Initialize()
        {
            _prefabPath = "Prefabs/TreeObj";
            _hierarchyName = "TreeObj";
            _instantiateCount = 10;
            _onDestroyedPosX = -50f;
            _instantiateSpace = 5f;
            _instantiateSpaceGap = 15f;

            foreach (var _parent in _treeParents)
            {
                List<TreeUI> _lineTrees = _parent.GetComponentsInChildren<TreeUI>(true)
                    .OrderBy(b => b.transform.localPosition.x)
                    .ToList();
                _trees.Add(_lineTrees);
            }
            
            _coroutine = StartCoroutine(DestroyAndInstantiateTerrains());
        }

        protected override IEnumerator DestroyAndInstantiateTerrains()
        {
            while (_playGame)
            {
                for (int i = 0; i < _trees.Count; i++)
                {
                    List<TreeUI> trees = _trees[i];
                    if (trees == null) trees = new List<TreeUI>();
                    
                    string postfix = _prefabPostFix.Length <= i ? "" : _prefabPostFix[i];
                    Transform parent = _treeParents.Length <= i ? transform : _treeParents[i];
                    
                    TreeUI[] destroyedTrees =
                        trees.Where(t => t.transform.position.x < _onDestroyedPosX).ToArray();

                    foreach (var tree in destroyedTrees)
                    {
                        trees.Remove(tree);
                        Managers.Resource.Destroy(tree.gameObject);
                    }

                    float? _postX = trees
                        .OrderBy(x => x.transform.localPosition.x)
                        .LastOrDefault()?
                        .transform.localPosition.x;
                    while (trees.Count < _instantiateCount)
                    {
                        if (!_postX.HasValue) _postX = 0f;
                    
                        GameObject treeObj = Managers.Resource.Instantiate($"{_prefabPath}{postfix}", parent);
                        if (treeObj == null)
                        {
                            Debug.LogWarning($"{_prefabPath}{postfix} could not be instantiated.");
                            continue;
                        }
                    
                        float space = Random.Range(_instantiateSpace, _instantiateSpace + _instantiateSpaceGap);
                        Vector3 intantiatePos = Vector3.right * (_postX.Value + space);
                        treeObj.transform.localPosition = intantiatePos;
                        treeObj.name = _hierarchyName;
                    
                        trees.Add(treeObj.GetComponent<TreeUI>());
                    
                        _postX = intantiatePos.x;
                    }
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}