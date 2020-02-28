using FreeSWITCH;
using PluginInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SimpleXMLDP
{
    public class SimpleXMLDPDispatcher : IPluginDispatcher
    {
        public int DispatchAPI(string args, Stream stream, ManagedSession session)
        {
            throw new NotImplementedException();
        }

        public void DispatchDialPlan(string args, ManagedSession session)
        {
            throw new NotImplementedException();
        }

        public string DispatchXMLCallback(string section, string tag, string key, string value, Event evt)
        {
            if (section != "dialplan")
                return null;
            var context = evt.GetHeader("Hunt-Context"); // the context
            var destination = evt.GetHeader("Hunt-Destination-Number"); // the dialed number or "DID"
            var ani = evt.GetHeader("Hunt-ANI"); // The ANI/CallerID number

            Log.WriteLine(LogLevel.Console, $"SimpleXMLDP: lookup ctx = {context} dest = {destination} ani = {ani}");

            switch (destination)
            {
                case "1111":
                    var l = new List<string>();
                    l.Add("sleep,2000");
                    l.Add("ring_ready");
                    l.Add("sleep,6000");
                    l.Add("answer");
                    l.Add("sleep,5000");
                    l.Add("hangup,NORMAL_CLEARING");
                    return new FreeSWITCH.Helpers.fsDialPlanDocument(context, l).ToXMLString();

                default:
                    break;
            }

            return null;

        }

        public IEnumerable<string> GetApiNames()
        {
            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetDPNames()
        {
            return Enumerable.Empty<string>();
        }

        public bool Onload()
        {
            return true;
        }
    }
}
