using System;
using System.Collections.Generic;

namespace Mod
{
    public class BossHistoryEntry
    {
        public string Name;
        public string MapName;
        public int MapID;
        public int ZoneID;
        public DateTime AppearedAt;
        public bool IsDead;
        public string Killer;
        public DateTime? DiedAt;
        public string DeathMessage;
    }

    public class BossDeathInfo
    {
        public bool isDeath;
        public string killer = "Chưa có thông tin";
        public string bossName = "";
    }

    public static class BossHistory
    {
        private static readonly List<BossHistoryEntry> entries = new();

        public static int Count => entries.Count;

        public static BossHistoryEntry Get(int index)
        {
            return index >= 0 && index < entries.Count ? entries[index] : null;
        }

        public static void Clear()
        {
            entries.Clear();
        }

        public static void RecordChat(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            string text = message.Trim();
            string lowerText = text.ToLowerInvariant();
            if (lowerText.Contains("xuất hiện") && (lowerText.Contains("boss") || text.StartsWith("!")))
            {
                try
                {
                    ShowBoss boss = new(text);
                    entries.Add(new BossHistoryEntry
                    {
                        Name = boss.nameBoss,
                        MapName = boss.mapName,
                        MapID = boss.mapID,
                        ZoneID = boss.zoneID,
                        AppearedAt = DateTime.Now,
                        Killer = "Chưa có thông tin",
                        DeathMessage = string.Empty
                    });
                }
                catch
                {
                }

                return;
            }

            BossDeathInfo deathInfo = ParseDeath(text);
            if (deathInfo.isDeath)
            {
                RecordDeath(text, deathInfo);
            }
        }

