// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using Mediapipe.Tasks.Vision.FaceLandmarker;

namespace Mediapipe.Allocator
{
    public class FaceAdaptationManager : AdaptationManagerBase<FaceLandmarkerResult>
    {
        LandmarksPacket _mouthPacket;
        MouthAdapter _mouthAdapter;

        protected override void Start()
        {
            base.Start();

            GenerateLandmarksList(478);

            _mouthPacket = new(_landmarks, new int[13] { 13, 14, 306, 78, 311, 81, 402, 178, 17, 473, 468, 334, 105 });
            _mouthAdapter = new(_vrmObject.transform.Find("Face").gameObject, _mouthPacket);
        }

        public override void ApplyMediapipeResult(FaceLandmarkerResult recognitionResult)
        {
            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.faceLandmarks[0].landmarks[i];
            }

            _mouthAdapter.ForwardApply();
        }
    }

} // namespace Mediapipe.Allocator
