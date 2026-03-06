using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StreamTweak
{
    public class SessionEntry
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TriggerMode { get; set; } = "Auto"; // "Auto" | "Manual"
        public string OriginalSpeed { get; set; } = string.Empty;

        [JsonIgnore]
        public string DurationDisplay
        {
            get
            {
                if (EndTime == null) return "Active";
                var d = EndTime.Value - StartTime;
                return d.TotalMinutes >= 1
                    ? $"{(int)d.TotalMinutes}m {d.Seconds}s"
                    : $"{d.Seconds}s";
            }
        }

        [JsonIgnore]
        public string StartTimeDisplay => StartTime.ToString("dd/MM  HH:mm");
    }

    public static class SessionLogger
    {
        private const int MaxSessions = 10;
        private static readonly string LogPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "StreamTweak", "sessions.json");

        private static string? _activeSessionId = null;

        public static void StartSession(string triggerMode, string originalSpeed)
        {
            try
            {
                var sessions = Load();
                var entry = new SessionEntry
                {
                    StartTime = DateTime.Now,
                    TriggerMode = triggerMode,
                    OriginalSpeed = originalSpeed
                };

                _activeSessionId = entry.Id;
                sessions.Insert(0, entry);

                if (sessions.Count > MaxSessions)
                    sessions = sessions.Take(MaxSessions).ToList();

                Save(sessions);
            }
            catch { }
        }

        public static void EndSession()
        {
            if (_activeSessionId == null) return;
            try
            {
                var sessions = Load();
                var entry = sessions.FirstOrDefault(s => s.Id == _activeSessionId);
                if (entry != null)
                {
                    entry.EndTime = DateTime.Now;
                    Save(sessions);
                }
                _activeSessionId = null;
            }
            catch { }
        }

        public static List<SessionEntry> Load()
        {
            try
            {
                if (!File.Exists(LogPath)) return new List<SessionEntry>();
                string json = File.ReadAllText(LogPath);
                return JsonSerializer.Deserialize<List<SessionEntry>>(json) ?? new List<SessionEntry>();
            }
            catch { return new List<SessionEntry>(); }
        }

        private static void Save(List<SessionEntry> sessions)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.WriteAllText(LogPath, JsonSerializer.Serialize(sessions,
                new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
