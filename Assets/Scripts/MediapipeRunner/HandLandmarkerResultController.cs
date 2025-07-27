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

using Mediapipe.Unity;
using Mediapipe.Tasks.Vision.HandLandmarker;

namespace Mediapipe.UnityRunner.HandLandmarkDetection
{
    public class HandLandmarkerResultController : AnnotationController<MultiHandLandmarkListAnnotation>
    {
        public UnityEvent<HandLandmarkerResult> onHandTargetUpdated;

        protected HandLandmarkerResult _currentTarget;

        public void DrawNow(HandLandmarkerResult target)
        {
            target.CloneTo(ref _currentTarget);
            SyncNow();
        }

        public void DrawLater(HandLandmarkerResult target) => UpdateCurrentTarget(target);

        protected void UpdateCurrentTarget(HandLandmarkerResult newTarget)
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
            if (_currentTarget.handLandmarks != null)
            {
                onHandTargetUpdated?.Invoke(_currentTarget);
            }
        }
    }
}// namespace Mediapipe.UnityRunner.FaceLandmarkDetection
