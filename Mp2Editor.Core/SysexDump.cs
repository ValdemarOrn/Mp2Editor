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
		public static string GetHex(byte[] data, int perLine)
		{
			var blocks = data.Chunk(perLine);
			var output = "";
			int i = 0;

			foreach (var chunk in blocks)
			{
				output += i.ToString("d3") + ": " + string.Join(" ", chunk.Select(x => x.ToString("X2"))) + Environment.NewLine;
				i += perLine;
			}

			return output;
		}

		public static string GetDecimal(byte[] data, int perLine)
		{
			var blocks = data.Chunk(perLine);
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
