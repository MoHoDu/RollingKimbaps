using UnityEngine;
using System;
using UnityEngine.UI;
using ManagerSystem.UIs;

namespace UIs.Base
{
    [RequireComponent(typeof(Canvas), typeof(GraphicRaycaster))]
    public class CanvasUI : BindUI
    {
        public int CanvasUIDepth { get; private set; }
        public Action OnClosedEvent { get; set; } = null;

        private Canvas canvasComponent;

        public virtual void Open() { }

        public virtual void Close()
        {
            // UI매니저로 하여금 이 UI를 끄도록 시킴
            CanvasManager.Instance.ReleseUI(this);

            // Closed 시에 발생되는 이벤트 실행
            OnClosedEvent?.Invoke();
            OnClosedEvent = null;
        }

        public virtual void SetInfoInUI(params object[] infos) { }

        public void SetUIDepth(int depth)
        {
            SetCanvasComponent();
            if (canvasComponent == null)
            {
                Debug.LogError($"[Canvas panel error] Not found Canvas component in {gameObject.name}");
                return;
            }

            canvasComponent.overrideSorting = true;
            canvasComponent.sortingOrder = depth;
            CanvasUIDepth = depth;
        }

        public virtual void CallAfterSetting() { }

        private void SetCanvasComponent()
        {
            if (canvasComponent == null) canvasComponent = GetComponent<Canvas>();
        }
    }
}