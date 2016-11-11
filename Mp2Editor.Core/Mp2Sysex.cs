using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LowProfile.Core.Extensions;

namespace Mp2Editor.Core
{
	public class Mp2Sysex
	{
		/// <summary>
		/// Triggers a sysex patch dump on channel 1
		/// </summary>
		private static byte[] RequestDump = new[] { 0xF0, 0x0D, 0x00, 0x08, 0x03, 0x7F, 0x69, 0xF7 }.Select(x => (byte)x).ToArray();
        private static byte[] ProgramHeader = new[] { 0xF0, 0x0D, 0x00, 0x09, 0x03, 0x7F }.Select(x => (byte)x).ToArray();

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

	    public static byte[] RequestDumpSysex(int midichannel1_16)
	    {
	        var bytes = RequestDump.Select(x => x).ToArray();
	        bytes[2] = (byte)(midichannel1_16 - 1);
	        var checksum = ComputeChecksum(bytes, true, true);
	        bytes[6] = (byte)checksum;
	        return bytes;
	    }

        public static Tuple<string, Dictionary<Mp2Params, int>, byte[]> ParseProgram(byte[] data)
	    {
	        var computedChecksum = ComputeChecksum(data, true, true);
	        var expectedChecksum = data[data.Length - 2];
	        if (computedChecksum != expectedChecksum)
	            throw new Exception("Computed checksum does match program value");

            // insanity! they include the midi chanel IN THE HEADER!?
            if (data[0] != ProgramHeader[0] || data[1] != ProgramHeader[1] || data[3] != ProgramHeader[3] || data[4] != ProgramHeader[4] || data[5] != ProgramHeader[5])
                throw new Exception("Received value does not have the correct program header, it is not a valid MP2 program");

            int byteIdx = 6;
            int valuesRead = 0;
            var presetNameValues = new List<int>();
            var paramDict = new Dictionary<Mp2Params, int>();

            while (valuesRead < 16)
            {
                var val = data[byteIdx];
                if (val == 0x41)
                {
                    val = data[byteIdx + 1];
                    byteIdx += 2;
                }
                else
                {
                    byteIdx += 1;
                }

                presetNameValues.Add(val);
                valuesRead++;
            }
            
            valuesRead = 0;
            while (valuesRead <= (int)Mp2Params.LoopB)
            {
                var val = data[byteIdx];
                if (val == 0x41)
                {
                    val = data[byteIdx + 1];
                    byteIdx += 2;
                }
                else
                {
                    byteIdx += 1;
                }

                paramDict[(Mp2Params)valuesRead] = val;
                valuesRead++;
            }

            var remainder = data.Skip(byteIdx).ToArray();
            var nameString = new string(presetNameValues.Select(x => Mp2CharacterMap.ValueToChar[x]).ToArray());
            return Tuple.Create(nameString, paramDict, remainder);
	    }

	    public static int ComputeChecksum(byte[] data, bool includesSysexStart, bool includesChecksumAndSysexEnd)
	    {
	        if (includesSysexStart)
	            data = data.Skip(1).ToArray();
	        if (includesChecksumAndSysexEnd)
	            data = data.Take(data.Length - 2).ToArray();

            int val = 0;
            for (int i = 0; i < data.Length; i++)
            {
                val = val + data[i];
            }

            var result = (~(val & 0xff) + 1) & 0xFF;
            return result & 0x7F;
        }

        public static string GetHex(byte[] data, int perLine)
        {
            if (data == null)
                return "";

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

	    public static bool VerifyValidName(string value)
	    {
            if (value.Length > 16)
                return false;

	        foreach (var ch in value)
	        {
                if (!Mp2CharacterMap.ValueToChar.ContainsValue(ch))
                    return false;
	        }

	        return true;
	    }

	    public static byte[] CreateProgram(byte[] currentProgram, int midiChannel1_16, string programName, Mp2ParamState state)
	    {
	        var midiChannel = midiChannel1_16 - 1;
            var parsedCurrent = ParseProgram(currentProgram);
	        programName = programName.PadRight(16, ' ');

            var programNameValues = programName.Select(x => Mp2CharacterMap.ValueToChar.Single(y => y.Value == x).Key).ToArray();
	        var stateValues = state.GetIntegerValues();
            var integerValues = programNameValues.Concat(stateValues).ToArray();

	        var startIndex = 6;
	        var newProgram = ProgramHeader.ToList();
	        newProgram[2] = (byte)midiChannel;

	        for (int i = startIndex; i < integerValues.Length + startIndex; i++)
	        {
	            var value = integerValues[i - startIndex];
	            if (value > 63)
	                newProgram.Add(0x41);

	            newProgram.Add((byte)value);
	        }

	        newProgram.AddRange(parsedCurrent.Item3);

            var checksum = Mp2Sysex.ComputeChecksum(newProgram.ToArray(), true, true);
	        newProgram[newProgram.Count - 2] = (byte)checksum;
	        return newProgram.ToArray();
	    }
	}
}
