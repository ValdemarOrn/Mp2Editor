using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LowProfile.Core.Ui;
using Microsoft.Win32;
using Mp2Editor.Core;

namespace Mp2Editor
{
	class MainViewModel : ViewModelBase
	{
	    private readonly object sendLock = new object();
        private readonly string programDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Programs");
        private readonly string defaultProgramFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "DEFAULT_PROGRAM.syx");

        private MidiConnection midiConnection { get; set; }
	    private byte[] currentProgram;
        private byte[] newProgram;
	    private string programName;
	    private KeyValuePair<int, string>? selectedMidiInput;
	    private KeyValuePair<int, string>? selectedMidiOutput;
	    private ConfigSettings config;
	    private bool suppressMidiUpdate;
	    private bool suppressRefresh;

	    private volatile byte[] programToSend;
	    private Dictionary<string, string> programFiles;
	    private KeyValuePair<string, string> selectedProgramFile;
	    private int currentProgramNumber;
	    private int midiChannel;
	    private bool autoUpdate;
	    private bool loadOnProgramSelect;
	    private bool isRetry;

	    public MainViewModel()
		{
	        midiChannel = 1;
            RequestProgramCommand = new DelegateCommand(RequestProgram);
            SendToDeviceCommand = new DelegateCommand(SendToDevice);
	        SaveToFileCommand = new DelegateCommand(SaveToFile);
	        UpdateProgramNumberCommand = new DelegateCommand(UpdateProgramNumber);
	        LoadBlankCommand = new DelegateCommand(LoadBlank);

            State = new Mp2ParamState();
	        config = ConfigSettings.LoadFromFile();
	        LoadPrograms();
	        LoadConfig();
	        RefreshMidi();

	        Task.Run(() => UpdateDeviceLoop());
		}
        
	    public IReadOnlyDictionary<int, string> MidiInputs => MidiConnection.InputDevices;
        public IReadOnlyDictionary<int, string> MidiOutputs => MidiConnection.OutputDevices;

	    public KeyValuePair<int, string>? SelectedMidiInput
	    {
	        get { return selectedMidiInput; }
	        set
            {
                selectedMidiInput = value;
	            NotifyPropertyChanged();

                if (suppressMidiUpdate)
                    return; 

                RefreshMidi();
                config.MidiInput = selectedMidiInput;
                config.SaveToFile();
            }
	    }

	    public KeyValuePair<int, string>? SelectedMidiOutput
	    {
	        get { return selectedMidiOutput; }
	        set
            {
                selectedMidiOutput = value;
	            NotifyPropertyChanged();

                if (suppressMidiUpdate)
                    return;

                RefreshMidi();
                config.MidiOutput = selectedMidiOutput;
                config.SaveToFile();
            }
	    }

	    public Dictionary<string, string> ProgramFiles
	    {
	        get { return programFiles; }
	        set { programFiles = value; NotifyPropertyChanged(); }
	    }

	    public KeyValuePair<string, string> SelectedProgramFile
	    {
	        get { return selectedProgramFile; }
	        set
            {
                selectedProgramFile = value;
	            NotifyPropertyChanged();

                if (File.Exists(value.Value))
                {
                    var data = File.ReadAllBytes(value.Value);
                    this.ReceiveProgramHandler(data);
                }
	        }
	    }

	    public int CurrentProgramNumber
	    {
	        get { return currentProgramNumber; }
	        set
            {
                currentProgramNumber = value;
                NotifyPropertyChanged();
	            NotifyPropertyChanged(nameof(ProgramNumberDisplay));
	        }
	    }

	    public string ProgramNumberDisplay => (currentProgramNumber + 1).ToString("d3");

	    public int[] MidiChannels => Enumerable.Range(1, 16).ToArray();

	    public int MidiChannel
	    {
	        get { return midiChannel; }
	        set
	        {
	            if (value == 0) // here I'm using channel 1-16. IF set to zero, then default to the lowest channel, in this case, 1
	                value = 1;

                midiChannel = value;
                if (midiConnection != null)
                    midiConnection.Channel = value;

	            NotifyPropertyChanged();
	            config.MidiChannel = midiChannel;
	            config.SaveToFile();
	            Refresh();
	        }
	    }

