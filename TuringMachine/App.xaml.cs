using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using TuringMachine.Other;

namespace TuringMachine
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var u = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;
            if (u?.ActivationData != null && u.ActivationData.Length > 0)
            {
                Tools.StartupHelper = u.ActivationData[0];
            }
            base.OnStartup(e);
        }
    }
}
