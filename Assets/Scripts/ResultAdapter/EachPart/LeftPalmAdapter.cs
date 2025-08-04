using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LeftPalmAdapter : TrackingAdapterBase
    {
        public LeftPalmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
                        : base(partObject, landmarksPacket, sleeve) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               13             left elbow
         *        2               15             left wrist
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {

        }
    }
}// namespace Mediapipe.Allocator
