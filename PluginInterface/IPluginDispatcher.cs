using FreeSWITCH;
using System;
using System.Collections.Generic;
using System.Text;

namespace PluginInterface
{
    public interface IPluginDispatcher
    {
        bool Onload();

        void DispatchDialPlan(string args, ManagedSession session);

        void DispatchAPI(string args, ManagedSession session);

        IEnumerable<string> GetApiNames();

        IEnumerable<string> GetDPNames();
    }
}
