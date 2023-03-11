using System;
using System.Reflection;
using Il2CppInterop;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Hazel;
using Il2CppInterop.Runtime;
using System.Threading.Tasks;
using System.Net;
using System.Globalization;

namespace TownOfHost
{

    // https://github.com/Eisbison/TheOtherRoles/blob/main/TheOtherRoles/Helpers.cs
    public static class Helpers
    {
        public static Dictionary<string, Sprite> CachedSprites = new();
        public static Sprite LoadSpriteFromResources(string path, float pixelsPerUnit)
        {

            try
            {
                var texture = LoadTextureFromResources(path);
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            }
            catch
            {
                Logger.Error($"Error loading sprite from path: {path}", "LoadSprite");
            }
            return null;
        }

        public static Texture2D LoadTextureFromResources(string path)
        {

            try
            {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                var assembly = Assembly.GetExecutingAssembly();
                var stream = assembly.GetManifestResourceStream(path);
                var byteTexture = new byte[stream.Length];
                _ = stream.Read(byteTexture, 0, (int)stream.Length);
                LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                Logger.Error($"Error loading texture from resources: {path}", "LoadTexture");
            }
            return null;
        }

        public static Sprite LoadSpriteFromResourcesTOR(string path, float pixelsPerUnit)
        {
            try
            {
                if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
                Texture2D texture = LoadTextureFromResourcesTOR(path);
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
                sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
                return CachedSprites[path + pixelsPerUnit] = sprite;
            }
            catch
            {
                System.Console.WriteLine("Error loading sprite from path: " + path);
            }
            return null;
        }

        public static unsafe Texture2D LoadTextureFromResourcesTOR(string path)
        {
            try
            {
                Texture2D texture = new Texture2D(2, 2, TextureFormat.ARGB32, true);
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var length = stream.Length;
                var byteTexture = new Il2CppStructArray<byte>(length);
                stream.Read(new Span<byte>(IntPtr.Add(byteTexture.Pointer, IntPtr.Size * 4).ToPointer(), (int)length));
                ImageConversion.LoadImage(texture, byteTexture, false);
                return texture;
            }
            catch
            {
                System.Console.WriteLine("Error loading texture from resources: " + path);
            }
            return null;
        }

        public static AudioClip loadAudioClipFromResources(string path, string clipName = "UNNAMED_TOR_AUDIO_CLIP")
        {
            // must be "raw (headerless) 2-channel signed 32 bit pcm (le)" (can e.g. use Audacity� to export)
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Stream stream = assembly.GetManifestResourceStream(path);
                var byteAudio = new byte[stream.Length];
                _ = stream.Read(byteAudio, 0, (int)stream.Length);
                float[] samples = new float[byteAudio.Length / 4]; // 4 bytes per sample
                int offset;
                for (int i = 0; i < samples.Length; i++)
                {
                    offset = i * 4;
                    samples[i] = (float)BitConverter.ToInt32(byteAudio, offset) / Int32.MaxValue;
                }
                int channels = 2;
                int sampleRate = 48000;
                AudioClip audioClip = AudioClip.Create(clipName, samples.Length, channels, sampleRate, false);
                audioClip.SetData(samples, 0);
                Logger.Info("Got audio clip successfully.", "Helpers.loadAudioClipFromResources");
                return audioClip;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString(), "Error Loading Audio Clip.");
                Logger.SendInGame($"Error loading audio clip \\/\n{ex}");
                System.Console.WriteLine("Error loading AudioClip from resources: " + path);
                return null;
            }
            /* Usage example:
            AudioClip exampleClip = Helpers.loadAudioClipFromResources("TownOfHost.Resources.exampleClip.raw");
            SoundManager.Instance.PlaySound(exampleClip, false, 0.8f);
            */
        }

        private delegate bool DelegateLoadImage(IntPtr tex, IntPtr data, bool markNonReadable);
        private static DelegateLoadImage _callLoadImage;

        private static bool LoadImage(Texture2D tex, byte[] data, bool markNonReadable)
        {
            _callLoadImage ??= IL2CPP.ResolveICall<DelegateLoadImage>("UnityEngine.ImageConversion::LoadImage");
            var il2cppArray = (Il2CppStructArray<byte>)data;

            return _callLoadImage.Invoke(tex.Pointer, il2cppArray.Pointer, markNonReadable);
        }

        public static string ColorString(Color c, string s)
        {
            return $"<color=#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}{ToByte(c.a):X2}>{s}</color>";
        }

        private static byte ToByte(float f)
        {
            f = Mathf.Clamp01(f);
            return (byte)(f * 255);
        }
    }

    public class PlayerVersion
    {
        public readonly Version version;
        public readonly string tag;
        public PlayerVersion(string ver, string tag_str)
        {
            version = Version.Parse(ver);
            tag = tag_str;
        }
        public bool IsEqual(PlayerVersion pv)
        {
            return pv.version == version && pv.tag == tag;
        }
    }
}