using Unity.VisualScripting;
using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class HipAdapter : TrackingAdapterBase
    {
        public HipAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, sleeve, unfixX, unfixY, unfixZ) {
        }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               23             left  hip
         *        1               24             right hip
　　　　 *        2               11           left  shoulder
         *        3               12           right shoulder
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null)
        {
            Vector3 leftHip = Landmark(0);
            Vector3 rightHip = Landmark(1);
            Vector3 hipCenter = (leftHip + rightHip) * 0.5f;

            if(!LandmarkVisibility(0) && !LandmarkVisibility(1))
            {
                _poseMatrix = NeutralHipMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 right = (rightHip - leftHip).normalized;
            Vector3 neck = (Landmark(2) + Landmark(3)) * 0.5f;
            Vector3 upHint = (hipCenter - neck).normalized;

            Vector3 forward = Vector3.Cross(right, upHint).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, hipCenter);

            // Make it less sensitive to small movements.
            // This prevents meaningless vibrations from occurring in the model when you are stationary.
            Quaternion stableRotationLHS = ToSmoothStair(_poseMatrix.RotationLHS);

            ApplyRotation(stableRotationLHS);
        }

        private static PoseMatrix NeutralHipMatrix()
        {
            return PoseMatrix.SetBasisAndPosition(new Vector3(-1.0f,  0.0f,  0.0f),
                                                  new Vector3( 0.0f, +1.0f,  0.0f),
                                                  new Vector3( 0.0f,  0.0f, -1.0f),
                                                  new Vector3( 0.5f,  0.5f,  0.0f));
        }
    }
}// namespace Mediapipe.Allocator
