﻿using UnityEngine;

namespace Panels.Base
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour 
    {
        private RectTransform safeAreaRect;
        private Canvas canvas;
        private Rect lastSafeArea;

        [ContextMenu("SetArea")]
        public void SetSafeArea() {
            safeAreaRect = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            OnRectTransformDimensionsChange();
        }

        private void OnRectTransformDimensionsChange() {

            if (GetSafeArea() != lastSafeArea && canvas != null) {
                lastSafeArea = GetSafeArea();
                UpdateSizeToSafeArea();
            }
        }

        private void UpdateSizeToSafeArea() {

            var safeArea = GetSafeArea();
            var inverseSize = new Vector2(1f, 1f) / canvas.pixelRect.size; 
            var newAnchorMin = Vector2.Scale(safeArea.position, inverseSize);
            var newAnchorMax = Vector2.Scale(safeArea.position + safeArea.size, inverseSize);

            safeAreaRect.anchorMin = newAnchorMin;
            safeAreaRect.anchorMax = newAnchorMax;

            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
        }

        private Rect GetSafeArea() {
#if UNITY_EDITOR && UNITY_STANDALONE
            return new Rect(132, 102, 2172, 1023);
#else
            return Screen.safeArea;
#endif
        } 
    }
}