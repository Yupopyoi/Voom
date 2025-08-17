using UnityEngine;

namespace Mediapipe.Allocator
{
    public class HipAdapter : TrackingAdapterBase
    {
        public HipAdapter(GameObject partObject, LandmarksPacket landmarksPacket)
                                                            : base(partObject, landmarksPacket) {}

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               23             left  hip
         *        1               24             right hip
　　　　 *        2               11           left  shoulder
         *        3               12           right shoulder
         */

        // Points
        Vector3 _leftHip;
        Vector3 _rightHip;

        public bool CanRotateAroundXaxis  { get; set; } = false;

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            _leftHip = Landmark(0);
            _rightHip = Landmark(1);

            Vector3 hipCenter = (_leftHip + _rightHip) * 0.5f;

            if (Is2D())
            {
                // In 2D mode (sitting mode), it is sufficient to enable hip rotation only around the z-axis.
                // Z-Angle is defined as the angle between the "Spine Vector" projected onto the xy plane, and the global y-axis.
                Vector3 shoulderCenter = (Landmark(2) + Landmark(3)) * 0.5f;

                // You can get the hip position pretty accurately when you're sitting down.
                Vector3 spineVector = hipCenter - shoulderCenter;

                static float EulerAngleZ(Vector3 spineVector)
                {
                    Vector2 spineVectorProjectedXYPlane = ((Vector2)spineVector).normalized;

                    // Calculate the dot product with the x-axis (not y-axis) to be able to distinguish
                    // between positive and negative (left and right) directions.
                    float cos = Vector2.Dot(spineVectorProjectedXYPlane, Vector2.right /* x-axis */);
                    return Mathf.Acos(cos) * Mathf.Rad2Deg;
                }

                float EulerAngleX(Vector3 spineVector)
                {
                    if (!CanRotateAroundXaxis) return InitialTransform().x;

                    Vector2 spineVectorProjectedYZPlane = new Vector2(spineVector.y, spineVector.z).normalized;

                    float cos = Vector2.Dot(spineVectorProjectedYZPlane, Vector2.right /* z-axis */);
                    
                    return ToSmoothStair(Mathf.Acos(cos) * Mathf.Rad2Deg) - InitialTransform().x;
                }

                Quaternion stableRotation = 
                    Quaternion.Euler(new Vector3(EulerAngleX(spineVector), 0.0f, EulerAngleZ(spineVector) - 90.0f));

                ApplyRotation(PreventUnwantedRotation(stableRotation));

                return;
            }

            // 3D Mode (All Body Tracking) ------------------------------

            if (!LandmarkVisibility(0) && !LandmarkVisibility(1))
            {
                _poseMatrix = NeutralHipMatrix();

                ApplyRotation(_poseMatrix.RotationLHS);
                return;
            }

            Vector3 right = (_rightHip - _leftHip).normalized;

            Vector3 neck = (Landmark(2) + Landmark(3)) * 0.5f;
            Vector3 upHint = (hipCenter - neck).normalized;

            Vector3 forward = Vector3.Cross(right, upHint).normalized;
            Vector3 up = Vector3.Cross(forward, right).normalized;

            _poseMatrix = PoseMatrix.SetBasisAndPosition(right, up, forward, hipCenter);

            // Make it less sensitive to small movements.
            // This prevents meaningless vibrations from occurring in the model when you are stationary.
            Quaternion stableRotationLHS = ToSmoothStair(_poseMatrix.RotationLHS);

            ApplyRotation(PreventUnwantedRotation(stableRotationLHS));
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
