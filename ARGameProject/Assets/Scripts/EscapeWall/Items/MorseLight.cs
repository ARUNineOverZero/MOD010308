using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace EscapeWall.Items
{
    public class MorseLight : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light _light;
        [SerializeField] private Renderer _renderer;
        [Header("Emission")]
        [SerializeField] private Color _emissionColor = Color.white;
        [SerializeField] private float _emissionIntensity = 2f;

        [Header("Message")]
        [SerializeField] private string _message = "SOS";
        [SerializeField] private bool _playOnStart = true;

        [Header("Timing")]
        [SerializeField] private float _dotDuration = 0.2f;
        [SerializeField] private float _dashDuration = 0.6f;

        [Header("Gaps (multipliers of dot duration)")]
        [SerializeField] private float _symbolGapMultiplier = 1f;
        [SerializeField] private float _letterGapMultiplier = 3f;
        [SerializeField] private float _wordGapMultiplier = 7f;

        [Header("State")]
        [SerializeField] private bool _startOn = false;

        private Coroutine _blinkCoroutine;
        private string _currentMorse;
        private bool _isPlaying;
        private bool _isOn;

        private MaterialPropertyBlock _propBlock;
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        private static readonly Dictionary<char, string> MorseMap = new()
        {
            {'A', ".-"},    {'B', "-..."},  {'C', "-.-."}, {'D', "-.."},
            {'E', "."},     {'F', "..-."},  {'G', "--."},  {'H', "...."},
            {'I', ".."},    {'J', ".---"},  {'K', "-.-"},  {'L', ".-.."},
            {'M', "--"},    {'N', "-."},    {'O', "---"},  {'P', ".--."},
            {'Q', "--.-"},  {'R', ".-."},   {'S', "..."},  {'T', "-"},
            {'U', "..-"},   {'V', "...-"},  {'W', ".--"},  {'X', "-..-"},
            {'Y', "-.--"},  {'Z', "--.."},
            {'0', "-----"}, {'1', ".----"}, {'2', "..---"}, {'3', "...--"},
            {'4', "....-"}, {'5', "....."}, {'6', "-...."}, {'7', "--..."},
            {'8', "---.."}, {'9', "----."},
            {'.', ".-.-.-"},{',', "--..--"},{'?', "..--.."},{'\'', ".----."},
            {'!', "-.-.--"},{'/', "-..-."}, {'(', "-.--."}, {')', "-.--.-"},
            {'&', ".-..."}, {':', "---..."},{';', "-.-.-."},{'=', "-...-"},
            {'+', ".-.-."}, {'-', "-....-"},{'_', "..--.-"},{'"', ".-..-."},
            {'$', "...-..-"},{'@', ".--.-."}
        };

        private void Awake()
        {
            if (_light == null)
                _light = GetComponent<Light>();

            if (_renderer == null)
                _renderer = GetComponent<Renderer>();

            _propBlock = new MaterialPropertyBlock();
            _isOn = _startOn;
            SetLight(_isOn);
        }

        private void OnEnable()
        {
            SetLight(_isOn);

            if (_playOnStart && !string.IsNullOrWhiteSpace(_message))
                PlayText(_message);
        }

        private void OnDisable()
        {
            Stop();
        }

        private void Start()
        {
            if (_playOnStart && !string.IsNullOrWhiteSpace(_message))
                PlayText(_message);
        }

        public void PlayText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;
            _message = text;
            PlayMorse(TextToMorse(text));
        }

        public void PlayMorse(string morse)
        {
            Stop();
            _currentMorse = morse;
            _isPlaying = true;
            _blinkCoroutine = StartCoroutine(BlinkMorseLoop());
        }

        public void Stop()
        {
            _isPlaying = false;

            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }

            SetLight(false);
        }

        private IEnumerator BlinkMorseLoop()
        {
            while (_isPlaying && !string.IsNullOrEmpty(_currentMorse))
            {
                foreach (char c in _currentMorse)
                {
                    if (!_isPlaying) yield break;

                    switch (c)
                    {
                        case '.':
                            yield return Blink(_dotDuration);
                            yield return Gap(_symbolGapMultiplier);
                            break;

                        case '-':
                            yield return Blink(_dashDuration);
                            yield return Gap(_symbolGapMultiplier);
                            break;

                        case ' ':
                            yield return Gap(_letterGapMultiplier - _symbolGapMultiplier);
                            break;

                        case '/':
                        case '|':
                            yield return Gap(_wordGapMultiplier - _symbolGapMultiplier);
                            break;
                    }
                }

                yield return Gap(_wordGapMultiplier);
            }

            SetLight(false);
            _blinkCoroutine = null;
        }

        private IEnumerator Blink(float duration)
        {
            SetLight(true);
            yield return new WaitForSeconds(duration);
            SetLight(false);
        }

        private IEnumerator Gap(float multiplier)
        {
            if (multiplier > 0f)
                yield return new WaitForSeconds(_dotDuration * multiplier);
        }

        private void SetLight(bool on)
        {
            _isOn = on;

            if (_light != null)
                _light.enabled = on;

            if (_renderer != null && _propBlock != null)
            {
                _renderer.GetPropertyBlock(_propBlock);

                if (on)
                    _propBlock.SetColor(EmissionColorId, _emissionColor * _emissionIntensity);
                else
                    _propBlock.SetColor(EmissionColorId, Color.black);

                _renderer.SetPropertyBlock(_propBlock);
            }
        }

        public static string TextToMorse(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            var sb = new StringBuilder();
            text = text.ToUpperInvariant();

            foreach (char c in text)
            {
                if (c == ' ')
                {
                    sb.Append("/ ");
                    continue;
                }

                if (MorseMap.TryGetValue(c, out string code))
                {
                    sb.Append(code);
                    sb.Append(' ');
                }
            }

            return sb.ToString().Trim();
        }
    }
}