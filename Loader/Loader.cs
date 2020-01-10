using System;
using System.Runtime.InteropServices;
using FreeSWITCH;

namespace FreeSWITCH
{
    // TODO: Need to have types from SWIG available here through an assembly reference that will be marshalled through callbacks
    public sealed class Loader
    {
        public delegate InterfaceCallbacks LoadDelegate();

	public delegate int TestCallback();

        [StructLayout(LayoutKind.Sequential)]
        public struct InterfaceCallbacks
        {
            public IntPtr OnTest;
        }
	
        public static InterfaceCallbacks Load()
        {
            return new InterfaceCallbacks {
	        OnTest = Marshal.GetFunctionPointerForDelegate<TestCallback>(OnTestHandler)
	    };
        }

        private static int OnTestHandler()
        {
            // return something unique to confirm we can call it from native code
	    Log.WriteLine(LogLevel.Info, "Logging from managed code in OnTestHandler!");
            return 42;
        }
    }
}
