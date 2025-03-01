using UnityEngine;

namespace Mediapipe.Allocator
{
    public class PartAssignor : MonoBehaviour
    {
        [SerializeField] GameObject VRM_Model;

        public Transform FetchTransformOfPart(string partName)
        {
            if (VRM_Model == null) return null;

            foreach (Transform child in GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains(partName))
                {
                    return child;
                }
            }
            return null;
        }
    }
}// namespace Mediapipe.Allocator
