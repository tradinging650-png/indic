using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace NinjaTrader.NinjaScript.Indicators
{
	// Token: 0x02000005 RID: 5
	public class RetailTraderPointV3 : Indicator
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002A54 File Offset: 0x00000C54
		protected override void OnStateChange()
		{
			if (base.State == State.SetDefaults)
			{
				base.Description = "RT Gamma Levels V3 — Optimized Cloud Edition";
				base.Name = "RT Gamma Levels V3";
				base.Calculate = Calculate.OnEachTick;
				base.IsOverlay = true;
				base.DisplayInDataBox = true;
				base.DrawOnPricePanel = true;
				base.PaintPriceMarkers = false;
				base.ScaleJustification = ScaleJustification.Right;
				base.IsSuspendedWhileInactive = false;
				this.LicenseKey = "";
				this.Ticker = "NQ_NDX";
				this.DaysToLoad = 2;
				this.ShowPivotLevel = true;
				this.ShowMajorPosVol = true;
				this.ShowMajorNegVol = true;
				this.ShowMajorPosOI = true;
				this.ShowMajorNegOI = true;
				this.ShowNetGamma = false;
				this.ShowLongGamma = true;
				this.ShowShortGamma = true;
				this.ShowMajorCall = true;
				this.ShowMajorPut = true;
				this.PointSize = 3;
				this.PriceMultiplier = 1.0;
				this.PivotLevelColor = Brushes.Yellow;
				this.MajorPosVolColor = Brushes.Lime;
				this.MajorNegVolColor = Brushes.Red;
				this.MajorPosOIColor = Brushes.DeepSkyBlue;
				this.MajorNegOIColor = Brushes.OrangeRed;
				this.NetGammaColor = Brushes.Gold;
				this.LongGammaColor = Brushes.Cyan;
				this.ShortGammaColor = Brushes.Magenta;
				this.MajorCallColor = Brushes.DodgerBlue;
				this.MajorPutColor = Brushes.Orange;
				this.ShowFlowCircles = true;
				this.FlowThreshold = 100000.0;
				this.CircleSize = 20;
				this.CircleOutlineWidth = 3;
				this.CircleOpacity = 60;
				this.ShowFlowText = true;
				this.CircleTextSize = 8;
				this.ConvexityPositiveColor = Brushes.Turquoise;
				this.ConvexityNegativeColor = Brushes.Purple;
				this.GammaFlowPositiveColor = Brushes.LimeGreen;
				this.GammaFlowNegativeColor = Brushes.Crimson;
				this.UseOneDTE = false;
				this.ShowLegend = true;
				this.ShowStats = true;
				this.LegendX = 10;
				this.LegendY = 50;
				this.LegendFontSize = 10;
				this.LegendDecimals = 2;
				this.LegendBackground = true;
				this.LegendBackgroundAlpha = 180;
				this.LegendTextColor = Brushes.White;
				this.EnforceSessionTime = false;
				return;
			}
			if (base.State == State.Configure)
			{
				try
				{
					this.easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
				}
				catch
				{
					base.Print("RT Gamma Levels: Eastern timezone init failed");
				}
				ServicePointManager.DefaultConnectionLimit = 10;
				return;
			}
			if (base.State != State.DataLoaded)
			{
				if (base.State == State.Terminated)
				{
					if (this.dbPollTimer != null)
					{
						this.dbPollTimer.Dispose();
						this.dbPollTimer = null;
					}
					if (this.cachedTextFormat != null)
					{
						this.cachedTextFormat.Dispose();
						this.cachedTextFormat = null;
					}
					if (this.circleTextFormat != null)
					{
						this.circleTextFormat.Dispose();
						this.circleTextFormat = null;
					}
					this.DisposeBrushCache();
				}
				return;
			}
			if (string.IsNullOrEmpty(this.LicenseKey))
			{
				this.statusMessage = "ERROR: License key is empty!";
				base.Print("RT Gamma Levels: LicenseKey is empty!");
				return;
			}
			if (!this.ValidateLicense())
			{
				base.Print("RT Gamma Levels: License invalid — " + this.licenseMessage);
				return;
			}
			this.LoadAllData();
			this.dbPollTimer = new Timer(new TimerCallback(this.OnPollTimerElapsed), null, 5000, 5000);
			base.Print(string.Format("RT Gamma Levels: License OK, loaded {0} records", this.totalRecords));
		}

		// Token: 0x06000003 RID: 3 RVA: 0x00002D8C File Offset: 0x00000F8C
		private void DisposeBrushCache()
		{
			if (this.pointBrushCache != null)
			{
				for (int i = 0; i < this.pointBrushCache.Length; i++)
				{
					SharpDX.Direct2D1.SolidColorBrush solidColorBrush = this.pointBrushCache[i];
					if (solidColorBrush != null)
					{
						solidColorBrush.Dispose();
					}
					this.pointBrushCache[i] = null;
				}
				this.pointBrushCache = null;
			}
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush2 = this.flowPosBrush;
			if (solidColorBrush2 != null)
			{
				solidColorBrush2.Dispose();
			}
			this.flowPosBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush3 = this.flowNegBrush;
			if (solidColorBrush3 != null)
			{
				solidColorBrush3.Dispose();
			}
			this.flowNegBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush4 = this.convPosBrush;
			if (solidColorBrush4 != null)
			{
				solidColorBrush4.Dispose();
			}
			this.convPosBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush5 = this.convNegBrush;
			if (solidColorBrush5 != null)
			{
				solidColorBrush5.Dispose();
			}
			this.convNegBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush6 = this.flowTextBrush;
			if (solidColorBrush6 != null)
			{
				solidColorBrush6.Dispose();
			}
			this.flowTextBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush7 = this.legendBgBrush;
			if (solidColorBrush7 != null)
			{
				solidColorBrush7.Dispose();
			}
			this.legendBgBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush8 = this.legendTextBrush;
			if (solidColorBrush8 != null)
			{
				solidColorBrush8.Dispose();
			}
			this.legendTextBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush9 = this.errorBgBrush;
			if (solidColorBrush9 != null)
			{
				solidColorBrush9.Dispose();
			}
			this.errorBgBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush10 = this.errorBorderBrush;
			if (solidColorBrush10 != null)
			{
				solidColorBrush10.Dispose();
			}
			this.errorBorderBrush = null;
			SharpDX.Direct2D1.SolidColorBrush solidColorBrush11 = this.errorTextBrush;
			if (solidColorBrush11 != null)
			{
				solidColorBrush11.Dispose();
			}
			this.errorTextBrush = null;
			this.lastRenderTarget = null;
		}

		// Token: 0x06000004 RID: 4 RVA: 0x00002ED0 File Offset: 0x000010D0
		private void EnsureBrushCache()
		{
			if (this.lastRenderTarget == base.RenderTarget && this.pointBrushCache != null)
			{
				return;
			}
			this.DisposeBrushCache();
			this.lastRenderTarget = base.RenderTarget;
			float num = (float)this.CircleOpacity / 100f;
			float alpha = Math.Min(1f, num + 0.3f);
			this.pointBrushCache = new SharpDX.Direct2D1.SolidColorBrush[9];
			this.pointBrushCache[0] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.LongGammaColor));
			this.pointBrushCache[1] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.ShortGammaColor));
			this.pointBrushCache[2] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorCallColor));
			this.pointBrushCache[3] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorPutColor));
			this.pointBrushCache[4] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.PivotLevelColor));
			this.pointBrushCache[5] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorPosVolColor));
			this.pointBrushCache[6] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorNegVolColor));
			this.pointBrushCache[7] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorPosOIColor));
			this.pointBrushCache[8] = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, this.BrushToColor4(this.MajorNegOIColor));
			Color4 color = this.BrushToColor4(this.GammaFlowPositiveColor);
			color.Alpha = num;
			Color4 color2 = this.BrushToColor4(this.GammaFlowNegativeColor);
			color2.Alpha = num;
			Color4 color3 = this.BrushToColor4(this.ConvexityPositiveColor);
			color3.Alpha = num;
			Color4 color4 = this.BrushToColor4(this.ConvexityNegativeColor);
			color4.Alpha = num;
			this.flowPosBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, color);
			this.flowNegBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, color2);
			this.convPosBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, color3);
			this.convNegBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, color4);
			this.flowTextBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, new Color4(1f, 1f, 1f, alpha));
			this.legendBgBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, new Color4(0f, 0f, 0f, (float)this.LegendBackgroundAlpha / 255f));
			this.legendTextBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, SharpDX.Color.White);
			this.errorBgBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, new Color4(0.2f, 0f, 0f, 0.85f));
			this.errorBorderBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, new Color4(1f, 0.3f, 0.3f, 1f));
			this.errorTextBrush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, new Color4(1f, 0.4f, 0.4f, 1f));
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000031DC File Offset: 0x000013DC
		private string EdgeFunctionGet(string queryParams)
		{
			string result;
			try
			{
				HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://134.209.228.88:8080/indicator-api?" + queryParams);
				httpWebRequest.Method = "GET";
				httpWebRequest.ContentType = "application/json";
				httpWebRequest.Timeout = 30000;
				httpWebRequest.KeepAlive = true;
				httpWebRequest.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);
				using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
				{
					using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
					{
						result = streamReader.ReadToEnd();
					}
				}
			}
			catch (WebException ex)
			{
				HttpWebResponse httpWebResponse2 = ex.Response as HttpWebResponse;
				if (httpWebResponse2 != null)
				{
					using (StreamReader streamReader2 = new StreamReader(httpWebResponse2.GetResponseStream(), Encoding.UTF8))
					{
						string arg = streamReader2.ReadToEnd();
						base.Print(string.Format("RT Gamma Levels HTTP Error: {0} — {1}", (int)httpWebResponse2.StatusCode, arg));
						goto IL_F0;
					}
				}
				base.Print("RT Gamma Levels HTTP Error: " + ex.Message);
				IL_F0:
				result = null;
			}
			catch (Exception ex2)
			{
				base.Print("RT Gamma Levels HTTP Error: " + ex2.Message);
				result = null;
			}
			return result;
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000333C File Offset: 0x0000153C
		private string ExtractJsonValue(string json, string key)
		{
			string text = "\"" + key + "\"";
			int num = json.IndexOf(text);
			if (num < 0)
			{
				return null;
			}
			int num2 = json.IndexOf(':', num + text.Length);
			if (num2 < 0)
			{
				return null;
			}
			int num3 = json.IndexOf('"', num2 + 1);
			int num4 = json.IndexOf('"', num3 + 1);
			if (num3 < 0 || num4 <= num3)
			{
				return null;
			}
			return json.Substring(num3 + 1, num4 - num3 - 1);
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000033B4 File Offset: 0x000015B4
		private string ExtractJsonArray(string json, string key)
		{
			string text = "\"" + key + "\"";
			int num = json.IndexOf(text);
			if (num < 0)
			{
				return null;
			}
			int num2 = json.IndexOf('[', num + text.Length);
			if (num2 < 0)
			{
				return null;
			}
			int num3 = 0;
			for (int i = num2; i < json.Length; i++)
			{
				if (json[i] == '[')
				{
					num3++;
				}
				else if (json[i] == ']')
				{
					num3--;
				}
				if (num3 == 0)
				{
					return json.Substring(num2, i - num2 + 1);
				}
			}
			return null;
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00003444 File Offset: 0x00001644
		private bool ValidateLicense()
		{
			bool result;
			try
			{
				this.machineHwid = this.GetMachineHWID();
				string queryParams = "action=validate&hwid=" + this.machineHwid + "&license_key=" + this.LicenseKey;
				string text = this.EdgeFunctionGet(queryParams);
				if (string.IsNullOrEmpty(text))
				{
					this.licenseMessage = "No response from server";
					this.statusMessage = "❌ Connection error";
					this.licenseValid = false;
					result = false;
				}
				else if (text.Contains("\"error\""))
				{
					string text2 = this.ExtractJsonValue(text, "error");
					this.licenseMessage = (text2 ?? "License validation failed");
					this.statusMessage = "❌ " + this.licenseMessage;
					this.licenseValid = false;
					result = false;
				}
				else if (text.Contains("\"status\":\"ok\"") || text.Contains("\"status\": \"ok\""))
				{
					this.licenseValid = true;
					this.licenseMessage = "License valid";
					this.statusMessage = "✅ License verified";
					base.Print("RT Gamma Levels: License validated — " + this.LicenseKey);
					result = true;
				}
				else
				{
					this.licenseMessage = "Unexpected response";
					this.licenseValid = false;
					result = false;
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels License Error: " + ex.Message);
				this.licenseMessage = "License validation error: " + ex.Message;
				this.statusMessage = "❌ License validation error";
				this.licenseValid = false;
				result = false;
			}
			return result;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000035C8 File Offset: 0x000017C8
		private string GetMachineHWID()
		{
			string result;
			try
			{
				string text = Environment.MachineName;
				try
				{
					using (ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
					{
						using (ManagementObjectCollection.ManagementObjectEnumerator enumerator = managementObjectSearcher.Get().GetEnumerator())
						{
							if (enumerator.MoveNext())
							{
								ManagementObject managementObject = (ManagementObject)enumerator.Current;
								string str = text;
								string str2 = "|";
								object obj = managementObject["ProcessorId"];
								text = str + str2 + ((obj != null) ? obj.ToString() : null);
							}
						}
					}
				}
				catch
				{
				}
				using (SHA256 sha = SHA256.Create())
				{
					result = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-", "").Substring(0, 16);
				}
			}
			catch
			{
				result = "UNKNOWN";
			}
			return result;
		}

		// Token: 0x0600000A RID: 10 RVA: 0x000036DC File Offset: 0x000018DC
		private void LoadAllData()
		{
			try
			{
				object obj = this.dataLock;
				lock (obj)
				{
					this.dataByTimestamp.Clear();
					this.dataByDate.Clear();
				}
				this.statusMessage = "☁ Loading data...";
				string queryParams = string.Concat(new string[]
				{
					"action=data&hwid=",
					this.machineHwid,
					"&license_key=",
					this.LicenseKey,
					"&ticker=",
					this.Ticker,
					"&days=",
					this.DaysToLoad.ToString(),
					"&tables=orderflow,classic_majors,state_majors"
				});
				string text = this.EdgeFunctionGet(queryParams);
				if (string.IsNullOrEmpty(text))
				{
					this.statusMessage = "☁ Cloud | No response";
				}
				else if (text.Contains("\"error\""))
				{
					string text2 = this.ExtractJsonValue(text, "error");
					this.statusMessage = "❌ " + (text2 ?? "Load error");
					base.Print("RT Gamma Levels: Load error — " + text2);
				}
				else
				{
					string text3 = this.ExtractJsonArray(text, "orderflow");
					string text4 = this.ExtractJsonArray(text, "classic_majors");
					string text5 = this.ExtractJsonArray(text, "state_majors");
					if (text3 != null)
					{
						this.ParseSupabaseArray(text3, "orderflow");
					}
					if (text4 != null)
					{
						this.ParseSupabaseArray(text4, "classic_majors");
					}
					if (text5 != null)
					{
						this.ParseSupabaseArray(text5, "state_majors");
					}
					this.totalRecords = this.dataByTimestamp.Count;
					if (this.totalRecords > 0)
					{
						long num = 0L;
						foreach (long num2 in this.dataByTimestamp.Keys)
						{
							if (num2 > num)
							{
								num = num2;
							}
						}
						this.latestData = this.dataByTimestamp[num];
						this.lastLoadedTimestamp = num;
						this.dataLoaded = true;
						this.renderCacheDirty = true;
						this.statusMessage = string.Format("☁ Cloud | {0} records loaded", this.totalRecords);
					}
					else
					{
						this.statusMessage = "☁ Cloud | No data for " + this.Ticker;
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels LoadData Error: " + ex.Message);
				this.statusMessage = "Load Error: " + ex.Message;
			}
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00003994 File Offset: 0x00001B94
		private void PollForNewData()
		{
			try
			{
				int num = 0;
				long num2 = this.lastLoadedTimestamp;
				string queryParams = string.Concat(new string[]
				{
					"action=poll&hwid=",
					this.machineHwid,
					"&license_key=",
					this.LicenseKey,
					"&ticker=",
					this.Ticker,
					"&since_ts=",
					num2.ToString(),
					"&tables=orderflow,classic_majors,state_majors"
				});
				string text = this.EdgeFunctionGet(queryParams);
				if (string.IsNullOrEmpty(text) || text.Contains("\"error\""))
				{
					this.pollErrorCount++;
				}
				else
				{
					string text2 = this.ExtractJsonArray(text, "orderflow");
					string text3 = this.ExtractJsonArray(text, "classic_majors");
					string text4 = this.ExtractJsonArray(text, "state_majors");
					if (text2 != null)
					{
						num += this.ParseSupabaseArray(text2, "orderflow");
					}
					if (text3 != null)
					{
						num += this.ParseSupabaseArray(text3, "classic_majors");
					}
					if (text4 != null)
					{
						num += this.ParseSupabaseArray(text4, "state_majors");
					}
					this.pollErrorCount = 0;
					if (num > 0)
					{
						object obj = this.dataLock;
						lock (obj)
						{
							this.totalRecords = this.dataByTimestamp.Count;
							long num3 = 0L;
							foreach (long num4 in this.dataByTimestamp.Keys)
							{
								if (num4 > num3)
								{
									num3 = num4;
								}
							}
							this.latestData = this.dataByTimestamp[num3];
							this.lastLoadedTimestamp = num3;
							this.dataLoaded = true;
							this.renderCacheDirty = true;
							this.statusMessage = string.Format("☁ Cloud | Spot: {0:F2} | {1} recs | New: {2}", this.latestData.Spot, this.totalRecords, num);
						}
						this.hasNewData = true;
						this.consecutiveEmptyPolls = 0;
						this.ForceChartRefreshAsync();
					}
					else
					{
						this.consecutiveEmptyPolls++;
						if (this.consecutiveEmptyPolls >= 3)
						{
							this.hasNewData = true;
							this.consecutiveEmptyPolls = 0;
							this.ForceChartRefreshAsync();
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.pollErrorCount++;
				base.Print(string.Format("RT Gamma Levels Poll Error #{0}: {1}", this.pollErrorCount, ex.Message));
				this.statusMessage = string.Format("Poll Error #{0}: {1}", this.pollErrorCount, ex.Message);
			}
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00003C64 File Offset: 0x00001E64
		private int ParseSupabaseArray(string json, string tableName)
		{
			int num = 0;
			try
			{
				if (string.IsNullOrEmpty(json) || json.Trim() == "[]")
				{
					return 0;
				}
				string text = json.Trim();
				if (text[0] != '[')
				{
					return 0;
				}
				string text2 = text.Substring(1, text.Length - 2).Trim();
				if (text2.Length == 0)
				{
					return 0;
				}
				int num2 = 0;
				int num3 = -1;
				for (int i = 0; i < text2.Length; i++)
				{
					if (text2[i] == '{')
					{
						if (num2 == 0)
						{
							num3 = i;
						}
						num2++;
					}
					else if (text2[i] == '}')
					{
						num2--;
						if (num2 == 0 && num3 >= 0)
						{
							string objStr = text2.Substring(num3, i - num3 + 1);
							this.ProcessRecord(objStr, tableName);
							num++;
							num3 = -1;
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels Parse Error (" + tableName + "): " + ex.Message);
			}
			return num;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00003D78 File Offset: 0x00001F78
		private void ProcessRecord(string objStr, string tableName)
		{
			Dictionary<string, string> dictionary = this.ParseFlatJson(objStr);
			if (dictionary == null)
			{
				return;
			}
			long num = this.ParseLong(dictionary, "timestamp");
			if (num <= 0L)
			{
				return;
			}
			object obj = this.dataLock;
			GammaLevelRecordV3 gammaLevelRecordV;
			lock (obj)
			{
				if (!this.dataByTimestamp.TryGetValue(num, out gammaLevelRecordV))
				{
					long num2 = 1L;
					while (num2 <= 2L && !this.dataByTimestamp.TryGetValue(num - num2, out gammaLevelRecordV) && !this.dataByTimestamp.TryGetValue(num + num2, out gammaLevelRecordV))
					{
						num2 += 1L;
					}
				}
				if (gammaLevelRecordV == null)
				{
					gammaLevelRecordV = new GammaLevelRecordV3();
					gammaLevelRecordV.Timestamp = num;
					gammaLevelRecordV.DateTime = this.GetStr(dictionary, "datetime");
					gammaLevelRecordV.Date = this.GetStr(dictionary, "date");
					gammaLevelRecordV.Ticker = this.Ticker;
					this.dataByTimestamp[gammaLevelRecordV.Timestamp] = gammaLevelRecordV;
					DateTime date;
					if (!DateTime.TryParseExact(gammaLevelRecordV.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
					{
						date = DateTimeOffset.FromUnixTimeSeconds(num).DateTime.Date;
					}
					if (!this.dataByDate.ContainsKey(date))
					{
						this.dataByDate[date] = new List<GammaLevelRecordV3>();
					}
					this.dataByDate[date].Add(gammaLevelRecordV);
					this.dayListsSorted = false;
				}
			}
			double num3 = this.ParseDouble(dictionary, "spot");
			if (num3 > 0.0)
			{
				gammaLevelRecordV.Spot = num3;
			}
			if (tableName == "orderflow")
			{
				gammaLevelRecordV.Z_MLGamma = this.ParseDouble(dictionary, "z_mlgamma");
				gammaLevelRecordV.Z_MSGamma = this.ParseDouble(dictionary, "z_msgamma");
				gammaLevelRecordV.O_MLGamma = this.ParseDouble(dictionary, "o_mlgamma");
				gammaLevelRecordV.O_MSGamma = this.ParseDouble(dictionary, "o_msgamma");
				gammaLevelRecordV.Zero_MCall = this.ParseDouble(dictionary, "zero_mcall");
				gammaLevelRecordV.Zero_MPut = this.ParseDouble(dictionary, "zero_mput");
				gammaLevelRecordV.One_MCall = this.ParseDouble(dictionary, "one_mcall");
				gammaLevelRecordV.One_MPut = this.ParseDouble(dictionary, "one_mput");
				gammaLevelRecordV.GexOFlow = this.ParseDouble(dictionary, "gexoflow");
				gammaLevelRecordV.OneGexOFlow = this.ParseDouble(dictionary, "one_gexoflow");
				gammaLevelRecordV.CrvOFlow = this.ParseDouble(dictionary, "cvroflow");
				gammaLevelRecordV.OneCrvOFlow = this.ParseDouble(dictionary, "one_cvroflow");
				return;
			}
			if (tableName == "classic_majors")
			{
				gammaLevelRecordV.ZeroGamma = this.ParseDouble(dictionary, "zero_gamma");
				gammaLevelRecordV.MajorPosVol = this.ParseDouble(dictionary, "mpos_vol");
				gammaLevelRecordV.MajorNegVol = this.ParseDouble(dictionary, "mneg_vol");
				gammaLevelRecordV.MajorPosOI = this.ParseDouble(dictionary, "mpos_oi");
				gammaLevelRecordV.MajorNegOI = this.ParseDouble(dictionary, "mneg_oi");
				gammaLevelRecordV.NetGexVol = this.ParseDouble(dictionary, "net_gex_vol");
				return;
			}
			if (tableName == "state_majors")
			{
				gammaLevelRecordV.StateZeroGamma = this.ParseDouble(dictionary, "zero_gamma");
				gammaLevelRecordV.StateMajorPosVol = this.ParseDouble(dictionary, "mpos_vol");
				gammaLevelRecordV.StateMajorNegVol = this.ParseDouble(dictionary, "mneg_vol");
				gammaLevelRecordV.StateNetGex = this.ParseDouble(dictionary, "net_gex_vol");
			}
		}

		// Token: 0x0600000E RID: 14 RVA: 0x000040C8 File Offset: 0x000022C8
		private Dictionary<string, string> ParseFlatJson(string objStr)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			int i = 1;
			int length = objStr.Length;
			while (i < length - 1)
			{
				while (i < length && (objStr[i] == ' ' || objStr[i] == '\t' || objStr[i] == '\r' || objStr[i] == '\n' || objStr[i] == ','))
				{
					i++;
				}
				if (i >= length - 1)
				{
					break;
				}
				if (objStr[i] != '"')
				{
					i++;
				}
				else
				{
					i++;
					int num = i;
					while (i < length && objStr[i] != '"')
					{
						i++;
					}
					string text = objStr.Substring(num, i - num);
					i++;
					while (i < length && (objStr[i] == ' ' || objStr[i] == ':'))
					{
						i++;
					}
					if (i >= length)
					{
						break;
					}
					string value;
					if (objStr[i] == '"')
					{
						i++;
						StringBuilder stringBuilder = new StringBuilder();
						while (i < length && objStr[i] != '"')
						{
							if (objStr[i] == '\\' && i + 1 < length)
							{
								i++;
								stringBuilder.Append(objStr[i]);
							}
							else
							{
								stringBuilder.Append(objStr[i]);
							}
							i++;
						}
						value = stringBuilder.ToString();
						i++;
					}
					else if (objStr[i] == 'n' && i + 3 < length && objStr.Substring(i, 4) == "null")
					{
						value = "";
						i += 4;
					}
					else if (objStr[i] == 't' && i + 3 < length && objStr.Substring(i, 4) == "true")
					{
						value = "true";
						i += 4;
					}
					else if (objStr[i] == 'f' && i + 4 < length && objStr.Substring(i, 5) == "false")
					{
						value = "false";
						i += 5;
					}
					else
					{
						int num2 = i;
						while (i < length && objStr[i] != ',' && objStr[i] != '}' && objStr[i] != ' ' && objStr[i] != '\t')
						{
							i++;
						}
						value = objStr.Substring(num2, i - num2).Trim();
					}
					if (!string.IsNullOrEmpty(text))
					{
						dictionary[text] = value;
					}
				}
			}
			return dictionary;
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002052 File Offset: 0x00000252
		private string GetStr(Dictionary<string, string> d, string key)
		{
			if (!d.ContainsKey(key))
			{
				return "";
			}
			return d[key];
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00004310 File Offset: 0x00002510
		private double ParseDouble(Dictionary<string, string> d, string key)
		{
			if (!d.ContainsKey(key) || string.IsNullOrEmpty(d[key]))
			{
				return 0.0;
			}
			double result;
			if (!double.TryParse(d[key], NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				return 0.0;
			}
			return result;
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00004364 File Offset: 0x00002564
		private long ParseLong(Dictionary<string, string> d, string key)
		{
			if (!d.ContainsKey(key) || string.IsNullOrEmpty(d[key]))
			{
				return 0L;
			}
			long result;
			if (!long.TryParse(d[key].Trim(new char[]
			{
				'"'
			}), out result))
			{
				return 0L;
			}
			return result;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x000043B0 File Offset: 0x000025B0
		private void OnPollTimerElapsed(object state)
		{
			if (this.isPolling)
			{
				return;
			}
			object obj = this.lockObject;
			lock (obj)
			{
				if (this.isPolling)
				{
					return;
				}
				this.isPolling = true;
			}
			try
			{
				this.PollForNewData();
				if (this.pollErrorCount >= 5)
				{
					base.Print("RT Gamma Levels: Too many errors, recreating timer...");
					this.pollErrorCount = 0;
					try
					{
						this.dbPollTimer.Change(5000, 5000);
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels Timer Error: " + ex.Message);
			}
			finally
			{
				obj = this.lockObject;
				lock (obj)
				{
					this.isPolling = false;
				}
			}
		}

		// Token: 0x06000013 RID: 19 RVA: 0x000044BC File Offset: 0x000026BC
		private void ForceChartRefreshAsync()
		{
			try
			{
				if (base.ChartControl != null && !base.ChartControl.Dispatcher.HasShutdownStarted)
				{
					base.ChartControl.Dispatcher.InvokeAsync(delegate()
					{
						try
						{
							base.ForceRefresh();
							this.lastForceRefreshTime = DateTime.UtcNow;
						}
						catch
						{
						}
					});
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x00004518 File Offset: 0x00002718
		protected override void OnBarUpdate()
		{
			if (this.hasNewData)
			{
				this.hasNewData = false;
				this.ForceChartRefreshAsync();
			}
			if (this.dataLoaded && (DateTime.UtcNow - this.lastForceRefreshTime).TotalSeconds >= 10.0)
			{
				this.ForceChartRefreshAsync();
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x00004570 File Offset: 0x00002770
		private GammaLevelRecordV3 FindCompositeDataForBar(DateTime barTime, int barPeriodSeconds)
		{
			object obj = this.dataLock;
			GammaLevelRecordV3 result;
			lock (obj)
			{
				if (this.dataByTimestamp.Count == 0)
				{
					result = null;
				}
				else
				{
					long num = new DateTimeOffset(barTime).ToUnixTimeSeconds();
					long num2 = num - (long)barPeriodSeconds;
					DateTime date = barTime.Date;
					DateTime date2 = barTime.AddDays(-1.0).Date;
					GammaLevelRecordV3 gammaLevelRecordV = null;
					foreach (DateTime key in new DateTime[]
					{
						date,
						date2
					})
					{
						List<GammaLevelRecordV3> list;
						if (this.dataByDate.TryGetValue(key, out list) && list.Count != 0)
						{
							int j = 0;
							int num3 = list.Count - 1;
							int num4 = list.Count;
							while (j <= num3)
							{
								int num5 = j + num3 >> 1;
								if (list[num5].Timestamp > num2)
								{
									num4 = num5;
									num3 = num5 - 1;
								}
								else
								{
									j = num5 + 1;
								}
							}
							int num6 = num4;
							while (num6 < list.Count && list[num6].Timestamp <= num)
							{
								if (gammaLevelRecordV == null)
								{
									gammaLevelRecordV = new GammaLevelRecordV3();
									gammaLevelRecordV.Timestamp = list[num6].Timestamp;
									gammaLevelRecordV.DateTime = list[num6].DateTime;
									gammaLevelRecordV.Date = list[num6].Date;
									gammaLevelRecordV.Ticker = list[num6].Ticker;
								}
								this.MergeIntoComposite(gammaLevelRecordV, list[num6]);
								num6++;
							}
						}
					}
					result = gammaLevelRecordV;
				}
			}
			return result;
		}

		// Token: 0x06000016 RID: 22 RVA: 0x00004758 File Offset: 0x00002958
		private void MergeIntoComposite(GammaLevelRecordV3 composite, GammaLevelRecordV3 source)
		{
			if (source.Spot > 0.0)
			{
				composite.Spot = source.Spot;
			}
			if (source.Z_MLGamma > 0.0 && (composite.Z_MLGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.Z_MLGamma = source.Z_MLGamma;
			}
			if (source.Z_MSGamma > 0.0 && (composite.Z_MSGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.Z_MSGamma = source.Z_MSGamma;
			}
			if (source.O_MLGamma > 0.0 && (composite.O_MLGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.O_MLGamma = source.O_MLGamma;
			}
			if (source.O_MSGamma > 0.0 && (composite.O_MSGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.O_MSGamma = source.O_MSGamma;
			}
			if (source.Zero_MCall > 0.0 && (composite.Zero_MCall == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.Zero_MCall = source.Zero_MCall;
			}
			if (source.Zero_MPut > 0.0 && (composite.Zero_MPut == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.Zero_MPut = source.Zero_MPut;
			}
			if (source.One_MCall > 0.0 && (composite.One_MCall == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.One_MCall = source.One_MCall;
			}
			if (source.One_MPut > 0.0 && (composite.One_MPut == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.One_MPut = source.One_MPut;
			}
			if (Math.Abs(source.GexOFlow) > Math.Abs(composite.GexOFlow))
			{
				composite.GexOFlow = source.GexOFlow;
			}
			if (Math.Abs(source.OneGexOFlow) > Math.Abs(composite.OneGexOFlow))
			{
				composite.OneGexOFlow = source.OneGexOFlow;
			}
			if (Math.Abs(source.CrvOFlow) > Math.Abs(composite.CrvOFlow))
			{
				composite.CrvOFlow = source.CrvOFlow;
			}
			if (Math.Abs(source.OneCrvOFlow) > Math.Abs(composite.OneCrvOFlow))
			{
				composite.OneCrvOFlow = source.OneCrvOFlow;
			}
			if (source.ZeroGamma > 0.0 && (composite.ZeroGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.ZeroGamma = source.ZeroGamma;
			}
			if (source.MajorPosVol > 0.0 && (composite.MajorPosVol == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.MajorPosVol = source.MajorPosVol;
			}
			if (source.MajorNegVol > 0.0 && (composite.MajorNegVol == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.MajorNegVol = source.MajorNegVol;
			}
			if (source.MajorPosOI > 0.0 && (composite.MajorPosOI == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.MajorPosOI = source.MajorPosOI;
			}
			if (source.MajorNegOI > 0.0 && (composite.MajorNegOI == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.MajorNegOI = source.MajorNegOI;
			}
			if (source.NetGexVol != 0.0 && (composite.NetGexVol == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.NetGexVol = source.NetGexVol;
			}
			if (source.StateZeroGamma > 0.0 && (composite.StateZeroGamma == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.StateZeroGamma = source.StateZeroGamma;
			}
			if (source.StateMajorPosVol > 0.0 && (composite.StateMajorPosVol == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.StateMajorPosVol = source.StateMajorPosVol;
			}
			if (source.StateMajorNegVol > 0.0 && (composite.StateMajorNegVol == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.StateMajorNegVol = source.StateMajorNegVol;
			}
			if (source.StateNetGex != 0.0 && (composite.StateNetGex == 0.0 || source.Timestamp >= composite.Timestamp))
			{
				composite.StateNetGex = source.StateNetGex;
			}
		}

		// Token: 0x06000017 RID: 23 RVA: 0x00004C4C File Offset: 0x00002E4C
		private bool IsWithinSessionHours(DateTime time)
		{
			if (this.easternTimeZone == null)
			{
				return true;
			}
			bool result;
			try
			{
				TimeSpan timeOfDay = TimeZoneInfo.ConvertTime(time, this.easternTimeZone).TimeOfDay;
				result = (timeOfDay >= new TimeSpan(9, 30, 0) && timeOfDay <= new TimeSpan(16, 0, 0));
			}
			catch
			{
				result = true;
			}
			return result;
		}

		// Token: 0x06000018 RID: 24 RVA: 0x00004CB8 File Offset: 0x00002EB8
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			this.EnsureBrushCache();
			if (!this.licenseValid)
			{
				this.DrawLicenseError(chartControl, chartScale);
				return;
			}
			if (this.dataLoaded)
			{
				this.BuildRenderCache();
				this.DrawGammaPoints(chartControl, chartScale);
				if (this.ShowFlowCircles)
				{
					this.DrawFlowCircles(chartControl, chartScale);
				}
			}
			if (this.ShowLegend && this.dataLoaded && this.latestData != null)
			{
				this.DrawLegend(chartControl, chartScale);
			}
		}

		// Token: 0x06000019 RID: 25 RVA: 0x00004D2C File Offset: 0x00002F2C
		private void BuildRenderCache()
		{
			int fromIndex = base.ChartBars.FromIndex;
			int toIndex = base.ChartBars.ToIndex;
			if (!this.renderCacheDirty && fromIndex == this.lastCacheFromIndex && toIndex == this.lastCacheToIndex)
			{
				return;
			}
			this.renderCache.Clear();
			this.lastCacheFromIndex = fromIndex;
			this.lastCacheToIndex = toIndex;
			this.renderCacheDirty = false;
			if (!this.dayListsSorted)
			{
				object obj = this.dataLock;
				lock (obj)
				{
					using (Dictionary<DateTime, List<GammaLevelRecordV3>>.ValueCollection.Enumerator enumerator = this.dataByDate.Values.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							enumerator.Current.Sort((GammaLevelRecordV3 a, GammaLevelRecordV3 b) => a.Timestamp.CompareTo(b.Timestamp));
						}
					}
				}
				this.dayListsSorted = true;
			}
			for (int i = fromIndex; i <= toIndex; i++)
			{
				if (i >= 0 && i < base.Bars.Count)
				{
					DateTime time = base.Bars.GetTime(i);
					if (!this.EnforceSessionTime || this.IsWithinSessionHours(time))
					{
						int barPeriodSeconds = this.GetBarPeriodSeconds(i);
						GammaLevelRecordV3 gammaLevelRecordV = this.FindCompositeDataForBar(time, barPeriodSeconds);
						if (gammaLevelRecordV != null && gammaLevelRecordV.Spot > 0.0)
						{
							this.renderCache[i] = gammaLevelRecordV;
						}
					}
				}
			}
		}

		// Token: 0x0600001A RID: 26 RVA: 0x00004EB0 File Offset: 0x000030B0
		private void DrawLicenseError(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				if (this.cachedTextFormat == null || this.cachedTextFormat.IsDisposed)
				{
					this.cachedTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Arial", FontWeight.Bold, FontStyle.Normal, 14f);
				}
				float num = (float)(base.ChartPanel.X + 20);
				float num2 = (float)(base.ChartPanel.Y + 20);
				float num3 = 350f;
				float height = 60f;
				base.RenderTarget.FillRectangle(new RectangleF(num, num2, num3, height), this.errorBgBrush);
				base.RenderTarget.DrawRectangle(new RectangleF(num, num2, num3, height), this.errorBorderBrush, 2f);
				base.RenderTarget.DrawText(this.statusMessage, this.cachedTextFormat, new RectangleF(num + 10f, num2 + 8f, num3 - 20f, 22f), this.errorTextBrush);
				base.RenderTarget.DrawText(this.licenseMessage, this.cachedTextFormat, new RectangleF(num + 10f, num2 + 32f, num3 - 20f, 22f), this.errorTextBrush);
			}
			catch
			{
			}
		}

		// Token: 0x0600001B RID: 27 RVA: 0x00004FF0 File Offset: 0x000031F0
		private int GetBarPeriodSeconds(int barIdx)
		{
			if (barIdx > 0 && barIdx < base.Bars.Count)
			{
				DateTime time = base.Bars.GetTime(barIdx);
				DateTime time2 = base.Bars.GetTime(barIdx - 1);
				return Math.Max(1, (int)(time - time2).TotalSeconds);
			}
			return 60;
		}

		// Token: 0x0600001C RID: 28 RVA: 0x00005044 File Offset: 0x00003244
		private void DrawGammaPoints(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				float radius = (float)this.PointSize;
				double priceMultiplier = this.PriceMultiplier;
				foreach (KeyValuePair<int, GammaLevelRecordV3> keyValuePair in this.renderCache)
				{
					int key = keyValuePair.Key;
					GammaLevelRecordV3 value = keyValuePair.Value;
					float x = (float)chartControl.GetXByBarIndex(base.ChartBars, key);
					if (this.ShowLongGamma && value.Z_MLGamma > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.Z_MLGamma * priceMultiplier), radius, this.pointBrushCache[0]);
					}
					if (this.ShowShortGamma && value.Z_MSGamma > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.Z_MSGamma * priceMultiplier), radius, this.pointBrushCache[1]);
					}
					if (this.ShowMajorCall && value.Zero_MCall > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.Zero_MCall * priceMultiplier), radius, this.pointBrushCache[2]);
					}
					if (this.ShowMajorPut && value.Zero_MPut > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.Zero_MPut * priceMultiplier), radius, this.pointBrushCache[3]);
					}
					if (this.ShowPivotLevel && value.ZeroGamma > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.ZeroGamma * priceMultiplier), radius, this.pointBrushCache[4]);
					}
					if (this.ShowMajorPosVol && value.MajorPosVol > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.MajorPosVol * priceMultiplier), radius, this.pointBrushCache[5]);
					}
					if (this.ShowMajorNegVol && value.MajorNegVol > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.MajorNegVol * priceMultiplier), radius, this.pointBrushCache[6]);
					}
					if (this.ShowMajorPosOI && value.MajorPosOI > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.MajorPosOI * priceMultiplier), radius, this.pointBrushCache[7]);
					}
					if (this.ShowMajorNegOI && value.MajorNegOI > 0.0)
					{
						this.DrawPoint(x, (float)chartScale.GetYByValue(value.MajorNegOI * priceMultiplier), radius, this.pointBrushCache[8]);
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels GammaPoint render error: " + ex.Message);
			}
		}

		// Token: 0x0600001D RID: 29 RVA: 0x0000531C File Offset: 0x0000351C
		private void DrawPoint(float x, float y, float radius, SharpDX.Direct2D1.SolidColorBrush brush)
		{
			Ellipse ellipse = new Ellipse(new Vector2(x, y), radius, radius);
			base.RenderTarget.FillEllipse(ellipse, brush);
		}

		// Token: 0x0600001E RID: 30 RVA: 0x00005348 File Offset: 0x00003548
		private void DrawFlowCircles(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				if (this.circleTextFormat == null || this.circleTextFormat.IsDisposed)
				{
					this.circleTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Arial", FontWeight.Bold, FontStyle.Normal, (float)this.CircleTextSize);
					this.circleTextFormat.TextAlignment = TextAlignment.Center;
					this.circleTextFormat.ParagraphAlignment = ParagraphAlignment.Center;
				}
				foreach (KeyValuePair<int, GammaLevelRecordV3> keyValuePair in this.renderCache)
				{
					int key = keyValuePair.Key;
					GammaLevelRecordV3 value = keyValuePair.Value;
					double num = this.UseOneDTE ? value.OneGexOFlow : value.GexOFlow;
					double num2 = this.UseOneDTE ? value.OneCrvOFlow : value.CrvOFlow;
					if (Math.Abs(num) >= this.FlowThreshold || Math.Abs(num2) >= this.FlowThreshold)
					{
						float num3 = (float)chartControl.GetXByBarIndex(base.ChartBars, key);
						float num4 = (float)chartScale.GetYByValue(value.Spot * this.PriceMultiplier);
						float num5 = (float)this.CircleSize / 2f;
						Vector2 center = new Vector2(num3, num4);
						Ellipse ellipse = new Ellipse(center, num5, num5);
						SharpDX.Direct2D1.SolidColorBrush brush = (num >= 0.0) ? this.flowPosBrush : this.flowNegBrush;
						SharpDX.Direct2D1.SolidColorBrush brush2 = (num2 >= 0.0) ? this.convPosBrush : this.convNegBrush;
						base.RenderTarget.FillEllipse(ellipse, brush);
						base.RenderTarget.DrawEllipse(ellipse, brush2, (float)this.CircleOutlineWidth);
						if (this.ShowFlowText)
						{
							double value2 = (Math.Abs(num) >= Math.Abs(num2)) ? num : num2;
							string text = this.FormatFlowValue(value2);
							RectangleF layoutRect = new RectangleF(num3 - num5, num4 - num5, (float)this.CircleSize, (float)this.CircleSize);
							base.RenderTarget.DrawText(text, this.circleTextFormat, layoutRect, this.flowTextBrush);
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels FlowCircle render error: " + ex.Message);
			}
		}

		// Token: 0x0600001F RID: 31 RVA: 0x0000559C File Offset: 0x0000379C
		private string FormatFlowValue(double value)
		{
			double num = Math.Abs(value);
			string arg = (value < 0.0) ? "-" : "";
			if (num >= 1000000.0)
			{
				return string.Format("{0}{1:F1}M", arg, num / 1000000.0);
			}
			if (num >= 1000.0)
			{
				return string.Format("{0}{1:F0}K", arg, num / 1000.0);
			}
			return string.Format("{0}{1:F0}", arg, num);
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000562C File Offset: 0x0000382C
		private Color4 BrushToColor4(System.Windows.Media.Brush brush)
		{
			System.Windows.Media.SolidColorBrush solidColorBrush = brush as System.Windows.Media.SolidColorBrush;
			if (solidColorBrush != null)
			{
				System.Windows.Media.Color color = solidColorBrush.Color;
				return new Color4((float)color.R / 255f, (float)color.G / 255f, (float)color.B / 255f, (float)color.A / 255f);
			}
			return SharpDX.Color.White;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00005694 File Offset: 0x00003894
		private void DrawLegend(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				float num = (float)this.LegendX;
				float num2 = (float)this.LegendY;
				float num3 = (float)(this.LegendFontSize + 4);
				float num4 = 280f;
				int num5 = 1;
				if (this.ShowPivotLevel)
				{
					num5++;
				}
				if (this.ShowMajorPosVol)
				{
					num5++;
				}
				if (this.ShowMajorNegVol)
				{
					num5++;
				}
				if (this.ShowMajorPosOI)
				{
					num5++;
				}
				if (this.ShowMajorNegOI)
				{
					num5++;
				}
				if (this.ShowNetGamma)
				{
					num5++;
				}
				if (this.ShowLongGamma)
				{
					num5++;
				}
				if (this.ShowShortGamma)
				{
					num5++;
				}
				if (this.ShowMajorCall)
				{
					num5++;
				}
				if (this.ShowMajorPut)
				{
					num5++;
				}
				if (this.ShowFlowCircles)
				{
					num5 += 3;
				}
				if (this.ShowStats)
				{
					num5 += 2;
				}
				float height = (float)num5 * num3 + 20f;
				if (this.LegendBackground)
				{
					RectangleF rect = new RectangleF(num, num2, num4, height);
					base.RenderTarget.FillRectangle(rect, this.legendBgBrush);
				}
				if (this.cachedTextFormat == null || this.cachedTextFormat.IsDisposed)
				{
					this.cachedTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Consolas", (float)this.LegendFontSize);
				}
				SharpDX.Direct2D1.SolidColorBrush brush = this.legendTextBrush;
				float num6 = num2 + 5f;
				string format = string.Format("F{0}", this.LegendDecimals);
				this.DrawLegendLine(this.statusMessage, num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
				num6 += num3;
				if (this.ShowPivotLevel)
				{
					this.DrawLegendLine("Pivot Level: " + ((this.latestData.ZeroGamma > 0.0) ? (this.latestData.ZeroGamma * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorPosVol)
				{
					this.DrawLegendLine("Major + (Vol): " + ((this.latestData.MajorPosVol > 0.0) ? (this.latestData.MajorPosVol * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorNegVol)
				{
					this.DrawLegendLine("Major - (Vol): " + ((this.latestData.MajorNegVol > 0.0) ? (this.latestData.MajorNegVol * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorPosOI)
				{
					this.DrawLegendLine("Major + (OI): " + ((this.latestData.MajorPosOI > 0.0) ? (this.latestData.MajorPosOI * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorNegOI)
				{
					this.DrawLegendLine("Major - (OI): " + ((this.latestData.MajorNegOI > 0.0) ? (this.latestData.MajorNegOI * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowNetGamma)
				{
					this.DrawLegendLine("Net Gamma: " + this.FormatFlowValue(this.latestData.NetGexVol), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowLongGamma)
				{
					this.DrawLegendLine("Long Γ Strike: " + ((this.latestData.Z_MLGamma > 0.0) ? (this.latestData.Z_MLGamma * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowShortGamma)
				{
					this.DrawLegendLine("Short Γ Strike: " + ((this.latestData.Z_MSGamma > 0.0) ? (this.latestData.Z_MSGamma * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorCall)
				{
					this.DrawLegendLine("Major Call: " + ((this.latestData.Zero_MCall > 0.0) ? (this.latestData.Zero_MCall * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowMajorPut)
				{
					this.DrawLegendLine("Major Put: " + ((this.latestData.Zero_MPut > 0.0) ? (this.latestData.Zero_MPut * this.PriceMultiplier).ToString(format) : "N/A"), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowFlowCircles)
				{
					this.DrawLegendLine("───────────────────", num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
					double value = this.UseOneDTE ? this.latestData.OneGexOFlow : this.latestData.GexOFlow;
					double value2 = this.UseOneDTE ? this.latestData.OneCrvOFlow : this.latestData.CrvOFlow;
					string str = this.UseOneDTE ? "1DTE" : "0DTE";
					this.DrawLegendLine("Gamma Flow (" + str + "): " + this.FormatFlowValue(value), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
					this.DrawLegendLine("Convexity Flow (" + str + "): " + this.FormatFlowValue(value2), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
				if (this.ShowStats)
				{
					this.DrawLegendLine("───────────────────", num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
					this.DrawLegendLine(string.Format("Spot: {0} | Records: {1}", this.latestData.Spot.ToString(format), this.totalRecords), num + 5f, num6, num4 - 10f, num3, this.cachedTextFormat, brush);
					num6 += num3;
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Gamma Levels Legend render error: " + ex.Message);
			}
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00005E2C File Offset: 0x0000402C
		private void DrawLegendLine(string text, float x, float y, float width, float height, SharpDX.DirectWrite.TextFormat textFormat, SharpDX.Direct2D1.SolidColorBrush brush)
		{
			RectangleF layoutRect = new RectangleF(x, y, width, height);
			base.RenderTarget.DrawText(text, textFormat, layoutRect, brush);
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000023 RID: 35 RVA: 0x0000206A File Offset: 0x0000026A
		// (set) Token: 0x06000024 RID: 36 RVA: 0x00002072 File Offset: 0x00000272
		[NinjaScriptProperty]
		[Display(Name = "License Key", Description = "Lisans anahtarınızı girin", Order = 1, GroupName = "1. License")]
		public string LicenseKey { get; set; }

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000025 RID: 37 RVA: 0x0000207B File Offset: 0x0000027B
		// (set) Token: 0x06000026 RID: 38 RVA: 0x00002083 File Offset: 0x00000283
		[NinjaScriptProperty]
		[Display(Name = "Ticker", Description = "Ticker symbol (e.g. NQ_NDX, ES_SPX)", Order = 2, GroupName = "1. License")]
		public string Ticker { get; set; }

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x06000027 RID: 39 RVA: 0x0000208C File Offset: 0x0000028C
		// (set) Token: 0x06000028 RID: 40 RVA: 0x00002094 File Offset: 0x00000294
		[NinjaScriptProperty]
		[Range(1, 30)]
		[Display(Name = "Days to Load", Order = 3, GroupName = "1. License")]
		public int DaysToLoad { get; set; }

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x06000029 RID: 41 RVA: 0x0000209D File Offset: 0x0000029D
		// (set) Token: 0x0600002A RID: 42 RVA: 0x000020A5 File Offset: 0x000002A5
		[Display(Name = "Show Pivot Level", Order = 1, GroupName = "2. Chart Display")]
		public bool ShowPivotLevel { get; set; }

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600002B RID: 43 RVA: 0x000020AE File Offset: 0x000002AE
		// (set) Token: 0x0600002C RID: 44 RVA: 0x000020B6 File Offset: 0x000002B6
		[Display(Name = "Show Major + (Vol)", Order = 2, GroupName = "2. Chart Display")]
		public bool ShowMajorPosVol { get; set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x0600002D RID: 45 RVA: 0x000020BF File Offset: 0x000002BF
		// (set) Token: 0x0600002E RID: 46 RVA: 0x000020C7 File Offset: 0x000002C7
		[Display(Name = "Show Major - (Vol)", Order = 3, GroupName = "2. Chart Display")]
		public bool ShowMajorNegVol { get; set; }

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x0600002F RID: 47 RVA: 0x000020D0 File Offset: 0x000002D0
		// (set) Token: 0x06000030 RID: 48 RVA: 0x000020D8 File Offset: 0x000002D8
		[Display(Name = "Show Major + (OI)", Order = 4, GroupName = "2. Chart Display")]
		public bool ShowMajorPosOI { get; set; }

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x06000031 RID: 49 RVA: 0x000020E1 File Offset: 0x000002E1
		// (set) Token: 0x06000032 RID: 50 RVA: 0x000020E9 File Offset: 0x000002E9
		[Display(Name = "Show Major - (OI)", Order = 5, GroupName = "2. Chart Display")]
		public bool ShowMajorNegOI { get; set; }

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000033 RID: 51 RVA: 0x000020F2 File Offset: 0x000002F2
		// (set) Token: 0x06000034 RID: 52 RVA: 0x000020FA File Offset: 0x000002FA
		[Display(Name = "Show Net Gamma", Order = 6, GroupName = "2. Chart Display")]
		public bool ShowNetGamma { get; set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000035 RID: 53 RVA: 0x00002103 File Offset: 0x00000303
		// (set) Token: 0x06000036 RID: 54 RVA: 0x0000210B File Offset: 0x0000030B
		[Display(Name = "Show Long Gamma Strike", Order = 7, GroupName = "2. Chart Display")]
		public bool ShowLongGamma { get; set; }

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000037 RID: 55 RVA: 0x00002114 File Offset: 0x00000314
		// (set) Token: 0x06000038 RID: 56 RVA: 0x0000211C File Offset: 0x0000031C
		[Display(Name = "Show Short Gamma Strike", Order = 8, GroupName = "2. Chart Display")]
		public bool ShowShortGamma { get; set; }

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000039 RID: 57 RVA: 0x00002125 File Offset: 0x00000325
		// (set) Token: 0x0600003A RID: 58 RVA: 0x0000212D File Offset: 0x0000032D
		[Display(Name = "Show Major Call", Order = 9, GroupName = "2. Chart Display")]
		public bool ShowMajorCall { get; set; }

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x0600003B RID: 59 RVA: 0x00002136 File Offset: 0x00000336
		// (set) Token: 0x0600003C RID: 60 RVA: 0x0000213E File Offset: 0x0000033E
		[Display(Name = "Show Major Put", Order = 10, GroupName = "2. Chart Display")]
		public bool ShowMajorPut { get; set; }

		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600003D RID: 61 RVA: 0x00002147 File Offset: 0x00000347
		// (set) Token: 0x0600003E RID: 62 RVA: 0x0000214F File Offset: 0x0000034F
		[Range(1, 10)]
		[Display(Name = "Point Size", Order = 1, GroupName = "3. Point Settings")]
		public int PointSize { get; set; }

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x0600003F RID: 63 RVA: 0x00002158 File Offset: 0x00000358
		// (set) Token: 0x06000040 RID: 64 RVA: 0x00002160 File Offset: 0x00000360
		[Range(0.01, 100.0)]
		[Display(Name = "Price Multiplier", Order = 2, GroupName = "3. Point Settings")]
		public double PriceMultiplier { get; set; }

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000041 RID: 65 RVA: 0x00002169 File Offset: 0x00000369
		// (set) Token: 0x06000042 RID: 66 RVA: 0x00002171 File Offset: 0x00000371
		[Display(Name = "Show Flow Circles", Order = 1, GroupName = "4. Flow Circles")]
		public bool ShowFlowCircles { get; set; }

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x06000043 RID: 67 RVA: 0x0000217A File Offset: 0x0000037A
		// (set) Token: 0x06000044 RID: 68 RVA: 0x00002182 File Offset: 0x00000382
		[NinjaScriptProperty]
		[Range(0, 10000000)]
		[Display(Name = "Flow Threshold", Description = "Eşik değer (ör: 100000 = 100K)", Order = 2, GroupName = "4. Flow Circles")]
		public double FlowThreshold { get; set; }

		// Token: 0x17000012 RID: 18
		// (get) Token: 0x06000045 RID: 69 RVA: 0x0000218B File Offset: 0x0000038B
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00002193 File Offset: 0x00000393
		[Range(10, 60)]
		[Display(Name = "Circle Size (px)", Order = 3, GroupName = "4. Flow Circles")]
		public int CircleSize { get; set; }

		// Token: 0x17000013 RID: 19
		// (get) Token: 0x06000047 RID: 71 RVA: 0x0000219C File Offset: 0x0000039C
		// (set) Token: 0x06000048 RID: 72 RVA: 0x000021A4 File Offset: 0x000003A4
		[Range(1, 8)]
		[Display(Name = "Outline Width", Order = 4, GroupName = "4. Flow Circles")]
		public int CircleOutlineWidth { get; set; }

		// Token: 0x17000014 RID: 20
		// (get) Token: 0x06000049 RID: 73 RVA: 0x000021AD File Offset: 0x000003AD
		// (set) Token: 0x0600004A RID: 74 RVA: 0x000021B5 File Offset: 0x000003B5
		[Range(10, 100)]
		[Display(Name = "Opacity (%)", Order = 5, GroupName = "4. Flow Circles")]
		public int CircleOpacity { get; set; }

		// Token: 0x17000015 RID: 21
		// (get) Token: 0x0600004B RID: 75 RVA: 0x000021BE File Offset: 0x000003BE
		// (set) Token: 0x0600004C RID: 76 RVA: 0x000021C6 File Offset: 0x000003C6
		[Display(Name = "Show Amount Text", Order = 6, GroupName = "4. Flow Circles")]
		public bool ShowFlowText { get; set; }

		// Token: 0x17000016 RID: 22
		// (get) Token: 0x0600004D RID: 77 RVA: 0x000021CF File Offset: 0x000003CF
		// (set) Token: 0x0600004E RID: 78 RVA: 0x000021D7 File Offset: 0x000003D7
		[Range(6, 14)]
		[Display(Name = "Text Font Size", Order = 7, GroupName = "4. Flow Circles")]
		public int CircleTextSize { get; set; }

		// Token: 0x17000017 RID: 23
		// (get) Token: 0x0600004F RID: 79 RVA: 0x000021E0 File Offset: 0x000003E0
		// (set) Token: 0x06000050 RID: 80 RVA: 0x000021E8 File Offset: 0x000003E8
		[Display(Name = "Use 1 DTE (instead of 0 DTE)", Order = 8, GroupName = "4. Flow Circles")]
		public bool UseOneDTE { get; set; }

		// Token: 0x17000018 RID: 24
		// (get) Token: 0x06000051 RID: 81 RVA: 0x000021F1 File Offset: 0x000003F1
		// (set) Token: 0x06000052 RID: 82 RVA: 0x000021F9 File Offset: 0x000003F9
		[XmlIgnore]
		[Display(Name = "Convexity Positive (Ring)", Order = 1, GroupName = "5. Flow Colors")]
		public System.Windows.Media.Brush ConvexityPositiveColor { get; set; }

		// Token: 0x17000019 RID: 25
		// (get) Token: 0x06000053 RID: 83 RVA: 0x00002202 File Offset: 0x00000402
		// (set) Token: 0x06000054 RID: 84 RVA: 0x0000220F File Offset: 0x0000040F
		[Browsable(false)]
		public string ConvexityPositiveColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.ConvexityPositiveColor);
			}
			set
			{
				this.ConvexityPositiveColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700001A RID: 26
		// (get) Token: 0x06000055 RID: 85 RVA: 0x0000221D File Offset: 0x0000041D
		// (set) Token: 0x06000056 RID: 86 RVA: 0x00002225 File Offset: 0x00000425
		[XmlIgnore]
		[Display(Name = "Convexity Negative (Ring)", Order = 2, GroupName = "5. Flow Colors")]
		public System.Windows.Media.Brush ConvexityNegativeColor { get; set; }

		// Token: 0x1700001B RID: 27
		// (get) Token: 0x06000057 RID: 87 RVA: 0x0000222E File Offset: 0x0000042E
		// (set) Token: 0x06000058 RID: 88 RVA: 0x0000223B File Offset: 0x0000043B
		[Browsable(false)]
		public string ConvexityNegativeColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.ConvexityNegativeColor);
			}
			set
			{
				this.ConvexityNegativeColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700001C RID: 28
		// (get) Token: 0x06000059 RID: 89 RVA: 0x00002249 File Offset: 0x00000449
		// (set) Token: 0x0600005A RID: 90 RVA: 0x00002251 File Offset: 0x00000451
		[XmlIgnore]
		[Display(Name = "Gamma Flow Positive (Fill)", Order = 3, GroupName = "5. Flow Colors")]
		public System.Windows.Media.Brush GammaFlowPositiveColor { get; set; }

		// Token: 0x1700001D RID: 29
		// (get) Token: 0x0600005B RID: 91 RVA: 0x0000225A File Offset: 0x0000045A
		// (set) Token: 0x0600005C RID: 92 RVA: 0x00002267 File Offset: 0x00000467
		[Browsable(false)]
		public string GammaFlowPositiveColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.GammaFlowPositiveColor);
			}
			set
			{
				this.GammaFlowPositiveColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700001E RID: 30
		// (get) Token: 0x0600005D RID: 93 RVA: 0x00002275 File Offset: 0x00000475
		// (set) Token: 0x0600005E RID: 94 RVA: 0x0000227D File Offset: 0x0000047D
		[XmlIgnore]
		[Display(Name = "Gamma Flow Negative (Fill)", Order = 4, GroupName = "5. Flow Colors")]
		public System.Windows.Media.Brush GammaFlowNegativeColor { get; set; }

		// Token: 0x1700001F RID: 31
		// (get) Token: 0x0600005F RID: 95 RVA: 0x00002286 File Offset: 0x00000486
		// (set) Token: 0x06000060 RID: 96 RVA: 0x00002293 File Offset: 0x00000493
		[Browsable(false)]
		public string GammaFlowNegativeColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.GammaFlowNegativeColor);
			}
			set
			{
				this.GammaFlowNegativeColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000020 RID: 32
		// (get) Token: 0x06000061 RID: 97 RVA: 0x000022A1 File Offset: 0x000004A1
		// (set) Token: 0x06000062 RID: 98 RVA: 0x000022A9 File Offset: 0x000004A9
		[XmlIgnore]
		[Display(Name = "Pivot Level", Order = 1, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush PivotLevelColor { get; set; }

		// Token: 0x17000021 RID: 33
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000022B2 File Offset: 0x000004B2
		// (set) Token: 0x06000064 RID: 100 RVA: 0x000022BF File Offset: 0x000004BF
		[Browsable(false)]
		public string PivotLevelColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.PivotLevelColor);
			}
			set
			{
				this.PivotLevelColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000022 RID: 34
		// (get) Token: 0x06000065 RID: 101 RVA: 0x000022CD File Offset: 0x000004CD
		// (set) Token: 0x06000066 RID: 102 RVA: 0x000022D5 File Offset: 0x000004D5
		[XmlIgnore]
		[Display(Name = "Major + (Vol)", Order = 2, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorPosVolColor { get; set; }

		// Token: 0x17000023 RID: 35
		// (get) Token: 0x06000067 RID: 103 RVA: 0x000022DE File Offset: 0x000004DE
		// (set) Token: 0x06000068 RID: 104 RVA: 0x000022EB File Offset: 0x000004EB
		[Browsable(false)]
		public string MajorPosVolColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorPosVolColor);
			}
			set
			{
				this.MajorPosVolColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000024 RID: 36
		// (get) Token: 0x06000069 RID: 105 RVA: 0x000022F9 File Offset: 0x000004F9
		// (set) Token: 0x0600006A RID: 106 RVA: 0x00002301 File Offset: 0x00000501
		[XmlIgnore]
		[Display(Name = "Major - (Vol)", Order = 3, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorNegVolColor { get; set; }

		// Token: 0x17000025 RID: 37
		// (get) Token: 0x0600006B RID: 107 RVA: 0x0000230A File Offset: 0x0000050A
		// (set) Token: 0x0600006C RID: 108 RVA: 0x00002317 File Offset: 0x00000517
		[Browsable(false)]
		public string MajorNegVolColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorNegVolColor);
			}
			set
			{
				this.MajorNegVolColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000026 RID: 38
		// (get) Token: 0x0600006D RID: 109 RVA: 0x00002325 File Offset: 0x00000525
		// (set) Token: 0x0600006E RID: 110 RVA: 0x0000232D File Offset: 0x0000052D
		[XmlIgnore]
		[Display(Name = "Major + (OI)", Order = 4, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorPosOIColor { get; set; }

		// Token: 0x17000027 RID: 39
		// (get) Token: 0x0600006F RID: 111 RVA: 0x00002336 File Offset: 0x00000536
		// (set) Token: 0x06000070 RID: 112 RVA: 0x00002343 File Offset: 0x00000543
		[Browsable(false)]
		public string MajorPosOIColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorPosOIColor);
			}
			set
			{
				this.MajorPosOIColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000028 RID: 40
		// (get) Token: 0x06000071 RID: 113 RVA: 0x00002351 File Offset: 0x00000551
		// (set) Token: 0x06000072 RID: 114 RVA: 0x00002359 File Offset: 0x00000559
		[XmlIgnore]
		[Display(Name = "Major - (OI)", Order = 5, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorNegOIColor { get; set; }

		// Token: 0x17000029 RID: 41
		// (get) Token: 0x06000073 RID: 115 RVA: 0x00002362 File Offset: 0x00000562
		// (set) Token: 0x06000074 RID: 116 RVA: 0x0000236F File Offset: 0x0000056F
		[Browsable(false)]
		public string MajorNegOIColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorNegOIColor);
			}
			set
			{
				this.MajorNegOIColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700002A RID: 42
		// (get) Token: 0x06000075 RID: 117 RVA: 0x0000237D File Offset: 0x0000057D
		// (set) Token: 0x06000076 RID: 118 RVA: 0x00002385 File Offset: 0x00000585
		[XmlIgnore]
		[Display(Name = "Net Gamma", Order = 6, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush NetGammaColor { get; set; }

		// Token: 0x1700002B RID: 43
		// (get) Token: 0x06000077 RID: 119 RVA: 0x0000238E File Offset: 0x0000058E
		// (set) Token: 0x06000078 RID: 120 RVA: 0x0000239B File Offset: 0x0000059B
		[Browsable(false)]
		public string NetGammaColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.NetGammaColor);
			}
			set
			{
				this.NetGammaColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700002C RID: 44
		// (get) Token: 0x06000079 RID: 121 RVA: 0x000023A9 File Offset: 0x000005A9
		// (set) Token: 0x0600007A RID: 122 RVA: 0x000023B1 File Offset: 0x000005B1
		[XmlIgnore]
		[Display(Name = "Long Gamma", Order = 7, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush LongGammaColor { get; set; }

		// Token: 0x1700002D RID: 45
		// (get) Token: 0x0600007B RID: 123 RVA: 0x000023BA File Offset: 0x000005BA
		// (set) Token: 0x0600007C RID: 124 RVA: 0x000023C7 File Offset: 0x000005C7
		[Browsable(false)]
		public string LongGammaColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.LongGammaColor);
			}
			set
			{
				this.LongGammaColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700002E RID: 46
		// (get) Token: 0x0600007D RID: 125 RVA: 0x000023D5 File Offset: 0x000005D5
		// (set) Token: 0x0600007E RID: 126 RVA: 0x000023DD File Offset: 0x000005DD
		[XmlIgnore]
		[Display(Name = "Short Gamma", Order = 8, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush ShortGammaColor { get; set; }

		// Token: 0x1700002F RID: 47
		// (get) Token: 0x0600007F RID: 127 RVA: 0x000023E6 File Offset: 0x000005E6
		// (set) Token: 0x06000080 RID: 128 RVA: 0x000023F3 File Offset: 0x000005F3
		[Browsable(false)]
		public string ShortGammaColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.ShortGammaColor);
			}
			set
			{
				this.ShortGammaColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000030 RID: 48
		// (get) Token: 0x06000081 RID: 129 RVA: 0x00002401 File Offset: 0x00000601
		// (set) Token: 0x06000082 RID: 130 RVA: 0x00002409 File Offset: 0x00000609
		[XmlIgnore]
		[Display(Name = "Major Call", Order = 9, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorCallColor { get; set; }

		// Token: 0x17000031 RID: 49
		// (get) Token: 0x06000083 RID: 131 RVA: 0x00002412 File Offset: 0x00000612
		// (set) Token: 0x06000084 RID: 132 RVA: 0x0000241F File Offset: 0x0000061F
		[Browsable(false)]
		public string MajorCallColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorCallColor);
			}
			set
			{
				this.MajorCallColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000032 RID: 50
		// (get) Token: 0x06000085 RID: 133 RVA: 0x0000242D File Offset: 0x0000062D
		// (set) Token: 0x06000086 RID: 134 RVA: 0x00002435 File Offset: 0x00000635
		[XmlIgnore]
		[Display(Name = "Major Put", Order = 10, GroupName = "6. Gamma Colors")]
		public System.Windows.Media.Brush MajorPutColor { get; set; }

		// Token: 0x17000033 RID: 51
		// (get) Token: 0x06000087 RID: 135 RVA: 0x0000243E File Offset: 0x0000063E
		// (set) Token: 0x06000088 RID: 136 RVA: 0x0000244B File Offset: 0x0000064B
		[Browsable(false)]
		public string MajorPutColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.MajorPutColor);
			}
			set
			{
				this.MajorPutColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000034 RID: 52
		// (get) Token: 0x06000089 RID: 137 RVA: 0x00002459 File Offset: 0x00000659
		// (set) Token: 0x0600008A RID: 138 RVA: 0x00002461 File Offset: 0x00000661
		[Display(Name = "Show Legend", Order = 1, GroupName = "7. Legend")]
		public bool ShowLegend { get; set; }

		// Token: 0x17000035 RID: 53
		// (get) Token: 0x0600008B RID: 139 RVA: 0x0000246A File Offset: 0x0000066A
		// (set) Token: 0x0600008C RID: 140 RVA: 0x00002472 File Offset: 0x00000672
		[Display(Name = "Show Statistics", Order = 2, GroupName = "7. Legend")]
		public bool ShowStats { get; set; }

		// Token: 0x17000036 RID: 54
		// (get) Token: 0x0600008D RID: 141 RVA: 0x0000247B File Offset: 0x0000067B
		// (set) Token: 0x0600008E RID: 142 RVA: 0x00002483 File Offset: 0x00000683
		[Range(0, 2000)]
		[Display(Name = "Legend X Position", Order = 3, GroupName = "7. Legend")]
		public int LegendX { get; set; }

		// Token: 0x17000037 RID: 55
		// (get) Token: 0x0600008F RID: 143 RVA: 0x0000248C File Offset: 0x0000068C
		// (set) Token: 0x06000090 RID: 144 RVA: 0x00002494 File Offset: 0x00000694
		[Range(0, 2000)]
		[Display(Name = "Legend Y Position", Order = 4, GroupName = "7. Legend")]
		public int LegendY { get; set; }

		// Token: 0x17000038 RID: 56
		// (get) Token: 0x06000091 RID: 145 RVA: 0x0000249D File Offset: 0x0000069D
		// (set) Token: 0x06000092 RID: 146 RVA: 0x000024A5 File Offset: 0x000006A5
		[Range(8, 20)]
		[Display(Name = "Font Size", Order = 5, GroupName = "7. Legend")]
		public int LegendFontSize { get; set; }

		// Token: 0x17000039 RID: 57
		// (get) Token: 0x06000093 RID: 147 RVA: 0x000024AE File Offset: 0x000006AE
		// (set) Token: 0x06000094 RID: 148 RVA: 0x000024B6 File Offset: 0x000006B6
		[Range(0, 4)]
		[Display(Name = "Value Decimals", Order = 6, GroupName = "7. Legend")]
		public int LegendDecimals { get; set; }

		// Token: 0x1700003A RID: 58
		// (get) Token: 0x06000095 RID: 149 RVA: 0x000024BF File Offset: 0x000006BF
		// (set) Token: 0x06000096 RID: 150 RVA: 0x000024C7 File Offset: 0x000006C7
		[Display(Name = "Show Background", Order = 7, GroupName = "7. Legend")]
		public bool LegendBackground { get; set; }

		// Token: 0x1700003B RID: 59
		// (get) Token: 0x06000097 RID: 151 RVA: 0x000024D0 File Offset: 0x000006D0
		// (set) Token: 0x06000098 RID: 152 RVA: 0x000024D8 File Offset: 0x000006D8
		[Range(0, 255)]
		[Display(Name = "Background Alpha", Order = 8, GroupName = "7. Legend")]
		public int LegendBackgroundAlpha { get; set; }

		// Token: 0x1700003C RID: 60
		// (get) Token: 0x06000099 RID: 153 RVA: 0x000024E1 File Offset: 0x000006E1
		// (set) Token: 0x0600009A RID: 154 RVA: 0x000024E9 File Offset: 0x000006E9
		[XmlIgnore]
		[Display(Name = "Text Color", Order = 9, GroupName = "7. Legend")]
		public System.Windows.Media.Brush LegendTextColor { get; set; }

		// Token: 0x1700003D RID: 61
		// (get) Token: 0x0600009B RID: 155 RVA: 0x000024F2 File Offset: 0x000006F2
		// (set) Token: 0x0600009C RID: 156 RVA: 0x000024FF File Offset: 0x000006FF
		[Browsable(false)]
		public string LegendTextColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.LegendTextColor);
			}
			set
			{
				this.LegendTextColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700003E RID: 62
		// (get) Token: 0x0600009D RID: 157 RVA: 0x0000250D File Offset: 0x0000070D
		// (set) Token: 0x0600009E RID: 158 RVA: 0x00002515 File Offset: 0x00000715
		[Display(Name = "Enforce Session Time (9:30-16:00 ET)", Order = 1, GroupName = "8. Session")]
		public bool EnforceSessionTime { get; set; }

		// Token: 0x0400000D RID: 13
		private const string EDGE_FUNCTION_URL = "http://134.209.228.88:8080/indicator-api";

		// Token: 0x0400000E RID: 14
		private const int POLL_INTERVAL_SEC = 5;

		// Token: 0x0400000F RID: 15
		private bool licenseValid;

		// Token: 0x04000010 RID: 16
		private string licenseMessage = "";

		// Token: 0x04000011 RID: 17
		private string machineHwid = "";

		// Token: 0x04000012 RID: 18
		private Dictionary<long, GammaLevelRecordV3> dataByTimestamp = new Dictionary<long, GammaLevelRecordV3>();

		// Token: 0x04000013 RID: 19
		private Dictionary<DateTime, List<GammaLevelRecordV3>> dataByDate = new Dictionary<DateTime, List<GammaLevelRecordV3>>();

		// Token: 0x04000014 RID: 20
		private GammaLevelRecordV3 latestData;

		// Token: 0x04000015 RID: 21
		private string statusMessage = "Initializing...";

		// Token: 0x04000016 RID: 22
		private bool dataLoaded;

		// Token: 0x04000017 RID: 23
		private int totalRecords;

		// Token: 0x04000018 RID: 24
		private long lastLoadedTimestamp;

		// Token: 0x04000019 RID: 25
		private Timer dbPollTimer;

		// Token: 0x0400001A RID: 26
		private volatile bool isPolling;

		// Token: 0x0400001B RID: 27
		private volatile bool hasNewData;

		// Token: 0x0400001C RID: 28
		private int pollErrorCount;

		// Token: 0x0400001D RID: 29
		private int consecutiveEmptyPolls;

		// Token: 0x0400001E RID: 30
		private object lockObject = new object();

		// Token: 0x0400001F RID: 31
		private object dataLock = new object();

		// Token: 0x04000020 RID: 32
		private TimeZoneInfo easternTimeZone;

		// Token: 0x04000021 RID: 33
		private SharpDX.DirectWrite.TextFormat cachedTextFormat;

		// Token: 0x04000022 RID: 34
		private SharpDX.DirectWrite.TextFormat circleTextFormat;

		// Token: 0x04000023 RID: 35
		private Dictionary<int, GammaLevelRecordV3> renderCache = new Dictionary<int, GammaLevelRecordV3>();

		// Token: 0x04000024 RID: 36
		private volatile bool renderCacheDirty = true;

		// Token: 0x04000025 RID: 37
		private int lastCacheFromIndex = -1;

		// Token: 0x04000026 RID: 38
		private int lastCacheToIndex = -1;

		// Token: 0x04000027 RID: 39
		private SharpDX.Direct2D1.SolidColorBrush[] pointBrushCache;

		// Token: 0x04000028 RID: 40
		private SharpDX.Direct2D1.SolidColorBrush flowPosBrush;

		// Token: 0x04000029 RID: 41
		private SharpDX.Direct2D1.SolidColorBrush flowNegBrush;

		// Token: 0x0400002A RID: 42
		private SharpDX.Direct2D1.SolidColorBrush convPosBrush;

		// Token: 0x0400002B RID: 43
		private SharpDX.Direct2D1.SolidColorBrush convNegBrush;

		// Token: 0x0400002C RID: 44
		private SharpDX.Direct2D1.SolidColorBrush flowTextBrush;

		// Token: 0x0400002D RID: 45
		private SharpDX.Direct2D1.SolidColorBrush legendBgBrush;

		// Token: 0x0400002E RID: 46
		private SharpDX.Direct2D1.SolidColorBrush legendTextBrush;

		// Token: 0x0400002F RID: 47
		private SharpDX.Direct2D1.SolidColorBrush errorBgBrush;

		// Token: 0x04000030 RID: 48
		private SharpDX.Direct2D1.SolidColorBrush errorBorderBrush;

		// Token: 0x04000031 RID: 49
		private SharpDX.Direct2D1.SolidColorBrush errorTextBrush;

		// Token: 0x04000032 RID: 50
		private RenderTarget lastRenderTarget;

		// Token: 0x04000033 RID: 51
		private bool dayListsSorted;

		// Token: 0x04000034 RID: 52
		private DateTime lastForceRefreshTime = DateTime.MinValue;

		// Token: 0x04000035 RID: 53
		private const int FORCE_REFRESH_INTERVAL_SEC = 10;
	}
}
