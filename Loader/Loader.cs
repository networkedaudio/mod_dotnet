using System;
using System.Runtime.InteropServices;
using FreeSWITCH;

namespace FreeSWITCH
{
    public sealed class Loader
    {
        private delegate InterfaceCallbacks LoadDelegate();

	private delegate int APICallback(string command, IntPtr sessionptr, IntPtr streamptr);

        [StructLayout(LayoutKind.Sequential)]
        private struct InterfaceCallbacks
        {
	    public IntPtr APICallback;
        }
	
        private static InterfaceCallbacks Load()
        {
            return new InterfaceCallbacks {
		APICallback = Marshal.GetFunctionPointerForDelegate<APICallback>(APIHandler)
	    };
        }

	private static int APIHandler(string command, IntPtr sessionptr, IntPtr streamptr)
	{
	    using ManagedSession session = new ManagedSession(new SWIGTYPE_p_switch_core_session_t(sessionptr, false));
	    using Stream stream = new Stream(new SWIGTYPE_p_switch_stream_handle_t(streamptr, false));

	    Log.WriteLine(LogLevel.Info, "Managed API: {0}", command);

	    stream.Write("+WOOT");
	    return 0;
	}
    }
}
