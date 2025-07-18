using UnityEngine;

namespace Mediapipe.Allocator
{
    public class ChestAdapter : TrackingAdapterBase
    {
        public ChestAdapter(GameObject partObject, LandmarksPacket landmarksPacket, bool unfixX = false, bool unfixY = false, bool unfixZ = true)
            : base(partObject, landmarksPacket, unfixX, unfixY, unfixZ) { }

        /*  [Landmark Index]
         * 
         *    Call Index    Mediapipe Index         Part
         *        0               11           left  shoulder
         *        1               12           right shoulder
         *        2               23             left  hip
         *        3               24             right hip
         */

        // Prevents the model from bending forward (hunchback).
        // The larger the value, the more straight (or in some cases, warped) the model will be.
        // This number may eventually change to property.
        private float _hunchbackCorrection = 30.0f;

        public override void ForwardApply(Rotation? parentRotation = null)
        {
            Vector3 shoulderVec = Landmark(0) - Landmark(1);

            Vector3 calculatedEulerAngles = CalculateSignedEulerAngles(shoulderVec);

            Vector3 chestRotationRawValue = new(CalculateRotationX() + _hunchbackCorrection, 
                                                Mathf.Clamp(calculatedEulerAngles.y, -90f, 90f), 
                                                calculatedEulerAngles.z);

            Vector3 propagatedRotation /* From parents ( = Hips) */ = parentRotation.GetValueOrDefault().ToVector3;

            Vector3 chestRotation = propagatedRotation - chestRotationRawValue;

            ApplyRotation(chestRotation);
        }

        private float CalculateRotationX()
        {
            Vector3 spineVec = (Landmark(2) + Landmark(3)) * 0.5f - (Landmark(0) + Landmark(1)) * 0.5f;

            if (spineVec == Vector3.zero)
                return 0f;

            spineVec.Normalize();

            float pitchRad = Mathf.Atan2(spineVec.z, spineVec.y);
            return -Mathf.Rad2Deg * pitchRad;
        }
    }
}// namespace Mediapipe.Allocator