	    public ICommand RequestProgramCommand { get; set; }
        public ICommand SendToDeviceCommand { get; set; }
        public ICommand SaveToFileCommand { get; set; }
        public ICommand UpdateProgramNumberCommand { get; set; }
        public ICommand LoadBlankCommand { get; set; }

	    public bool AutoUpdate
	    {
	        get { return autoUpdate; }
	        set
            {
                autoUpdate = value;
	            NotifyPropertyChanged();
                config.AutoUpdate = value;
	            config.SaveToFile();
	        }
	    }

	    public bool LoadOnProgramSelect
	    {
	        get { return loadOnProgramSelect; }
	        set
	        {
	            loadOnProgramSelect = value;
	            NotifyPropertyChanged();
                config.LoadOnProgramSelect = value;
	            config.SaveToFile();
	        }
	    }

	    public Mp2ParamState State { get; set; }
        public Dictionary<Mp2Params, double> Values => State.Values;
        public Dictionary<Mp2Params, string> Readouts => State.Readouts;
	    public string CurrentProgramHex => Mp2Sysex.GetHex(currentProgram, 10);
        public string NewProgramHex => Mp2Sysex.GetHex(newProgram, 10);

        public string ProgramName
	    {
	        get { return programName; }
	        set
	        {
	            var isValid = Mp2Sysex.VerifyValidName(value);
	            if (!isValid)
	                return;
	            
	            programName = value;
                NotifyPropertyChanged();
	            Refresh();
	        }
	    }

        public void Refresh()
        {
            if (suppressRefresh)
                return;

            State.RefreshAll();
            NotifyPropertyChanged(() => Readouts);

            if (currentProgram != null && programName != null && State != null)
            {
                newProgram = Mp2Sysex.CreateProgram(currentProgram, midiChannel, programName, State);
                NotifyPropertyChanged(nameof(NewProgramHex));

                if (AutoUpdate)
                {
                    lock (sendLock)
                    {
                        programToSend = newProgram;
                    }
                }
            }
        }

        public void LoadWebsite()
        {
            System.Diagnostics.Process.Start("http://google.com");
        }

        private void LoadConfig()
        {
            try
            {
                suppressMidiUpdate = true;
                AutoUpdate = config.AutoUpdate;
                MidiChannel = config.MidiChannel;
                LoadOnProgramSelect = config.LoadOnProgramSelect;

                if (config.MidiInput.HasValue)
                {
                    // match by key and name
                    var midiInputMatch = MidiInputs.SingleOrDefault(x => x.Key == config.MidiInput.Value.Key && x.Value == config.MidiInput.Value.Value);
                    if (midiInputMatch.Value != null)
                        SelectedMidiInput = midiInputMatch;
                    else
                    {
                        // keys can change, match by name, if only 1 match, use it
                        var midiInputMatches = MidiInputs.Where(x => x.Value == config.MidiInput.Value.Value).ToArray();
                        if (midiInputMatches.Length == 1)
                            SelectedMidiInput = midiInputMatches[0];
                    }
                }

                if (config.MidiOutput.HasValue)
                {
                    // match by key and name
                    var midiOutputMatch = MidiOutputs.SingleOrDefault(x => x.Key == config.MidiInput.Value.Key && x.Value == config.MidiOutput.Value.Value);
                    if (midiOutputMatch.Value != null)
                        SelectedMidiOutput = midiOutputMatch;
                    else
                    {
                        // keys can change, match by name, if only 1 match, use it
                        var midiOutputMatches = MidiOutputs.Where(x => x.Value == config.MidiOutput.Value.Value).ToArray();
                        if (midiOutputMatches.Length == 1)
                            SelectedMidiOutput = midiOutputMatches[0];
                    }
                }
            }
            finally
            { 
                suppressMidiUpdate = false;
            }
        }

