// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class BodyPositionAdapter : TrackingAdapterBase
    {
        // This is a unique class as a subclass of TrackingAdapterBase.
        // While other subclasses change "Rotation", this class changes the "Position" of the entire body.

        // In this class, the collider configuration will not be changed.
        // For changing the configuration, refer to BodyColliderAdapter.

        private Transform _bodyTransform;
        private float _maximumAmountOfMovementInGameView = 1.0f;
        private bool _isFliped = true;

        private readonly Queue<Vector3> _vecterCache;

        public BodyPositionAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
               : base(partObject, landmarksPacket, sleeve)
        { 
            _bodyTransform = partObject.transform; /* Root */

            _vecterCache = new(capacity: CACHE_SIZE);
        }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index [L/R] |        Part       |
         *   |:----------:|:----------------------:|:-----------------:|
         *   |     0      |           25           |     Left  knee    |
         *   |     1      |           26           |     Right knee    |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (!LandmarkVisibility(0) && !LandmarkVisibility(1) /* Both knees are not visible */ ) return;

            Vector3 centerPosition = (Landmark(0) + Landmark(1)) * 0.5f; // Range : [0,1]

            centerPosition = centerPosition.Add(-0.5f); // Range : [-0.5,0.5]

            centerPosition = centerPosition.Mul(2.0f); // Range : [-1,1];

            ApplyRotation(centerPosition);
        }

        protected void ApplyRotation(Vector3 v, bool isDebug = false)
        {
            AddVector3Cache(v);
            Vector3 averagePosition = AverageVector3(isDebug);

            if (_bodyTransform != null)
            {
                float flip = _isFliped ? -1.0f : 1.0f;

                var rootPos = _bodyTransform.position;
                rootPos.x = averagePosition.x * _maximumAmountOfMovementInGameView * flip;
                _bodyTransform.position = rootPos;
            }
        }

        private void AddVector3Cache(Vector3 v)
        {
            if (_vecterCache.Count >= CACHE_SIZE)
            {
                _vecterCache.Dequeue();
            }

            _vecterCache.Enqueue(v);
        }

        private Vector3 AverageVector3(bool isDebug = false)
        {
            if (_vecterCache.Count == 0)
            {
                return new Vector3(0.0f, 0.0f, 0.0f);
            }

            Vector3 sum = new(0.0f, 0.0f, 0.0f);

            int n = 1;
            foreach (var value in _vecterCache)
            {
                if (n++ < CACHE_SIZE - ValidCacheSize) continue;

                sum.x += value.x;
                sum.y += value.y;
                sum.z += value.z;
            }

            int c = ValidCacheSize;

            Vector3 averageVector3 = new(sum.x / c, sum.y / c, sum.z / c);

            if (isDebug)
            {
                GameLogger.Log(averageVector3, 2);
                Debug.Log(averageVector3.ToString());
            }

            return averageVector3;
        }
    }
}// namespace Mediapipe.Allocator

