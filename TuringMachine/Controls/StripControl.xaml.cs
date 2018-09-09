using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TuringMachine.Elements;

namespace TuringMachine.Controls
{
    /// <summary>
    /// Interaction logic for StripControl.xaml
    /// </summary>
    public partial class StripControl : UserControl
    {
        public StripControl()
        {
            InitializeComponent();
        }

        internal void SetLine(Strip strip)
        {
            int i = -strip.ZeroOffset;
            MainStrip.ItemsSource = strip.GetResult.Select(x => new CellView(x.ToString(), i++));
        }
        public void Select(int ex)
        {
            MainStrip.SelectedIndex = ex; 
            Decorator border = VisualTreeHelper.GetChild(MainStrip, 0) as Decorator;
            ScrollViewer scrollViewer = border.Child as ScrollViewer;  
            scrollViewer.ScrollToHorizontalOffset(
                MainStrip.SelectedIndex - Math.Floor(scrollViewer.ViewportWidth / 2));

             
        }
    }
}
