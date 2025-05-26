using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Panels.Base;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ManagerSystem
{
    public class StageManager : BaseManager
    {
        // 씬 전환 시점 이벤트
        private Dictionary<string, Action<float>> _onSceneLoading = new Dictionary<string, Action<float>>();
        private Action<float> _onSceneLoadingAll;
        
        // 씬 전환 이후 이벤트
        private Dictionary<string, Action<object>> _onSceneOpened = new Dictionary<string, Action<object>>();
        private Action _onSceneChangedAll;

        /// <summary>
        /// 비동기로 씬 로드
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <param name="data">씬 전환 이후 이벤트 함수 파라미터로 전달할 데이터</param>
        public void LoadSceneAsync(string sceneName, object data = null)
        {
            LoadSceneAsyncCoroutine(sceneName, data).Forget();
        }

        // 비동기로 씬을 로드하는 함수
        private async UniTaskVoid LoadSceneAsyncCoroutine(string sceneName, object data = null)
        {
            // 비동기 씬 로딩 시작
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
            
            // 씬이 존재하지 않은 경우 종료
            if (asyncLoad == null)
            {
                Debug.LogWarning($"[ERROR] 씬 전환 중 에러가 발생했습니다. {sceneName}에 해당하는 씬을 찾을 수 없습니다.");
                return;
            }
            
            // 씬이 자동으로 활성화 되지 않도록 설정
            asyncLoad.allowSceneActivation = false;
            
            // 로딩 진행률 업데이트
            while (!asyncLoad.isDone)
            {
                // 진행률 (0 ~ 0.9f)
                float progress = Mathf.Clamp01(asyncLoad.progress / .9f);
                
                // 이벤트 실행
                _onSceneLoadingAll?.Invoke(progress);
                _onSceneLoading[sceneName]?.Invoke(progress);
                
                // 로딩이 90% 완료된 상황
                if (asyncLoad.progress >= .9f)
                {
                    // 1초 대기 이후 100% 처리
                    await UniTask.WaitForSeconds(1);

                    progress = 1f;
                    
                    // 이벤트 실행
                    _onSceneLoadingAll?.Invoke(progress);
                    _onSceneLoading[sceneName]?.Invoke(progress);
                    
                    // 이벤트 실행 대기
                    await UniTask.WaitForSeconds(1);
                    
                    // 씬 활성화
                    asyncLoad.allowSceneActivation = true;
                    
                    // 씬 전환 이후 이벤트 호출
                    _onSceneChangedAll?.Invoke();
                    _onSceneOpened[sceneName]?.Invoke(data);
                }
            }
        }

        /// <summary>
        /// 특정 씬 전환 중에 실행될 이벤트 함수 등록
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <param name="action">이벤트</param>
        public void AddEventOnSceneLoading(string sceneName, Action<float> action)
        {
            if (_onSceneLoading.TryGetValue(sceneName, out Action<float> onLoadingSceneEvent))
            {
                onLoadingSceneEvent -= action;
                onLoadingSceneEvent += action;
            }
            else
            {
                _onSceneLoading.Add(sceneName, action);
            }
        }
        
        /// <summary>
        /// 모든 씬 전환 중 실행될 이벤트 함수 등록
        /// </summary>
        /// <param name="action">이벤트</param>
        public void AddEventOnSceneLoadingAll(Action<float> action)
        {
            _onSceneLoadingAll -= action;
            _onSceneLoadingAll += action;
        }
        
        /// <summary>
        /// 특정 씬 전환 완료 후 실행될 이벤트 함수 등록
        /// </summary>
        /// <param name="sceneName">씬 이름</param>
        /// <param name="action">이벤트</param>
        public void AddEventAfterSceneOpened(string sceneName, Action<object> action)
        {
            if (_onSceneOpened.TryGetValue(sceneName, out Action<object> onOpenedSceneEvent))
            {
                onOpenedSceneEvent -= action;
                onOpenedSceneEvent += action;
            }
            else
            {
                _onSceneOpened.Add(sceneName, action);
            }
        }

        /// <summary>
        /// 모든 씬 전환 완료 후 실행될 이벤트 함수 등록
        /// </summary>
        /// <param name="action">이벤트</param>
        public void AddEventOnSceneChanged(Action action)
        {
            _onSceneChangedAll -= action;
            _onSceneChangedAll += action;
        }
        
        /// <summary>
        /// 이름을 가지고 게임오브젝트를 씬에서 찾거나 새로 생성
        /// </summary>
        /// <param name="name">오브젝트 명</param>
        /// <returns>찾은 게임오브젝트</returns>
        public GameObject FindObjectOrCreate(string name)
        {
            GameObject obj = GameObject.Find(name);
            obj ??= new GameObject(name);
            return obj;
        }
        
        public SpawnLayer[] FindSpawnLayers()
        {
            return Managers.UI.GetComponentsFromPanel<SpawnLayer>();
        }
        
        public FlowLayer[] FindFlowLayers()
        {
            return Managers.UI.GetComponentsFromPanel<FlowLayer>();
        }
    }
}