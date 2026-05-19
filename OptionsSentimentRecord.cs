using System;

namespace NinjaTrader.NinjaScript.Indicators
{
	// Token: 0x0200000A RID: 10
	public class OptionsSentimentRecord
	{
		// Token: 0x17000070 RID: 112
		// (get) Token: 0x0600012C RID: 300 RVA: 0x000028E5 File Offset: 0x00000AE5
		// (set) Token: 0x0600012D RID: 301 RVA: 0x000028ED File Offset: 0x00000AED
		public long Timestamp { get; set; }

		// Token: 0x17000071 RID: 113
		// (get) Token: 0x0600012E RID: 302 RVA: 0x000028F6 File Offset: 0x00000AF6
		// (set) Token: 0x0600012F RID: 303 RVA: 0x000028FE File Offset: 0x00000AFE
		public string DateTime { get; set; }

		// Token: 0x17000072 RID: 114
		// (get) Token: 0x06000130 RID: 304 RVA: 0x00002907 File Offset: 0x00000B07
		// (set) Token: 0x06000131 RID: 305 RVA: 0x0000290F File Offset: 0x00000B0F
		public string Date { get; set; }

		// Token: 0x17000073 RID: 115
		// (get) Token: 0x06000132 RID: 306 RVA: 0x00002918 File Offset: 0x00000B18
		// (set) Token: 0x06000133 RID: 307 RVA: 0x00002920 File Offset: 0x00000B20
		public DateTime SessionDate { get; set; }

		// Token: 0x17000074 RID: 116
		// (get) Token: 0x06000134 RID: 308 RVA: 0x00002929 File Offset: 0x00000B29
		// (set) Token: 0x06000135 RID: 309 RVA: 0x00002931 File Offset: 0x00000B31
		public double Spot { get; set; }

		// Token: 0x17000075 RID: 117
		// (get) Token: 0x06000136 RID: 310 RVA: 0x0000293A File Offset: 0x00000B3A
		// (set) Token: 0x06000137 RID: 311 RVA: 0x00002942 File Offset: 0x00000B42
		public double AggDex { get; set; }

		// Token: 0x17000076 RID: 118
		// (get) Token: 0x06000138 RID: 312 RVA: 0x0000294B File Offset: 0x00000B4B
		// (set) Token: 0x06000139 RID: 313 RVA: 0x00002953 File Offset: 0x00000B53
		public double OneAggDex { get; set; }

		// Token: 0x17000077 RID: 119
		// (get) Token: 0x0600013A RID: 314 RVA: 0x0000295C File Offset: 0x00000B5C
		// (set) Token: 0x0600013B RID: 315 RVA: 0x00002964 File Offset: 0x00000B64
		public double AggCallDex { get; set; }

		// Token: 0x17000078 RID: 120
		// (get) Token: 0x0600013C RID: 316 RVA: 0x0000296D File Offset: 0x00000B6D
		// (set) Token: 0x0600013D RID: 317 RVA: 0x00002975 File Offset: 0x00000B75
		public double OneAggCallDex { get; set; }

		// Token: 0x17000079 RID: 121
		// (get) Token: 0x0600013E RID: 318 RVA: 0x0000297E File Offset: 0x00000B7E
		// (set) Token: 0x0600013F RID: 319 RVA: 0x00002986 File Offset: 0x00000B86
		public double AggPutDex { get; set; }

		// Token: 0x1700007A RID: 122
		// (get) Token: 0x06000140 RID: 320 RVA: 0x0000298F File Offset: 0x00000B8F
		// (set) Token: 0x06000141 RID: 321 RVA: 0x00002997 File Offset: 0x00000B97
		public double OneAggPutDex { get; set; }

		// Token: 0x1700007B RID: 123
		// (get) Token: 0x06000142 RID: 322 RVA: 0x000029A0 File Offset: 0x00000BA0
		// (set) Token: 0x06000143 RID: 323 RVA: 0x000029A8 File Offset: 0x00000BA8
		public double NetDex { get; set; }

		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000144 RID: 324 RVA: 0x000029B1 File Offset: 0x00000BB1
		// (set) Token: 0x06000145 RID: 325 RVA: 0x000029B9 File Offset: 0x00000BB9
		public double OneNetDex { get; set; }

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x06000146 RID: 326 RVA: 0x000029C2 File Offset: 0x00000BC2
		// (set) Token: 0x06000147 RID: 327 RVA: 0x000029CA File Offset: 0x00000BCA
		public double NetCallDex { get; set; }

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x06000148 RID: 328 RVA: 0x000029D3 File Offset: 0x00000BD3
		// (set) Token: 0x06000149 RID: 329 RVA: 0x000029DB File Offset: 0x00000BDB
		public double OneNetCallDex { get; set; }

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600014A RID: 330 RVA: 0x000029E4 File Offset: 0x00000BE4
		// (set) Token: 0x0600014B RID: 331 RVA: 0x000029EC File Offset: 0x00000BEC
		public double NetPutDex { get; set; }

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x0600014C RID: 332 RVA: 0x000029F5 File Offset: 0x00000BF5
		// (set) Token: 0x0600014D RID: 333 RVA: 0x000029FD File Offset: 0x00000BFD
		public double OneNetPutDex { get; set; }

		// Token: 0x17000081 RID: 129
		// (get) Token: 0x0600014E RID: 334 RVA: 0x00002A06 File Offset: 0x00000C06
		// (set) Token: 0x0600014F RID: 335 RVA: 0x00002A0E File Offset: 0x00000C0E
		public double Zgr { get; set; }

		// Token: 0x17000082 RID: 130
		// (get) Token: 0x06000150 RID: 336 RVA: 0x00002A17 File Offset: 0x00000C17
		// (set) Token: 0x06000151 RID: 337 RVA: 0x00002A1F File Offset: 0x00000C1F
		public double Ogr { get; set; }

		// Token: 0x17000083 RID: 131
		// (get) Token: 0x06000152 RID: 338 RVA: 0x00002A28 File Offset: 0x00000C28
		// (set) Token: 0x06000153 RID: 339 RVA: 0x00002A30 File Offset: 0x00000C30
		public double Zcvr { get; set; }

		// Token: 0x17000084 RID: 132
		// (get) Token: 0x06000154 RID: 340 RVA: 0x00002A39 File Offset: 0x00000C39
		// (set) Token: 0x06000155 RID: 341 RVA: 0x00002A41 File Offset: 0x00000C41
		public double Ocvr { get; set; }
	}
}
