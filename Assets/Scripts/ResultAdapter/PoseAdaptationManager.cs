// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.PoseLandmarker;
using System.Collections.ObjectModel;
using UnityEngine;
using UniVRM10;
using VRMController;

namespace Mediapipe.Allocator
{
    public class PoseAdaptationManager : AdaptationManagerBase<PoseLandmarkerResult>
    {
        public ReadOnlyCollection<Tasks.Components.Containers.NormalizedLandmark> Landmarks => _landmarks.AsReadOnly();

        // Entire Body (Root)
        LandmarksPacket _entireBodyPacket;
        BodyPositionAdapter _bodyPositionAdapter;

        // Entire Body (Collider)
        LandmarksPacket _colliderPacket;
        BodyColliderAdapter _bodyColliderAdapter;


        // Torso
        LandmarksPacket _hipPacket;
        HipAdapter _hipAdapter;
        LandmarksPacket _chestPacket;
        ChestAdapter _chestAdapter;

        // Left Arm
        LandmarksPacket _leftArmPacket;
        UpperArmAdapter _leftUpperArmAdapter;
        LowerArmAdapter _leftLowerArmAdapter;

        // Right Arm
        LandmarksPacket _rightArmPacket;
        UpperArmAdapter _rightUpperArmAdapter;
        LowerArmAdapter _rightLowerArmAdapter;

        // Head
        LandmarksPacket _headPacket;
        HeadAdapter _headAdapter;

        // Left Leg
        LandmarksPacket _leftLegPacket;
        UpperLegAdapter _leftUpperLegAdapter;
        LowerLegAdapter _leftLowerLegAdapter;
        LandmarksPacket _leftFootPacket;
        FootAdapter _leftFootAdapter;

        // Right Leg
        LandmarksPacket _rightLegPacket;
        UpperLegAdapter _rightUpperLegAdapter;
        LowerLegAdapter _rightLowerLegAdapter;
        LandmarksPacket _rightFootPacket;
        FootAdapter _rightFootAdapter;

        Sleeve sleeve;

        public PalmVectors PalmVectors{ private get; set; }

        public Sleeve Sleeve{ private get; set; }

        protected override void OnEnable()
        {
            base.OnEnable();

            var vrmInstance = _vrmObject.GetComponent<Vrm10Instance>();
            vrmInstance.UpdateType = Vrm10Instance.UpdateTypes.None;

            GenerateLandmarksList(33);

            // Definition of "Adapters" that apply the result of MediaPipe to each part
            // and "Packets" that convey information to Adapter.]
            // For more details, see https://ai.google.dev/edge/mediapipe/solutions/vision/pose_landmarker
            _entireBodyPacket = new(_landmarks, new int[2] { 25, 26 });
            _bodyPositionAdapter = new(FindChildByName("Root"), _entireBodyPacket, Sleeve);

            _colliderPacket = new(_landmarks, new int[7] { 11, 23, 24, 25, 26, 27, 28 });
            _bodyColliderAdapter = new(FindChildByName("Body"), _colliderPacket, Sleeve);

            _hipPacket = new(_landmarks, new int[4] { 23, 24, 11, 12 });
            _hipAdapter = new(FindChildByName("Hip"), _hipPacket, Sleeve);

            _chestPacket = new(_landmarks, new int[4] { 11, 12, 23, 24});
            _chestAdapter = new(FindChildByName("Chest"), _chestPacket, Sleeve);

            _leftArmPacket = new(_landmarks, new int[6] { 11, 13, 12, 15, 23, 24 });
            _leftUpperArmAdapter = new(FindChildByName("L_UpperArm"), _leftArmPacket, Sleeve, isLeft: true);
            _leftLowerArmAdapter = new(FindChildByName("L_LowerArm"), _leftArmPacket, Sleeve, isLeft: true);

            _rightArmPacket = new(_landmarks, new int[6] { 12, 14, 11, 16, 24, 23 });
            _rightUpperArmAdapter = new(FindChildByName("R_UpperArm"), _rightArmPacket, Sleeve, isLeft: false);
            _rightLowerArmAdapter = new(FindChildByName("R_LowerArm"), _rightArmPacket, Sleeve, isLeft: false);

            _leftLegPacket = new(_landmarks, new int[7] { 11, 12, 23, 24, 25, 27, 28 });
            _leftUpperLegAdapter = new(FindChildByName("L_UpperLeg"), _leftLegPacket, Sleeve, isLeft: true);
            _leftLowerLegAdapter = new(FindChildByName("L_LowerLeg"), _leftLegPacket, Sleeve, isLeft: true);

            _leftFootPacket = new(_landmarks, new int[4] { 25, 27, 29, 31 });
            _leftFootAdapter = new(FindChildByName("L_Foot"), _leftFootPacket, Sleeve, isLeft: true);

            _rightFootPacket = new(_landmarks, new int[4] { 26, 28, 30, 32 });
            _rightFootAdapter = new(FindChildByName("R_Foot"), _rightFootPacket, Sleeve, isLeft: false);

            _rightLegPacket = new(_landmarks, new int[7] { 12, 11, 24, 23, 26, 28, 27 });
            _rightUpperLegAdapter = new(FindChildByName("R_UpperLeg"), _rightLegPacket, Sleeve, isLeft: false);
            _rightLowerLegAdapter = new(FindChildByName("R_LowerLeg"), _rightLegPacket, Sleeve, isLeft: false);

            _headPacket = new(_landmarks, new int[4] { 7, 8, 11, 12 });
            _headAdapter = new(FindChildByName("Head"), _headPacket, Sleeve);

            if(_operationDimension == OperationDimension.TwoDimension)
            {
                TrackingAdapterBase.ValidCacheSize = TrackingAdapterBase.ValidCacheSize * 3;
            }
        }

