using GameDatas;
using UnityEngine;

namespace Panels.Base
{
    public class FlowLayer : BindUI
    {
        [SerializeField] private float gap = 1f;
        
        public virtual void Flow(float tickDistance)
        {
            float flowDistance = tickDistance * gap;
            
            transform.transform.position += Vector3.left * flowDistance * Time.deltaTime;
        }
    }
}