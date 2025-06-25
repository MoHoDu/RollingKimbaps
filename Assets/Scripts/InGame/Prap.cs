using System;
using ManagerSystem;
using ManagerSystem.InGame;
using UIs;
using UIs.Base;
using UnityEngine;

namespace InGame
{
    public class Prap : BindUI
    {
        public PrapData prapData;
        public Vector3 PrevPosition = new Vector3(0, 100, 0);
        
        public virtual void OnSpawned(params object[] args)
        {
            OnChangedPosition();
        }

        public float GetWidth()
        {
            Bounds? allBounds = GetAllBounds();
            if (allBounds.HasValue && allBounds.Value != null)
            {
                return GetAllBounds().Value.size.x;
            }
            return 0f;
        }
        
        public float GetLeftPosWorldX()
        { 
            Bounds? allBounds = GetAllBounds();
            if (allBounds.HasValue && allBounds.Value != null)
            {
                return allBounds.Value.min.x;
            }
            return transform.position.x;
        }
        
        public float GetRightPosWorldX()
        { 
            Bounds? allBounds = GetAllBounds();
            if (allBounds.HasValue && allBounds.Value != null)
            {
                return allBounds.Value.max.x;
            }
            return transform.position.x;
        }

        public float GetLeftPosLocalX(Transform parent)
        {
            Bounds? allBounds = GetAllBounds();
            if (allBounds.HasValue && allBounds.Value != null)
            {
                Vector3 localLeft = parent.InverseTransformPoint(allBounds.Value.min);
                return localLeft.x;
            }
            
            return transform.position.x;
        }
        
        public float GetRightPosLocalX(Transform parent)
        {
            Bounds? allBounds = GetAllBounds();
            if (allBounds.HasValue && allBounds.Value != null)
            {
                Vector3 locaRight = parent.InverseTransformPoint(allBounds.Value.max);
                return locaRight.x;
            }
            
            return transform.position.x;
        }
        
        public float GetPivotToLeftEdgeOffsetLocalX()
        {
            Bounds? bounds = GetAllBounds();
            if (!bounds.HasValue || bounds.Value == null)
                return 0;

            Vector3 worldLeft = bounds.Value.min;
            Vector3 worldPivot = transform.position;

            Vector3 localLeft = transform.InverseTransformPoint(worldLeft);
            Vector3 localPivot = transform.InverseTransformPoint(worldPivot); // usually (0,0,0)

            return localPivot.x - localLeft.x;
        }
        
        public float GetPivotToRightEdgeOffsetLocalX()
        {
            Bounds? bounds = GetAllBounds();
            if (!bounds.HasValue || bounds.Value == null)
                return 0;

            Vector3 worldRight = bounds.Value.max;
            Vector3 worldPivot = transform.position;

            Vector3 localRight = transform.InverseTransformPoint(worldRight);
            Vector3 localPivot = transform.InverseTransformPoint(worldPivot); // usually (0,0,0)

            return localRight.x - localPivot.x;
        }

        public void OnChangedPosition()
        {
            if (this is Character) return;
            
            if (transform.position != PrevPosition)
            {
                Managers.InGame?.Prap?.FixedPrapPosition(this, PrevPosition);
                PrevPosition = transform.position;
            }
        }

        protected Bounds? GetAllBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 0; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);
            }
            
            return combinedBounds;
        }

        protected virtual void FixedUpdate()
        {
            if (transform.hasChanged)
            {
                OnChangedPosition();
            }
            
            float destroyedLineX = ScreenScaler.ONDESTROYED_POSX;
            if (transform.position.x < destroyedLineX)
            {
                OnDestroy();
            }
        }

        public virtual void OnDestroy()
        {
            Managers.InGame?.Prap?.DestroyPrap(this);
        }
    }
}