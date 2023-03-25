using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communications
{
    // Set and response are the same depending on who is sending
    public class SetConfiguration : ConfigurationResponse
    {
        public SetConfiguration()
        {
            CommandType = Communications.CommandType.SetConfiguration;
        }
    }
}
