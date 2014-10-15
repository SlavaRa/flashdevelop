/*
 * Win32 controls scrolling management
 */
 
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using PluginCore;

namespace Win32
{
	public class Scrolling
	{
		public const int SB_HORZ = 0;
		public const int SB_VERT = 1; 
		const int SB_LEFT = 6;
		const int SB_RIGHT = 7;
		const int WM_HSCROLL = 0x0114;
		const int WM_VSCROLL = 0x0115;

		public static void scrollToBottom(Control ctrl)
		{
			int min, max;
			OSHelper.API.GetScrollRange(ctrl.Handle, SB_VERT, out min, out max);
			OSHelper.API.SendMessage(ctrl.Handle, WM_VSCROLL, SB_RIGHT, max);
		}
		
		public static void scrollToTop(Control ctrl)
		{
			OSHelper.API.SendMessage(ctrl.Handle, WM_VSCROLL, SB_LEFT, 0);
		}
		
		public static void scrollToRight(Control ctrl)
		{
			int min, max;
			OSHelper.API.GetScrollRange(ctrl.Handle, SB_HORZ, out min, out max);
			OSHelper.API.SendMessage(ctrl.Handle, WM_HSCROLL, SB_RIGHT, max);
		}
		
		public static void scrollToLeft(Control ctrl)
		{
			OSHelper.API.SendMessage(ctrl.Handle, WM_HSCROLL, SB_LEFT, 0);
		}
	}
}
