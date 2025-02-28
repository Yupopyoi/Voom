using UnityEngine;
using System.Collections;

namespace Screen.Stream
{
    public class CameraCapture : MonoBehaviour
    {
        Camera _streamCamera;
        [SerializeField] RenderTexture _renderTexture;

        void Start()
        {
            _streamCamera = GetComponent<Camera>();
            _streamCamera.targetTexture = _renderTexture;

            StartCoroutine(CaptureLoop());
        }

        IEnumerator CaptureLoop()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();

                RenderTexture.active = _renderTexture;
            }
        }

        private void OnApplicationQuit()
        {
            _renderTexture = null;
            RenderTexture.active = null;
        }
    }
}// namespace Screen.Stream
