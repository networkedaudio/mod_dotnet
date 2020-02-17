using System;
using System.Collections.Concurrent;
using System.IO;
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

        // The FreeSWITCH APP interface callback delegate
        private delegate void NativeAPPCallback(IntPtr sessionptr, string data);

        // The FreeSWITCH APP interface callback delegate
        private delegate string NativeXMLCallback(string section, string tag, string key, string value, IntPtr eventptr);

        // Contains all native callbacks that will be called from the native host, each callback
        // is produced by marshalling delegates to native function pointers
        // Important: Must maintain the same structure in native_callbacks_t in mod_coreclr.c
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeCallbacks
        {
            public IntPtr NativeAPICallback;
            public IntPtr NativeAPPCallback;
            public IntPtr NativeXMLCallback;
        }

        // This is the only predefined entry point, this must match what mod_coreclr.c is looking for
        public static NativeCallbacks Load()
        {
            // Register some reserved Managed API's
            sAPIRegistry.TryAdd("load", LoadAPI);

            // Return the marshalled callbacks for the native interfaces
            return new NativeCallbacks
            {
                NativeAPICallback = Marshal.GetFunctionPointerForDelegate<NativeAPICallback>(NativeAPIHandler),
                NativeAPPCallback = Marshal.GetFunctionPointerForDelegate<NativeAPPCallback>(NativeAPPHandler),
                NativeXMLCallback = Marshal.GetFunctionPointerForDelegate<NativeXMLCallback>(NativeXMLHandler)
            };
        }

        // The Managed API interface callback delegate
        public delegate string APICallback(string args, ManagedSession session);
        // The Managed API callback registry
        // TODO: value type subject to change to a more complex type including an APIAttribute configuration
        private static ConcurrentDictionary<string, APICallback> sAPIRegistry = new ConcurrentDictionary<string, APICallback>();

        // This is the FreeSWITCH API interface callback handler which is bound to "coreclr" API commands
        private static int NativeAPIHandler(string command, IntPtr sessionptr, IntPtr streamptr)
        {
            using ManagedSession session = new ManagedSession(new SWIGTYPE_p_switch_core_session_t(sessionptr, false));
            using Stream stream = new Stream(new SWIGTYPE_p_switch_stream_handle_t(streamptr, false));


            // for now just call other dispatcher and return Todo: remove the rest of this code
            return PluginsContainer.DispatchAPI(command, stream, session);

            string args = command;
            if (!ParseArgument(ref args, out command, ' '))
            {
                Log.WriteLine(LogLevel.Error, "Missing Managed API");
                stream.Write("-ERROR Missing Managed API");
                return 0;
            }
            Log.WriteLine(LogLevel.Info, "Managed API: {0} {1}", command, args);

            if (!sAPIRegistry.TryGetValue(command.ToLower(), out APICallback callback))
            {
                Log.WriteLine(LogLevel.Error, "Managed API does not exist");
                stream.Write("-ERROR Managed API does not exist");
                return 0;
            }
            string result = null;
            try
            {
                result = callback(args, session);
            }
            catch (Exception)
            {
                // TODO: Log more of the exception data out
                Log.WriteLine(LogLevel.Error, "Managed API exception");
                result = "-ERROR Managed API exception";
            }
            if (result != null) stream.Write(result);
            return 0;
        }

        // The Managed APP interface callback delegate
        public delegate void APPCallback(string args, ManagedSession session);
        // The Managed APP callback registry
        // TODO: value type subject to change to a more complex type including an APPAttribute configuration
        private static ConcurrentDictionary<string, APPCallback> sAPPRegistry = new ConcurrentDictionary<string, APPCallback>();

        // This is the FreeSWITCH APP interface callback handler which is bound to "coreclr" APP commands
        private static void NativeAPPHandler(IntPtr sessionptr, string data)
        {
            using ManagedSession session = new ManagedSession(new SWIGTYPE_p_switch_core_session_t(sessionptr, false));

            PluginsContainer.DispatchDialPlanApp(data, session);
            return;

            string args = data;
            if (!ParseArgument(ref args, out string command, ' '))
            {
                Log.WriteLine(LogLevel.Error, "Missing Managed APP");
                return;
            }
            Log.WriteLine(LogLevel.Info, "Managed APP: {0} {1}", command, args);

            if (!sAPPRegistry.TryGetValue(command.ToLower(), out APPCallback callback))
            {
                Log.WriteLine(LogLevel.Error, "Managed APP does not exist");
                return;
            }
            try
            {
                callback(args, session);
            }
            catch (Exception)
            {
                // TODO: Log more of the exception data out
                Log.WriteLine(LogLevel.Error, "Managed APP exception");
            }
        }

        // The Managed XML interface callback delegate
        public delegate void XMLCallback(string section, string tag, string key, string value, Event evt, ref string result);

        public static event XMLCallback OnXMLSearch;

        // This is the FreeSWITCH XML interface callback handler
        private static string NativeXMLHandler(string section, string tag, string key, string value, IntPtr eventptr)
        {
            using Event evt = new Event(new SWIGTYPE_p_switch_event_t(eventptr, false), 0);
            Log.WriteLine(LogLevel.Info, "Managed XML Handler: {0} - {1} - {2} - {3}", section, tag, key, value);
            return PluginsContainer.DispatchXMLCallback(section, tag, key, value, evt);
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

        // Managed API for loading user assemblies and reflecting on attributes to register callbacks
        private static string LoadAPI(string args, ManagedSession session)
        {
            string path = Path.GetFullPath(args);
            if (!Path.HasExtension(path)) path = Path.ChangeExtension(path, ".dll");
            if (!File.Exists(path))
            {
                Log.WriteLine(LogLevel.Error, "File not found: {0}", path);
                return "-ERROR File not found";
            }

            // TODO: Load the assembly, kick off reflection scan for relevant attributes, add API's to sAPIRegistry

            return "+OK " + path;
        }
    }
}
