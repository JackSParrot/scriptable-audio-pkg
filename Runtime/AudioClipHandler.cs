using UnityEngine;

namespace JackSParrot.Services.Audio
{
    public class AudioClipHandler : MonoBehaviour
    {
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if(Data != null)
                {
                    _source.volume = Data.Volume * _volume;
                }
            }
        }
        public SFXData Data { get; private set; } = null;
        public event System.Action<AudioClipHandler> OnDestroyed = h => { };
        
        public bool IsAlive => _elapsed < _duration || _looping;
        public int Id = -1;

        private float _volume = 1f;
        private Transform _toFollow = null;
        private Transform _transform;
        private AudioSource _source = null;
        private float _elapsed = 0f;
        private float _duration = 0f;
        private bool _looping = false;

        private void Awake()
        {
            _transform = transform;
            if(_source == null)
            {
                _source = gameObject.AddComponent<AudioSource>();
            }
        }

        public void Reset()
        {
            Data = null;
            _source.Stop();
            _looping = false;
            _elapsed = 0f;
            _duration = 0f;
            _toFollow = null;
            _transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
            Id = -1;
        }

        public void UpdateHandler(float deltaTime)
        {
            _elapsed += deltaTime;
            if(_toFollow != null)
            {
                _transform.position = _toFollow.position;
            }
        }

        public void Play(SFXData data)
        {
            this.Data = data;
            _duration = 9999f;
            #if ADDRESSABLE_ASSETS
            if (data.ReferencedClip.Asset != null)
            {
                OnLoaded(data.ReferencedClip.Asset as AudioClip);
            }
            else if(data.ReferencedClip.OperationHandle.IsValid())
            {
                if(data.ReferencedClip.OperationHandle.IsDone)
                {
                    OnLoaded(data.ReferencedClip.OperationHandle.Result as AudioClip);
                }
                else
                {
                    data.ReferencedClip.OperationHandle.Completed += h => OnLoaded(h.Result as AudioClip);
                }
            }
            else
            {
                try
                {
                    data.ReferencedClip.LoadAssetAsync().Completed += h => OnLoaded(h.Result);
                }
                catch (System.Exception) { }
            }
#else
            OnLoaded(data.Clip);
#endif
        }

        public void PlayAt(SFXData data, Vector3 position)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = position;
        }

        public void PlayFollowing(SFXData data, Transform parent)
        {
            Play(data);
            _source.spatialBlend = 1f;
            _transform.position = parent.position;
            _toFollow = parent;
        }

        private void OnLoaded(AudioClip clip)
        {
            if (clip == null)
            {
                Debug.LogError($"Cannot load audio clip: {Data.ClipName}");
                return;
            }
            gameObject.SetActive(true);
            gameObject.name = Data.ClipName;
            _source.volume = Data.Volume * _volume;
            _source.pitch = Data.Pitch;
            _source.clip = clip;
            _source.loop = Data.Loop;
            _source.spatialBlend = 0f;
            _source.Play();
            _toFollow = null;
            _looping = Data.Loop;
            _duration = clip.length;
        }

        private void OnDestroy()
        {
            OnDestroyed(this);
        }
    }
}

