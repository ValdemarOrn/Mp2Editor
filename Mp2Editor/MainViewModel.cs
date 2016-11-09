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

		public MainViewModel()
		{
			//LoadSysexCommand = new DelegateCommand(LoadSysex);
		    TestCommand = new DelegateCommand(Test);
		    State = new Mp2ParamState();
		}

	    private void Test(object obj)
	    {
	        State = new Mp2ParamState();
            NotifyPropertyChanged(() => Values);
	        Refresh();
	    }

	    public ICommand LoadSysexCommand { get; set; }
        public ICommand TestCommand { get; set; }

        public Mp2ParamState State { get; set; }

	    public Dictionary<Mp2Params, double> Values => State.Values;
	    public Dictionary<Mp2Params, string> Readouts => State.Readouts;

	    public void Refresh()
	    {
	        State.RefreshAll();
	        NotifyPropertyChanged(() => Readouts);
	    }
	}
}
