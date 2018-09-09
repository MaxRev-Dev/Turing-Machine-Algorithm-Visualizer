using FontAwesome.WPF;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using TuringMachine.Elements;
using TuringMachine.Managers;
using TuringMachine.Other;

namespace TuringMachine.Controls
{
    /// <summary>
    /// Interaction logic for CellControl.xaml
    /// </summary>
    public partial class CellControl : UserControl 
    {
        public CellControl()
        {
            InitializeComponent();
            MinHeight = MinWidth = 50;
            clcr.Click += Clcr_Click;
            RuleInput.KeyUp += RuleInput_KeyUp; 
        }
        public CellControl(RuleState state) : this()
        {
            SetValue(CurrentRuleStateProperty, state);

        }



        private void RuleInput_KeyUp(object sender, KeyEventArgs e)
        {
            var t = RuleInput.Text;
            try
            {
                var bk = ((RuleState)GetValue(CurrentRuleStateProperty)).Clone();
                RuleState rs = (RuleState)GetValue(CurrentRuleStateProperty);

                rs.ReferenceState = State.ConvUserInputString(t);
                SetValue(CurrentRuleStateProperty, rs);
                RuleStateChange(this, new DependencyPropertyChangedEventArgs
                    (CurrentRuleStateProperty, bk, rs));
            }
            catch (FormatException) { }

        }

        private void Clcr_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)GetValue(EditModeProperty))
            {
                SetValue(EditModeProperty, false);
                SetValue(BackgroundProperty, new SolidColorBrush(Colors.Transparent));
            }
            else
            {
                SetValue(EditModeProperty, true);
                SetValue(BackgroundProperty, new SolidColorBrush(Colors.Azure));
            }
        }
        private void SetInput(RuleState e)
        {
            RuleInput.Text = e.ReferenceState.GetAsUserInputString();
        }

        public event StateMachine.StateEvent RuleStateChanged; 

        static void RuleStateChange(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var old = (RuleState)e.OldValue;
            var newer = (RuleState)e.NewValue;
            var c = o as CellControl;
            c.SetInput(newer); 
            c.RuleStateChanged?
                .Invoke(c, new StateMachine.
                RuleStateEventArgs(newer, old)
                { State = StateMachine.RuleStateEventArgs.OnState.CellControlState });
            c.vsPan.DataContext = newer.ReferenceState;
        }

           
        public RuleState CurrentRuleState
        {
            get { return (RuleState)GetValue(CurrentRuleStateProperty); }
            set { SetValue(CurrentRuleStateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RuleState.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentRuleStateProperty =
            DependencyProperty.Register("CurrentRuleState", typeof(RuleState), typeof(CellControl),
                new PropertyMetadata(
                    new RuleState(0, ' ', new State(0, State.Direction.Stop, ' '))
                    , new PropertyChangedCallback(RuleStateChange)));



        public bool EditMode
        {
            get { return (bool)GetValue(EditModeProperty); }
            set { SetValue(EditModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditModeProperty =
            DependencyProperty.Register("EditMode", typeof(bool), typeof(CellControl),
                new PropertyMetadata(true));


    }
    public class MarkerTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var m = (State.Direction)value;
            switch (m)
            {
                case State.Direction.Stop:
                    return FontAwesomeIcon.Pause;
                case State.Direction.Left:
                    return FontAwesomeIcon.ArrowLeft;
                case State.Direction.Right:
                    return FontAwesomeIcon.ArrowRight;
                //case State.Direction.Up:
                //    return FontAwesomeIcon.ArrowUp;
                //case State.Direction.Down:
                //    return FontAwesomeIcon.ArrowDown;
                default:
                    return FontAwesomeIcon.Asterisk;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NameApConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter + "" + value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class NameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter + "" + value.ToString()[0].GetLowerIndex();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
