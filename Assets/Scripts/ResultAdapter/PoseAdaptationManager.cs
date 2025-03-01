using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Mediapipe.Tasks.Vision.PoseLandmarker;

using UniVRM10;

namespace Mediapipe.Allocator
{
    public struct Rotation
    {
        public float x;
        public float y;
        public float z;

        public Rotation(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public readonly Vector3 ToVector3 => new(x, y, z);

        public override readonly string ToString()
        {
            return $"Rotation (x: {x}, y: {y}, z: {z})";
        }
    }

    public class LandmarksPacket
    {
        public List<Tasks.Components.Containers.NormalizedLandmark> Landmarks { get; private set; }
        public int Capacity { get; private set; }
        private readonly int[] Indexes;

        public LandmarksPacket(List<Tasks.Components.Containers.NormalizedLandmark> landmarks, int[] indexes)
        {
            Landmarks = landmarks;
            Indexes = indexes;
            Capacity = Indexes.Length;
        }

        public Tasks.Components.Containers.NormalizedLandmark GetLandmark(int index)
        {
            return Landmarks[Indexes[index]];
        }
    }

    public class PoseAdaptationManager : MonoBehaviour
    {
        GameObject _vrmObject;

        List<Tasks.Components.Containers.NormalizedLandmark> _landmarks;

        LandmarksPacket _chestPacket;
        ChestAdapter _chestAdapter;

        void Start()
        {
            Vrm10Instance vrmInstance = FindFirstObjectByType<Vrm10Instance>();
            if (vrmInstance != null)
            {
                _vrmObject = vrmInstance.gameObject;
                Debug.Log($"VRM Model found: {_vrmObject.name}");
            }
            else
            {
                Debug.Log("No VRM Model found.");
                return;
            }

            List<GameObject> _allChildren = new();
            GetAllChildren(_vrmObject.transform, _allChildren);

            _landmarks = Enumerable.Repeat(new Tasks.Components.Containers.NormalizedLandmark(), 33).ToList();

            _chestPacket = new(_landmarks, new int[2] { 11, 12 });
            _chestAdapter = new(_allChildren.FirstOrDefault(obj => obj.name.Contains("Chest")), _chestPacket);
        }

        private void GetAllChildren(Transform parent, List<GameObject> list)
        {
            foreach (Transform child in parent)
            {
                list.Add(child.gameObject);
                GetAllChildren(child, list);
            }
        }

        public void ApplyMediapipeResult(PoseLandmarkerResult recognitionResult)
        {

            for (int i = 0; i < _landmarks.Count; i++)
            {
                _landmarks[i] = recognitionResult.poseLandmarks[0].landmarks[i];
            }

            _chestAdapter.ForwardApply();
        }
    }
}// namespace Mediapipe.Allocator