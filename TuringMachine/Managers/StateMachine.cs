using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TuringMachine.Elements;

namespace TuringMachine.Managers
{
    public class ConfigManager
    {
        public T LoadObject<T>(string file = null)
        {
            string u = string.IsNullOrEmpty(file) ? LoadFile() : file;
            if (!string.IsNullOrWhiteSpace(u) && File.Exists(u))
                using (var v = File.OpenText(u))
                    try
                    {
                        return JsonConvert.DeserializeObject<T>(v.ReadToEnd());
                    }
                    catch (Exception ex) { ShowLoadError(ex); }
            return default(T);
        }
        public void SaveObject(object obj)
        {
            var u = SaveFile();
            if (!string.IsNullOrWhiteSpace(u))
                File.WriteAllText(u, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }

        string fileExtension = ".tmr";
        private string SaveFile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "TuringMachineRules",
                DefaultExt = fileExtension,
                Filter = $"Turing Machine Rules  ({fileExtension})|*{fileExtension}"
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            return null;
        }
        private string LoadFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog
            {
                FileName = "TuringMachineRules",
                DefaultExt = fileExtension,
                Filter = $"Turing Machine Rules  ({fileExtension})|*{fileExtension}"
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            return null;
        }

        internal T LoadSettings<T>(string stStr)
        {
            var u = (string)Properties.Settings.Default[stStr];
            if (!string.IsNullOrEmpty(u))
                try
                {
                    return JsonConvert.DeserializeObject<T>(u);
                }
                catch (Exception ex)
                {
                    ShowLoadError(ex);
                }
            return default(T);

        }

        private void ShowLoadError(Exception ex)
        {
            var msw = ex.StackTrace.Split('\n').Select(x=>x.Trim('\r'));
            var ms = string.Join("\n", msw.Reverse().Take(5).ToArray());
            MessageBox.Show("Unable to load rules file." +
        "\nMake sure it has correct syntax" +
        "\nHere's some additional info" +
        $"\n\n\n\n{ex.Message}" +
        $"\n{ms}","Parsing error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        internal void SaveSettings(string stStr, object obj)
        {
            Properties.Settings.Default[stStr] = JsonConvert.SerializeObject(obj);
            Properties.Settings.Default.Save();
        }
    }

    public class Config
    {
        public string FilePath { get; set; }
        public int ExecDelayMs { get; set; } = 1000;

        public List<RuleState> GenerateRules()
        {
            return new List<RuleState>()
                {
                    new RuleState(0, ' ', new State(1, State.Direction.Stop,'1')),
                    new RuleState(1, ' ', new State(1, State.Direction.Right,'1')),
                    new RuleState(2, ' ', new State(1, State.Direction.Right,'1')),
                    new RuleState(0, '1', new State(1, State.Direction.Stop,'1')),
                    new RuleState(1, '1', new State(2, State.Direction.Left,' ')),
                    new RuleState(2, '1', new State(2, State.Direction.Right,'1')),
                    new RuleState(0, '0', new State(1, State.Direction.Stop,' ')),
                    new RuleState(1, '0', new State(2, State.Direction.Right,' ')),
                    new RuleState(2, '0', new State(1, State.Direction.Right,' ')),
                };
        }
    }
    public class StateMachine : IDisposable
    {
        public StateMachine()
        {
            Config = new Config();
            worker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true
            };
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
            manager = GetManager(manager?.Table);
        }

        #region Vars
        public Config Config;
        public event EventHandler HistoryChanged;
        private StateManager manager;
        private readonly BackgroundWorker worker;

        private volatile bool
            IsPaused = false,
            IsOneTime = false,
            IsWorking = false;
        public string CurrentWord { get; set; }
        public char CurrentBlankSymb { get; set; }
        public List<RuleState> Rules => manager?.Table;
        public string GetCurrentResult => manager?.GetResult;
        public bool IsBusy => worker.IsBusy;
        public int Selected => manager.Selected;
        public bool IsFinished { get; private set; }
        public Strip GetStrip => manager.GetStrip;
        public ObservableCollection<RuleState> History => manager.History;
        #endregion

        ConfigManager cfgM = new ConfigManager();
        public void Save(RuleState[] states)
        {
            cfgM.SaveObject(manager.Table = states.ToList());
        }
        public void Load(string file = null)
        {
            manager.Table = cfgM.LoadObject<List<RuleState>>(file) ??
               manager.Table;
        }
        internal void LoadSetting()
        {
            manager.Table = cfgM.LoadSettings<List<RuleState>>("Rules");
            if (manager.Table == null) manager = GetManager(null);
        }
        internal void SaveSetting()
        {
            cfgM.SaveSettings("Rules", Rules);
        }
        internal void LogError(Exception ex)
        {
            var d = DateTime.Now.ToString("dd.MM.yyyy_H.mm.ss");
            using (var r = File.CreateText(Directory.GetCurrentDirectory() + "/" + d + ".log"))
            {
                new JsonSerializer() { Formatting = Formatting.Indented }
                    .Serialize(r, new Exception($"Caugth on {DateTime.Now.ToString()}", ex));
            }
        }
        internal void SetRules(RuleState[] ruleState)
        {
            manager.Table = ruleState.ToList();
        }
        private StateManager GetManager(List<RuleState> states)
        {
            return new StateManager(CurrentBlankSymb)
            {
                Table = states?.ToList() ?? Config.GenerateRules()
            };
        }
        private void ExecutionTaskAsync()
        {
            while (true)
            {
                OnNewState(manager.GoToState());
                if (worker.CancellationPending || IsFinished)
                    break;
                Task.Delay(Config.ExecDelayMs).Wait();
            }
        }
        public void SetSpeed(double v)
        {
            Config.ExecDelayMs = (int)(1000.0 / v);
        }
        internal void Start(RuleState[] ruleState, double v)
        {
            SetSpeed(v);
            Start(ruleState);
        }

        private void Manager_HistoryChanged(object sender, EventArgs e)
        {
            HistoryChanged?.Invoke(sender, e);
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            ExecutionTaskAsync();
            if (worker.CancellationPending)
                e.Cancel = true;
        }
        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled && !IsOneTime)
                if (IsPaused)
                    Paused?.Invoke();
                else
                {
                    if (!IsFinished)
                    {
                        manager.StopFinalize();
                        Stopped?.Invoke();
                    }
                }
            IsOneTime = false;
            IsWorking = false;
        }

