using ManagerSystem;
using Panels;
using UnityEngine;

namespace Utils
{
    public class GroundBuilder
    {
        private GroundUI _groundUI;

        public void InitBuilder(Transform parent)
        {
            GameObject groundObj = Managers.Resource.Instantiate("Prefabs/Ground", parent);
            groundObj.transform.localPosition = Vector3.zero;
            _groundUI = groundObj.GetComponent<GroundUI>();
        }
        
        public GroundUI Build()
        {
            return _groundUI;
        }

        public GroundBuilder SetParent(Transform parent)
        {
            _groundUI?.transform?.SetParent(parent, false);
            return this;
        }

        public GroundBuilder SetPosition(Vector3 position)
        {
            if (_groundUI?.transform == null) return this;
            _groundUI.transform.position = position;
            
            return this;
        }
        
        public GroundBuilder SetLocalPosition(Vector3 position)
        {
            if (_groundUI?.transform == null) return this;
            _groundUI.transform.localPosition = position;
            
            return this;
        }

        public GroundBuilder SetGroundWidth(int tileCount)
        {
            if (_groundUI?.transform == null) return this;
            _groundUI.Setup(tileCount);
            
            return this;
        }
    }
}