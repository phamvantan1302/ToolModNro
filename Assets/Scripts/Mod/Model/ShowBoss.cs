using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mod
{
	public class ShowBoss
	{
		public ShowBoss()
		{
			time = mSystem.currentTimeMillis();
			appearedAt = DateTime.Now;
			killer = "Chưa có thông tin";
			isDead = false;
			diedAt = null;
			deathMessage = "";
			mapName = "Chưa có thông tin";
			nameBoss = "BOSS";
			mapID = -1;
			zoneID = -1;
		}

		public ShowBoss(string a)
		{
			time = mSystem.currentTimeMillis();
			appearedAt = DateTime.Now;
			killer = "Chưa có thông tin";
			isDead = false;
			diedAt = null;
			deathMessage = "";
			zoneID = -1;
			mapID = -1;
			mapName = "Chưa có thông tin";
			nameBoss = "BOSS";

			if (string.IsNullOrEmpty(a))
			{
				return;
			}

			try
			{
				string text = a.Trim();
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

				if (text.StartsWith("BOSS ", StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(5).Trim();
				}
				else if (text.StartsWith("Boss: ", StringComparison.OrdinalIgnoreCase) || text.StartsWith("BOSS: ", StringComparison.OrdinalIgnoreCase))
				{
					text = text.Substring(6).Trim();
				}

				string[] appearKeywords = new string[]
				{
					" vừa xuất hiện tại ",
					" xuất hiện tại ",
					" vừa xuất hiện ở ",
					" xuất hiện ở ",
					" vừa xuất hiện ",
					" xuất hiện "
				};

				int appearIdx = -1;
				string matchedKeyword = "";
				foreach (string kw in appearKeywords)
				{
					appearIdx = text.IndexOf(kw, StringComparison.OrdinalIgnoreCase);
					if (appearIdx >= 0)
					{
						matchedKeyword = kw;
						break;
					}
				}

				if (appearIdx >= 0)
				{
					nameBoss = text.Substring(0, appearIdx).Trim();
					string afterAppear = text.Substring(appearIdx + matchedKeyword.Length).Trim();

					string[] zoneKeywords = new string[]
					{
						" khu vực ",
						" khu ",
						" kv "
					};

					int zoneIdx = -1;
					string matchedZoneKw = "";
					foreach (string zkw in zoneKeywords)
					{
						zoneIdx = afterAppear.IndexOf(zkw, StringComparison.OrdinalIgnoreCase);
						if (zoneIdx >= 0)
						{
							matchedZoneKw = zkw;
							break;
						}
					}

					if (zoneIdx >= 0)
					{
						mapName = afterAppear.Substring(0, zoneIdx).Trim();
						string zoneStr = afterAppear.Substring(zoneIdx + matchedZoneKw.Length).Trim();
						string digits = "";
						for (int i = 0; i < zoneStr.Length; i++)
						{
							if (char.IsDigit(zoneStr[i]))
							{
								digits += zoneStr[i];
							}
							else if (digits.Length > 0)
							{
								break;
							}
						}
						if (int.TryParse(digits, out int zid))
						{
							zoneID = zid;
						}
					}
					else
					{
						mapName = afterAppear.TrimEnd('.', '!', ' ');
					}

					mapName = mapName.Trim();
					mapID = ModFunc.GI().GetMapID(mapName);
				}
				else
				{
					nameBoss = text;
				}
			}
			catch
			{
			}
		}

		public void PaintBoss(mGraphics g, int x, int y, int align)
		{
			g.fillRect(GameCanvas.w - 20, y + 3, 20, 10, 2721889, 90);
			long diffS = (mSystem.currentTimeMillis() - time) / 1000;
			if (diffS < 0)
			{
				diffS = 0;
			}
			string timeAppear = (diffS < 60) ? (diffS + "s trước") : (NinjaUtil.getTimeAgo(diffS) + " trước");
			mFont mFont = mFont.tahoma_7_orange;
			if (isDead)
			{
				mFont = mFont.tahoma_7_grey;
			}
			else if (TileMap.mapName != null && mapName != null && TileMap.mapName.Trim().ToLower() == mapName.Trim().ToLower())
			{
				mFont = mFont.tahoma_7_red;
				for (int i = 0; i < GameScr.vCharInMap.size(); i++)
				{
					Char @char = (Char)GameScr.vCharInMap.elementAt(i);
					if (@char != null && @char.cName == nameBoss)
					{
						mFont = mFont.tahoma_7b_red;
						break;
					}
				}
			}
			mFont.drawStringBorder(g, string.Concat(new object[]
			{
				nameBoss,
				" - ",
				mapName,
				(mapID >= 0 ? (" [" + mapID + "]") : ""),
				(zoneID >= 0 ? " Khu " + zoneID : ""),
				" - ",
				timeAppear,
				" <<"
			}), x, y, align, mFont.tahoma_7_grey);
		}

		public string nameBoss;

		public string mapName;

		public string playerKill;

		public int mapID;

		public int zoneID;

		public long time;

		public DateTime appearedAt;

		public bool isDead;

		public string killer;

		public DateTime? diedAt;

		public string deathMessage;
	}
}
