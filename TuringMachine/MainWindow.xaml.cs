using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TuringMachine.Controls;
using TuringMachine.Elements;
using TuringMachine.Managers;
using TuringMachine.Other;

namespace TuringMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MinWidth = 800;
            MinHeight = 450;

            InitHandlers();
            stateMachine = new StateMachine();
            InitStrip();
            GR.Loaded += (s, e) => GR.SetControls(stateMachine.Rules);
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            if (!string.IsNullOrEmpty(Tools.StartupHelper))
                stateMachine.Load(Tools.StartupHelper);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            stateMachine.LogError(e.Exception);
        }

        readonly StateMachine stateMachine;

        private void InitHandlers()
        {
            BtnAddColumn.Click += BtnAddColumn_Click;
            BtnRemoveColumn.Click += BtnRemoveColumn_Click;
            BtnRevo.Click += BtnRevo_Click;
            BtnUndo.Click += BtnUndo_Click;
            BtnPause.Click += BtnPause_Click;
            BtnStart.Click += BtnStart_Click;
            BtnStop.Click += BtnStop_Click;
            BtnAddRow.Click += BtnAddRow_Click;
            BtnRemoveRow.Click += BtnRemoveRow_Click;

            BtnLoad.Click += BtnLoad_Click;
            BtnSave.Click += BtnSave_Click;

            WordInput.TextChanged += WordInput_TextChanged;

            Closing += MainWindow_Closing;
            SpdSelector.SelectionChanged += SpdSelector_SelectionChanged;

        }

        private void InitStrip()
        {
            stateMachine.LoadSetting();
            stateMachine.NewState += StateMachine_NewState;
            stateMachine.Paused += StateMachine_Paused;
            stateMachine.Started += StateMachine_Started;
            stateMachine.Stopped += StateMachine_Stopped;
            stateMachine.WasRedo += StateMachine_WasRedo;
            stateMachine.WasUndo += StateMachine_WasUndo;
            stateMachine.Finished += StateMachine_Finished;
            stateMachine.HistoryChanged += History_CollectionChanged;
        }

        #region SaveLoad
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Save(GR.GetCurrentStates());
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Load();
            GR.SetControls(stateMachine.Rules);
        }
        #endregion

        #region StateMachineEvents
        private void StateMachine_Finished(object sender, StateMachine.RuleStateEventArgs e)
        {
            Dispatcher.Invoke(() => UpdateWordView());
        }

        private void StateMachine_WasUndo(object sender, StateMachine.RuleStateEventArgs e)
        {
            Dispatcher.Invoke(() =>
                Selects(e));
        }

        private void StateMachine_WasRedo(object sender, StateMachine.RuleStateEventArgs e)
        {
            Dispatcher.Invoke(() =>
                Selects(e));
        }
        private void StateMachine_NewState(object sender, StateMachine.RuleStateEventArgs e)
        {

            Dispatcher.Invoke(() =>
                Selects(e));
        }
        private void StateMachine_Stopped()
        {
            GR.ClearSelections();
        }

        private void StateMachine_Started()
        {
            // throw new System.NotImplementedException();
        }

        private void StateMachine_Paused()
        {
            //  throw new System.NotImplementedException();
        }

        #endregion

        #region Updaters
        private void History_CollectionChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                HistoryViewList.ItemsSource =
                    stateMachine.History.Select(x => new HistoryControl(x));
                if (HistoryViewList.Items.Count > 0)
                    HistoryViewList.ScrollIntoView(HistoryViewList.Items[HistoryViewList.Items.Count - 1]);
                UpdateWordView();
            });
        }
        private void UpdateWordView()
        {
            ResBox.AppendText($"\n#{stateMachine.History.Count + 1} => \"{stateMachine.GetCurrentResult.Trim()}\"");
            ResBox.ScrollToEnd();
        }
        private void Selects(StateMachine.RuleStateEventArgs e)
        {
            try
            {
                GR.SelectCurrent(e.StateObject);
                MainStrip.SetLine(stateMachine.GetStrip);
                MainStrip.Select(stateMachine.Selected);
            }
            catch { }
        }
        private double GetPureSelSpeed()
        {
            return double.Parse(((ComboBoxItem)SpdSelector.SelectedValue).Content.ToString().Trim('x'));
        }
        private void WordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            stateMachine.CurrentBlankSymb = ' ';
            stateMachine.CurrentWord = WordInput.Text;
        }
        private void SpdSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            stateMachine.SetSpeed(GetPureSelSpeed());
        }
        #endregion

        #region MediaControls
        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Stop();
        }
        private void BtnPause_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Pause();
        }

        private void BtnUndo_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Undo();
        }

        private void BtnRevo_Click(object sender, RoutedEventArgs e)
        {
            stateMachine.Redo(GR.GetCurrentStates());
        }
        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            ResBox.Document.Blocks.Clear();
            if (string.IsNullOrWhiteSpace(WordInput.Text))
            {
                MessageBox.Show("Can't start with null word");
                return;
            }
            if (stateMachine.IsBusy)
            {
                MessageBox.Show("Machine is already running!");
                return;
            }
            stateMachine.Start(GR.GetCurrentStates(), GetPureSelSpeed());
        }
        #endregion

        #region RowColumnAddRemove
        private void BtnRemoveRow_Click(object sender, RoutedEventArgs e)
        {
            if (GR.Rows > 0)
                GR.RemoveRow();
        }

        private void BtnAddRow_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Controls.PromptDialog() { Title = "Enter row name" };
            if (dialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(dialog.ResponseText))
                {
                    MessageBox.Show("Failed to add empty name", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                GR.AddRow(dialog.ResponseText[0]);
            }
        }

        private void BtnRemoveColumn_Click(object sender, RoutedEventArgs e)
        {
            if (GR.Columns > 0)
                GR.RemoveColumn();
        }

        private void BtnAddColumn_Click(object sender, RoutedEventArgs e)
        {
            GR.AddColumn();
        }
        #endregion

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            stateMachine.SetRules(GR.GetCurrentStates());
            stateMachine.SaveSetting();
        }
    }
}
