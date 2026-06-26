using System;
using System.IO;
using System.Media;

// Same logic as Part 1 AudioPlayer — plays the greeting.wav on startup.
// SoundPlayer only works on Windows, which is fine for a WPF application.
namespace CyberBotGUI
{
    public class AudioPlayer
    {
        public static void PlayGreeting()
        {
            try
            {
                // Looks for the WAV file in the AudioPlayer subfolder next to the exe
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioPlayer", "greeting.wav");

                if (File.Exists(path))
                {
                    // PlaySync blocks the thread until the audio finishes.
                    // This is fine here because it runs on startup before the window loads.
                    SoundPlayer player = new SoundPlayer(path);
                    player.Play(); // use Play (async) so the window can load at the same time
                }
                // If the file is missing we just continue silently — no crash
            }
            catch (Exception)
            {
                // Audio failure should never stop the app from running
            }
        }
    }
}