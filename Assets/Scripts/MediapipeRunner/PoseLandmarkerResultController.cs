// Copyright (c) 2023 homuler
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using UnityEngine;
using UnityEngine.Events;

using Mediapipe.Tasks.Vision.PoseLandmarker;
using Mediapipe.Unity;

namespace Mediapipe.UnityRunner.PoseLandmarkDetection
{
    public class PoseLandmarkerResultController : AnnotationController<MultiPoseLandmarkListWithMaskAnnotation>
    {
        public UnityEvent<PoseLandmarkerResult> onPoseTargetUpdated;

        protected PoseLandmarkerResult _currentTarget;

        public void InitScreen(int maskWidth, int maskHeight) => annotation.InitMask(maskWidth, maskHeight);

        public void DrawNow(PoseLandmarkerResult target)
        {
            target.CloneTo(ref _currentTarget);
            SyncNow();
        }

        public void DrawLater(PoseLandmarkerResult target) => UpdateCurrentTarget(target);

        protected void UpdateCurrentTarget(PoseLandmarkerResult newTarget)
        {
            if (IsTargetChanged(newTarget, _currentTarget))
            {
                newTarget.CloneTo(ref _currentTarget);
                isStale = true;
            }
        }

        protected override void SyncNow()
        {
            isStale = false;

            if (_currentTarget.poseLandmarks != null)
            {
                onPoseTargetUpdated?.Invoke(_currentTarget);
            }
        }
    }
}