        private static void RecordDeath(string message, BossDeathInfo deathInfo)
        {
            BossHistoryEntry entry = null;

            if (!string.IsNullOrEmpty(deathInfo.bossName))
            {
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (!entries[i].IsDead && !string.IsNullOrEmpty(entries[i].Name) &&
                        (entries[i].Name.Equals(deathInfo.bossName, StringComparison.OrdinalIgnoreCase) ||
                         entries[i].Name.IndexOf(deathInfo.bossName, StringComparison.OrdinalIgnoreCase) >= 0 ||
                         deathInfo.bossName.IndexOf(entries[i].Name, StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        entry = entries[i];
                        break;
                    }
                }
            }

            if (entry == null)
            {
                for (int i = entries.Count - 1; i >= 0; i--)
                {
                    if (!entries[i].IsDead)
                    {
                        entry = entries[i];
                        break;
                    }
                }
            }

            if (entry == null)
            {
                return;
            }

            entry.IsDead = true;
            entry.DiedAt = DateTime.Now;
            entry.DeathMessage = message;
            entry.Killer = deathInfo.killer;
        }

        public static bool IsDeathMessage(string lowerMessage)
        {
            return lowerMessage.Contains("tiêu diệt")
                || lowerMessage.Contains("hạ gục")
                || lowerMessage.Contains("đánh bại")
                || lowerMessage.Contains("bị hạ")
                || lowerMessage.Contains("đã chết")
                || lowerMessage.Contains("ghê chưa");
        }

        public static BossDeathInfo ParseDeath(string message)
        {
            BossDeathInfo info = new BossDeathInfo();
            if (string.IsNullOrWhiteSpace(message))
            {
                return info;
            }

            string text = message.Trim();
            while (text.StartsWith("!") || text.StartsWith(":") || text.StartsWith(" "))
            {
                text = text.Substring(1).Trim();
            }
            if (text.StartsWith("["))
            {
                int closeBracket = text.IndexOf(']');
                if (closeBracket > 0 && closeBracket < text.Length - 1)
                {
                    text = text.Substring(closeBracket + 1).Trim();
                }
            }
            while (text.StartsWith("!") || text.StartsWith(":") || text.StartsWith(" "))
            {
                text = text.Substring(1).Trim();
            }

            string lower = text.ToLowerInvariant();
            if (!IsDeathMessage(lower))
            {
                return info;
            }

            info.isDeath = true;
            info.killer = "Chưa có thông tin";
            info.bossName = "";

            // Passive pattern: "...đã bị tiêu diệt bởi {Killer}" or "...bởi {Killer}" or "...do {Killer}"
            string[] passiveMarkers = new string[] { " bởi ", " do ", " nguoi choi ", " người chơi " };
            foreach (string pm in passiveMarkers)
            {
                int pIdx = lower.IndexOf(pm, StringComparison.Ordinal);
                if (pIdx >= 0)
                {
                    string killerPart = text.Substring(pIdx + pm.Length).Trim(' ', '.', ':', '!', ',', '。');
                    if (!string.IsNullOrEmpty(killerPart))
                    {
                        info.killer = killerPart;
                    }
                    string bossPart = text.Substring(0, pIdx).Trim();
                    string[] passiveEndings = new string[] {
                        " đã bị tiêu diệt",
                        " bị tiêu diệt",
                        " đã bị hạ gục",
                        " bị hạ gục",
                        " đã bị đánh bại",
                        " bị đánh bại",
                        " đã chết",
                        " đã bị hạ",
                        " bị hạ"
                    };
                    foreach (string pe in passiveEndings)
                    {
                        int peIdx = bossPart.IndexOf(pe, StringComparison.OrdinalIgnoreCase);
                        if (peIdx >= 0)
                        {
                            bossPart = bossPart.Substring(0, peIdx).Trim();
                            break;
                        }
                    }
                    info.bossName = CleanBossName(bossPart);
                    return info;
                }
            }

            // Active pattern: "{Killer} vừa tiêu diệt được {Boss}, ghê chưa ghê chưa.."
            string[] activeMarkers = new string[] {
                " vừa tiêu diệt được ",
                " đã tiêu diệt được ",
                " vừa hạ gục được ",
                " đã hạ gục được ",
                " vừa đánh bại được ",
                " đã đánh bại được ",
                " vừa tiêu diệt ",
                " đã tiêu diệt ",
                " vừa hạ gục ",
                " đã hạ gục ",
                " vừa đánh bại ",
                " đã đánh bại ",
                " tiêu diệt được ",
                " hạ gục được ",
                " tiêu diệt ",
                " hạ gục ",
                " đánh bại "
            };

            foreach (string am in activeMarkers)
            {
                int amIdx = lower.IndexOf(am, StringComparison.Ordinal);
                if (amIdx >= 0)
                {
                    string killerPart = text.Substring(0, amIdx).Trim(' ', '.', ':', '!', ',');
                    if (killerPart.StartsWith("Người chơi ", StringComparison.OrdinalIgnoreCase))
                    {
                        killerPart = killerPart.Substring(11).Trim();
                    }
                    else if (killerPart.StartsWith("người chơi ", StringComparison.OrdinalIgnoreCase))
                    {
                        killerPart = killerPart.Substring(11).Trim();
                    }

                    if (killerPart.EndsWith(" vừa", StringComparison.OrdinalIgnoreCase))
                    {
                        killerPart = killerPart.Substring(0, killerPart.Length - 4).Trim();
                    }
                    else if (killerPart.EndsWith(" đã", StringComparison.OrdinalIgnoreCase))
                    {
                        killerPart = killerPart.Substring(0, killerPart.Length - 3).Trim();
                    }

                    if (!string.IsNullOrEmpty(killerPart))
                    {
                        info.killer = killerPart;
                    }

                    string afterAm = text.Substring(amIdx + am.Length).Trim();
                    string[] cutoffs = new string[] { " nhận được ", " nhan duoc ", " ghê chưa", " ghe chua", ",", ".", "!" };
                    int earliestCut = afterAm.Length;
                    foreach (string co in cutoffs)
                    {
                        int cIdx = afterAm.IndexOf(co, StringComparison.OrdinalIgnoreCase);
                        if (cIdx >= 0 && cIdx < earliestCut)
                        {
                            earliestCut = cIdx;
                        }
                    }
                    string bossPart = afterAm.Substring(0, earliestCut).Trim();
                    info.bossName = CleanBossName(bossPart);
                    return info;
                }
            }

            return info;
        }

        public static string ExtractKiller(string message, string lowerMessage)
        {
            BossDeathInfo info = ParseDeath(message);
            return info.killer;
        }

        public static string CleanBossName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }
            string b = name.Trim(' ', '.', ':', '!', ',', '。');
            if (b.StartsWith("Boss ", StringComparison.OrdinalIgnoreCase))
            {
                b = b.Substring(5).Trim();
            }
            else if (b.StartsWith("BOSS ", StringComparison.OrdinalIgnoreCase))
            {
                b = b.Substring(5).Trim();
            }
            if (b.StartsWith("được ", StringComparison.OrdinalIgnoreCase))
            {
                b = b.Substring(5).Trim();
            }
            return b.Trim();
        }
    }
}
