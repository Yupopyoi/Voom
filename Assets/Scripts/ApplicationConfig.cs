using UnityEngine;

namespace GeneralConfig
{
    public class ApplicationConfig : MonoBehaviour
    {
        [SerializeField] bool _runInBackground = false;

        public bool RunInBackgroundMode
        {
            get
            {
                return _runInBackground;
            }
            set
            {
                _runInBackground = value;
                ChangeBackgroundMode();
            }
        }

        void ChangeBackgroundMode()
        {
            Application.runInBackground = _runInBackground;
        }

        void Start()
        {
            ChangeBackgroundMode();
        }
    }
}// namespace GeneralConfig
