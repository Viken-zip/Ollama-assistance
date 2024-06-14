using System;

namespace Ollama_assistance.Services
{
    public class GlobalStateService
    {
        private static GlobalStateService _instance;

        public static GlobalStateService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalStateService();
                }
                return _instance;
            }
        }

        private GlobalStateService()
        {
            _isMicrophoneOn = false;
        }

        private bool _isMicrophoneOn;

        public bool IsMicrophoneOn
        {
            get { return _isMicrophoneOn; }
            set
            {
                if (_isMicrophoneOn != value)
                {
                    _isMicrophoneOn = value;
                    OnMicrophoneStateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public event EventHandler OnMicrophoneStateChanged;
    }
}