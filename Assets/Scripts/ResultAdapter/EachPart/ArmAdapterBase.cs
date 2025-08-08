// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;

namespace Mediapipe.Allocator
{
    public abstract class ArmAdapterBase : TrackingAdapterBase
    {
        protected readonly bool _isLeft;
        protected readonly float RightMinus;

        public ArmAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, bool isLeft)
                    : base(partObject, landmarksPacket) 
        {
            _isLeft = isLeft;
            RightMinus = isLeft ? 1.0f : -1.0f;
        }

        protected override abstract Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false);
    }
}//Mediapipe.Allocator
