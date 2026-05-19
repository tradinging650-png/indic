using System;

namespace NinjaTrader.NinjaScript.Indicators
{
	// Token: 0x02000007 RID: 7
	public class GammaLevelRecordV3
	{
		// Token: 0x1700003F RID: 63
		// (get) Token: 0x060000A4 RID: 164 RVA: 0x00002532 File Offset: 0x00000732
		// (set) Token: 0x060000A5 RID: 165 RVA: 0x0000253A File Offset: 0x0000073A
		public long Timestamp { get; set; }

		// Token: 0x17000040 RID: 64
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x00002543 File Offset: 0x00000743
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x0000254B File Offset: 0x0000074B
		public string DateTime { get; set; }

		// Token: 0x17000041 RID: 65
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x00002554 File Offset: 0x00000754
		// (set) Token: 0x060000A9 RID: 169 RVA: 0x0000255C File Offset: 0x0000075C
		public string Date { get; set; }

		// Token: 0x17000042 RID: 66
		// (get) Token: 0x060000AA RID: 170 RVA: 0x00002565 File Offset: 0x00000765
		// (set) Token: 0x060000AB RID: 171 RVA: 0x0000256D File Offset: 0x0000076D
		public string Ticker { get; set; }

		// Token: 0x17000043 RID: 67
		// (get) Token: 0x060000AC RID: 172 RVA: 0x00002576 File Offset: 0x00000776
		// (set) Token: 0x060000AD RID: 173 RVA: 0x0000257E File Offset: 0x0000077E
		public double Spot { get; set; }

		// Token: 0x17000044 RID: 68
		// (get) Token: 0x060000AE RID: 174 RVA: 0x00002587 File Offset: 0x00000787
		// (set) Token: 0x060000AF RID: 175 RVA: 0x0000258F File Offset: 0x0000078F
		public double Z_MLGamma { get; set; }

		// Token: 0x17000045 RID: 69
		// (get) Token: 0x060000B0 RID: 176 RVA: 0x00002598 File Offset: 0x00000798
		// (set) Token: 0x060000B1 RID: 177 RVA: 0x000025A0 File Offset: 0x000007A0
		public double Z_MSGamma { get; set; }

		// Token: 0x17000046 RID: 70
		// (get) Token: 0x060000B2 RID: 178 RVA: 0x000025A9 File Offset: 0x000007A9
		// (set) Token: 0x060000B3 RID: 179 RVA: 0x000025B1 File Offset: 0x000007B1
		public double O_MLGamma { get; set; }

		// Token: 0x17000047 RID: 71
		// (get) Token: 0x060000B4 RID: 180 RVA: 0x000025BA File Offset: 0x000007BA
		// (set) Token: 0x060000B5 RID: 181 RVA: 0x000025C2 File Offset: 0x000007C2
		public double O_MSGamma { get; set; }

		// Token: 0x17000048 RID: 72
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x000025CB File Offset: 0x000007CB
		// (set) Token: 0x060000B7 RID: 183 RVA: 0x000025D3 File Offset: 0x000007D3
		public double Zero_MCall { get; set; }

		// Token: 0x17000049 RID: 73
		// (get) Token: 0x060000B8 RID: 184 RVA: 0x000025DC File Offset: 0x000007DC
		// (set) Token: 0x060000B9 RID: 185 RVA: 0x000025E4 File Offset: 0x000007E4
		public double Zero_MPut { get; set; }

		// Token: 0x1700004A RID: 74
		// (get) Token: 0x060000BA RID: 186 RVA: 0x000025ED File Offset: 0x000007ED
		// (set) Token: 0x060000BB RID: 187 RVA: 0x000025F5 File Offset: 0x000007F5
		public double One_MCall { get; set; }

		// Token: 0x1700004B RID: 75
		// (get) Token: 0x060000BC RID: 188 RVA: 0x000025FE File Offset: 0x000007FE
		// (set) Token: 0x060000BD RID: 189 RVA: 0x00002606 File Offset: 0x00000806
		public double One_MPut { get; set; }

		// Token: 0x1700004C RID: 76
		// (get) Token: 0x060000BE RID: 190 RVA: 0x0000260F File Offset: 0x0000080F
		// (set) Token: 0x060000BF RID: 191 RVA: 0x00002617 File Offset: 0x00000817
		public double GexOFlow { get; set; }

