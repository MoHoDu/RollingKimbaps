#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UIs;

namespace Editors
{
    [ExecuteInEditMode] // Editor 모드에서만 작동
    [CustomEditor(typeof(IngredientsInKimbapUI))]
    public class IngredientsInKimbapUIEditor : Editor
    {
        private IngredientsInKimbapUI ui;
        private SerializedProperty positionsProp;
        private int maxCount;
        private int currentCount = 1;
        private bool isUI = false;

        void OnEnable()
        {
            ui = (IngredientsInKimbapUI)target;
            positionsProp = serializedObject.FindProperty("Settings");

            // maxCount 초기화
            maxCount = BaseValues.MAX_COLLECTED_INGREDIENTS;
            
            // UI인지 확인 
            isUI = ui.TryGetComponent(out RectTransform _);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.LabelField("Inner Ingredient Visualizer Settings", EditorStyles.boldLabel);
            
            // SO 에셋 필드 노출
            EditorGUILayout.PropertyField(positionsProp, new GUIContent("Position SO"));

            if (positionsProp.objectReferenceValue != null)
            {
                // SO데이터 추출
                InnerPlacePositionSO so = (InnerPlacePositionSO)positionsProp.objectReferenceValue;
                
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

                // so 데이터 초기화 
                InitInfoList(so);
                
                // 해당 키의 info 노출
                InnerPosition info = so.infos[currentCount];
                List<Vector3> list = info.placePositions;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = EditorGUILayout.Vector3Field($"Pos {i + 1}", list[i]);
                }
                
                if (GUI.changed)
                {
                    // Inspector에서 값 변경 시 즉시 씬 뷰 기즈모도 업데이트
                    EditorUtility.SetDirty(so);
                    SceneView.RepaintAll();
                }
            }
            else
            {
                // 메시지 노출
                EditorGUILayout.HelpBox("먼저 InnerPlacePositionSO를 할당하세요.", MessageType.Warning);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void InitInfoList(in InnerPlacePositionSO so)
        {
            // 개수가 부족한 경우 info 정보 추가
            if (so.infos.Count < maxCount)
            {
                int infosCount = so.infos.Count;
                for (int i = infosCount; i <= maxCount; i++)
                {
                    so.infos.Add(new InnerPosition(i));
                }
            }
        }
        
        // 씬 뷰에서 기즈모(핸들) 그리기
        void OnSceneGUI()
        {
            if (positionsProp.objectReferenceValue != null)
            {
                // SO데이터 추출
                InnerPlacePositionSO so = (InnerPlacePositionSO)positionsProp.objectReferenceValue;
                
                // so데이터 초기화
                InitInfoList(so);
                
                // 현재 딕셔너리에서 포지션 리스트 가져오기
                InnerPosition info = so.infos[currentCount];
                List<Vector3> list = info.placePositions;

                EditorGUI.BeginChangeCheck();
                Color[] handlesColor = new Color[]
                {
                    Color.cyan, Color.green, Color.yellow, Color.red, Color.blue, Color.black, Color.gray
                };
                for (int i = 0; i < list.Count; i++)
                {
                    Vector3 pos = list[i];
                    if (isUI)
                    {
                        var rect = ui.GetComponent<RectTransform>();
                        pos = rect.TransformPoint(pos);
                    }
                    // 핸들 표시
                    Color curColor = i < handlesColor.Length ? handlesColor[i] : handlesColor[handlesColor.Length % i];
                    Handles.color = curColor;
                    // 센터 핸들
                    Vector3 handle = Handles.PositionHandle(pos, Quaternion.identity);
                    // 핸들 박스
                    float width = 1f;
                    float height = 1f;
                    // 사각형 꼭짓점 (로컬 공간)
                    float halfW = width  * 0.5f;
                    float halfH = height * 0.5f;
                    Vector3[] verts;
                    if (isUI)
                    {
                        halfW *= 30f;
                        halfH *= 30f;
                        verts = new Vector3[4] {
                            new Vector3(pos.x - halfW, pos.y - halfH, 0),
                            new Vector3(pos.x - halfW, pos.y + halfH, 0),
                            new Vector3(pos.x + halfW, pos.y + halfH, 0),
                            new Vector3(pos.x + halfW, pos.y - halfH, 0)
                        };
                    }
                    else
                    {
                        verts = new Vector3[4] {
                            new Vector3(handle.x - halfW, handle.y - halfH, 0),
                            new Vector3(handle.x - halfW, handle.y + halfH, 0),
                            new Vector3(handle.x + halfW, handle.y + halfH, 0),
                            new Vector3(handle.x + halfW, handle.y - halfH, 0)
                        };
                    }
                    Handles.DrawSolidRectangleWithOutline(
                        verts,
                        new Color(curColor.r, curColor.g, curColor.b, 0.1f),
                        curColor
                    );
                    
                    Handles.matrix = Matrix4x4.identity;
                    
                    // 변경 감지 시 로컬 좌표로 변환하여 저장
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(ui, "Move Ingredient Position");
                        Vector3 localPos = ui.transform.InverseTransformPoint(handle);
                        list[i] = localPos;
                        info.placePositions = list;
                        EditorUtility.SetDirty(so);
                    }
                }
            }
        }
    }
}
#endif
