using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Midi;

namespace Mp2Editor.Core
{
	public class SysexTx
	{
		private Midi.OutputDevice output;

		public static byte[] ReceiveSysex()
		{
			var input = Midi.InputDevice.InstalledDevices.Single(x => x.Name.Contains("USB"));
			try
			{
				input.Open();
				byte[] output = null;
				InputDevice.SysExHandler handler = msg =>
				{
					output = msg.Data;
				};
				input.SysEx += handler;
				input.StartReceiving(null, true);

				while (output == null)
				{
					Thread.Sleep(500);
				}

				return output;
			}
			finally
			{
				if (input.IsReceiving)
					input.StopReceiving();

				if (input.IsOpen)
					input.Close();
			}
		}

		public static void RequestPatch()
		{
			var output = Midi.OutputDevice.InstalledDevices.Single(x => x.Name.Contains("USB"));
			try
			{
				output.Open();
				output.SendSysEx(Mp2Sysex.RequestDump);
			}
			finally
			{
				if (output.IsOpen)
					output.Close();
			}
		}
	}
}
