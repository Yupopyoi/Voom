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
using TMPro;
using System.Diagnostics;

namespace Mediapipe.UnityRunner.PoseLandmarkDetection
{
    public class PoseLandmarkerResultController : AnnotationController<MultiPoseLandmarkListWithMaskAnnotation>
    {
        public UnityEvent<PoseLandmarkerResult> onPoseTargetUpdated;

        protected PoseLandmarkerResult _currentTarget;

        [SerializeField] private bool _canDisplayFPS = false;
        [SerializeField] private TextMeshProUGUI _fpsText;

        private readonly TimeIntervalTracker _tracker = new();
        private double _elapsedSum = 0.0;
        private const double UPDATE_INTERVAL = 500.0;

        float _mediaPipeFPS = 0.0f;

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

            if (_currentTarget.poseLandmarks == null) return;
            
            onPoseTargetUpdated?.Invoke(_currentTarget);

            if(_drawAnnotation)
            {
                annotation.Draw(_currentTarget.poseLandmarks, false);
            }

            double elapsed = _tracker.ElapsedSinceLastCall();
            _elapsedSum += elapsed;

            if(_elapsedSum > UPDATE_INTERVAL)
            {
                _elapsedSum = 0.0;
                _mediaPipeFPS = (float)(1000.0 / elapsed);

                FPSHolder.MediaPipeFPS = _mediaPipeFPS;

                if (_fpsText == null) return;

                if (_canDisplayFPS)
                {
                    _fpsText.text = $"MP FPS : {_mediaPipeFPS:F1}";
                }
                else
                {
                    _fpsText.text = $"";
                }
            }
        }
    }

    public class TimeIntervalTracker
    {
        private Stopwatch stopwatch;

        public TimeIntervalTracker()
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
        }

        public double ElapsedSinceLastCall()
        {
            double elapsed = stopwatch.Elapsed.TotalMilliseconds;
            stopwatch.Restart();
            return elapsed;
        }
    }
}
