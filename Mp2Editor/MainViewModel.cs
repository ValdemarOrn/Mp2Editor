using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LowProfile.Core.Ui;
using Mp2Editor.Core;

namespace Mp2Editor
{
	class MainViewModel : ViewModelBase
	{
		private string lastHex;
		private string lastAscii;
		private string currentHex;
		private string currentAscii;
		private SysexDump lastDump;
		private SysexDump currentDump;
		private string differenceString;

		public MainViewModel()
		{
			LoadSysexCommand = new DelegateCommand(LoadSysex);
		}

		public string LastHex
		{
			get { return lastHex; }
			set { lastHex = value; NotifyPropertyChanged(); }
		}

		public string LastAscii
		{
			get { return lastAscii; }
			set { lastAscii = value; NotifyPropertyChanged(); }
		}

		public string CurrentHex
		{
			get { return currentHex; }
			set { currentHex = value; NotifyPropertyChanged(); }
		}

		public string CurrentAscii
		{
			get { return currentAscii; }
			set { currentAscii = value; NotifyPropertyChanged(); }
		}

		public string DifferenceString
		{
			get { return differenceString; }
			set { differenceString = value; NotifyPropertyChanged(); }
		}

		public ICommand LoadSysexCommand { get; set; }

		private void LoadSysex(object obj)
		{
			var str = obj.ToString();
			byte[] syx = null;
			var t = Task.Run(() => syx = SysexTx.ReceiveSysex());
			while (!t.IsCompleted)
			{
				Thread.Sleep(300);
				SysexTx.RequestPatch();
			}
			t.Wait();

			var dump = new SysexDump(syx);

			if (str == "Old")
			{
				lastDump = dump;
				LastHex = dump.GetHex(16);
				LastAscii = dump.GetDecimal(16);
			}
			else if (str == "New")
			{
				currentDump = dump;
				CurrentHex = dump.GetHex(16);
				CurrentAscii = dump.GetDecimal(16);
			}

			if (currentDump != null && lastDump != null)
				Compare();
		}

		private void Compare()
		{
			var output = "";
			output += string.Format("Last Len: {0}", lastDump.Data.Length);
			output += string.Format(", Current Len: {0}", currentDump.Data.Length);
			output += "  ";

			int i = 0;
			while (true)
			{
				if (i >= currentDump.Data.Length)
					break;
				if (i >= lastDump.Data.Length)
					break;

				var areSame = currentDump.Data[i] == lastDump.Data[i];
				if (!areSame)
					output += i + ", ";

				i++;
			}

			DifferenceString = output;
		}


	}
}
