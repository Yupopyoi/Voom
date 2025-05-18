// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.PoseLandmarker;

namespace Mediapipe.Allocator
{
    public class PoseAdaptationManager : AdaptationManagerBase<PoseLandmarkerResult>
    {
        LandmarksPacket _chestPacket;
        ChestAdapter _chestAdapter;

        protected override void Start()
        {
            base.Start();

            GenerateLandmarksList(33);

            _chestPacket = new(_landmarks, new int[2] { 11, 12 });
            _chestAdapter = new(FindChildByName("Chest"), _chestPacket);
        }

        public override void ApplyMediapipeResult(PoseLandmarkerResult recognitionResult)
        {
            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.poseLandmarks[0].landmarks[i];
            }

            _chestAdapter.ForwardApply();
        }
    }

}// namespace Mediapipe.Allocator
