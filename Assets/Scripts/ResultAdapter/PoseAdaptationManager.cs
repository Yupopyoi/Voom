// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.PoseLandmarker;
using UniVRM10;
using VRMController;

namespace Mediapipe.Allocator
{
    public class PoseAdaptationManager : AdaptationManagerBase<PoseLandmarkerResult>
    {
        // Torso
        LandmarksPacket _hipPacket;
        HipAdapter _hipAdapter;
        LandmarksPacket _chestPacket;
        ChestAdapter _chestAdapter;

        // Left Arm
        LandmarksPacket _leftUpperArmPacket;
        LeftUpperArmAdapter _leftUpperArmAdapter;
        LandmarksPacket _leftLowerArmPacket;
        LeftLowerArmAdapter _leftLowerArmAdapter;

        bool _usePoseAdaptation = true;
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
            // and "Packets" that convey information to Adapter.
            _hipPacket = new(_landmarks, new int[4] { 23, 24, 11, 12 });
            _hipAdapter = new(FindChildByName("Hip"), _hipPacket, Sleeve, false, false, true);

            _chestPacket = new(_landmarks, new int[4] { 11, 12, 23, 24});
            _chestAdapter = new(FindChildByName("Chest"), _chestPacket, Sleeve , true, true, true);

            _leftUpperArmPacket = new(_landmarks, new int[6] { 11, 13, 12, 15, 23, 24 });
            _leftUpperArmAdapter = new(FindChildByName("L_UpperArm"), _leftUpperArmPacket, Sleeve, true, true, true);

            _leftLowerArmPacket = new(_landmarks, new int[6] { 11, 13, 12, 15, 23, 24 });
            _leftLowerArmAdapter = new(FindChildByName("L_LowerArm"), _leftUpperArmPacket, Sleeve, false, false, true);
        }

        public override void ApplyMediapipeResult(PoseLandmarkerResult recognitionResult)
        {
            if (!_usePoseAdaptation) return;

            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.poseLandmarks[0].landmarks[i];
            }

            if (_vrmObject == null)
            {
                return;
            }

            _hipAdapter.ForwardApply();
            _chestAdapter.ForwardApply(_hipAdapter.PoseMatrix);
            _leftUpperArmAdapter.ForwardApply(_chestAdapter.PoseMatrix);
            _leftLowerArmAdapter.ForwardApply(_leftUpperArmAdapter.PoseMatrix);

           // _leftUpperArmAdapter.ReverseApply(PalmVectors);
        }
    }

}// namespace Mediapipe.Allocator
