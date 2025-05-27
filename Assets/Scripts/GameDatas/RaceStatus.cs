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
        public float TickDistance => Velocity * _tickTime;  // 틱(1초)동안의 이동 거리
        
        // 계산을 위한 값들 
        private readonly float _startVelocity = 1f; 
        private readonly float _maxVelocity = 8f;
        private readonly float _addedVelocity = 0.03f;
        
        // DI
        private float _tickTime = 1f;

        public RaceStatus()
        {
            Initialize();
        }
        
        public void Initialize(float tickTime = 1f)
        {
            InitVelocity();
            InitTime();
            InitDistance();
            _tickTime = tickTime;
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
            Time += _tickTime;
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