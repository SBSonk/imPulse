using UnityEngine;

namespace ImPulse.Helpers
{
    public static class AudioHelper
    {
        /// <summary>
        /// Spawns a temporary 2D AudioSource that destroys itself after playing.
        /// Useful for pooled objects or objects that are destroyed instantly.
        /// </summary>
        public static void PlayDetached2DSound(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return;
            
            GameObject audioObj = new GameObject("TempAudio_" + clip.name);
            AudioSource source = audioObj.AddComponent<AudioSource>();
            source.clip = clip;
            source.volume = volume;
            source.spatialBlend = 0f; 
            source.Play();
            
            Object.Destroy(audioObj, clip.length + 0.1f);
        }
    }
}