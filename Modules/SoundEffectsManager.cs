using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace TownOfHost
{
    // Class to preload all audio/sound effects that are contained in the embedded resources.
    // The effects are made available through the soundEffects Dict / the get and the play methods.
    // all TOR Code
    public static class SoundEffectsManager

    {
        private static Dictionary<string, AudioClip> soundEffects;

        public static void Load()
        {
            //Logger.Info("SoundEffectsManager. Sound Check 1", "SoundEffectsManager");
            soundEffects = new Dictionary<string, AudioClip>();
            Assembly assembly = Assembly.GetExecutingAssembly();
            string[] resourceNames = assembly.GetManifestResourceNames();
           //Logger.Info("SoundEffectsManager. Sound Check 2", "SoundEffectsManager");
            foreach (string resourceName in resourceNames)
            {
                if (resourceName.Contains("TownOfHost.Resources.SoundEffects.") && resourceName.Contains(".raw"))
                {
                    Logger.Info($"Found Song! {resourceName}", "SoundEffectsManager");
                    soundEffects.Add(resourceName, Helpers.loadAudioClipFromResources(resourceName));
                }
            }
            foreach(var pair in soundEffects)
            {
                Logger.Info($"Pair.Key: {pair.Key.ToString()}", "Pair.Key");
                Logger.Info($"Pair.Value: {pair.Value.ToString()}", "Pair.Value");
                if (pair.Value == null)
                {
                    Logger.Info($"Pair.Value is equal to null.", "Pair.Value");
                }
            }
        }

        public static AudioClip get(string path)
        {
            // Convenience: As as SoundEffects are stored in the same folder, allow using just the name as well
            if (!path.Contains(".")) path = "TownOfHost.Resources.SoundEffects.music." + path + ".raw";
            Logger.Info($"Song Path: {path}", "SoundEffectsManager");
            if (soundEffects.TryGetValue(path, out AudioClip returnValue))
            {
                if (returnValue == null)
                {
                    Logger.Info($"Clip is Equal to null.", "SoundEffectsManager (get function)");
                    soundEffects[path] = Helpers.loadAudioClipFromResources(path, "exampleClip");
                    returnValue = soundEffects[path];
                }
                return returnValue;
            }
            else {
               Logger.Info($"Clip is Equal to null.", "SoundEffectsManager (get function)");
               return null;
            }
        }


        public static void play(string path, bool loop = false, float volume = 0.8f)
        {
            AudioClip clipToPlay = get(path);
            if (clipToPlay == null)
            {
                Logger.Info($"Clip is Equal to null.", "SoundEffectsManager (play function)");
                var newpath = path;
                if (!newpath.Contains(".")) newpath = "TownOfHost.Resources.SoundEffects.music." + newpath + ".raw";
                //clipToPlay = Helpers.loadAudioClipFromResources(newpath, "exampleClip");
            }
            // if (false) clipToPlay = get("exampleClip"); for april fools?
            stop(path);
            SoundManager.Instance.PlaySound(clipToPlay, loop, volume);
        }

        public static void stop(string path)
        {
            AudioClip clipToStop = get(path);
            if (clipToStop == null)
            {
                Logger.Info($"Clip is Equal to null.", "SoundEffectsManager (stop function)");
                var newpath = path;
                if (!newpath.Contains(".")) newpath = "TownOfHost.Resources.SoundEffects.music." + newpath + ".raw";
                //clipToStop = Helpers.loadAudioClipFromResources(newpath, "exampleClip");
            }
            SoundManager.Instance.StopSound(clipToStop);
        }

        public static void stopAll()
        {
            if (soundEffects == null) return;
            foreach (var path in soundEffects.Keys) stop(path);
        }
    }
}