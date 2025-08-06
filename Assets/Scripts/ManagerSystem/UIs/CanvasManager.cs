using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UIs.Base;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerSystem.UIs
{
    public class CanvasManager : MonoBehaviour
    {
        private Dictionary<string, CanvasUI> uiDict = new Dictionary<string, CanvasUI>();
        private Dictionary<string, int> uiCountDict = new Dictionary<string, int>();

        private Canvas canvas;
        private SafeArea safeArea;
        private Transform _defaultParent;       // 생성되는 CanvasUI 디폴트 부모

        private static CanvasManager instance;

        public static CanvasManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogWarning("[Manager Error] CanvasManager is null. Please check the inspector.");
                }

                return instance;
            }
        }

        [SerializeField] private int baseDepth = 100;
        [SerializeField] private int gap = 10;

        public RectTransform MainRect { get; private set; }
        public int UICount => uiDict.Count;

        protected virtual void Start()
        {
            // 존재하는 캔버스와 RectTransform 가져오기
            MainRect = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();

            // SafeArea 설정
            RefreshSafeArea();

            // 디폴트 부모 설정
            _defaultParent = safeArea.transform;

            // SafeArea 아래로 하위 오브젝트 정렬
            List<Transform> childTr = new();
            foreach (Transform child in transform)
            {
                if (child == safeArea.transform) continue;
                childTr.Add(child);
            }

            foreach (Transform child in childTr)
            {
                child.SetParent(safeArea.transform, false);
            }

            // Scaler 조절 
            CanvasScaler canvasScaler = GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                // 스크린 사이즈에 맞게 스케일 조절
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                // 정해진 스크린 비율에 맞게 조절
                canvasScaler.referenceResolution = new Vector2(BaseValues.SCREEN_WIDTH, BaseValues.SCREEN_HEIGHT);
                // 스크린에 띄울 때에 확장 시켜서 채움
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }

            // 활성화 되어 있는 내부 캔버스UI를 가져옴
            CanvasUI[] uiList = GetComponentsInChildren<CanvasUI>(true);

            foreach (CanvasUI ui in uiList)
            {
                uiDict.Add(ui.name, ui);
            }

            instance = this;
        }

        [ContextMenu("RefreshSafeArea")]
        public void RefreshSafeArea()
        {
            // SafeArea 가져오기
            if (safeArea == null)
            {
                safeArea = transform.GetComponentInChildren<SafeArea>();

                if (safeArea == null)
                {
                    GameObject go = new GameObject("SafeArea", typeof(SafeArea), typeof(RectTransform));
                    RectTransform rt = go.GetComponent<RectTransform>();
                    safeArea = go.GetComponent<SafeArea>();

                    go.transform.SetParent(transform);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.anchorMin = new Vector2(0f, 0f);
                    rt.anchorMax = new Vector2(1f, 1f);

                    MainRect = rt;
                }
            }

            safeArea.SetSafeArea();
        }

        /// <summary>
        /// 새로운 CanvasUI를 생성하고, 리스트에 추가
        /// </summary>
        /// <param name="uiName">UI이름</param>
        /// <param name="infos">세팅이 필요한 정보</param>
        /// <typeparam name="T">UI 컴포넌트</typeparam>
        /// <returns>생성된 UI</returns>
        public T AddCanvasUI<T>(string uiName = null, Transform parent = null, [CanBeNull] params object[] infos) where T : CanvasUI
        {
            // uiName이 Null이라면, 컴포넌트 타입의 이름으로 합니다.
            uiName ??= typeof(T).Name;

            // 리소스에서 이름으로 UI프리팹 검색, 없다면 정지
            string uiPath = $"{BaseValues.CanvasUIDirectory}/{uiName}";
            parent ??= _defaultParent;
            GameObject go = Managers.Resource.Instantiate(uiPath, parent);
            if (go is null)
            {
                Debug.LogWarning($"[Canvas warn] Fail to Instantiate : {uiName}");
                return null;
            }

            // 이름 설정
            go.name = uiName;

            // 내부에 캔버스 캔버스UI를 불러와 Open
            CanvasUI canvasUI = go.GetComponent<CanvasUI>();
            canvasUI.Open();

            // 캔버스 메니저 하위로 옮김
            RectTransform rect = go.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            rect.SetParent(parent, false);
            rect.sizeDelta = size;

            // 캔버스에서 해당 UI의 깊이 설정
            int depth = baseDepth + (uiDict.Count * gap);
            canvasUI.SetUIDepth(depth);

            // uiCountDict에서 uiName으로 카운트 가져오기
            int uiCount = 1;
            if (uiCountDict.TryGetValue(uiName, out uiCount))
            {
                // 카운트가 있다면, 카운트 증가
                uiCount++;
                uiCountDict[uiName] = uiCount;
            }
            else
            {
                // 없다면, 새로 추가
                uiCountDict.Add(uiName, 1);
            }

            // UI목록에 추가 
            if (!uiDict.TryAdd(uiName, canvasUI))
            {
                // $"{uiName}{(카운트 + 1)}"을 키값으로 uiDict에 ui 추가
                string key = $"{uiName}@{uiCount}";
                if (!uiDict.ContainsKey(key))
                {
                    uiDict.Add(key, canvasUI);
                }
            }

            // 만약 설정해야 하는 정보가 있다면, 정보를 UI에 등록 
            if (infos != null)
            {
                canvasUI.SetInfoInUI(infos);
            }

            // UI의 RectTransform 설정 
            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // UI 활성화 후, 실행되어야 하는 세팅 실시
            go.SetActive(true);
            canvasUI.CallAfterSetting();

            return canvasUI as T;
        }

        /// <summary>
        /// Canvas UI를 제거
        /// </summary>
        /// <param name="uiName">제거할 UI 이름</param>
        public void RemoveCanvasUI(CanvasUI uiObj)
        {
            if (uiDict.Count == 0) return;
            uiObj?.Close();
        }

        /// <summary>
        /// Canvas UI를 제거
        /// </summary>
        /// <param name="uiName">제거할 UI 이름</param>
        public void RemoveCanvasUI(string uiName)
        {
            if (uiDict.Count == 0) return;

            if (uiCountDict.TryGetValue(uiName, out int uiCount))
            {
                uiName = $"{uiName}@{uiCount}";
            }

            if (uiDict.TryGetValue(uiName, out CanvasUI ui))
            {
                ui?.Close();    // BaseUI의 Close에서 CanvasManager.ReleaseUI(this)를 실행
            }
        }

        /// <summary>
        /// 모든 Canvas UI를 제거
        /// </summary>
        public void RemoveAllUIs()
        {
            List<CanvasUI> tempList = new List<CanvasUI>(uiDict.Values);
            foreach (var ui in tempList)
            {
                ui.Close();     // BaseUI의 Close에서 CanvasManager.ReleaseUI(this)를 실행
            }
            uiDict.Clear();
        }

        /// <summary>
        /// UI를 실제로 종료하고, 남아있는 Canvas UI들의 Sorting을 재정리
        /// </summary>
        /// <param name="baseUI">종료하는 UI 컴포넌트</param>
        public void ReleseUI(CanvasUI baseUI)
        {
            if (baseUI is null) return;

            // UI 리스트에서 해당하는 BaseUI 객체를 가져옴 (첫 번째 값)
            var uiData = uiDict.FirstOrDefault(data => data.Value.Equals(baseUI));

            if (uiData.Key == null)
            {
                Debug.LogWarning($"Fail to get from open ui list : {baseUI}");
                return;
            }
            // UI 리스트에서 해당 키를 제거
            uiDict.Remove(uiData.Key);

            // 제거된 만큼 카운트 감소
            // 만약 키값에 @숫자가 있다면, 해당 숫자만 가져와서 uiCountDict을 업데이트
            string key = uiData.Key;
            if (key.Contains('@'))
            {
                string uiName = key.Split('@')[0];
                if (uiCountDict.TryGetValue(uiName, out int uiCount))
                {
                    uiCount--;
                    if (uiCount <= 0)
                    {
                        uiCountDict.Remove(uiName);
                    }
                    else
                    {
                        uiCountDict[uiName] = uiCount;
                    }
                }
            }
            else
            {
                // 키값에 @숫자가 없다면, 1개 이므로 그냥 제거
                uiCountDict.Remove(key);
            }

            // 빠지는 UI들로 인하여 UI Sorting 재조정
            foreach (var ui in uiDict)
            {
                // 제거될 uiData보다 더 Depth가 큰 UI를 한 단계 위로 올림
                if (ui.Value.CanvasUIDepth > uiData.Value.CanvasUIDepth)
                {
                    ui.Value.SetUIDepth(ui.Value.CanvasUIDepth - gap);
                }
            }
            // UI 객체 제거
            Managers.Resource.Destroy(baseUI.gameObject);
        }

        /// <summary>
        /// Canvas 내에서 이름으로 UI를 가져옴
        /// </summary>
        /// <param name="uiName">UI 이름</param>
        /// <typeparam name="T">UI 컴포넌트 타입</typeparam>
        /// <returns>해당하는 UI</returns>
        public T GetUI<T>(string uiName = null) where T : CanvasUI
        {
            // uiName이 Null인 경우 T 타입 이름으로 검색
            uiName ??= typeof(T).Name;

            if (uiCountDict.TryGetValue(uiName, out int uiCount))
            {
                // uiCount가 1보다 크면, @숫자를 붙여서 검색
                if (uiCount > 1)
                {
                    uiName = $"{uiName}@{uiCount}";
                }
            }

            if (uiDict.TryGetValue(uiName, out CanvasUI ui))
            {
                return ui.GetComponent<T>();
            }

            return null;
        }

        /// <summary>
        /// UI를 가져오는데 없다면, 새로 생성하여 가져옴 
        /// </summary>
        /// <param name="uiName">UI이름</param>
        /// <param name="infos">UI에 설정할 데이터들</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetUIOrCreateUI<T>(string uiName = null, Transform parent = null, [CanBeNull] params object[] infos) where T : CanvasUI
        {
            T ui = GetUI<T>(uiName);

            if (ui is null)
            {
                ui = AddCanvasUI<T>(uiName, parent, infos);
            }

            return ui;
        }

        /// <summary>
        /// Canvas 내에서 이름으로 CanvasUI를 가져옴
        /// </summary>
        /// <param name="uiName">UI 이름</param>
        /// <returns>해당하는 UI</returns>
        public CanvasUI GetUI(string uiName)
        {
            if (uiCountDict.TryGetValue(uiName, out int uiCount))
            {
                // uiCount가 1보다 크면, @숫자를 붙여서 검색
                if (uiCount > 1)
                {
                    uiName = $"{uiName}@{uiCount}";
                }
            }

            if (uiDict.TryGetValue(uiName, out CanvasUI ui))
            {
                return ui.GetComponent<CanvasUI>();
            }

            return null;
        }

        /// <summary>
        /// Canvas 내에서 타입으로 UI를 가져옴 
        /// </summary>
        /// <typeparam name="T">UI 컴포넌트 타입</typeparam>
        /// <returns>해당하는 UI</returns>
        public T GetUIFromType<T>() where T : CanvasUI
        {
            foreach (var ui in uiDict)
            {
                if (ui.Value is T)
                {
                    return ui.Value as T;
                }
            }
            return null;
        }
    }
}