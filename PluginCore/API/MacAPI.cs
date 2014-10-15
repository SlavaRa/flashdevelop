
using System;
using System.Runtime.InteropServices;

namespace PluginCore
{
	public class MacAPI : APIUtils
	{
		public MacAPI()
		{
		}

		public IntPtr LoadLibrary(string fileName) 
		{
			return dlopen(fileName, RTLD_NOW);
		}

		IntPtr APIUtils.SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			// FIXME: NOT IMPL
			return new IntPtr();
		}

		int APIUtils.GetScrollPos(IntPtr hWnd, int nBar)
		{
			// FIXME: NOT IMPL
			return -1;
		}

		bool APIUtils.GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos)
		{
			// FIXME: NOT IMPL
			lpMinPos = 0;
			lpMaxPos = 0;
			return false;
		}

		const int RTLD_NOW = 2;

		[DllImport("libdl.so")]
		private static extern IntPtr dlopen(String fileName, int flags);
	}
}

