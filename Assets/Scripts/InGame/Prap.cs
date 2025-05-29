using System;
using ManagerSystem;
using ManagerSystem.InGame;
using Panels;
using Panels.Base;
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
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0;
            
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 0; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);   
            }
            
            return combinedBounds.size.x;
        }
        
        public float GetStartPosWorldX()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return 0;
            
            Bounds combinedBounds = renderers[0].bounds;
            for (int i = 0; i < renderers.Length; i++)
            {
                combinedBounds.Encapsulate(renderers[i].bounds);   
            }

            float leftX = combinedBounds.min.x;
            return leftX;
        }

        public float GetStartPosLocalX()
        {
            float x = GetStartPosWorldX();
            Vector3 localPos = transform.InverseTransformPoint(new Vector3(x, 0, 0));
            return localPos.x;
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

        protected virtual void FixedUpdate()
        {
            if (transform.hasChanged)
            {
                OnChangedPosition();
            }
            
            float destroyedLineX = ScreenScaler.ONDESTROYED_POSX;
            if (transform.position.x > destroyedLineX)
            {
                OnDestroy();
            }
        }

        public void OnDestroy()
        {
            Managers.InGame?.Prap?.DestroyPrap(this);
        }
    }
}