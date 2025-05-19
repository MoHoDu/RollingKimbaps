using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ManagerSystem;
using Panels.Base;
using UnityEngine;
using Utils;

namespace Panels
{
    public class GroundsPanel : BindUI
    {
        protected List<GroundUI> grounds = new List<GroundUI>();
        
        protected string _groundObjName = "Ground";
        protected int _instantiateCount = 10;
        protected float _onDestroyedPosX = -100f;
        
        protected float _instantiateSpace = 14.5f;
        protected float _instantiateSpaceGap = 5.8f;
        protected int _minTileCount = 5;
        protected int _maxTileCount = 30;
        
        protected float _startPointX = -25f;
        protected int _startPointWidth = 30;

        protected bool _playGame = true;
        protected Coroutine _coroutine;
        
        protected GroundBuilder _groundBuilder = new GroundBuilder();
        
        [Range(0, 1)] public float Hardness { get; set; } = 0f;

        protected override void Initialize()
        {
            base.Initialize();
            
            // 처음 시작 지점의 평지 생성 
            AddGround(_startPointWidth);

            _coroutine = StartCoroutine(DestroyAndInstantiateGrounds());
        }

        protected void AddGround(int tileCount)
        {
            if (grounds.Count > 1) SortGrounds();

            float space = Random.Range(_instantiateSpace, _instantiateSpace + _instantiateSpaceGap);
            
            GroundUI lastGround = grounds.LastOrDefault();
            Vector3 newGroundPos = Vector3.right * _startPointX;
            if (lastGround != null)
            {
                Vector3 lastGroundEndPos = lastGround.transform.localPosition + (Vector3.right * lastGround.GroundWidth);
                newGroundPos = lastGroundEndPos + (Vector3.right * space);
            }
            
            _groundBuilder.InitBuilder(transform);
            GroundUI newGround = _groundBuilder.SetGroundWidth(tileCount).SetLocalPosition(newGroundPos).Build();
            newGround.name = _groundObjName;
            
            if (newGround != null) grounds.Add(newGround);
        }

        private void SortGrounds()
        {
            grounds.Sort((b1, b2) => b1.transform.position.x.CompareTo(b2.transform.position.x));
        }
        
        protected virtual IEnumerator DestroyAndInstantiateGrounds()
        {
            while (_playGame)
            {
                GroundUI[] destroyedGrounds =
                    grounds.Where(t => t.transform.position.x < _onDestroyedPosX).ToArray();

                foreach (var ground in destroyedGrounds)
                {
                    grounds.Remove(ground);
                    Managers.Resource.Destroy(ground.gameObject);
                }

                SortGrounds();
                float? _postX = grounds
                    .LastOrDefault()?
                    .transform.localPosition.x;
                while (grounds.Count < _instantiateCount)
                {
                    if (!_postX.HasValue) _postX = 0f;
                    
                    AddGround(GetRandomTileCount());
                    
                    SortGrounds();
                    _postX = grounds
                        .LastOrDefault()?
                        .transform.localPosition.x;
                }

                yield return new WaitForSeconds(0.5f);
            }
        }

        private int GetRandomTileCount()
        {
            (int, int) longTile = (15, _maxTileCount);
            (int, int) smallTile = (_minTileCount, 14);

            bool isLong = Random.Range(0f, 1f) >= Hardness;
            int tileCount = _minTileCount;
            
            if (isLong)
            {
                tileCount = Random.Range(longTile.Item1, longTile.Item2);
            }
            else
            {
                tileCount = Random.Range(smallTile.Item1, smallTile.Item2);
            }
            
            return tileCount;
        }
    }
}