using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Panels.Base;
using UnityEngine;
using UnityEngine.UI;

namespace ManagerSystem
{
    public class CanvasManager : MonoBehaviour
    {
        private Dictionary<string, CanvasUI> uiDict = new Dictionary<string, CanvasUI>();

        private Canvas canvas;

        private static CanvasManager instance;

        public static CanvasManager Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.LogError("[Manager Error] CanvasManager is null. Please check the inspector.");
                }

                return instance;
            }
        }

        [SerializeField] private int baseDepth = 100;
        [SerializeField] private int gap = 10;

        public RectTransform MainRect { get; private set; }
        public int UICount => uiDict.Count;

        protected virtual void Awake()
        {
            // 존재하는 캔버스와 RectTransform 가져오기
            MainRect = GetComponent<RectTransform>();
            canvas = GetComponent<Canvas>();

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

        /// <summary>
        /// 새로운 CanvasUI를 생성하고, 리스트에 추가
        /// </summary>
        /// <param name="uiName">UI이름</param>
        /// <param name="infos">세팅이 필요한 정보</param>
        /// <typeparam name="T">UI 컴포넌트</typeparam>
        /// <returns>생성된 UI</returns>
        public T AddCanvasUI<T>(string uiName = null, [CanBeNull] params object[] infos) where T : CanvasUI
        {
            // uiName이 Null이라면, 컴포넌트 타입의 이름으로 합니다.
            uiName ??= typeof(T).Name;

            // 이미 켜져 있다면, 정지
            if (uiDict.TryGetValue(uiName, out CanvasUI ui))
            {
                return null;
            }

            // 리소스에서 이름으로 UI프리팹 검색, 없다면 정지
            string uiPath = $"{BaseValues.CanvasUIDirectory}/{uiName}";
            GameObject go = Managers.Resource.Instantiate(uiPath, transform);
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
            rect.SetParent(transform, false);

            // 캔버스에서 해당 UI의 깊이 설정
            int depth = baseDepth + (uiDict.Count * gap);
            canvasUI.SetUIDepth(depth);

            // UI목록에 추가 
            uiDict.Add(uiName, canvasUI);

            // 만약 설정해야 하는 정보가 있다면, 정보를 UI에 등록 
            if (infos != null)
            {
                canvasUI.SetInfoInPanel(infos);
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
        public void RemoveCanvasUI(string uiName)
        {
            if (uiDict.Count == 0) return;

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
        public T GetUIOrCreateUI<T>(string uiName = null, [CanBeNull] params object[] infos) where T : CanvasUI
        {
            T ui = GetUI<T>(uiName);

            if (ui is null)
            {
                ui = AddCanvasUI<T>(uiName, infos);
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