// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.PoseLandmarker;
using UniVRM10;

namespace Mediapipe.Allocator
{
    public class PoseAdaptationManager : AdaptationManagerBase<PoseLandmarkerResult>
    {
        LandmarksPacket _hipPacket;
        HipAdapter _hipAdapter;
        LandmarksPacket _chestPacket;
        ChestAdapter _chestAdapter;

        bool _usePoseAdaptation = true;

        protected override void OnEnable()
        {
            base.OnEnable();

            var vrmInstance = _vrmObject.GetComponent<Vrm10Instance>();
            vrmInstance.UpdateType = Vrm10Instance.UpdateTypes.None;

            GenerateLandmarksList(33);

            // Definition of "Adapters" that apply the result of MediaPipe to each part
            // and "Packets" that convey information to Adapter.
            _hipPacket = new(_landmarks, new int[2] { 23, 24 });
            _hipAdapter = new(FindChildByName("Hip"), _hipPacket, false, true, true);
            _chestPacket = new(_landmarks, new int[4] { 11, 12, 23, 24 });
            _chestAdapter = new(FindChildByName("Chest"), _chestPacket, true, true, true);
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
            _chestAdapter.ForwardApply(_hipAdapter.LatestRotation);
        }
    }

}// namespace Mediapipe.Allocator
