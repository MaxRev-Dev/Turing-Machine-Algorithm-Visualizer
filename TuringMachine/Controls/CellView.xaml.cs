using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TuringMachine.Controls
{
    /// <summary>
    /// Interaction logic for CellView.xaml
    /// </summary>
    public partial class CellView : UserControl
    {
        public CellView()
        {
            InitializeComponent();
        }
        class Wrap
        {
            public string Content { get;   set; }
            public int Index { get; set; }
        }
        public CellView(string content,int index):this()
        {
            DataContext = new Wrap() { Content = content, Index = index };
        }
    }
}
