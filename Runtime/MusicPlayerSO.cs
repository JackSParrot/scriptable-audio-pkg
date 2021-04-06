using System.Collections;
using UnityEngine;
using System;
using JackSParrot.Utils;

namespace JackSParrot.Services.Audio
{
    [CreateAssetMenu(fileName = "New MusicPlayer", menuName = "JackSParrot/Services/Audio/MusicPlayer", order = 1)]
    public class MusicPlayerSO : Service
    {
        [SerializeField] [Range(0f,1f)] private float _volume = 1f;
        [SerializeField] private AudioClipsStorer _clipStorer = null;
        [SerializeField] private CoroutineRunnerServiceSO _coroutineRunner = null;
        
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if(_playingClip != null)
                {
                    _source.volume = _playingClip.Volume * _volume;
                }
            }
        }

        private AudioSource _source = null;
        private SFXData _playingClip = null;
        private bool _initialized = false;

        public static MusicPlayerSO CreateInstance(AudioClipsStorer clipsStorer, CoroutineRunnerServiceSO coroutineRunner)
        {
            MusicPlayerSO retVal = ScriptableObject.CreateInstance<MusicPlayerSO>();
            retVal._coroutineRunner = coroutineRunner;
            retVal._clipStorer = clipsStorer;
            return retVal;
        }

        public void Play(string clipName)
        {
            Initialize();
            if(_playingClip != null && string.Equals(clipName, _playingClip.ClipName, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }
            if(_playingClip != null)
            {
                StopAndReleaseClip();
            }

            if(string.IsNullOrEmpty(clipName))
            {
                _playingClip = null;
                return;
            }
            var clip = _clipStorer.GetClipByName(clipName);
            _playingClip = clip;
#if ADDRESSABLE_ASSETS
            clip.ReferencedClip.LoadAssetAsync<AudioClip>().Completed += h => OnClipLoaded(h.Result);
#else
            OnClipLoaded((_playingClip.Clip));
#endif
        }

        public void CrossFade(string clipName, float duration)
        {
            _coroutineRunner.StopAllCoroutines(this);
            _coroutineRunner.StartCoroutine(this, CrossFadeCoroutine(clipName, duration));
        }

        public override void Dispose()
        {
            if (!_initialized)
            {
                return;
            }
            if (_playingClip != null)
            {
                StopAndReleaseClip();
            }
            _coroutineRunner.StopAllCoroutines(this);
            _initialized = false;
            Destroy(_source.gameObject);
        }

        protected override void Reset()
        {
            Initialize();
        }

        private void OnClipLoaded(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError($"Cannot load audio clip: {_playingClip.ClipName}");
                return;
            }
            _source.clip = clip;
            _source.loop = _playingClip.Loop;
            _source.pitch = _playingClip.Pitch;
            _source.volume = _playingClip.Volume * _volume;
            _source.Play();
        }

        private void StopAndReleaseClip()
        {
            _source.Stop();
#if ADDRESSABLE_ASSETS
                _playingClip.ReferencedClip.ReleaseAsset();
#endif
        }

        private IEnumerator CrossFadeCoroutine(string fadeTo, float duration)
        {
            float halfDuraion = duration * 0.5f;
            _coroutineRunner.StartCoroutine(this, FadeOutCoroutine(halfDuraion));
            yield return new WaitForSeconds(halfDuraion);
            Play(fadeTo);
            _coroutineRunner.StartCoroutine(this, FadeInCoroutine(halfDuraion));
        }

        private IEnumerator FadeOutCoroutine(float duration)
        {
            float remaining = duration;
            while(remaining > 0)
            {
                _source.volume = _volume * remaining / duration;
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        private IEnumerator FadeInCoroutine(float duration)
        {
            float remaining = duration;
            while(remaining > 0)
            {
                _source.volume = _volume * (1f - (remaining / duration));
                yield return null;
                remaining -= Time.deltaTime;
            }
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (_clipStorer == null)
            {
                Debug.LogError($"Missing dependency: AudioClipStorer");
                return;
            }
            if (_coroutineRunner == null)
            {
                Debug.LogError($"Missing dependency: CoroutineRunnerServiceSO");
                return;
            }
            _source = new GameObject("MusicPlayer").AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            DontDestroyOnLoad(_source.gameObject);
            _initialized = true;
        }
    }
}
