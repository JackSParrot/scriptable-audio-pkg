using UnityEngine;
using System.Collections.Generic;
using JackSParrot.Utils;
using System;
using UnityEditor;

namespace JackSParrot.Services.Audio
{
    [CreateAssetMenu(fileName = "New SFXPlayer", menuName = "JackSParrot/Services/Audio/SFXPlayer", order = 1)]
    public class SFXPlayerSO : Service
    {
        [SerializeField] [Range(0f,1f)]private float _volume = 1f;
        [SerializeField] private AudioClipsStorer _clipStorer = null;
        [SerializeField] private EventDispatcherServiceSO _eventDispatcher = null;
        [SerializeField] private UpdaterServiceSO _updater = null;
        
        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                foreach(var handler in _handlers)
                {
                    handler.Volume = _volume;
                }
            }
        }
        
        private List<AudioClipHandler> _handlers = new List<AudioClipHandler>();
        private Dictionary<SFXData, int> _loadedClips = new Dictionary<SFXData, int>();
        private int _idGenerator = 0;
        private bool _initialized = false;

        public static SFXPlayerSO CreateInstance(AudioClipsStorer clipsStorer, UpdaterServiceSO updater, EventDispatcherServiceSO eventDispatcher)
        {
            SFXPlayerSO retVal = ScriptableObject.CreateInstance<SFXPlayerSO>();
            retVal._eventDispatcher = eventDispatcher;
            retVal._clipStorer = clipsStorer;
            retVal._updater = updater;
            return retVal;
        }

        public int Play(string clipName)
        {
            Initialize();
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipName));
            return handler.Id;
        }

        public int Play(string clipName, Vector3 at)
        {
            Initialize();
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipName), at);
            return handler.Id;
        }

        public int Play(string clipName, Transform toFollow)
        {
            Initialize();
            var handler = GetFreeHandler();
            handler.Play(GetClipToPlay(clipName), toFollow);
            return handler.Id;
        }

        public void StopPlaying(int id)
        {
            foreach (var handler in _handlers)
            {
                if (handler.Id == id)
                {
                    StopPlaying(handler);
                }
            }
        }

        public override void Dispose()
        {
            if (!_initialized)
            {
                return;
            }

            _initialized = false;
            _updater.UnscheduleUpdate(UpdateDelta);
            foreach(var handler in _handlers)
            {
                if (handler != null)
                {
                    Destroy(handler.gameObject);
                }
            }
            _handlers.Clear();
            _idGenerator = 0;
            _loadedClips.Clear();
            _eventDispatcher.RemoveListener<SceneManagementServiceSO.SceneUnloadedEvent>(OnSceneUnloaded);
        }

        public void ReleaseReferenceCache()
        {
#if ADDRESSABLE_ASSETS
            foreach(var kvp in _loadedClips)
            {
                if(kvp.Value < 1)
                {
                    if(kvp.Key.ReferencedClip.Asset != null)
                    {
                        try
                        {
                            kvp.Key.ReferencedClip.ReleaseAsset();
                        }
                        catch (Exception) { }
                    }
                }
            }
#endif
        }

        private void OnSceneUnloaded(SceneManagementServiceSO.SceneUnloadedEvent e)
        {
            ReleaseReferenceCache();
        }

        private AudioClipHandler CreateHandler()
        {
            var newHandler = new GameObject("sfx_handler").AddComponent<AudioClipHandler>();
            newHandler.Reset();
            _handlers.Add(newHandler);
            newHandler.Id = _idGenerator++;
            newHandler.Volume = _volume;
            DontDestroyOnLoad(newHandler.gameObject);
            return newHandler;
        }

        private AudioClipHandler GetFreeHandler()
        {
            foreach (var handler in _handlers)
            {
                if(!handler.IsAlive)
                {
                    handler.Id = _idGenerator++;
                    handler.Volume = _volume;
                    return handler;
                }
            }
            return CreateHandler();
        }

        private SFXData GetClipToPlay(string clipName)
        {
            foreach(var kvp in _loadedClips)
            {
                if(kvp.Key.ClipName.Equals(clipName, StringComparison.InvariantCultureIgnoreCase))
                {
                    _loadedClips[kvp.Key] += 1;
                    return kvp.Key;
                }
            }
            var sfx = _clipStorer.GetClipByName(clipName);
            _loadedClips.Add(sfx, 1);
            return sfx;
        }

        private void ReleasePlayingClip(SFXData clip)
        {
            if(clip != null && _loadedClips.ContainsKey(clip))
            {
                _loadedClips[clip] = Mathf.Max(0, _loadedClips[clip] -1);
            }
        }

        private void UpdateDelta(float deltaTime)
        {
            foreach (var handler in _handlers)
            {
                if (handler.IsAlive)
                {
                    handler.UpdateHandler(deltaTime);
                    if(!handler.IsAlive)
                    {
                        StopPlaying(handler);
                    }
                }
            }
        }

        private void StopPlaying(AudioClipHandler handler)
        {
            handler.Reset();
            ReleasePlayingClip(handler.Data);
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

            if (_eventDispatcher == null)
            {
                Debug.LogError($"Missing dependency: EventDispatcherServiceSO");
                return;
            }

            if (_updater == null)
            {
                Debug.LogError($"Missing dependency: UpdaterServiceSO");
                return;
            }
            for(int i = 0; i < 10; ++i)
            {
                CreateHandler();
            }
            _updater.ScheduleUpdate(UpdateDelta);
            _eventDispatcher.AddListener<SceneManagementServiceSO.SceneUnloadedEvent>(OnSceneUnloaded);
            _initialized = true;
        }

        private void OnDisable()
        {
            Dispose();
        }
    }
}
