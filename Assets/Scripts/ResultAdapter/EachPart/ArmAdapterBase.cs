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

            if(isDebug) GameLogger.Log(stableEulerAngles);

            return Quaternion.Euler(stableEulerAngles);
        }

        private Vector4 TorsoArea()
        {
            Vector3 shoulderCenterPoint = (Landmark(0) + Landmark(2)) * 0.5f;
            Vector3 hipCenterPoint = (Landmark(4) + Landmark(5)) * 0.5f;

            Vector3 shoulderVector = (Landmark(0) - Landmark(2)) * 0.7f;
            Vector3 hipVector = (Landmark(4) - Landmark(5)) * 0.7f;

            float x1 = Mathf.Min(hipCenterPoint.x - hipVector.x, shoulderCenterPoint.x - shoulderVector.x);
            float y1 = Mathf.Max(shoulderCenterPoint.y - shoulderVector.y, shoulderCenterPoint.y + shoulderVector.y);
            float x2 = Mathf.Max(hipCenterPoint.x + hipVector.x, shoulderCenterPoint.x + shoulderVector.x);
            float y2 = Mathf.Min(hipCenterPoint.y - hipVector.y, hipCenterPoint.y + hipVector.y);

            return new Vector4(x1, y1, x2, y1 + (y2 - y1) * 0.5f);
        }

        protected bool IsInTorsoArea(Vector2 point)
        {
            var torsoArea = TorsoArea();

            if (point.x < torsoArea.x || point.x > torsoArea.z) return false;
            if (point.y < torsoArea.y || point.y > torsoArea.w) return false;

            return true;
        }

        protected static PoseMatrix NeutralArmMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f, 0.0f, 0.0f),
                                                  new Vector3(0.0f, +1.0f, 0.0f),
                                                  new Vector3(0.0f, 0.0f, -1.0f),
                                                  new Vector3(0.0f, 0.0f, 0.0f));
        }
    }
}//Mediapipe.Allocator
