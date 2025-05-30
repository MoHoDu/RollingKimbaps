using UnityEngine;

namespace Panels.Base
{
    public class FlowLayer : BindUI
    {
        [SerializeField] private float gap = 1f;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float elapsed = 0f;
        private bool isMoving = false;
        private float tickDuration = 1f;

        protected override void Initialize()
        {
            startPosition = transform.localPosition;
            targetPosition = transform.localPosition;
        }

        public void SetDuration(float timer)
        {
            tickDuration = timer > 0 ? timer : 1f;
        }
        
        public virtual void Flow(float tickDistance)
        {
            // 앞으로 1초 안에 targetDistance만큼 우측으로 position을 움직여야 함
            // 단, 다음 Tick()과 자연스럽게 연결되도록 해야 함
            
            // 이동 시작점과 목표점 계산
            startPosition = transform.localPosition;
            targetPosition = startPosition - Vector3.right * (tickDistance * gap);
            elapsed = 0f;
            isMoving = true;
        }

        private void Update()
        {
            if (isMoving)
            {
                // deltaTime만큼 elapsed에 더함
                elapsed += Time.deltaTime;
                // tickDuration 대비 elapsed의 %를 0~1로 변환
                float t = Mathf.Clamp01(elapsed / tickDuration);
                // 실제 위치를 시작 위치와 타겟 위치의 t 비율만큼 이동
                transform.localPosition = Vector3.Lerp(startPosition, targetPosition, t);
                if (t >= 1f)
                {
                    isMoving = false;
                }
            }
        }
    }
}