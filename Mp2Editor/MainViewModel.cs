using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LowProfile.Core.Ui;
using Mp2Editor.Core;

namespace Mp2Editor
{
	class MainViewModel : ViewModelBase
	{
        private MidiConnection midiConnection { get; set; }
	    private byte[] currentProgram;
        private byte[] newProgram;

        public MainViewModel()
		{
            RequestProgramCommand = new DelegateCommand(RequestProgram);
		    TestCommand = new DelegateCommand(Test);
		    State = new Mp2ParamState();
		    midiConnection = new MidiConnection(
		        MidiConnection.InputDevices.Single(x => x.Value.Contains("USB")).Key,
		        MidiConnection.OutputDevices.Single(x => x.Value.Contains("USB")).Key);

            midiConnection.SysexCallback += ReceiveProgramHandler;
		}
        
	    public ICommand RequestProgramCommand { get; set; }
        public ICommand TestCommand { get; set; }

        public Mp2ParamState State { get; set; }

        public Dictionary<Mp2Params, double> Values => State.Values;
        public Dictionary<Mp2Params, string> Readouts => State.Readouts;
	    public string CurrentProgramHex => Mp2Sysex.GetHex(currentProgram, 10);
        public string NewProgramHex => Mp2Sysex.GetHex(newProgram, 10);

        public void Refresh()
	    {
	        State.RefreshAll();
            NotifyPropertyChanged(() => Readouts);
	    }

        private void ReceiveProgramHandler(byte[] data)
        {
            try
            {
                /*var checksumComputed = Mp2Sysex.ComputeChecksum(data, true, true);
                var checksumExpected = data[data.Length - 2];
                if (checksumComputed != checksumExpected)
                    throw new FieldAccessException("Invalid program received, checksum value does not match");*/

                currentProgram = data;
                var parsed = Mp2Sysex.ParseProgram(data);
                State.SetProgram(parsed.Item2);
                NotifyPropertyChanged(nameof(Values));
                NotifyPropertyChanged(nameof(Readouts));
                NotifyPropertyChanged(nameof(CurrentProgramHex));
            }
            catch (Exception ex)
            {
                if (ex is FieldAccessException)
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MainWindow.OnUnhandledException(ex);
            }
        }

        private void RequestProgram(object obj)
        {
            midiConnection.SendSysex(Mp2Sysex.RequestDump);
        }

        private void Test(object obj)
        {
            State = new Mp2ParamState();
            NotifyPropertyChanged(() => Values);
            Refresh();
        }

    }
}