        public override void ApplyMediapipeResult(PoseLandmarkerResult recognitionResult)
        {
            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.poseLandmarks[0].landmarks[i];
            }

            if (_vrmObject == null)
            {
                return;
            }

            // Chest and Head

            _hipAdapter.ForwardApply();
            _chestAdapter.ForwardApply(_hipAdapter.PoseMatrix);

            _headAdapter.ForwardApply(parentQuaternion: _hipAdapter.LatestQuaternion * _chestAdapter.LatestQuaternion);

            // Arm

            _leftUpperArmAdapter.ForwardApply(parentMatrix : _chestAdapter.PoseMatrix);
            _leftLowerArmAdapter.ForwardApply();

            _rightUpperArmAdapter.ForwardApply(parentMatrix: _chestAdapter.PoseMatrix);
            _rightLowerArmAdapter.ForwardApply();

            // Leg

            _leftUpperLegAdapter.ForwardApply(parentQuaternion: _hipAdapter.LatestQuaternion);
            _leftLowerLegAdapter.ForwardApply();

            _leftFootAdapter.ForwardApply();

            _rightUpperLegAdapter.ForwardApply(parentQuaternion: _hipAdapter.LatestQuaternion);
            _rightLowerLegAdapter.ForwardApply();

            _rightFootAdapter.ForwardApply();

            // Entire Body

            _bodyColliderAdapter.ForwardApply();
            Vector3 positionOfLowerObject = FetchPositionOfLowerObject(_bodyColliderAdapter.LowestIndex);
            _bodyColliderAdapter.UpdateColliderHeight(positionOfLowerObject);

            _bodyPositionAdapter.RegisterHeight(positionOfLowerObject);
            _bodyPositionAdapter.ForwardApply();

        }

        private Vector3 FetchPositionOfLowerObject(int lowerObjectID)
        {
            /*
             *   | 0 | L_UpperLeg |
             *   | 1 | R_UpperLeg |
             *   | 2 | L_LowerLeg |
             *   | 3 | R_LowerLeg |
             *   | 4 |   L_Foot   |
             *   | 5 |   R_Foot   |
             */

            Vector3 position;

            position = lowerObjectID switch
            {
                0 => _leftUpperLegAdapter.PartObjectPosition,
                1 => _rightUpperLegAdapter.PartObjectPosition,
                2 => _leftLowerLegAdapter.PartObjectPosition,
                3 => _rightLowerLegAdapter.PartObjectPosition,
                4 => _leftFootAdapter.PartObjectPosition,
                5 => _rightFootAdapter.PartObjectPosition,
                _ => Vector3.negativeInfinity,
            };

            return position;
        }
    }

}// namespace Mediapipe.Allocator
