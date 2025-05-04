using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Panels.Base;


namespace Attributes
{
    public class Bind : Attribute
    {
        public string ObjectName;

        public Bind(string inObjectName)
        {
            ObjectName = inObjectName;
        }
    }

    public class BindAttribute
    {
        private class BindInfo
        {
            public readonly FieldInfo FieldInfo;
            public readonly Bind Attribute;

            public BindInfo(FieldInfo fieldInfo, Bind attribute)
            {
                FieldInfo = fieldInfo;
                Attribute = attribute;
            }
        }

        private static BindInfo[] _container = null;

        /// <summary>
        /// Bind Attribute를 지닌 필드값에 실제 값을 바인딩
        /// </summary>
        /// <param name="target">바인딩을 원하는 MonoBehaviour 오브젝트</param>
        public static void InstallBindings(MonoBehaviour target)
        {
            // 리플렉션을 통해서 필드값들을 가져오고, 배열로 저장 
            // Bind Attribute 타입을 가진 필드만 가져와서 FieldInfo와 Bind 컴포넌트를 BuildInfo로 저장 -> 배열
            _container = target.GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.IsDefined(typeof(Bind), false))
                .Select(p => new BindInfo(p, GetBindAttributes(p)))
                .ToArray();

            // 아무런 필드값이 없다면, 정지
            if (_container == null)
                return;

            // 바인딩을 위해 잠시 생성하는 딕셔너리 - 하위 Transform 객체들을 담을 예정
            Dictionary<string, Transform> cachedChildTransform = new Dictionary<string, Transform>();

            // 메서드 내에서 캐시된 Transform 객체들을 담음
            target.transform.CachedChildTransform(cachedChildTransform);

            // (BindInfo배열인)컨테이너를 돌면서 바인딩
            foreach (var item in _container)
            {
                // 캐시된 Transform 객체 중에서 이름으로 탐색
                if (cachedChildTransform.TryGetValue(item.Attribute.ObjectName, out var outTransform) == true)
                {
                    // GameObject라면, FieldInfo에 Value로 해당 GameObject 등록
                    if (item.FieldInfo.FieldType == typeof(GameObject))
                    {
                        item.FieldInfo.SetValue(target, outTransform.gameObject);
                    }
                    else
                    {
                        // 배열이라면, 배열 내부 항목들의 타입을 확인한 뒤에 해당 타입으로 자식 오브젝트들을 탐색해서 그 배열 값을 FieldInfo의 Value로 저장
                        if (item.FieldInfo.FieldType.IsArray)
                        {
                            Type type = item.FieldInfo.FieldType.GetElementType();
                            Component[] components = outTransform.GetComponentsInChildren(type, true);
                            Array filledArray = Array.CreateInstance(type, components.Length);
                            Array.Copy(components, filledArray, components.Length);
                            item.FieldInfo.SetValue(target, filledArray);
                        }
                        // 그 외에 해당하는 Field의 타입으로 컴포넌트를 탐색하고, 있다면 그 컴포넌트를 FieldInfo의 Value로 저장
                        else
                        {
                            var component = outTransform.GetComponent(item.FieldInfo.FieldType);
                            if (component == null)
                            {
                                continue;
                            }

                            item.FieldInfo.SetValue(target, component);
                        }
                    }

                    // 만약 해당 Transform이 BindUI 컴포넌트를 들고 있다면, 
                    if (outTransform.TryGetComponent<BindUI>(out var bindBase))
                    {
                        // 하위에 있는 또 다른 BindUI 먼저 Binding
                        // 이래야 부모 BindUI에서 Initialize() 등에서 필드값을 가지고 무언가를 할 때에 에러가 나지 않음
                        bindBase.InstallBindings();
                    }
                }
            }

            // 바인딩 완료 여부를 알리고, 캐시 데이터는 제거
            Debug.Log($"Binding installed for {target.name}");
            cachedChildTransform.Clear();
        }

        /// <summary>
        /// 특정 필드에서 Bind라는 커스텀 Attribute값을 가져옴
        /// </summary>
        /// <param name="fieldInfo">Bind를 지닌 필드</param>
        /// <returns>해당 필드의 Bind 값</returns>
        private static Bind GetBindAttributes(FieldInfo fieldInfo)
        {
            return fieldInfo.GetCustomAttributes(typeof(Bind), false).FirstOrDefault() as Bind;
        }
    }
}