        private void NotifyStop()
        {
            MessageBox.Show("Зупиніть процес перед змінами", "Увага", MessageBoxButton.OK, MessageBoxImage.Asterisk);
        }

        private RuleStateEventArgs InitWork()
        {
            IsWorking = true;
            manager = GetManager(manager?.Table);

            manager.HistoryChanged += Manager_HistoryChanged;
            manager.Word = CurrentWord;
            try
            {
                return manager.InitStrip();
            }
            catch (InvalidOperationException)
            {
                worker.CancelAsync();
                return null;
            }
        }
        void IsFinishCheck()
        {
            if (IsFinished)
            {
                MessageBox.Show("It's a final state!", "Completed", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
        }
        private void FinishCheck(RuleStateEventArgs e)
        {
            var addedState = e?.StateObject;
            if (addedState == null) worker.CancelAsync();
            if (addedState is FinishedState)
            {
                worker.CancelAsync();
                Finished?.Invoke(this, new RuleStateEventArgs(addedState)
                {
                    State = RuleStateEventArgs.OnState.NewState | RuleStateEventArgs.OnState.Finished
                });
            }
        }

        #region Commands
        internal void Start(RuleState[] states)
        {
            IsFinished = false;
            manager.Table = states.ToList();
            if (!IsPaused)
                OnNewState(InitWork());
            if (worker.IsBusy)
                worker.CancelAsync();
            worker.RunWorkerAsync();
            Started?.Invoke();
        }
        internal void Stop()
        {
            IsPaused = false;
            IsWorking = false;
            if (worker.IsBusy)
                worker.CancelAsync();
            else Worker_RunWorkerCompleted(worker, // cleaning..
                new RunWorkerCompletedEventArgs(null, null, true));
        }
        internal void Undo()
        {
            if (manager.History.Count == 0)
            {
                MessageBox.Show("It`s initial state. Nothing to reverse from history");
            }
            if (IsPaused)
                OnUndo(manager.ReverseToState());
            else if (IsWorking)
                NotifyStop();
        }
        internal void Redo(RuleState[] states)
        {
            IsFinishCheck();
            manager.Table = states.ToList();
            if (IsPaused)
                OnRedo(manager.GoToState());
            else if (worker.IsBusy)
                NotifyStop();
            else
            {
                IsPaused = true;
                IsOneTime = true;
                if (!IsWorking)
                {
                    OnRedo(InitWork());
                    return;
                }
                OnRedo(manager.GoToState());
            }
        }
        internal void Pause()
        {
            IsPaused = true;
            worker.CancelAsync();
        }
        #endregion

        #region EventManage
        public delegate void Eventer();
        public delegate void StateEvent(object sender, RuleStateEventArgs e);
        public class RuleStateEventArgs : EventArgs
        {
            public enum OnState
            {
                NewState, Undo, Redo, CellControlState,
                Finished
            }
            public RuleState StateObject { get; }
            public RuleState OldStateObject { get; }
            public RuleState NextStateObject { get; }
            public OnState State { get; set; }

            public RuleStateEventArgs(RuleState newEvent)
            {
                StateObject = newEvent;
            }
            public RuleStateEventArgs(RuleState newEvent, RuleState oldEvent)
                : this(newEvent)
            {
                OldStateObject = oldEvent;
            }
            public RuleStateEventArgs(RuleState newEvent, RuleState oldEvent, RuleState nextEvent)
                : this(newEvent, oldEvent)
            {
                NextStateObject = nextEvent;
            }
        }
        public event Eventer Paused;
        public event Eventer Started;
        public event Eventer Stopped;
        public event StateEvent WasUndo;
        public event StateEvent WasRedo;
        public event StateEvent NewState;
        public event StateEvent Finished;
        protected virtual void OnUndo(RuleStateEventArgs e)
        {
            if (e != null)
                e.State = RuleStateEventArgs.OnState.Undo;
            WasUndo?.Invoke(this, e);
        }
        protected virtual void OnRedo(RuleStateEventArgs e)
        {
            if (e != null)
                e.State = RuleStateEventArgs.OnState.Redo;
            FinishCheck(e);
            OnNewState(e);
            WasRedo?.Invoke(this, e);
        }
        protected virtual void OnNewState(RuleStateEventArgs e)
        {
            FinishCheck(e);
            NewState?.Invoke(this, e);
        }
        #endregion

        public void Dispose()
        {
            worker.RunWorkerCompleted -= Worker_RunWorkerCompleted;
            worker.DoWork -= Worker_DoWork;
            worker.CancelAsync();
            worker.Dispose();
        }

    }
}
