using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PluginCore
{
	public static class OSHelper
	{
		static OSHelper()
		{
			if(Path.DirectorySeparatorChar == '\\')
				_api = new WinAPI();
			else if(IsRunningOnMac())
				_api = new MacAPI();
			else if(IsRunningOnLinux())
				_api = new LinuxAPI();
			else
			{
				throw new UnknownOSTypeException(Environment.OSVersion);
			}
		}

		public static APIUtils API
		{
			get { return _api; }
		}

		[DllImport("libc")]
		static extern int uname(IntPtr buf);

		public static bool IsRunningOnMac()
		{
			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				if(uname(buf) == 0)
				{
					string os = Marshal.PtrToStringAnsi(buf);
					if(os == "Darwin")
						return true;
				}
			}
			catch
			{
				// don't do anything
			}
			finally
			{
				if(buf != IntPtr.Zero)
					Marshal.FreeHGlobal(buf);
			}

			return false;
		}

		public static bool IsRunningOnLinux() 
		{
			var p = (int) Environment.OSVersion.Platform;
			return (p == 4) || (p == 6) || (p == 128);
		}

		private static APIUtils _api;
	}

	public class UnknownOSTypeException : Exception
	{
		public UnknownOSTypeException(OperatingSystem os)
			: base(os.ToString())
		{
		}
	}
}

