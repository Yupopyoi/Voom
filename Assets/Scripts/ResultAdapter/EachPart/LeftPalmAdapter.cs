using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class LeftPalmAdapter : TrackingAdapterBase
    {
        public LeftPalmAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
                        : base(partObject, landmarksPacket, sleeve, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               13             left elbow
         *        2               15             left wrist
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null)
        {

        }
    }
}// namespace Mediapipe.Allocator
