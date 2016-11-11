using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mp2Editor.Core
{
    public enum Mp2Params
    {
        Voicing = 0,
        Drive,
        Od,
        Master,
        CompIn,
        CompThreshold,
        CompGain,
        CompRatio,
        ToneLo,
        ToneMid,
        ToneHigh,
        TonePres,
        EqBand1,
        EqBand2,
        EqBand3,
        EqBand4,
        EqBand5,
        EqBand6,
        EqBand7,
        EqBand8,
        EqBand9,
        NoiseIn,
        NoiseMode,
        NoiseThresholdGate,
        NoiseThresholdFader,
        TremIn,
        TremDepth,
        TremRate,
        TremWaveform,
        WahIn,
        WahMode,
        WahPedalStartPt,
        WahSensitivity,
        WahDelay,
        WahDepth,
        WahRate,
        WahEndpoint,
        WahWaveform,
        ChorusIn,
        ChorusDepth,
        ChorusRate,
        LoopIn,
        LoopA,
        LoopB,
    }

    public class Mp2ParamState
    {
        private static Dictionary<Mp2Params, int> integerScalars = new Dictionary<Mp2Params, int>
        {
            { Mp2Params.Voicing, 10 },
            { Mp2Params.Drive, 101 },
            { Mp2Params.Od, 101 },
            { Mp2Params.Master, 101 },

            { Mp2Params.CompIn, 2 },
            { Mp2Params.CompGain, 101 },
            { Mp2Params.CompThreshold, 101 },
            { Mp2Params.CompRatio, 9 },

            { Mp2Params.ToneLo, 13 },
            { Mp2Params.ToneMid, 13 },
            { Mp2Params.ToneHigh, 13 },
            { Mp2Params.TonePres, 13 },

            { Mp2Params.EqBand1, 13 },
            { Mp2Params.EqBand2, 13 },
            { Mp2Params.EqBand3, 13 },
            { Mp2Params.EqBand4, 13 },
            { Mp2Params.EqBand5, 13 },
            { Mp2Params.EqBand6, 13 },
            { Mp2Params.EqBand7, 13 },
            { Mp2Params.EqBand8, 13 },
            { Mp2Params.EqBand9, 13 },

            { Mp2Params.NoiseIn, 2 },
            { Mp2Params.NoiseMode, 2 },
            { Mp2Params.NoiseThresholdGate, 101 },
            { Mp2Params.NoiseThresholdFader, 101 },

            { Mp2Params.TremIn, 2 },
            { Mp2Params.TremDepth, 101 },
            { Mp2Params.TremRate, 101 },
            { Mp2Params.TremWaveform, 3 },

            { Mp2Params.WahIn, 2 },
            { Mp2Params.WahMode, 3 },
            { Mp2Params.WahPedalStartPt, 101 },
            { Mp2Params.WahSensitivity, 101 },
            { Mp2Params.WahDelay, 101 },
            { Mp2Params.WahDepth, 101 },
            { Mp2Params.WahRate, 101 },
            { Mp2Params.WahEndpoint, 101 },
            { Mp2Params.WahWaveform, 2 },

            { Mp2Params.ChorusIn, 2 },
            { Mp2Params.ChorusDepth, 101 },
            { Mp2Params.ChorusRate, 101 },

            { Mp2Params.LoopIn, 2 },
            { Mp2Params.LoopA, 101 },
            { Mp2Params.LoopB, 101 },
        };

        private static Dictionary<int, string> VoicingNames = new Dictionary<int, string>
        {
            { 0, "Crystal Cln" },
            { 1, "Spanky Cln" },
            { 2, "Fat Clean" },
            { 3, "Vint Brwn" },
            { 4, "Warm Vint" },
            { 5, "Dyn Vint" },
            { 6, "Warm HiGain" },
            { 7, "Dyn HiGain" },
            { 8, "Ult HiGain" },
            { 9, "Fat HiGain " },
        };

        private static string FormatInOut(int value)
        {
            return value == 0 ? "Out" : "In";
        }

        private static string FormatCompRatio(int integerValue)
        {
            switch (integerValue)
            {
                case 0:
                    return "1.5:1";
                case 1:
                    return "2:1";
                case 2:
                    return "3:1";
                case 3:
                    return "4:1";
                case 4:
                    return "6:1";
                case 5:
                    return "8:1";
                case 6:
                    return "10:1";
                case 7:
                    return "15:1";
                case 8:
                    return "30:1";
                default:
                    return "---";
            }
        }

        private string FormatDb(int val)
        {
            var dbVal = -12 + val * 2;
            return dbVal + " dB";
        }

        private string FormatWaveform(int val)
        {
            if (val == 0) return "Sin";
            if (val == 1) return "Tri";
            if (val == 2) return "Surf";
            return "---";
        }

        private string FormatWahMode(int val)
        {
            if (val == 0) return "Pedal";
            if (val == 1) return "Triggered";
            if (val == 2) return "Auto";
            return "---";
        }

        public Mp2ParamState()
        {
            Values = new Dictionary<Mp2Params, double>();
            Readouts = new Dictionary<Mp2Params, string>();

            foreach (var kvp in integerScalars)
                Values[kvp.Key] = 0.0;

            RefreshAll();
        }

        public Dictionary<Mp2Params, double> Values { get; set; }
        public Dictionary<Mp2Params, string> Readouts { get; set; }

        public void SetProgram(Dictionary<Mp2Params, int> intValues)
        {
            foreach (var kvp in intValues)
            {
                var floatVal = kvp.Value / (double)(integerScalars[kvp.Key] - 1.0);
                Values[kvp.Key] = floatVal;
            }

            RefreshAll();
        }

        public int[] GetIntegerValues()
        {
            return Values.OrderBy(x => x.Key)
                .Select(kvp => GetInt(kvp.Key))
                .ToArray();
        }

        public void RefreshAll()
        {
            var readouts = new Dictionary<Mp2Params, string>();
            
            readouts[Mp2Params.Voicing] = VoicingNames[GetInt(Mp2Params.Voicing)];
            readouts[Mp2Params.Drive] = GetInt(Mp2Params.Drive).ToString();
            readouts[Mp2Params.Od] = GetInt(Mp2Params.Od).ToString();
            readouts[Mp2Params.Master] = GetInt(Mp2Params.Master).ToString();
            
            readouts[Mp2Params.CompIn] = FormatInOut(GetInt(Mp2Params.CompIn));
            readouts[Mp2Params.CompGain] = GetInt(Mp2Params.CompGain).ToString();
            readouts[Mp2Params.CompThreshold] = GetInt(Mp2Params.CompThreshold).ToString();
            readouts[Mp2Params.CompRatio] = FormatCompRatio(GetInt(Mp2Params.CompRatio));

            readouts[Mp2Params.ToneLo] = FormatDb(GetInt(Mp2Params.ToneLo));
            readouts[Mp2Params.ToneMid] = FormatDb(GetInt(Mp2Params.ToneMid));
            readouts[Mp2Params.ToneHigh] = FormatDb(GetInt(Mp2Params.ToneHigh));
            readouts[Mp2Params.TonePres] = FormatDb(GetInt(Mp2Params.TonePres));

            readouts[Mp2Params.EqBand1] = FormatDb(GetInt(Mp2Params.EqBand1));
            readouts[Mp2Params.EqBand2] = FormatDb(GetInt(Mp2Params.EqBand2));
            readouts[Mp2Params.EqBand3] = FormatDb(GetInt(Mp2Params.EqBand3));
            readouts[Mp2Params.EqBand4] = FormatDb(GetInt(Mp2Params.EqBand4));
            readouts[Mp2Params.EqBand5] = FormatDb(GetInt(Mp2Params.EqBand5));
            readouts[Mp2Params.EqBand6] = FormatDb(GetInt(Mp2Params.EqBand6));
            readouts[Mp2Params.EqBand7] = FormatDb(GetInt(Mp2Params.EqBand7));
            readouts[Mp2Params.EqBand8] = FormatDb(GetInt(Mp2Params.EqBand8));
            readouts[Mp2Params.EqBand9] = FormatDb(GetInt(Mp2Params.EqBand9));

            readouts[Mp2Params.NoiseIn] = FormatInOut(GetInt(Mp2Params.NoiseIn));
            readouts[Mp2Params.NoiseMode] = GetInt(Mp2Params.NoiseMode) == 0 ? "Fader" : "Gate";
            readouts[Mp2Params.NoiseThresholdGate] = GetInt(Mp2Params.NoiseThresholdGate).ToString();
            readouts[Mp2Params.NoiseThresholdFader] = GetInt(Mp2Params.NoiseThresholdFader).ToString();

            readouts[Mp2Params.TremIn] = FormatInOut(GetInt(Mp2Params.TremIn));
            readouts[Mp2Params.TremDepth] = GetInt(Mp2Params.TremDepth).ToString();
            readouts[Mp2Params.TremRate] = GetInt(Mp2Params.TremRate).ToString();
            readouts[Mp2Params.TremWaveform] = FormatWaveform(GetInt(Mp2Params.TremWaveform));

            readouts[Mp2Params.WahIn] = FormatInOut(GetInt(Mp2Params.WahIn));
            readouts[Mp2Params.WahMode] = FormatWahMode(GetInt(Mp2Params.WahMode));
            readouts[Mp2Params.WahPedalStartPt] = GetInt(Mp2Params.WahPedalStartPt).ToString();
            readouts[Mp2Params.WahSensitivity] = GetInt(Mp2Params.WahSensitivity).ToString();
            readouts[Mp2Params.WahDelay] = GetInt(Mp2Params.WahDelay).ToString();
            readouts[Mp2Params.WahDepth] = GetInt(Mp2Params.WahDepth).ToString();
            readouts[Mp2Params.WahRate] = GetInt(Mp2Params.WahRate).ToString();
            readouts[Mp2Params.WahEndpoint] = GetInt(Mp2Params.WahEndpoint).ToString();
            readouts[Mp2Params.WahWaveform] = FormatWaveform(GetInt(Mp2Params.WahWaveform));

            readouts[Mp2Params.ChorusIn] = FormatInOut(GetInt(Mp2Params.ChorusIn));
            readouts[Mp2Params.ChorusDepth] = GetInt(Mp2Params.ChorusDepth).ToString();
            readouts[Mp2Params.ChorusRate] = GetInt(Mp2Params.ChorusRate).ToString();

            readouts[Mp2Params.LoopIn] = FormatInOut(GetInt(Mp2Params.LoopIn));
            readouts[Mp2Params.LoopA] = GetInt(Mp2Params.LoopA).ToString();
            readouts[Mp2Params.LoopB] = GetInt(Mp2Params.LoopB).ToString();
            
            Readouts = readouts;
        }
        
        private int GetInt(Mp2Params param)
        {
            double value;
            var ok = Values.TryGetValue(param, out value);
            if (!ok) value = 0.0;
            var scalar = integerScalars[param];

            return (int)(value * (scalar - 0.001));
        }
    }
}
