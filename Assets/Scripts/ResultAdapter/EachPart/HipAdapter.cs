using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class HipAdapter : TrackingAdapterBase
    {
        public HipAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, sleeve, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               23             left  hip
         *        1               24             right hip
         */

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            /*
            Vector3 leftHip = Landmark(0);
            Vector3 rightHip = Landmark(1);

            Vector3 hipVec = leftHip - rightHip;

            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(hipVec);

            Vector3 hipRotationValue = new(calculatedEulerAngles.x,
                                             Mathf.Clamp(calculatedEulerAngles.y, -100f, 100f),
                                             calculatedEulerAngles.z);


            ApplyRotation(hipRotationValue);
            */

            Vector3 leftHip = Landmark(0);
            Vector3 rightHip = Landmark(1);

            // 1. ヒップの中心位置
            Vector3 hipCenter = (leftHip + rightHip) * 0.5f;

            // 2. ローカル座標系の定義
            Vector3 right = (rightHip - leftHip).normalized;

            // 仮のup方向（頭部方向に近い方向を定義）
            Vector3 neck = Landmark(2);  // 例: SpineやNeckなど
            Vector3 up = (hipCenter - neck).normalized;

            // orthonormal basis
            Vector3 forward = Vector3.Cross(right, up).normalized;
            up = Vector3.Cross(forward, right).normalized;

            // 3. 同次変換行列を作る（回転成分のみ使用）
            Matrix4x4 T = Matrix4x4.identity;
            T.SetColumn(0, new Vector4(right.x, right.y, right.z, 0));
            T.SetColumn(1, new Vector4(up.x, up.y, up.z, 0));
            T.SetColumn(2, new Vector4(forward.x, forward.y, forward.z, 0));
            T.SetColumn(3, new Vector4(hipCenter.x, hipCenter.y, hipCenter.z, 1));

            // 4. 回転部分を取り出して適用
            Quaternion rotation = T.rotation;
            Vector3 euler = rotation.eulerAngles;

            ApplyRotation(euler);
        }
    }
}// namespace Mediapipe.Allocator
