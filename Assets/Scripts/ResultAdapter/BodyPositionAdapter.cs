using UnityEngine;
using VRMController;

namespace Mediapipe.Allocator
{
    public class BodyPositionAdapter : TrackingAdapterBase
    {
        // This is a unique class as a subclass of TrackingAdapterBase.
        // While other subclasses change "Rotation", this class changes the "Position" of the entire body.

        private Transform _bodyTransform;
        private float _maximumAmountOfMovementInGameView = 1.0f;

        public BodyPositionAdapter(GameObject partObject, LandmarksPacket landmarksPacket, Sleeve sleeve)
               : base(partObject, landmarksPacket, sleeve)
        { 
            _bodyTransform = partObject.transform;
        }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               12           right shoulder
         *        2               23             left  hip
         *        3               24             right hip
         */

        public override void ForwardApply(PoseMatrix? parentMatrix = null, Quaternion? parentQuaternion = null)
        {
            Vector2 centerOfChest = GetCenterOfChest();

            // Take the center of the screen as the origin.
            centerOfChest = centerOfChest.Add(-0.5f);

            // Change the coordinate range from [-0.5, 0.5] to [-1, 1].
            centerOfChest *= -2.0f;

            var pos = _bodyTransform.position;
            pos.x = centerOfChest.x * _maximumAmountOfMovementInGameView;
            _bodyTransform.position = pos;
        }

        private Vector2 GetCenterOfChest()
        {
            /* [Coordinate of Landmarks]
             
                  Right                            Left
                   0.0            0.5              1.0
               0.0  ---------------------------------> x
                   |                                 :
                   |        (1)         (0)          :
                   |                                 :
                   |                                 :
               0.5 |               * <- Center       :
                   |                                 :
                   |                                 :
                   |        (3)         (2)          :
                   |.................................:
               1.0 v             <Screen>
                   y
            */
            Vector2 shoulderVector = Landmark(0) - Landmark(1);
            Vector2 hipVector = Landmark(2) - Landmark(3);

            Vector2 spineVector = (hipVector * 0.5f - shoulderVector * 0.5f);

            return new(Landmark(1).x + shoulderVector.x * 0.5f + spineVector.x * 0.5f,
                       Landmark(1).y + shoulderVector.y * 0.5f + spineVector.y * 0.5f);
        }
    }
}// namespace Mediapipe.Allocator

public static class VectorExtensions
{
    public static Vector2 Add(this Vector2 vector, float addValue)
    {
        return vector + new Vector2(addValue, addValue);
    }

    public static Vector3 Add(this Vector3 vector, float addValue)
    {
        return vector + new Vector3(addValue, addValue, addValue);
    }
}
