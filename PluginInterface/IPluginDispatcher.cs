using System;
using System.Collections.Generic;
using System.Text;

namespace PluginInterface
{
    public interface IPluginDispatcher
    {
        bool Onload();

        void DispatchDialPlan();

        void DispatchAPI();

        IEnumerable<string> GetApiNames();

        IEnumerable<string> GetDPNames();
    }
}
