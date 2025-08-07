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
    public class BodyColliderAdapter : TrackingAdapterBase
    {
        // This is a unique class as a subclass of TrackingAdapterBase.
        // While other subclasses change "Rotation", this class changes the "Position" of the entire body.

        // In this class, the collider configuration will not be changed.
        // For changing the configuration, refer to BodyColliderAdapter.

        private CapsuleCollider _bodyCollider;
        private readonly float _modelHeight;

        private readonly Queue<float> _heightCache;

        private int _lowestIndex;
        private float _height;

        private bool _isJumping; //ToDo


        public int LowestIndex => _lowestIndex;

        public BodyColliderAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
               : base(partObject, landmarksPacket, sleeve)
        {
            _bodyCollider = partObject.GetComponent<CapsuleCollider>();

            if(_bodyCollider != null) _modelHeight = _bodyCollider.height;

            _heightCache = new(capacity: CACHE_SIZE);
        }

        /*  [Landmark Index]
         * 
         *   | Call Index |  Mediapipe Index  |        Part       |
         *   |:----------:|:-----------------:|:-----------------:|
         *   |     0      |        11         |        nose       |
         *   |     1      |        23         |     left  hip     |
         *   |     2      |        24         |     right hip     |
         *   |     3      |        25         |     left  knee    |
         *   |     4      |        26         |     right knee    |
         *   |     5      |        27         |    left  ankle    |
         *   |     6      |        28         |    right ankle    |
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            if (_bodyCollider == null) return;

            bool willPass = false;
            if (!LandmarkVisibility(1) && !LandmarkVisibility(2) /* Both hips are not visible */ ) willPass = true;

            if (!LandmarkVisibility(3) && !LandmarkVisibility(4) /* Both knees are not visible */ ) willPass = true;

            if (!LandmarkVisibility(5) && !LandmarkVisibility(6) /* Both ancles are not visible */ ) willPass = true;

            if(willPass)
            {
                _lowestIndex = -1;
                return;
            }

            float maxY = 0.0f;
            _lowestIndex = 0;
            for (int callIndex = 1; callIndex <= 6; callIndex++)
            { 
                if(!LandmarkVisibility(callIndex)) continue;

                if(maxY < Landmark(callIndex).y)
                {
                    maxY = Landmark(callIndex).y;
                    _lowestIndex = callIndex - 1;
                }
            }
        }

        public void UpdateColliderHeight(Vector3 lowestPosition)
        {
            if (_bodyCollider == null) return;

            if (lowestPosition.y == float.NegativeInfinity)
            {
                _bodyCollider.height = _modelHeight;
                return;
            }

            _height = _modelHeight - lowestPosition.y;

            if(_height > _modelHeight) _height = _modelHeight;

            _bodyCollider.height = _height;
        }
    }
}// namespace Mediapipe.Allocator

