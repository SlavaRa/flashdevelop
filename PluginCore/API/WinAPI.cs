using System;
using System.Runtime.InteropServices;
using ScintillaNet.Enums;

namespace PluginCore
{
	public class WinAPI : APIUtils
	{
		public WinAPI()
		{
		}

		IntPtr APIUtils.LoadLibrary(string fileName) 
		{
			return LoadLibrary(fileName);
		}

		IntPtr APIUtils.SendMessage(IntPtr hWnd, int msg, int wParam, int lParam)
		{
			return SendMessage(hWnd, msg, wParam, lParam);
		}

		int APIUtils.GetScrollPos(IntPtr hWnd, int nBar)
		{
			return GetScrollPos(hWnd, nBar);
		}

		bool APIUtils.GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos)
		{
			return GetScrollRange(hWnd, nBar, out lpMinPos, out lpMaxPos);
		}

		[DllImport("kernel32.dll")]
		public extern static IntPtr LoadLibrary(string lpLibFileName);
		
		[DllImport ("user32.dll")]
		public static extern IntPtr CreateWindowEx(uint dwExStyle, string lpClassName, string lpWindowName, uint dwStyle, int x, int y, int width, int height, IntPtr hWndParent, int hMenu, IntPtr hInstance, string lpParam);
		
		[DllImport("kernel32.dll", EntryPoint = "SendMessage")]
		public static extern int SendMessageStr(IntPtr hWnd, int message, int data, string s);
		
		[DllImport("user32.dll")]
		public  static extern IntPtr SetFocus(IntPtr hwnd);

		[DllImport("user32", CharSet=CharSet.Auto)]
		public static extern bool GetScrollRange(IntPtr hWnd, int nBar, out int lpMinPos, out int lpMaxPos);

		[DllImport( "User32.dll" )]
		public static extern int GetScrollPos(IntPtr hWnd, int nBar);

		[DllImport("user32", CharSet=CharSet.Auto)] 
		public static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);
		
	}
	
}
