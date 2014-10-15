using System;

namespace PluginCore
{
	public interface APIUtils
	{
		IntPtr LoadLibrary(string fileName);
		//void FreeLibrary(IntPtr handle);
		//IntPtr GetProcAddress(IntPtr dllHandle, string name);


		IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		int GetScrollPos(IntPtr hWnd, int nBar);
		bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);
	}
}

