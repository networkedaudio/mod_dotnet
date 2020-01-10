using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using FreeSWITCH;

namespace FreeSWITCH
{
    public static class Loader
    {
	// The primary entry point delegate for obtaining the native callbacks during host initialization
        private delegate NativeCallbacks LoadDelegate();

	// The FreeSWITCH API interface callback delegate
	private delegate int NativeAPICallback(string command, IntPtr sessionptr, IntPtr streamptr);

	// Contains all native callbacks that will be called from the native host, each callback
	// is produced by marshalling delegates to native function pointers
	// Important: Must maintain the same structure in native_callbacks_t in mod_coreclr.c
        [StructLayout(LayoutKind.Sequential)]
        private struct NativeCallbacks
        {
	    public IntPtr NativeAPICallback;
        }

	// This is the only predefined entry point, this must match what mod_coreclr.c is looking for
        private static NativeCallbacks Load()
        {
	    sAPICallbacks.TryAdd("test", TestAPI);
            return new NativeCallbacks {
		NativeAPICallback = Marshal.GetFunctionPointerForDelegate<NativeAPICallback>(NativeAPIHandler)
	    };
        }

	// The Managed API interface callback delegate
	public delegate bool APICallback(string args, ref string result);
	private static ConcurrentDictionary<string, APICallback> sAPICallbacks = new ConcurrentDictionary<string, APICallback>();
	
	// This is the FreeSWITCH API interface callback handler which is bound to "coreclr" API commands
	private static int NativeAPIHandler(string command, IntPtr sessionptr, IntPtr streamptr)
	{
	    using ManagedSession session = new ManagedSession(new SWIGTYPE_p_switch_core_session_t(sessionptr, false));
	    using Stream stream = new Stream(new SWIGTYPE_p_switch_stream_handle_t(streamptr, false));

	    string args = command;
	    if (!ParseArgument(ref args, out command, ' '))
	    {
	        Log.WriteLine(LogLevel.Error, "Missing Managed API");
		stream.Write("-ERROR Missing Managed API");
		return 1;
	    }
	    Log.WriteLine(LogLevel.Info, "Managed API: {0} {1}", command, args);

	    if (!sAPICallbacks.TryGetValue(command.ToLower(), out APICallback callback))
	    {
		Log.WriteLine(LogLevel.Error, "Managed API does not exist");
		stream.Write("-ERROR Managed API does not exist");
		return 1;
	    }
	    string result = null;
	    bool succeeded = false;
	    try
	    {
	    	succeeded = callback(args, ref result);
	    }
	    catch (Exception)
	    {
	        Log.WriteLine(LogLevel.Error, "Managed API Exception");
		// TODO: Write more of the exception data out
	    }
	    if (result != null) stream.Write(result);
	    return succeeded ? 0 : 1;
	}

	// TODO: Put this somewhere more reusable
	private static bool ParseArgument(ref string args, out string arg, char separator)
	{
	    args = args.Trim();
	    int eoa = args.IndexOf(separator);
	    arg = null;
	    if (eoa < 0)
	    {
		string tmp = args;
		args = string.Empty;
		if (tmp.Length > 0) arg = tmp;
	        return arg != null;
	    }
	    arg = args.Substring(0, eoa);
	    args = args.Remove(0, eoa + 1).Trim();
	    return true;
	}

	private static bool TestAPI(string args, ref string result)
	{
	    Log.WriteLine(LogLevel.Info, "Managed TestAPI Executed");
	    result = "+WOOHOO";
	    return true;
	}
    }
}
