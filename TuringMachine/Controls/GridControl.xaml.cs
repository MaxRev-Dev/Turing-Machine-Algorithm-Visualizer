using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TuringMachine.Elements;
using TuringMachine.Other;

namespace TuringMachine.Controls
{
    /// <summary>
    /// Interaction logic for CellControl.xaml
    /// </summary>
    public partial class GridControl : UserControl
    {
        public GridControl()
        {
            InitializeComponent();
        }
        private ObservableCollection<IGrouping<char, RuleState>> States { get; set; }
        public int Columns => _columns;
        public int Rows => _rows;
        private int _columns => MainGrid.ColumnDefinitions.Count - 1;
        private int _rows => MainGrid.RowDefinitions.Count - 1;
        public RuleState[] GetCurrentStates()
        {
            return MainGrid.Children.Cast<UIElement>()
                .Where(x => x.GetType() == typeof(CellControl))
                .Cast<CellControl>()
                .Select(x => x.CurrentRuleState).ToArray();
        }
        RowDefinition GetRow(int ind) => new RowDefinition() { MinHeight = 20, Name = "Row" + ind };
        ColumnDefinition GetColumn(int ind) => new ColumnDefinition() { MinWidth = 100, Name = "Col" + ind };
        TextBlock GetHeader(string text) => new TextBlock()
        {
            Text = text,
            FontSize = 18,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
            VerticalAlignment = System.Windows.VerticalAlignment.Center
        };
        State DefaultState => new State(0, State.Direction.Stop, ' ');

        public void SetControls(IEnumerable<RuleState> states)
        {
            States = new ObservableCollection<IGrouping<char, RuleState>>
                (states.GroupBy(x => x.Marker).ToArray());
            UpdateAll();
        }
        private void UpdateAll()
        {
            MainGrid.ColumnDefinitions.Clear();
            MainGrid.RowDefinitions.Clear();
            MainGrid.Children.Clear();
            SetHeaders();
            if (States.Count == 0) return;
            CreateColumns(States.First());
            CreateRows(States);
            for (int i = 0; i < States.Count(); i++)
                SetRow(i);
        }
        private void CreateRows(IEnumerable<IGrouping<char, RuleState>> states)
        {
            int r = 0;
            foreach (var i in states)
                MainGrid.RowDefinitions.Add(GetRow(r++));
        }

        internal IEnumerable<UIElement> ClearSelections()
        {
            var els = MainGrid.Children.Cast<UIElement>().Where(x => x.GetType() == typeof(CellControl));
            foreach (var i in els) i.SetValue(BackgroundProperty, new SolidColorBrush(Colors.Transparent));
            return els;
        }

        private void CreateColumns(IGrouping<char, RuleState> grouping)
        {
            foreach (var i in grouping)
                MainGrid.ColumnDefinitions.Add(GetColumn(i.Q));
        }


        RuleState[] DefaultItems(char marker)
        {
            var defItems = new RuleState[_columns];
            for (int i = 0; i < _columns; i++)
                defItems[i] = new RuleState(i, marker, DefaultState);
            return defItems;
        }

        internal void SelectCurrent(RuleState stateObject)
        {
            if (stateObject == null) return;
            CellControl rb = (CellControl)
            ClearSelections().Where(x =>
            {
                var t = (((CellControl)x).CurrentRuleState);
                return t.ReferenceState.Equals(stateObject.ReferenceState)
                  && t.Marker == stateObject.Marker && t.Q == stateObject.Q;

            }).FirstOrDefault();

            if (rb != null)
                rb.SetValue(BackgroundProperty, new SolidColorBrush(Colors.Green));
        }

        public void AddRow(char marker)
        {
            if (States.Where(x => x.Key == marker).Count() > 0)
            {
                MessageBox.Show($"Row with state marker '{marker}' already exists");
                return;
            }
            States.Add(DefaultItems(marker).GroupBy(x => x.Marker).First());
            UpdateAll();
        }
        public void AddColumn()
        {
            for (int i = 0; i < States.Count; i++)
                States[i] = States[i].Append(new RuleState(0, States[i].Key, DefaultState)).GroupBy(x => x.Marker).First();
            UpdateAll();

        }
        private void ReInitArray()
        {
            var h = MainGrid.Children.Cast<UIElement>()
                .Where(x => x.GetType() == typeof(CellControl))
                .Cast<CellControl>()
                .Select(x => x.CurrentRuleState);
            SetControls(h);
        }
        public void RemoveRow()
        {
            if (Rows == 1)
            {
                MessageBox.Show("State list can't be empty. Initial state is whitespace");
                return;
            }
            var t = MainGrid.Children.Cast<UIElement>().Where(x =>
                (int)x.GetValue(Grid.RowProperty) == MainGrid.RowDefinitions.Count - 1
                ).ToArray();
            var srt = t.Cast<CellControl>().Select(x => x.CurrentRuleState);
            for (int i = 0; i < t.Count(); i++)
                MainGrid.Children.Remove(t[i]);
            MainGrid.RowDefinitions.RemoveAt(MainGrid.RowDefinitions.Count - 1);
            ReInitArray();
        }
        public void RemoveColumn()
        {
            if (Columns == 1)
            {
                MessageBox.Show("State list can't be empty. Initial state is whitespace");
                return;
            }
            var t = MainGrid.Children.Cast<UIElement>().Where(x =>
                (int)x.GetValue(Grid.ColumnProperty) == MainGrid.ColumnDefinitions.Count - 1
                ).ToArray();

            for (int i = 0; i < t.Count(); i++)
                MainGrid.Children.Remove(t[i]);
            MainGrid.ColumnDefinitions.RemoveAt(MainGrid.ColumnDefinitions.Count - 1);
            ReInitArray();
        }
        private void SetHeaders()
        {
            MainGrid.RowDefinitions.Add(new RowDefinition()
            {
                Name = "HeaderRow",
                MinHeight = 20,
                Height = new System.Windows.GridLength(10, System.Windows.GridUnitType.Pixel)
            });
            MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Name = "HeaderColumn",
                Width = new System.Windows.GridLength(10, System.Windows.GridUnitType.Pixel),
                MinWidth = 20
            });
            if (States.Count > 0)
                for (int i = 0; i < States[0].ToArray().Count(); i++)
                {
                    var tb = GetHeader("Q" + i.ToString()[0].GetLowerIndex());
                    Grid.SetRow(tb, 0);
                    Grid.SetColumn(tb, i + 1);
                    MainGrid.Children.Add(tb);
                }

            for (int i = 0; i < States.Count(); i++)
            {
                var tb = GetHeader($"\"{ States.ToArray()[i].Key.ToString()}\"");
                Grid.SetRow(tb, i + 1);
                Grid.SetColumn(tb, 0);
                MainGrid.Children.Add(tb);
            }
        }
        private void SetRow(int k)
        {
            var r = States[k].ToArray();
            for (int i = 0; i < r.Count(); i++)
            {
                var t = new CellControl(r[i]);
                Grid.SetRow(t, k + 1);
                Grid.SetColumn(t, i + 1);//header offset
                MainGrid.Children.Add(t);
            }
        }

    }
}
