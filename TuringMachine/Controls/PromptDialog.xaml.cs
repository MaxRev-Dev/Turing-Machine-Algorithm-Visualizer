﻿using System;
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
using System.Windows.Shapes;

namespace TuringMachine.Controls
{
    /// <summary>
    /// Interaction logic for PromptDialog.xaml
    /// </summary>
    public partial class PromptDialog : Window
    {
        public PromptDialog()
        {
            InitializeComponent();
            OkBtn.Click += OkBtn_Click;
        } 
        public string ResponseText
        {
            get { return ResponseTextBox.Text; }
            set { ResponseTextBox.Text = value; }
        }


        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
