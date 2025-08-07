// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public abstract class ArmAdapterBase : TrackingAdapterBase
    {
        protected readonly bool _isLeft;
        protected readonly float RightMinus;

        public ArmAdapterBase(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool isLeft)
                    : base(partObject, landmarksPacket, sleeve) 
        {
            _isLeft = isLeft;
            RightMinus = isLeft ? 1.0f : -1.0f;
        }

        protected override Quaternion PreventUnwantedRotation(Quaternion smoothedRotationLHS, bool isDebug = false)
        {
            var stableEulerAngles = smoothedRotationLHS.eulerAngles;

            stableEulerAngles.z -= 10 * RightMinus;

            if (stableEulerAngles.x > 270)
            {
                stableEulerAngles.x = 0.0f;
            }
            if (stableEulerAngles.y > 180)
            {
                stableEulerAngles.y -= 360.0f;
            }
            if (stableEulerAngles.z > 180)
            {
                stableEulerAngles.z -= 360.0f;
            }

            stableEulerAngles.y = Mathf.Clamp(stableEulerAngles.y, -90.0f, 90.0f);
            stableEulerAngles.z = Mathf.Clamp(stableEulerAngles.z, -90.0f, 80.0f);

            if (isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }
    }
}//Mediapipe.Allocator
