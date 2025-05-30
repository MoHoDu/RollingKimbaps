using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Panels;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameDatas
{
    [Serializable]
    public class RaceStatus
    {
        public float Velocity { get; private set; }
        public float Time { get; private set; }
        public float TravelDistance { get; private set;}    // 총 이동 거리 
        public float TickDistance => Velocity * TickTime;  // 틱(1초)동안의 이동 거리
        public float MaxVelocity => _maxVelocity;
        
        // 계산을 위한 값들 
        private readonly float _startVelocity = 5f; 
        private readonly float _maxVelocity = 20f;
        private readonly float _addedVelocity = 0.2f;
        
        // DI
        public float TickTime { get; private set; } = 1f;

        public RaceStatus()
        {
            Initialize();
        }
        
        public void Initialize(float tickTime = 1f)
        {
            InitVelocity();
            InitTime();
            InitDistance();
            TickTime = tickTime;
        }
        
        public void InitVelocity()
        {
            Velocity = _startVelocity;
        }
        
        public void AddVelocity()
        {
            Velocity += _addedVelocity;
            if (Velocity >= _maxVelocity) Velocity = _maxVelocity;
        }
        
        public void StopVelocity()
        {
            Velocity = 0f;
        }

        public void InitTime()
        {
            Time = 0f;
        }

        public void AddTime()
        {
            Time += TickTime;
        }

        public void InitDistance()
        {
            TravelDistance = 0f;
        }

        public void AddDistance()
        {
            TravelDistance += TickDistance;
        }
    }
}