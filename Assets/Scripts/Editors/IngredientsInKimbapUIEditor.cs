#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Panels;

namespace Editors
{
    [CustomEditor(typeof(IngredientsInKimbapUI))]
    public class IngredientsInKimbapUIEditor : Editor
    {
        private IngredientsInKimbapUI ui;
        private SerializedProperty positionsProp;
        private int maxCount;
        private int currentCount = 1;

        void OnEnable()
        {
            ui = (IngredientsInKimbapUI)target;
            positionsProp = serializedObject.FindProperty("_ingredientPositions");

            // maxCount 초기화
            maxCount = BaseValues.MAX_COLLECTED_INGREDIENTS;

            // 딕셔너리 키-리스트 초기화 (없으면 새로 생성)
            for (int i = 1; i <= maxCount; i++)
            {
                if (!ui._ingredientPositions.ContainsKey(i))
                    ui._ingredientPositions[i] = new List<Vector3>(new Vector3[i]);
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // maxCount 표시 (읽기 전용)
            EditorGUILayout.LabelField("Max Ingredients", maxCount.ToString());

            // currentCount 슬라이더
            int newCount = EditorGUILayout.IntSlider("Current Count", currentCount, 1, maxCount);
            if (newCount != currentCount)
            {
                currentCount = newCount;
                // 기즈모 업데이트를 위해 씬 갱신
                SceneView.RepaintAll();
            }

            EditorGUILayout.Space();

            // 해당 키의 Vector3 리스트 노출
            List<Vector3> list = ui._ingredientPositions[currentCount];
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = EditorGUILayout.Vector3Field($"Pos {i+1}", list[i]);
            }

            if (serializedObject.ApplyModifiedProperties())
            {
                // Inspector에서 값 변경 시 즉시 씬 뷰 기즈모도 업데이트
                EditorUtility.SetDirty(ui);
                SceneView.RepaintAll();
            }
        }

        // 씬 뷰에서 기즈모(핸들) 그리기
        void OnSceneGUI()
        {
            // 현재 딕셔너리에서 포지션 리스트 가져오기
            if (!ui._ingredientPositions.TryGetValue(currentCount, out var list)) return;

            EditorGUI.BeginChangeCheck();
            for (int i = 0; i < list.Count; i++)
            {
                // 월드 좌표로 변환
                Vector3 worldPos = ui.transform.TransformPoint(list[i]);
                // 핸들 표시
                Vector3 newWorldPos = Handles.PositionHandle(worldPos, Quaternion.identity);
                // 변경 감지 시 로컬 좌표로 변환하여 저장
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(ui, "Move Ingredient Position");
                    Vector3 localPos = ui.transform.InverseTransformPoint(newWorldPos);
                    list[i] = localPos;
                    ui._ingredientPositions[currentCount] = list;
                    EditorUtility.SetDirty(ui);
                }
            }
        }
    }
}
#endif
