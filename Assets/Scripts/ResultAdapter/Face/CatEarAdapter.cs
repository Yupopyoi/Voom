using UnityEngine;
using System.Collections.ObjectModel;

namespace Mediapipe.Allocator
{
    public class CatEarAdapter : EmotionAdapterBase
    {
        private readonly ReadOnlyCollection<float> _eyeControlValues;

        private readonly Transform _transformEarR;
        private readonly Transform _transformEarL;

        private bool _haveCatEar = false;

        #region General Properties

        // Whether the cat ears move spontaneously in response to facial expressions
        // There is still passive ear movement in response to body movement.
        public bool _canMoveResponseToExpression = true;

        // Amount of ear growth when looking surprised (Greater than 1)
        public float _growthAmount = 1.05f;

        #endregion

        public CatEarAdapter(GameObject earObjectR, GameObject earObjectL, LandmarksPacket landmarksPacket, ReadOnlyCollection<float> eyeControlValues)
            : base(new GameObject(), landmarksPacket)
        {
            _eyeControlValues = eyeControlValues;

            bool haveCatEarR = !IsEmptyGameObject(earObjectR);
            bool haveCatEarL = !IsEmptyGameObject(earObjectL);

            if (haveCatEarR && haveCatEarL)
            {
                _haveCatEar = true;
                _transformEarR = earObjectR.GetComponent<Transform>();
                _transformEarL = earObjectL.GetComponent<Transform>();
            }
        }

        public override void ForwardApply()
        {
            if (!_haveCatEar) return;

            if (!_canMoveResponseToExpression) return;

            float surpriseValue = Sigmoid((_eyeControlValues[4] + _eyeControlValues[5]) * 0.5f, 0.3f) * 0.01f; // Average & [0,100] -> [0,1]

            float growthValue = surpriseValue * _growthAmount > 1.0f ? surpriseValue * _growthAmount : 1.0f;

            _transformEarR.localScale = new Vector3(1.0f, growthValue, 1.0f);
            _transformEarL.localScale = new Vector3(1.0f, growthValue, 1.0f);
        }
    }
}
