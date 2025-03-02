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

using UnityEngine.Events;

using Mediapipe.Unity;
using Mediapipe.Tasks.Vision.FaceLandmarker;

namespace Mediapipe.UnityRunner.FaceLandmarkDetection
{
    public class FaceLandmarkerResultController : AnnotationController<MultiFaceLandmarkListAnnotation>
    {
        public UnityEvent<FaceLandmarkerResult> onFaceTargetUpdated;

        protected FaceLandmarkerResult _currentTarget;

        private readonly object _currentTargetLock = new object();

        public void DrawNow(FaceLandmarkerResult target)
        {
            target.CloneTo(ref _currentTarget);
            SyncNow();
        }

        public void DrawLater(FaceLandmarkerResult target) => UpdateCurrentTarget(target);

        protected void UpdateCurrentTarget(FaceLandmarkerResult newTarget)
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
            if (_currentTarget.faceLandmarks != null)
            {
                onFaceTargetUpdated?.Invoke(_currentTarget);
            }
        }
    }
}// namespace Mediapipe.UnityRunner.FaceLandmarkDetection
