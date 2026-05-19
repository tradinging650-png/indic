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
	// Token: 0x02000008 RID: 8
	public class RetailTraderOptionsVolumeV3 : Indicator
	{
		// Token: 0x060000DB RID: 219 RVA: 0x00005F40 File Offset: 0x00004140
		protected override void OnStateChange()
		{
			if (base.State == State.SetDefaults)
			{
				base.Description = "RT Options Sentiment V3 - Optimized Cloud Edition";
				base.Name = "RT Options Sentiment V3";
				base.Calculate = Calculate.OnEachTick;
				base.IsOverlay = false;
				base.DisplayInDataBox = true;
				base.DrawOnPricePanel = false;
				base.PaintPriceMarkers = false;
				base.ScaleJustification = ScaleJustification.Right;
				base.IsSuspendedWhileInactive = false;
				this.LicenseKey = "";
				this.Ticker = "NQ_NDX";
				this.DaysToLoad = 2;
				this.DataSource = SentimentDataSource.AggregatedDelta;
				this.DTEMode = SentimentDTESelection.ZeroDTE;
				this.FilterMode = SentimentFilter.All;
				this.CallAreaColor = Brushes.Lime;
				this.PutAreaColor = Brushes.Magenta;
				this.AreaOpacity = 60;
				this.ZeroLineColor = Brushes.Gray;
				this.ShowSwitchLabels = true;
				this.SwitchFontSize = 9;
				this.SwitchLineColor = Brushes.Yellow;
				this.ShowPercentage = true;
				this.PercentFontSize = 12;
				this.ShowLegend = true;
				this.LegendFontSize = 10;
				this.EnforceSessionTime = false;
				base.AddPlot(new Stroke(Brushes.Lime, 2f), PlotStyle.Line, "CallValue");
				base.AddPlot(new Stroke(Brushes.Magenta, 2f), PlotStyle.Line, "PutValue");
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
					base.Print("RT Options Sentiment: Eastern timezone init failed");
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
					this.DisposeDxResources();
				}
				return;
			}
			if (string.IsNullOrEmpty(this.LicenseKey))
			{
				this.statusMessage = "ERROR: License key is empty!";
				base.Print("RT Options Sentiment: LicenseKey is empty!");
				return;
			}
			if (!this.ValidateLicense())
			{
				base.Print("RT Options Sentiment: License invalid - " + this.licenseMessage);
				return;
			}
			this.LoadAllData();
			this.dbPollTimer = new Timer(new TimerCallback(this.OnPollTimerElapsed), null, 15000, 15000);
			base.Print(string.Format("RT Options Sentiment: License OK, loaded {0} records", this.totalRecords));
		}

		// Token: 0x060000DC RID: 220 RVA: 0x00006170 File Offset: 0x00004370
		private void DisposeDxResources()
		{
			if (this.legendTextFormat != null)
			{
				this.legendTextFormat.Dispose();
				this.legendTextFormat = null;
			}
			if (this.percentTextFormat != null)
			{
				this.percentTextFormat.Dispose();
				this.percentTextFormat = null;
			}
			if (this.switchTextFormat != null)
			{
				this.switchTextFormat.Dispose();
				this.switchTextFormat = null;
			}
			if (this.errorTextFormat != null)
			{
				this.errorTextFormat.Dispose();
				this.errorTextFormat = null;
			}
			if (this.errorBackgroundBrush != null)
			{
				this.errorBackgroundBrush.Dispose();
				this.errorBackgroundBrush = null;
			}
			if (this.errorBorderBrush != null)
			{
				this.errorBorderBrush.Dispose();
				this.errorBorderBrush = null;
			}
			if (this.errorTextBrush != null)
			{
				this.errorTextBrush.Dispose();
				this.errorTextBrush = null;
			}
			if (this.callAreaBrush != null)
			{
				this.callAreaBrush.Dispose();
				this.callAreaBrush = null;
			}
			if (this.callLineBrush != null)
			{
				this.callLineBrush.Dispose();
				this.callLineBrush = null;
			}
			if (this.putAreaBrush != null)
			{
				this.putAreaBrush.Dispose();
				this.putAreaBrush = null;
			}
			if (this.putLineBrush != null)
			{
				this.putLineBrush.Dispose();
				this.putLineBrush = null;
			}
			if (this.zeroLineBrush != null)
			{
				this.zeroLineBrush.Dispose();
				this.zeroLineBrush = null;
			}
			if (this.switchLineBrush != null)
			{
				this.switchLineBrush.Dispose();
				this.switchLineBrush = null;
			}
			if (this.panelBackgroundBrush != null)
			{
				this.panelBackgroundBrush.Dispose();
				this.panelBackgroundBrush = null;
			}
			if (this.legendBackgroundBrush != null)
			{
				this.legendBackgroundBrush.Dispose();
				this.legendBackgroundBrush = null;
			}
			if (this.whiteTextBrush != null)
			{
				this.whiteTextBrush.Dispose();
				this.whiteTextBrush = null;
			}
			if (this.callTextBrush != null)
			{
				this.callTextBrush.Dispose();
				this.callTextBrush = null;
			}
			if (this.putTextBrush != null)
			{
				this.putTextBrush.Dispose();
				this.putTextBrush = null;
			}
		}

		// Token: 0x060000DD RID: 221 RVA: 0x00006354 File Offset: 0x00004554
		private void EnsureDxResources()
		{
			if (base.RenderTarget == null)
			{
				return;
			}
			if (this.legendTextFormat == null || this.legendTextFormat.IsDisposed)
			{
				this.legendTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Consolas", (float)this.LegendFontSize);
			}
			if (this.percentTextFormat == null || this.percentTextFormat.IsDisposed)
			{
				this.percentTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Arial", FontWeight.Bold, FontStyle.Normal, (float)this.PercentFontSize);
			}
			if (this.switchTextFormat == null || this.switchTextFormat.IsDisposed)
			{
				this.switchTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Arial", FontWeight.Bold, FontStyle.Normal, (float)this.SwitchFontSize);
				this.switchTextFormat.TextAlignment = TextAlignment.Center;
			}
			if (this.errorTextFormat == null || this.errorTextFormat.IsDisposed)
			{
				this.errorTextFormat = new SharpDX.DirectWrite.TextFormat(Globals.DirectWriteFactory, "Arial", FontWeight.Bold, FontStyle.Normal, 14f);
			}
			Color4 color = this.BrushToColor4(this.CallAreaColor);
			Color4 color2 = this.BrushToColor4(this.PutAreaColor);
			Color4 color3 = this.BrushToColor4(this.ZeroLineColor);
			Color4 color4 = this.BrushToColor4(this.SwitchLineColor);
			Color4 color5 = color;
			color5.Alpha = (float)this.AreaOpacity / 100f;
			Color4 color6 = color;
			color6.Alpha = Math.Min(1f, color5.Alpha + 0.3f);
			Color4 color7 = color2;
			color7.Alpha = (float)this.AreaOpacity / 100f;
			Color4 color8 = color2;
			color8.Alpha = Math.Min(1f, color7.Alpha + 0.3f);
			color3.Alpha = 0.5f;
			this.RefreshBrush(ref this.errorBackgroundBrush, new Color4(0.2f, 0f, 0f, 0.85f));
			this.RefreshBrush(ref this.errorBorderBrush, new Color4(1f, 0.3f, 0.3f, 1f));
			this.RefreshBrush(ref this.errorTextBrush, new Color4(1f, 0.4f, 0.4f, 1f));
			this.RefreshBrush(ref this.callAreaBrush, color5);
			this.RefreshBrush(ref this.callLineBrush, color6);
			this.RefreshBrush(ref this.putAreaBrush, color7);
			this.RefreshBrush(ref this.putLineBrush, color8);
			this.RefreshBrush(ref this.zeroLineBrush, color3);
			this.RefreshBrush(ref this.switchLineBrush, color4);
			this.RefreshBrush(ref this.panelBackgroundBrush, new Color4(0f, 0f, 0f, 0.6f));
			this.RefreshBrush(ref this.legendBackgroundBrush, new Color4(0f, 0f, 0f, 0.5f));
			this.RefreshBrush(ref this.whiteTextBrush, SharpDX.Color.White);
			this.RefreshBrush(ref this.callTextBrush, color);
			this.RefreshBrush(ref this.putTextBrush, color2);
		}

		// Token: 0x060000DE RID: 222 RVA: 0x000026FD File Offset: 0x000008FD
		private void RefreshBrush(ref SharpDX.Direct2D1.SolidColorBrush brush, Color4 color)
		{
			if (base.RenderTarget == null)
			{
				return;
			}
			if (brush == null || brush.IsDisposed)
			{
				brush = new SharpDX.Direct2D1.SolidColorBrush(base.RenderTarget, color);
				return;
			}
			if (brush.Color != color)
			{
				brush.Color = color;
			}
		}

		// Token: 0x060000DF RID: 223 RVA: 0x00006638 File Offset: 0x00004838
		private void RebuildLookupCaches()
		{
			object obj = this.dataLock;
			lock (obj)
			{
				using (Dictionary<DateTime, List<OptionsSentimentRecord>>.ValueCollection.Enumerator enumerator = this.dataByDate.Values.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						enumerator.Current.Sort((OptionsSentimentRecord left, OptionsSentimentRecord right) => left.Timestamp.CompareTo(right.Timestamp));
					}
				}
				this.barRecordCache.Clear();
				this.totalRecords = this.dataByTimestamp.Count;
				this.dataLoaded = (this.totalRecords > 0);
			}
		}

		// Token: 0x060000E0 RID: 224 RVA: 0x00006700 File Offset: 0x00004900
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
						base.Print(string.Format("RT Options Sentiment HTTP Error: {0} - {1}", (int)httpWebResponse2.StatusCode, arg));
						goto IL_F0;
					}
				}
				base.Print("RT Options Sentiment HTTP Error: " + ex.Message);
				IL_F0:
				result = null;
			}
			catch (Exception ex2)
			{
				base.Print("RT Options Sentiment HTTP Error: " + ex2.Message);
				result = null;
			}
			return result;
		}

		// Token: 0x060000E1 RID: 225 RVA: 0x0000333C File Offset: 0x0000153C
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

		// Token: 0x060000E2 RID: 226 RVA: 0x000033B4 File Offset: 0x000015B4
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

		// Token: 0x060000E3 RID: 227 RVA: 0x00006860 File Offset: 0x00004A60
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
					this.statusMessage = "Connection error";
					this.licenseValid = false;
					result = false;
				}
				else if (text.Contains("\"error\""))
				{
					string text2 = this.ExtractJsonValue(text, "error");
					this.licenseMessage = (text2 ?? "License validation failed");
					this.statusMessage = this.licenseMessage;
					this.licenseValid = false;
					result = false;
				}
				else if (text.Contains("\"status\":\"ok\"") || text.Contains("\"status\": \"ok\""))
				{
					this.licenseValid = true;
					this.licenseMessage = "License valid";
					this.statusMessage = "License verified";
					base.Print("RT Options Sentiment: License validated - " + this.LicenseKey);
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
				base.Print("RT Options Sentiment License Error: " + ex.Message);
				this.licenseMessage = "License validation error: " + ex.Message;
				this.statusMessage = "License validation error";
				this.licenseValid = false;
				result = false;
			}
			return result;
		}

		// Token: 0x060000E4 RID: 228 RVA: 0x000035C8 File Offset: 0x000017C8
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

		// Token: 0x060000E5 RID: 229 RVA: 0x000069D8 File Offset: 0x00004BD8
		private void LoadAllData()
		{
			try
			{
				object obj = this.dataLock;
				lock (obj)
				{
					this.dataByTimestamp.Clear();
					this.dataByDate.Clear();
					this.barRecordCache.Clear();
					this.latestData = null;
					this.totalRecords = 0;
					this.lastLoadedTimestamp = 0L;
					this.dataLoaded = false;
				}
				this.statusMessage = "Loading data...";
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
					"&tables=orderflow"
				});
				string text = this.EdgeFunctionGet(queryParams);
				if (string.IsNullOrEmpty(text))
				{
					this.statusMessage = "Cloud | No response";
				}
				else if (text.Contains("\"error\""))
				{
					string text2 = this.ExtractJsonValue(text, "error");
					this.statusMessage = (text2 ?? "Load error");
					base.Print("RT Options Sentiment: Load error - " + text2);
				}
				else
				{
					string text3 = this.ExtractJsonArray(text, "orderflow");
					if (text3 != null)
					{
						this.ParseSupabaseArray(text3);
					}
					this.RebuildLookupCaches();
					if (this.totalRecords > 0)
					{
						this.statusMessage = string.Format("Cloud | {0} records loaded", this.totalRecords);
					}
					else
					{
						this.statusMessage = "Cloud | No data for " + this.Ticker;
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment LoadData Error: " + ex.Message);
				this.statusMessage = "Load Error: " + ex.Message;
			}
		}

		// Token: 0x060000E6 RID: 230 RVA: 0x00006BD4 File Offset: 0x00004DD4
		private void PollForNewData()
		{
			try
			{
				long num = this.lastLoadedTimestamp;
				string queryParams = string.Concat(new string[]
				{
					"action=poll&hwid=",
					this.machineHwid,
					"&license_key=",
					this.LicenseKey,
					"&ticker=",
					this.Ticker,
					"&since_ts=",
					num.ToString(),
					"&tables=orderflow"
				});
				string text = this.EdgeFunctionGet(queryParams);
				if (string.IsNullOrEmpty(text) || text.Contains("\"error\""))
				{
					this.pollErrorCount++;
				}
				else
				{
					string text2 = this.ExtractJsonArray(text, "orderflow");
					int num2 = 0;
					if (text2 != null)
					{
						num2 = this.ParseSupabaseArray(text2);
					}
					this.pollErrorCount = 0;
					if (num2 > 0)
					{
						this.RebuildLookupCaches();
						object obj = this.dataLock;
						lock (obj)
						{
							this.statusMessage = ((this.latestData != null) ? string.Format("Cloud | Spot: {0:F2} | {1} recs | New: {2}", this.latestData.Spot, this.totalRecords, num2) : string.Format("Cloud | {0} recs | New: {1}", this.totalRecords, num2));
						}
						this.hasNewData = true;
						this.consecutiveEmptyPolls = 0;
					}
					else
					{
						this.consecutiveEmptyPolls++;
						if (this.consecutiveEmptyPolls >= 6)
						{
							this.hasNewData = true;
							this.consecutiveEmptyPolls = 0;
						}
					}
				}
			}
			catch (Exception ex)
			{
				this.pollErrorCount++;
				base.Print(string.Format("RT Options Sentiment Poll Error #{0}: {1}", this.pollErrorCount, ex.Message));
				this.statusMessage = string.Format("Poll Error #{0}: {1}", this.pollErrorCount, ex.Message);
			}
		}

		// Token: 0x060000E7 RID: 231 RVA: 0x00006DE4 File Offset: 0x00004FE4
		private int ParseSupabaseArray(string json)
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
							this.ProcessRecord(objStr);
							num++;
							num3 = -1;
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Parse Error: " + ex.Message);
			}
			return num;
		}

		// Token: 0x060000E8 RID: 232 RVA: 0x00006EF0 File Offset: 0x000050F0
		private void ProcessRecord(string objStr)
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
			OptionsSentimentRecord optionsSentimentRecord;
			lock (obj)
			{
				if (!this.dataByTimestamp.TryGetValue(num, out optionsSentimentRecord))
				{
					optionsSentimentRecord = new OptionsSentimentRecord();
					optionsSentimentRecord.Timestamp = num;
					optionsSentimentRecord.DateTime = this.GetStr(dictionary, "datetime");
					optionsSentimentRecord.Date = this.GetStr(dictionary, "date");
					optionsSentimentRecord.SessionDate = this.ParseSessionDate(optionsSentimentRecord.Date, num);
					this.dataByTimestamp[optionsSentimentRecord.Timestamp] = optionsSentimentRecord;
					List<OptionsSentimentRecord> list;
					if (!this.dataByDate.TryGetValue(optionsSentimentRecord.SessionDate, out list))
					{
						list = new List<OptionsSentimentRecord>();
						this.dataByDate[optionsSentimentRecord.SessionDate] = list;
					}
					list.Add(optionsSentimentRecord);
				}
			}
			double num2 = this.ParseDouble(dictionary, "spot");
			if (num2 > 0.0)
			{
				optionsSentimentRecord.Spot = num2;
			}
			optionsSentimentRecord.AggDex = this.ParseDouble(dictionary, "agg_dex");
			optionsSentimentRecord.OneAggDex = this.ParseDouble(dictionary, "one_agg_dex");
			optionsSentimentRecord.AggCallDex = this.ParseDouble(dictionary, "agg_call_dex");
			optionsSentimentRecord.OneAggCallDex = this.ParseDouble(dictionary, "one_agg_call_dex");
			optionsSentimentRecord.AggPutDex = this.ParseDouble(dictionary, "agg_put_dex");
			optionsSentimentRecord.OneAggPutDex = this.ParseDouble(dictionary, "one_agg_put_dex");
			optionsSentimentRecord.NetDex = this.ParseDouble(dictionary, "net_dex");
			optionsSentimentRecord.OneNetDex = this.ParseDouble(dictionary, "one_net_dex");
			optionsSentimentRecord.NetCallDex = this.ParseDouble(dictionary, "net_call_dex");
			optionsSentimentRecord.OneNetCallDex = this.ParseDouble(dictionary, "one_net_call_dex");
			optionsSentimentRecord.NetPutDex = this.ParseDouble(dictionary, "net_put_dex");
			optionsSentimentRecord.OneNetPutDex = this.ParseDouble(dictionary, "one_net_put_dex");
			optionsSentimentRecord.Zgr = this.ParseDouble(dictionary, "zgr");
			optionsSentimentRecord.Ogr = this.ParseDouble(dictionary, "ogr");
			optionsSentimentRecord.Zcvr = this.ParseDouble(dictionary, "zcvr");
			optionsSentimentRecord.Ocvr = this.ParseDouble(dictionary, "ocvr");
			obj = this.dataLock;
			lock (obj)
			{
				if (optionsSentimentRecord.Timestamp >= this.lastLoadedTimestamp)
				{
					this.lastLoadedTimestamp = optionsSentimentRecord.Timestamp;
					this.latestData = optionsSentimentRecord;
				}
			}
		}

		// Token: 0x060000E9 RID: 233 RVA: 0x000040C8 File Offset: 0x000022C8
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

		// Token: 0x060000EA RID: 234 RVA: 0x00002052 File Offset: 0x00000252
		private string GetStr(Dictionary<string, string> d, string key)
		{
			if (!d.ContainsKey(key))
			{
				return "";
			}
			return d[key];
		}

		// Token: 0x060000EB RID: 235 RVA: 0x00004310 File Offset: 0x00002510
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

		// Token: 0x060000EC RID: 236 RVA: 0x00004364 File Offset: 0x00002564
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

		// Token: 0x060000ED RID: 237 RVA: 0x00007180 File Offset: 0x00005380
		private DateTime ParseSessionDate(string dateValue, long timestamp)
		{
			DateTime dateTime;
			if (!string.IsNullOrEmpty(dateValue) && DateTime.TryParse(dateValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out dateTime))
			{
				return dateTime.Date;
			}
			DateTime result;
			try
			{
				result = DateTimeOffset.FromUnixTimeSeconds(timestamp).LocalDateTime;
				result = result.Date;
			}
			catch
			{
				result = Globals.Now.Date;
			}
			return result;
		}

		// Token: 0x060000EE RID: 238 RVA: 0x000071EC File Offset: 0x000053EC
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
					base.Print("RT Options Sentiment: Too many errors, recreating timer...");
					this.pollErrorCount = 0;
					try
					{
						this.dbPollTimer.Change(15000, 15000);
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Timer Error: " + ex.Message);
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

		// Token: 0x060000EF RID: 239 RVA: 0x000072F8 File Offset: 0x000054F8
		private void GetCallPutValues(OptionsSentimentRecord record, out double callVal, out double putVal)
		{
			callVal = 0.0;
			putVal = 0.0;
			if (record == null)
			{
				return;
			}
			bool flag = this.DTEMode == SentimentDTESelection.OneDTE;
			switch (this.DataSource)
			{
			case SentimentDataSource.AggregatedDelta:
				callVal = (flag ? record.OneAggCallDex : record.AggCallDex);
				putVal = (flag ? record.OneAggPutDex : record.AggPutDex);
				break;
			case SentimentDataSource.NetDelta:
				callVal = (flag ? record.OneNetCallDex : record.NetCallDex);
				putVal = (flag ? record.OneNetPutDex : record.NetPutDex);
				break;
			case SentimentDataSource.GammaImbalance:
			{
				double num = flag ? record.Ogr : record.Zgr;
				if (num >= 0.0)
				{
					callVal = num;
					putVal = 0.0;
				}
				else
				{
					callVal = 0.0;
					putVal = num;
				}
				break;
			}
			case SentimentDataSource.Convexity:
			{
				double num2 = flag ? record.Ocvr : record.Zcvr;
				if (num2 >= 0.0)
				{
					callVal = num2;
					putVal = 0.0;
				}
				else
				{
					callVal = 0.0;
					putVal = num2;
				}
				break;
			}
			}
			if (this.FilterMode == SentimentFilter.BuyOnly)
			{
				putVal = 0.0;
				return;
			}
			if (this.FilterMode == SentimentFilter.SellOnly)
			{
				callVal = 0.0;
			}
		}

		// Token: 0x060000F0 RID: 240 RVA: 0x00007444 File Offset: 0x00005644
		private OptionsSentimentRecord FindDataForBar(DateTime barTime)
		{
			object obj = this.dataLock;
			OptionsSentimentRecord result;
			lock (obj)
			{
				if (this.dataByTimestamp.Count == 0)
				{
					result = null;
				}
				else
				{
					long num = new DateTimeOffset(barTime).ToUnixTimeSeconds();
					OptionsSentimentRecord optionsSentimentRecord;
					List<OptionsSentimentRecord> list;
					if (this.barRecordCache.TryGetValue(num, out optionsSentimentRecord))
					{
						result = optionsSentimentRecord;
					}
					else if (!this.dataByDate.TryGetValue(barTime.Date, out list) || list.Count == 0)
					{
						result = null;
					}
					else
					{
						int i = 0;
						int num2 = list.Count - 1;
						while (i <= num2)
						{
							int num3 = i + (num2 - i) / 2;
							long timestamp = list[num3].Timestamp;
							if (timestamp < num)
							{
								i = num3 + 1;
							}
							else
							{
								if (timestamp <= num)
								{
									this.barRecordCache[num] = list[num3];
									return list[num3];
								}
								num2 = num3 - 1;
							}
						}
						OptionsSentimentRecord optionsSentimentRecord2 = null;
						long num4 = long.MaxValue;
						if (num2 >= 0)
						{
							optionsSentimentRecord2 = list[num2];
							num4 = Math.Abs(num - optionsSentimentRecord2.Timestamp);
						}
						if (i < list.Count)
						{
							OptionsSentimentRecord optionsSentimentRecord3 = list[i];
							long num5 = Math.Abs(num - optionsSentimentRecord3.Timestamp);
							if (num5 < num4)
							{
								optionsSentimentRecord2 = optionsSentimentRecord3;
								num4 = num5;
							}
						}
						if (optionsSentimentRecord2 != null && num4 <= 300L)
						{
							this.barRecordCache[num] = optionsSentimentRecord2;
							result = optionsSentimentRecord2;
						}
						else
						{
							result = null;
						}
					}
				}
			}
			return result;
		}

		// Token: 0x060000F1 RID: 241 RVA: 0x000075E8 File Offset: 0x000057E8
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

		// Token: 0x060000F2 RID: 242 RVA: 0x00007654 File Offset: 0x00005854
		protected override void OnBarUpdate()
		{
			if (this.hasNewData)
			{
				this.hasNewData = false;
				try
				{
					base.ForceRefresh();
				}
				catch
				{
				}
			}
			if (base.CurrentBar < 0)
			{
				return;
			}
			OptionsSentimentRecord optionsSentimentRecord = this.FindDataForBar(base.Time[0]);
			if (optionsSentimentRecord != null)
			{
				double value;
				double value2;
				this.GetCallPutValues(optionsSentimentRecord, out value, out value2);
				base.Values[0][0] = value;
				base.Values[1][0] = value2;
			}
		}

		// Token: 0x060000F3 RID: 243 RVA: 0x000076D8 File Offset: 0x000058D8
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			base.OnRender(chartControl, chartScale);
			if (base.RenderTarget == null || base.ChartBars == null)
			{
				return;
			}
			this.EnsureDxResources();
			if (!this.licenseValid)
			{
				this.DrawLicenseError(chartControl, chartScale);
				return;
			}
			if (!this.dataLoaded)
			{
				return;
			}
			this.DrawAreaChart(chartControl, chartScale);
			if (this.ShowSwitchLabels)
			{
				this.DrawSwitchPoints(chartControl, chartScale);
			}
			if (this.ShowPercentage && this.latestData != null)
			{
				this.DrawPercentageLabel(chartControl, chartScale);
			}
			if (this.ShowLegend)
			{
				this.DrawInfoLegend(chartControl, chartScale);
			}
		}

		// Token: 0x060000F4 RID: 244 RVA: 0x00007760 File Offset: 0x00005960
		private void DrawLicenseError(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				float num = (float)(base.ChartPanel.X + 20);
				float num2 = (float)(base.ChartPanel.Y + 20);
				float num3 = 350f;
				float height = 60f;
				base.RenderTarget.FillRectangle(new RectangleF(num, num2, num3, height), this.errorBackgroundBrush);
				base.RenderTarget.DrawRectangle(new RectangleF(num, num2, num3, height), this.errorBorderBrush, 2f);
				base.RenderTarget.DrawText(this.statusMessage, this.errorTextFormat, new RectangleF(num + 10f, num2 + 8f, num3 - 20f, 22f), this.errorTextBrush);
				base.RenderTarget.DrawText(this.licenseMessage, this.errorTextFormat, new RectangleF(num + 10f, num2 + 32f, num3 - 20f, 22f), this.errorTextBrush);
			}
			catch
			{
			}
		}

		// Token: 0x060000F5 RID: 245 RVA: 0x00007860 File Offset: 0x00005A60
		private void DrawAreaChart(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				int fromIndex = base.ChartBars.FromIndex;
				int toIndex = base.ChartBars.ToIndex;
				float num = (float)chartScale.GetYByValue(0.0);
				this.renderXPoints.Clear();
				this.renderCallValues.Clear();
				this.renderPutValues.Clear();
				for (int i = fromIndex; i <= toIndex; i++)
				{
					if (i >= 0 && i < base.Bars.Count)
					{
						DateTime time = base.Bars.GetTime(i);
						if (!this.EnforceSessionTime || this.IsWithinSessionHours(time))
						{
							OptionsSentimentRecord optionsSentimentRecord = this.FindDataForBar(time);
							double item = 0.0;
							double item2 = 0.0;
							if (optionsSentimentRecord != null)
							{
								this.GetCallPutValues(optionsSentimentRecord, out item, out item2);
							}
							this.renderXPoints.Add((float)chartControl.GetXByBarIndex(base.ChartBars, i));
							this.renderCallValues.Add(item);
							this.renderPutValues.Add(item2);
						}
					}
				}
				if (this.renderXPoints.Count >= 2)
				{
					using (SharpDX.Direct2D1.PathGeometry pathGeometry = this.BuildAreaGeometry(chartScale, this.renderXPoints, this.renderCallValues, num))
					{
						if (pathGeometry != null)
						{
							base.RenderTarget.FillGeometry(pathGeometry, this.callAreaBrush);
						}
					}
					using (SharpDX.Direct2D1.PathGeometry pathGeometry2 = this.BuildAreaGeometry(chartScale, this.renderXPoints, this.renderPutValues, num))
					{
						if (pathGeometry2 != null)
						{
							base.RenderTarget.FillGeometry(pathGeometry2, this.putAreaBrush);
						}
					}
					this.DrawSeriesLines(chartScale, this.renderXPoints, this.renderCallValues, this.callLineBrush);
					this.DrawSeriesLines(chartScale, this.renderXPoints, this.renderPutValues, this.putLineBrush);
					base.RenderTarget.DrawLine(new Vector2(this.renderXPoints[0], num), new Vector2(this.renderXPoints[this.renderXPoints.Count - 1], num), this.zeroLineBrush, 1f);
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Area render error: " + ex.Message);
			}
		}

		// Token: 0x060000F6 RID: 246 RVA: 0x00007ACC File Offset: 0x00005CCC
		private SharpDX.Direct2D1.PathGeometry BuildAreaGeometry(ChartScale chartScale, List<float> xPoints, List<double> values, float zeroY)
		{
			if (xPoints.Count < 2 || values.Count < 2)
			{
				return null;
			}
			SharpDX.Direct2D1.PathGeometry pathGeometry = new SharpDX.Direct2D1.PathGeometry(base.RenderTarget.Factory);
			using (GeometrySink geometrySink = pathGeometry.Open())
			{
				bool flag = false;
				for (int i = 0; i < xPoints.Count - 1; i++)
				{
					double num = values[i];
					double num2 = values[i + 1];
					if (num != 0.0 || num2 != 0.0)
					{
						float x = xPoints[i];
						float x2 = xPoints[i + 1];
						float y = (float)chartScale.GetYByValue(num);
						float y2 = (float)chartScale.GetYByValue(num2);
						geometrySink.BeginFigure(new Vector2(x, zeroY), FigureBegin.Filled);
						geometrySink.AddLine(new Vector2(x, y));
						geometrySink.AddLine(new Vector2(x2, y2));
						geometrySink.AddLine(new Vector2(x2, zeroY));
						geometrySink.EndFigure(FigureEnd.Closed);
						flag = true;
					}
				}
				geometrySink.Close();
				if (!flag)
				{
					pathGeometry.Dispose();
					return null;
				}
			}
			return pathGeometry;
		}

		// Token: 0x060000F7 RID: 247 RVA: 0x00007BF4 File Offset: 0x00005DF4
		private void DrawSeriesLines(ChartScale chartScale, List<float> xPoints, List<double> values, SharpDX.Direct2D1.SolidColorBrush lineBrush)
		{
			if (lineBrush == null)
			{
				return;
			}
			for (int i = 0; i < xPoints.Count - 1; i++)
			{
				double num = values[i];
				double num2 = values[i + 1];
				if (num != 0.0 || num2 != 0.0)
				{
					base.RenderTarget.DrawLine(new Vector2(xPoints[i], (float)chartScale.GetYByValue(num)), new Vector2(xPoints[i + 1], (float)chartScale.GetYByValue(num2)), lineBrush, 1.5f);
				}
			}
		}

		// Token: 0x060000F8 RID: 248 RVA: 0x00007C80 File Offset: 0x00005E80
		private void DrawSwitchPoints(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				int fromIndex = base.ChartBars.FromIndex;
				int toIndex = base.ChartBars.ToIndex;
				bool flag = false;
				bool flag2 = false;
				float num = (float)base.ChartPanel.Y;
				float num2 = (float)(base.ChartPanel.Y + base.ChartPanel.H);
				for (int i = fromIndex; i <= toIndex; i++)
				{
					if (i >= 0 && i < base.Bars.Count)
					{
						DateTime time = base.Bars.GetTime(i);
						if (!this.EnforceSessionTime || this.IsWithinSessionHours(time))
						{
							OptionsSentimentRecord optionsSentimentRecord = this.FindDataForBar(time);
							if (optionsSentimentRecord != null)
							{
								double value;
								double value2;
								this.GetCallPutValues(optionsSentimentRecord, out value, out value2);
								double num3 = Math.Abs(value);
								double num4 = Math.Abs(value2);
								if (num3 != 0.0 || num4 != 0.0)
								{
									bool flag3 = num3 >= num4;
									if (flag2 && flag3 != flag)
									{
										float num5 = (float)chartControl.GetXByBarIndex(base.ChartBars, i);
										float num6 = 4f;
										float num7 = 3f;
										float num9;
										for (float num8 = num; num8 < num2; num8 = num9 + num7)
										{
											num9 = Math.Min(num8 + num6, num2);
											base.RenderTarget.DrawLine(new Vector2(num5, num8), new Vector2(num5, num9), this.switchLineBrush, 1.5f);
										}
										string text = flag3 ? "Switch to CALLS" : "Switch to PUTS";
										float y = num2 - (float)this.SwitchFontSize - 15f;
										RectangleF rectangleF = new RectangleF(num5 - 60f, y, 120f, (float)(this.SwitchFontSize + 6));
										base.RenderTarget.FillRectangle(rectangleF, this.panelBackgroundBrush);
										base.RenderTarget.DrawText(text, this.switchTextFormat, rectangleF, flag3 ? this.callTextBrush : this.putTextBrush);
									}
									flag = flag3;
									flag2 = true;
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Switch render error: " + ex.Message);
			}
		}

		// Token: 0x060000F9 RID: 249 RVA: 0x00007EAC File Offset: 0x000060AC
		private void DrawPercentageLabel(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				double value;
				double value2;
				this.GetCallPutValues(this.latestData, out value, out value2);
				double num = Math.Abs(value);
				double num2 = Math.Abs(value2);
				double num3 = num + num2;
				if (num3 != 0.0)
				{
					double num4 = num / num3 * 100.0;
					double num5 = num2 / num3 * 100.0;
					string str = (this.DTEMode == SentimentDTESelection.ZeroDTE) ? "0DTE" : "1DTE";
					string str2 = "";
					if (this.FilterMode == SentimentFilter.BuyOnly)
					{
						str2 = " | Buy Only";
					}
					else if (this.FilterMode == SentimentFilter.SellOnly)
					{
						str2 = " | Sell Only";
					}
					string text = "RT Options Sentiment   " + str + str2;
					string text2 = string.Format("CALLS {0:F0}%", num4);
					string text3 = string.Format("PUTS {0:F0}%", num5);
					float num6 = (float)(base.ChartPanel.X + 10);
					float num7 = (float)(base.ChartPanel.Y + 5);
					float num8 = (float)(this.PercentFontSize + 4);
					float num9 = 320f;
					float height = num8 * 3f + 10f;
					base.RenderTarget.FillRectangle(new RectangleF(num6, num7, num9, height), this.panelBackgroundBrush);
					base.RenderTarget.DrawText(text, this.percentTextFormat, new RectangleF(num6 + 5f, num7 + 3f, num9 - 10f, num8), this.whiteTextBrush);
					base.RenderTarget.DrawText(text2, this.percentTextFormat, new RectangleF(num6 + 5f, num7 + num8 + 3f, num9 / 2f - 10f, num8), this.callTextBrush);
					base.RenderTarget.DrawText(text3, this.percentTextFormat, new RectangleF(num6 + num9 / 2f, num7 + num8 + 3f, num9 / 2f - 10f, num8), this.putTextBrush);
					float y = num7 + num8 * 2f + 5f;
					float height2 = 6f;
					float num10 = num9 - 10f;
					float num11 = (float)(num4 / 100.0 * (double)num10);
					base.RenderTarget.FillRectangle(new RectangleF(num6 + 5f, y, num11, height2), this.callTextBrush);
					base.RenderTarget.FillRectangle(new RectangleF(num6 + 5f + num11, y, num10 - num11, height2), this.putTextBrush);
				}
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Percent render error: " + ex.Message);
			}
		}

		// Token: 0x060000FA RID: 250 RVA: 0x00008160 File Offset: 0x00006360
		private void DrawInfoLegend(ChartControl chartControl, ChartScale chartScale)
		{
			try
			{
				float num = (float)(base.ChartPanel.X + base.ChartPanel.W - 220);
				float num2 = (float)(base.ChartPanel.Y + 5);
				float num3 = (float)(this.LegendFontSize + 4);
				float num4 = 210f;
				float height = num3 * 4f + 10f;
				base.RenderTarget.FillRectangle(new RectangleF(num, num2, num4, height), this.legendBackgroundBrush);
				float num5 = num2 + 3f;
				base.RenderTarget.DrawText(this.statusMessage, this.legendTextFormat, new RectangleF(num + 5f, num5, num4 - 10f, num3), this.whiteTextBrush);
				num5 += num3;
				string text = this.DataSource.ToString();
				string text2 = (this.DTEMode == SentimentDTESelection.ZeroDTE) ? "0DTE" : "1DTE";
				base.RenderTarget.DrawText(string.Concat(new string[]
				{
					"Source: ",
					text,
					" (",
					text2,
					")"
				}), this.legendTextFormat, new RectangleF(num + 5f, num5, num4 - 10f, num3), this.whiteTextBrush);
				num5 += num3;
				string str = this.FilterMode.ToString();
				base.RenderTarget.DrawText("Filter: " + str, this.legendTextFormat, new RectangleF(num + 5f, num5, num4 - 10f, num3), this.whiteTextBrush);
				num5 += num3;
				base.RenderTarget.DrawText(string.Format("Records: {0}", this.totalRecords), this.legendTextFormat, new RectangleF(num + 5f, num5, num4 - 10f, num3), this.whiteTextBrush);
			}
			catch (Exception ex)
			{
				base.Print("RT Options Sentiment Legend render error: " + ex.Message);
			}
		}

		// Token: 0x060000FB RID: 251 RVA: 0x0000562C File Offset: 0x0000382C
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

		// Token: 0x1700005A RID: 90
		// (get) Token: 0x060000FC RID: 252 RVA: 0x0000273B File Offset: 0x0000093B
		// (set) Token: 0x060000FD RID: 253 RVA: 0x00002743 File Offset: 0x00000943
		[NinjaScriptProperty]
		[Display(Name = "License Key", Description = "Lisans anahtarinizi girin", Order = 1, GroupName = "1. License")]
		public string LicenseKey { get; set; }

		// Token: 0x1700005B RID: 91
		// (get) Token: 0x060000FE RID: 254 RVA: 0x0000274C File Offset: 0x0000094C
		// (set) Token: 0x060000FF RID: 255 RVA: 0x00002754 File Offset: 0x00000954
		[NinjaScriptProperty]
		[Display(Name = "Ticker", Description = "Ticker symbol (e.g. NQ_NDX, ES_SPX)", Order = 2, GroupName = "1. License")]
		public string Ticker { get; set; }

		// Token: 0x1700005C RID: 92
		// (get) Token: 0x06000100 RID: 256 RVA: 0x0000275D File Offset: 0x0000095D
		// (set) Token: 0x06000101 RID: 257 RVA: 0x00002765 File Offset: 0x00000965
		[NinjaScriptProperty]
		[Range(1, 30)]
		[Display(Name = "Days to Load", Order = 3, GroupName = "1. License")]
		public int DaysToLoad { get; set; }

		// Token: 0x1700005D RID: 93
		// (get) Token: 0x06000102 RID: 258 RVA: 0x0000276E File Offset: 0x0000096E
		// (set) Token: 0x06000103 RID: 259 RVA: 0x00002776 File Offset: 0x00000976
		[NinjaScriptProperty]
		[Display(Name = "Data Source", Description = "Veri kaynagi: AggregatedDelta, NetDelta, GammaImbalance, Convexity", Order = 1, GroupName = "2. Data Source")]
		public SentimentDataSource DataSource { get; set; }

		// Token: 0x1700005E RID: 94
		// (get) Token: 0x06000104 RID: 260 RVA: 0x0000277F File Offset: 0x0000097F
		// (set) Token: 0x06000105 RID: 261 RVA: 0x00002787 File Offset: 0x00000987
		[NinjaScriptProperty]
		[Display(Name = "DTE Mode", Description = "0DTE veya 1DTE", Order = 2, GroupName = "2. Data Source")]
		public SentimentDTESelection DTEMode { get; set; }

		// Token: 0x1700005F RID: 95
		// (get) Token: 0x06000106 RID: 262 RVA: 0x00002790 File Offset: 0x00000990
		// (set) Token: 0x06000107 RID: 263 RVA: 0x00002798 File Offset: 0x00000998
		[NinjaScriptProperty]
		[Display(Name = "Filter", Description = "All, BuyOnly, SellOnly", Order = 3, GroupName = "2. Data Source")]
		public SentimentFilter FilterMode { get; set; }

		// Token: 0x17000060 RID: 96
		// (get) Token: 0x06000108 RID: 264 RVA: 0x000027A1 File Offset: 0x000009A1
		// (set) Token: 0x06000109 RID: 265 RVA: 0x000027A9 File Offset: 0x000009A9
		[XmlIgnore]
		[Display(Name = "Call Area Color", Order = 1, GroupName = "3. Area Chart")]
		public System.Windows.Media.Brush CallAreaColor { get; set; }

		// Token: 0x17000061 RID: 97
		// (get) Token: 0x0600010A RID: 266 RVA: 0x000027B2 File Offset: 0x000009B2
		// (set) Token: 0x0600010B RID: 267 RVA: 0x000027BF File Offset: 0x000009BF
		[Browsable(false)]
		public string CallAreaColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.CallAreaColor);
			}
			set
			{
				this.CallAreaColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000062 RID: 98
		// (get) Token: 0x0600010C RID: 268 RVA: 0x000027CD File Offset: 0x000009CD
		// (set) Token: 0x0600010D RID: 269 RVA: 0x000027D5 File Offset: 0x000009D5
		[XmlIgnore]
		[Display(Name = "Put Area Color", Order = 2, GroupName = "3. Area Chart")]
		public System.Windows.Media.Brush PutAreaColor { get; set; }

		// Token: 0x17000063 RID: 99
		// (get) Token: 0x0600010E RID: 270 RVA: 0x000027DE File Offset: 0x000009DE
		// (set) Token: 0x0600010F RID: 271 RVA: 0x000027EB File Offset: 0x000009EB
		[Browsable(false)]
		public string PutAreaColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.PutAreaColor);
			}
			set
			{
				this.PutAreaColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000064 RID: 100
		// (get) Token: 0x06000110 RID: 272 RVA: 0x000027F9 File Offset: 0x000009F9
		// (set) Token: 0x06000111 RID: 273 RVA: 0x00002801 File Offset: 0x00000A01
		[Range(10, 100)]
		[Display(Name = "Area Opacity (%)", Order = 3, GroupName = "3. Area Chart")]
		public int AreaOpacity { get; set; }

		// Token: 0x17000065 RID: 101
		// (get) Token: 0x06000112 RID: 274 RVA: 0x0000280A File Offset: 0x00000A0A
		// (set) Token: 0x06000113 RID: 275 RVA: 0x00002812 File Offset: 0x00000A12
		[XmlIgnore]
		[Display(Name = "Zero Line Color", Order = 4, GroupName = "3. Area Chart")]
		public System.Windows.Media.Brush ZeroLineColor { get; set; }

		// Token: 0x17000066 RID: 102
		// (get) Token: 0x06000114 RID: 276 RVA: 0x0000281B File Offset: 0x00000A1B
		// (set) Token: 0x06000115 RID: 277 RVA: 0x00002828 File Offset: 0x00000A28
		[Browsable(false)]
		public string ZeroLineColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.ZeroLineColor);
			}
			set
			{
				this.ZeroLineColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x17000067 RID: 103
		// (get) Token: 0x06000116 RID: 278 RVA: 0x00002836 File Offset: 0x00000A36
		// (set) Token: 0x06000117 RID: 279 RVA: 0x0000283E File Offset: 0x00000A3E
		[Display(Name = "Show Switch Labels", Order = 1, GroupName = "4. Switch Detection")]
		public bool ShowSwitchLabels { get; set; }

		// Token: 0x17000068 RID: 104
		// (get) Token: 0x06000118 RID: 280 RVA: 0x00002847 File Offset: 0x00000A47
		// (set) Token: 0x06000119 RID: 281 RVA: 0x0000284F File Offset: 0x00000A4F
		[Range(6, 16)]
		[Display(Name = "Switch Font Size", Order = 2, GroupName = "4. Switch Detection")]
		public int SwitchFontSize { get; set; }

		// Token: 0x17000069 RID: 105
		// (get) Token: 0x0600011A RID: 282 RVA: 0x00002858 File Offset: 0x00000A58
		// (set) Token: 0x0600011B RID: 283 RVA: 0x00002860 File Offset: 0x00000A60
		[XmlIgnore]
		[Display(Name = "Switch Line Color", Order = 3, GroupName = "4. Switch Detection")]
		public System.Windows.Media.Brush SwitchLineColor { get; set; }

		// Token: 0x1700006A RID: 106
		// (get) Token: 0x0600011C RID: 284 RVA: 0x00002869 File Offset: 0x00000A69
		// (set) Token: 0x0600011D RID: 285 RVA: 0x00002876 File Offset: 0x00000A76
		[Browsable(false)]
		public string SwitchLineColorSerializable
		{
			get
			{
				return Serialize.BrushToString(this.SwitchLineColor);
			}
			set
			{
				this.SwitchLineColor = Serialize.StringToBrush(value);
			}
		}

		// Token: 0x1700006B RID: 107
		// (get) Token: 0x0600011E RID: 286 RVA: 0x00002884 File Offset: 0x00000A84
		// (set) Token: 0x0600011F RID: 287 RVA: 0x0000288C File Offset: 0x00000A8C
		[Display(Name = "Show Percentage", Order = 1, GroupName = "5. Percentage")]
		public bool ShowPercentage { get; set; }

		// Token: 0x1700006C RID: 108
		// (get) Token: 0x06000120 RID: 288 RVA: 0x00002895 File Offset: 0x00000A95
		// (set) Token: 0x06000121 RID: 289 RVA: 0x0000289D File Offset: 0x00000A9D
		[Range(8, 20)]
		[Display(Name = "Percentage Font Size", Order = 2, GroupName = "5. Percentage")]
		public int PercentFontSize { get; set; }

		// Token: 0x1700006D RID: 109
		// (get) Token: 0x06000122 RID: 290 RVA: 0x000028A6 File Offset: 0x00000AA6
		// (set) Token: 0x06000123 RID: 291 RVA: 0x000028AE File Offset: 0x00000AAE
		[Display(Name = "Show Legend", Order = 1, GroupName = "6. Legend")]
		public bool ShowLegend { get; set; }

		// Token: 0x1700006E RID: 110
		// (get) Token: 0x06000124 RID: 292 RVA: 0x000028B7 File Offset: 0x00000AB7
		// (set) Token: 0x06000125 RID: 293 RVA: 0x000028BF File Offset: 0x00000ABF
		[Range(8, 16)]
		[Display(Name = "Legend Font Size", Order = 2, GroupName = "6. Legend")]
		public int LegendFontSize { get; set; }

		// Token: 0x1700006F RID: 111
		// (get) Token: 0x06000126 RID: 294 RVA: 0x000028C8 File Offset: 0x00000AC8
		// (set) Token: 0x06000127 RID: 295 RVA: 0x000028D0 File Offset: 0x00000AD0
		[Display(Name = "Enforce Session Time (9:30-16:00 ET)", Order = 1, GroupName = "7. Session")]
		public bool EnforceSessionTime { get; set; }

		// Token: 0x04000082 RID: 130
		private const string EDGE_FUNCTION_URL = "http://134.209.228.88:8080/indicator-api";

		// Token: 0x04000083 RID: 131
		private const int POLL_INTERVAL_SEC = 15;

		// Token: 0x04000084 RID: 132
		private bool licenseValid;

		// Token: 0x04000085 RID: 133
		private string licenseMessage = "";

		// Token: 0x04000086 RID: 134
		private string machineHwid = "";

		// Token: 0x04000087 RID: 135
		private Dictionary<long, OptionsSentimentRecord> dataByTimestamp = new Dictionary<long, OptionsSentimentRecord>();

		// Token: 0x04000088 RID: 136
		private Dictionary<DateTime, List<OptionsSentimentRecord>> dataByDate = new Dictionary<DateTime, List<OptionsSentimentRecord>>();

		// Token: 0x04000089 RID: 137
		private Dictionary<long, OptionsSentimentRecord> barRecordCache = new Dictionary<long, OptionsSentimentRecord>();

		// Token: 0x0400008A RID: 138
		private OptionsSentimentRecord latestData;

		// Token: 0x0400008B RID: 139
		private string statusMessage = "Initializing...";

		// Token: 0x0400008C RID: 140
		private bool dataLoaded;

		// Token: 0x0400008D RID: 141
		private int totalRecords;

		// Token: 0x0400008E RID: 142
		private long lastLoadedTimestamp;

		// Token: 0x0400008F RID: 143
		private Timer dbPollTimer;

		// Token: 0x04000090 RID: 144
		private volatile bool isPolling;

		// Token: 0x04000091 RID: 145
		private volatile bool hasNewData;

		// Token: 0x04000092 RID: 146
		private int pollErrorCount;

		// Token: 0x04000093 RID: 147
		private int consecutiveEmptyPolls;

		// Token: 0x04000094 RID: 148
		private object lockObject = new object();

		// Token: 0x04000095 RID: 149
		private object dataLock = new object();

		// Token: 0x04000096 RID: 150
		private TimeZoneInfo easternTimeZone;

		// Token: 0x04000097 RID: 151
		private SharpDX.DirectWrite.TextFormat legendTextFormat;

		// Token: 0x04000098 RID: 152
		private SharpDX.DirectWrite.TextFormat percentTextFormat;

		// Token: 0x04000099 RID: 153
		private SharpDX.DirectWrite.TextFormat switchTextFormat;

		// Token: 0x0400009A RID: 154
		private SharpDX.DirectWrite.TextFormat errorTextFormat;

		// Token: 0x0400009B RID: 155
		private SharpDX.Direct2D1.SolidColorBrush errorBackgroundBrush;

		// Token: 0x0400009C RID: 156
		private SharpDX.Direct2D1.SolidColorBrush errorBorderBrush;

		// Token: 0x0400009D RID: 157
		private SharpDX.Direct2D1.SolidColorBrush errorTextBrush;

		// Token: 0x0400009E RID: 158
		private SharpDX.Direct2D1.SolidColorBrush callAreaBrush;

		// Token: 0x0400009F RID: 159
		private SharpDX.Direct2D1.SolidColorBrush callLineBrush;

		// Token: 0x040000A0 RID: 160
		private SharpDX.Direct2D1.SolidColorBrush putAreaBrush;

		// Token: 0x040000A1 RID: 161
		private SharpDX.Direct2D1.SolidColorBrush putLineBrush;

		// Token: 0x040000A2 RID: 162
		private SharpDX.Direct2D1.SolidColorBrush zeroLineBrush;

		// Token: 0x040000A3 RID: 163
		private SharpDX.Direct2D1.SolidColorBrush switchLineBrush;

		// Token: 0x040000A4 RID: 164
		private SharpDX.Direct2D1.SolidColorBrush panelBackgroundBrush;

		// Token: 0x040000A5 RID: 165
		private SharpDX.Direct2D1.SolidColorBrush legendBackgroundBrush;

		// Token: 0x040000A6 RID: 166
		private SharpDX.Direct2D1.SolidColorBrush whiteTextBrush;

		// Token: 0x040000A7 RID: 167
		private SharpDX.Direct2D1.SolidColorBrush callTextBrush;

		// Token: 0x040000A8 RID: 168
		private SharpDX.Direct2D1.SolidColorBrush putTextBrush;

		// Token: 0x040000A9 RID: 169
		private readonly List<float> renderXPoints = new List<float>(512);

		// Token: 0x040000AA RID: 170
		private readonly List<double> renderCallValues = new List<double>(512);

		// Token: 0x040000AB RID: 171
		private readonly List<double> renderPutValues = new List<double>(512);
	}
}
