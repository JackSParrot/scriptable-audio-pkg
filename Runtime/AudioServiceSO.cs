using JackSParrot.Utils;
using UnityEngine;

namespace JackSParrot.Services.Audio
{
    [CreateAssetMenu(fileName = "New AudioServiceSO", menuName = "JackSParrot/Services/Audio/AudioService", order = 0)]
    public class AudioServiceSO : Service
    {
        [SerializeField] [Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] [Range(0f, 1f)] private float _sfxVolume = 1f;
        [SerializeField] [Range(0f, 1f)] private float _musicVolume = 1f;

        [Header("Required")]
        [SerializeField] private MusicPlayerSO _musicPlayer = null;
        [SerializeField] private SFXPlayerSO _sfxPlayer = null;

        public static AudioServiceSO CreateInstance(MusicPlayerSO musicPlayer, SFXPlayerSO sfxPlayer)
        {
            AudioServiceSO retVal = ScriptableObject.CreateInstance<AudioServiceSO>();
            retVal._sfxPlayer = sfxPlayer;
            retVal._musicPlayer = musicPlayer;
            return retVal;
        }

        public float Volume
        {
            get=> _volume;
            set
            {
                _volume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
                _sfxPlayer.Volume = _sfxVolume * _volume;
            }
        }

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp(value, 0f, 1f);
                _musicPlayer.Volume = _musicVolume * _volume;
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set
            {
                _sfxVolume = Mathf.Clamp(value, 0f, 1f);
                _sfxPlayer.Volume = _sfxVolume * _volume;
            }
        }

        public void PlayMusic(string name) => _musicPlayer.Play(name);
        
        public void CrossFadeMusic(string clipName, float duration = 0.3f) => _musicPlayer.CrossFade(clipName, duration);

        public void PlaySFX(string clipName) => _sfxPlayer.Play(clipName);
        
        public void PlaySFXAt(string clipName, Vector3 at) => _sfxPlayer.PlayAt(clipName, at);
        
        public void PlaySFXFollowing(string clipName, Transform toFollow) => _sfxPlayer.PlayFollowing(clipName, toFollow);
        
        public int StartPlayingPlaySFX(string clipName) => _sfxPlayer.Play(clipName);

        public int StartPlayingPlaySFXAt(string clipName, Vector3 at) => _sfxPlayer.PlayAt(clipName, at);

        public int StartPlayingPlaySFXFollowing(string clipName, Transform toFollow) => _sfxPlayer.PlayFollowing(clipName, toFollow);

        public void StopPlayingSFX(int id) => _sfxPlayer.StopPlaying(id);
        
        public override void Dispose() { }

        protected override void Reset() { }
    }
}
