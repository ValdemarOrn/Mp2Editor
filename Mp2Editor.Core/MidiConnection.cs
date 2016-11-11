using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Midi;

namespace Mp2Editor.Core
{
    public class MidiConnection : IDisposable
    {
        static MidiConnection()
        {
            try
            {
                var inDevs = new Dictionary<int, string>();
                var inDevices2 = new Dictionary<int, InputDevice>();
                var outDevices2 = new Dictionary<int, OutputDevice>();

                for (int i = 0; i < InputDevice.InstalledDevices.Count; i++)
                {
                    inDevs[i] = InputDevice.InstalledDevices[i].Name;
                    inDevices2[i] = InputDevice.InstalledDevices[i];
                }

                InputDevices = inDevs;

                var outDevs = new Dictionary<int, string>();

                for (int i = 0; i < OutputDevice.InstalledDevices.Count; i++)
                {
                    outDevs[i] = OutputDevice.InstalledDevices[i].Name;
                    outDevices2[i] = OutputDevice.InstalledDevices[i];
                }

                OutputDevices = outDevs;
                inDevices = inDevices2;
                outDevices = outDevices2;
            }
            catch (Exception ex)
            {
                throw new Exception("Unable to scan for midi devices during startup", ex);
            }
        }

        private static readonly IReadOnlyDictionary<int, InputDevice> inDevices;
        private static readonly IReadOnlyDictionary<int, OutputDevice> outDevices;
        
        public static IReadOnlyDictionary<int, string> InputDevices { get; private set; }
        public static IReadOnlyDictionary<int, string> OutputDevices { get; private set; }



        public MidiConnection(int inputDeviceId, int outputDeviceId)
        {
            this.Input = inDevices[inputDeviceId];
            this.Output = outDevices[outputDeviceId];

            Input.Open();
            Input.SysEx += HandleSysex;
            Input.StartReceiving(null, true);

            Output.Open();
        }
        
        public int Channel { get; set; }
        public InputDevice Input { get; private set; }
        public OutputDevice Output { get; private set; }

        public event Action<byte[]> SysexCallback;

        public void SendSysex(byte[] sysex)
        {
            Output.SendSysEx(sysex);
        }

        public void Dispose()
        {
            if (Input.IsReceiving)
                Input.StopReceiving();
            if (Input.IsOpen)
                Input.Close();
            if (Output.IsOpen)
                Output.Close();
        }

        private void HandleSysex(SysExMessage msg)
        {
            if (SysexCallback != null)
                SysexCallback.Invoke(msg.Data);
        }

        public void SendProgramSelect(int programNumber)
        {
            Output.SendProgramChange((Channel)(this.Channel - 1), (Instrument)programNumber);
        }
    }
}
