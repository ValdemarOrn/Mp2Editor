using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mp2Editor.Core
{
	public class Mp2Sysex
	{
		/// <summary>
		/// Triggers a sysex patch dump on channel 1
		/// </summary>
		public static byte[] RequestDump = new[] { 0xF0, 0x0D, 0x00, 0x08, 0x03, 0x7F, 0x69, 0xF7 }.Select(x => (byte)x).ToArray();

		/*
		Header info:
		http://adadepot.com/index.php?topic=151.75

		This is for the ADA Mp1, but MP2 seems to be the same
		byte 1 ==> F0 (sysex start)
		byte 2 ==> 0D (ADA ID)
		byte 3 ==> midi channel (ch1 =00h ch16 0Fh)
		byte 4 ==> 09 send parameter command
		byte 5 ==> 01 (MP-1 ID)  -- 03 = MP-2 ID
		byte 6 ==> 7F (all parameters)

		for byte 4, the possible values seem to be:
		08 - Send Program Command
		09 - Load Program Command
		0A - Send Library Command
		0B - Load Library Command
		*/
	}
}
