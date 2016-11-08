using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowProfile.Core.Extensions;

namespace Mp2Editor.Core
{
	public class SysexDump
	{
		public SysexDump(byte[] data)
		{
			this.Data = data;
		}

		public byte[] Data { get; set; }

		public string GetHex(int perLine)
		{
			var blocks = Data.Chunk(perLine);
			var output = "";
			int i = 0;

			foreach (var chunk in blocks)
			{
				output += i.ToString("d3") + ": " + string.Join(" ", chunk.Select(x => x.ToString("X2"))) + Environment.NewLine;
				i += perLine;
			}

			return output;
		}

		public string GetDecimal(int perLine)
		{
			var blocks = Data.Chunk(perLine);
			var output = "";
			int i = 0;

			foreach (var chunk in blocks)
			{
				output += i.ToString("d3") + ": " + string.Join(" ", chunk.Select(x => x.ToString("d3"))) + Environment.NewLine;

				i += perLine;
			}

			return output;
		}
	}
}