        private void RefreshMidi()
        {
            if (!selectedMidiInput.HasValue)
                return;
            if (!selectedMidiOutput.HasValue)
                return;

            if (midiConnection != null)
            {
                midiConnection.SysexCallback -= ReceiveProgramHandler;
                midiConnection.Dispose();
                midiConnection = null;
            }
            
            midiConnection = new MidiConnection(selectedMidiInput.Value.Key, selectedMidiOutput.Value.Key);
            midiConnection.SysexCallback += ReceiveProgramHandler;
            midiConnection.Channel = midiChannel;
        }
        
        private void ReceiveProgramHandler(byte[] data)
        {
            try
            {
                suppressRefresh = true;
                currentProgram = data;
                var parsed = Mp2Sysex.ParseProgram(data);
                State.SetProgram(parsed.Item2);
                ProgramName = parsed.Item1.Trim();
                newProgram = Mp2Sysex.CreateProgram(currentProgram, midiChannel, programName, State);

                NotifyPropertyChanged(nameof(Values));
                NotifyPropertyChanged(nameof(Readouts));
                NotifyPropertyChanged(nameof(CurrentProgramHex));
                NotifyPropertyChanged(nameof(NewProgramHex));
                isRetry = false;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Computed checksum does match program value"))
                {
                    if (!isRetry)
                    {
                        isRetry = true;
                        RequestProgram(null);
                        return;
                    }
                    else
                    {
                        isRetry = false;
                        throw;
                    }
                }

                if (ex is FieldAccessException)
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MainWindow.OnUnhandledException(ex);
            }
            finally
            {
                suppressRefresh = false;
            }
        }

        private Task UpdateDeviceLoop()
        {
            while (true)
            {
                byte[] prg = null;
                Thread.Sleep(150);

                lock (sendLock)
                {
                    if (programToSend != null && AutoUpdate)
                    {
                        prg = programToSend;
                        programToSend = null;
                    }
                }

                if (prg != null)
                    midiConnection.SendSysex(prg);
            }
        }

        private void RequestProgram(object obj)
        {
            midiConnection.SendSysex(Mp2Sysex.RequestDumpSysex(midiChannel));
        }

        private void SendToDevice(object obj)
        {
            if (newProgram != null)
                midiConnection.SendSysex(newProgram);
        }

        private void LoadPrograms()
        {
            if (!Directory.Exists(programDirectory))
            {
                Directory.CreateDirectory(programDirectory);
                return;
            }

            ProgramFiles = Directory.GetFiles(programDirectory, "*.syx").ToDictionary(Path.GetFileNameWithoutExtension, x => x);
        }

        private void LoadBlank(object obj)
        {
            var data = File.ReadAllBytes(defaultProgramFile);
            this.ReceiveProgramHandler(data);
        }

        private void SaveToFile(object obj)
	    {
	        if (newProgram == null)
	        {
	            MessageBox.Show("No program loaded");
                return;
	        }

	        var filepath = Path.Combine((programName ?? "New Program").Trim() + ".syx");
            var dialog = new SaveFileDialog()
            {
                FileName = filepath,
                DefaultExt = ".syx",
                Filter = "Sysex File (.syx)|*.syx",
                RestoreDirectory = true,
                InitialDirectory = programDirectory
            };

            var dialogResult = dialog.ShowDialog();

            if (dialogResult.HasValue && dialogResult.Value)
            {
                File.WriteAllBytes(dialog.FileName, newProgram);
                LoadPrograms();
            }
        }

        private void UpdateProgramNumber(object obj)
        {
            var inc = Convert.ToInt32(obj.ToString());
            var newNumber = (currentProgramNumber + inc + 128) % 128;
            CurrentProgramNumber = newNumber;

            midiConnection.SendProgramSelect(CurrentProgramNumber);

            if (!LoadOnProgramSelect)
                return;

            Task.Delay(300).ContinueWith(_ =>
            {
                if (currentProgramNumber == newNumber)
                    RequestProgram(null);
                else
                {
                    
                }
            });
        }
	}
}
