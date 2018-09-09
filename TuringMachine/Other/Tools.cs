using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringMachine.Other
{
   public static class Tools
    {
        public static string StartupHelper = "";
        public static char GetLowerIndex(this char c)
        {
            char ex = c;
            if (ex >= '0' && ex <= '9')
            {
                ex = (char)(ex - '0' + 0x2080);
            }
            return ex;
        }
    }
}
