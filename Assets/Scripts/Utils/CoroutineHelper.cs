using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Utils
{
    public class CoroutineHelper : MonoBehaviour
    {
        private static CoroutineHelper _instance;
        private static List<Coroutine> _activeCoroutines = new List<Coroutine>();
        
        public static Coroutine StartNewCoroutine(IEnumerator routine)
        {
            if (_instance == null)
            { 
                GameObject go = new GameObject("CoroutineHelper");
                _instance = go.AddComponent<CoroutineHelper>();
            }
            
            Coroutine coroutine = _instance.StartCoroutine(routine);
            _activeCoroutines.Add(coroutine);
            return coroutine;
        }

        private void Update()
        {
            if (_activeCoroutines.Count > 0)
            {
                _activeCoroutines.RemoveAll(c => c == null);
            }
            
            if (_activeCoroutines.Count <= 0)
            {
                _instance = null;
                Destroy(gameObject);
            }
        }
    }
}