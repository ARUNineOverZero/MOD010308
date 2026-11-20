using UnityEngine;

namespace EscapeWall.Items
{
    [RequireComponent(typeof(AudioSource))]
    public class MorseToneSource : MonoBehaviour
    {
        [Header("Tone")]
        [SerializeField] private float _frequency = 800f;
        [SerializeField][Range(0f, 1f)] private float _volume = 0.4f;

        [Header("Spatial")]
        [SerializeField] private float _minDistance = 1f;
        [SerializeField] private float _maxDistance = 20f;

        [SerializeField] private AudioSource _audioSource;
        private bool _isOn;

        private void Awake()
        {
            if(_audioSource == null)
                _audioSource = GetComponent<AudioSource>();

            SetupAudioSource();
            GenerateToneClip();
        }

        private void SetupAudioSource()
        {
            _audioSource.playOnAwake = false;
            _audioSource.loop = true;
            _audioSource.spatialBlend = 1f;
            _audioSource.minDistance = _minDistance;
            _audioSource.maxDistance = _maxDistance;
            _audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
            _audioSource.volume = 0f;
        }

        private void GenerateToneClip()
        {
            const int sampleRate = 44100;
            const float duration = 1f;
            int sampleCount = (int)(sampleRate * duration);

            var clip = AudioClip.Create("MorseTone", sampleCount, 1, sampleRate, false);
            var data = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = Mathf.Sin(2f * Mathf.PI * _frequency * t);
            }

            clip.SetData(data, 0);
            _audioSource.clip = clip;
        }

        public void SetTone(bool on)
        {
            if (_isOn == on) return;
            _isOn = on;

            if (on)
            {
                if (!_audioSource.isPlaying)
                    _audioSource.Play();
                _audioSource.volume = _volume;
            }
            else
            {
                _audioSource.volume = 0f;
            }
        }

        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            if (_isOn)
                _audioSource.volume = _volume;
        }

        private void OnDisable() => SetTone(false);
    }
}