		// Token: 0x1700004D RID: 77
		// (get) Token: 0x060000C0 RID: 192 RVA: 0x00002620 File Offset: 0x00000820
		// (set) Token: 0x060000C1 RID: 193 RVA: 0x00002628 File Offset: 0x00000828
		public double OneGexOFlow { get; set; }

		// Token: 0x1700004E RID: 78
		// (get) Token: 0x060000C2 RID: 194 RVA: 0x00002631 File Offset: 0x00000831
		// (set) Token: 0x060000C3 RID: 195 RVA: 0x00002639 File Offset: 0x00000839
		public double CrvOFlow { get; set; }

		// Token: 0x1700004F RID: 79
		// (get) Token: 0x060000C4 RID: 196 RVA: 0x00002642 File Offset: 0x00000842
		// (set) Token: 0x060000C5 RID: 197 RVA: 0x0000264A File Offset: 0x0000084A
		public double OneCrvOFlow { get; set; }

		// Token: 0x17000050 RID: 80
		// (get) Token: 0x060000C6 RID: 198 RVA: 0x00002653 File Offset: 0x00000853
		// (set) Token: 0x060000C7 RID: 199 RVA: 0x0000265B File Offset: 0x0000085B
		public double ZeroGamma { get; set; }

		// Token: 0x17000051 RID: 81
		// (get) Token: 0x060000C8 RID: 200 RVA: 0x00002664 File Offset: 0x00000864
		// (set) Token: 0x060000C9 RID: 201 RVA: 0x0000266C File Offset: 0x0000086C
		public double MajorPosVol { get; set; }

		// Token: 0x17000052 RID: 82
		// (get) Token: 0x060000CA RID: 202 RVA: 0x00002675 File Offset: 0x00000875
		// (set) Token: 0x060000CB RID: 203 RVA: 0x0000267D File Offset: 0x0000087D
		public double MajorNegVol { get; set; }

		// Token: 0x17000053 RID: 83
		// (get) Token: 0x060000CC RID: 204 RVA: 0x00002686 File Offset: 0x00000886
		// (set) Token: 0x060000CD RID: 205 RVA: 0x0000268E File Offset: 0x0000088E
		public double MajorPosOI { get; set; }

		// Token: 0x17000054 RID: 84
		// (get) Token: 0x060000CE RID: 206 RVA: 0x00002697 File Offset: 0x00000897
		// (set) Token: 0x060000CF RID: 207 RVA: 0x0000269F File Offset: 0x0000089F
		public double MajorNegOI { get; set; }

		// Token: 0x17000055 RID: 85
		// (get) Token: 0x060000D0 RID: 208 RVA: 0x000026A8 File Offset: 0x000008A8
		// (set) Token: 0x060000D1 RID: 209 RVA: 0x000026B0 File Offset: 0x000008B0
		public double NetGexVol { get; set; }

		// Token: 0x17000056 RID: 86
		// (get) Token: 0x060000D2 RID: 210 RVA: 0x000026B9 File Offset: 0x000008B9
		// (set) Token: 0x060000D3 RID: 211 RVA: 0x000026C1 File Offset: 0x000008C1
		public double StateZeroGamma { get; set; }

		// Token: 0x17000057 RID: 87
		// (get) Token: 0x060000D4 RID: 212 RVA: 0x000026CA File Offset: 0x000008CA
		// (set) Token: 0x060000D5 RID: 213 RVA: 0x000026D2 File Offset: 0x000008D2
		public double StateMajorPosVol { get; set; }

		// Token: 0x17000058 RID: 88
		// (get) Token: 0x060000D6 RID: 214 RVA: 0x000026DB File Offset: 0x000008DB
		// (set) Token: 0x060000D7 RID: 215 RVA: 0x000026E3 File Offset: 0x000008E3
		public double StateMajorNegVol { get; set; }

		// Token: 0x17000059 RID: 89
		// (get) Token: 0x060000D8 RID: 216 RVA: 0x000026EC File Offset: 0x000008EC
		// (set) Token: 0x060000D9 RID: 217 RVA: 0x000026F4 File Offset: 0x000008F4
		public double StateNetGex { get; set; }
	}
}
