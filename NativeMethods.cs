using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShomreiTorah.Journal {
	static class NativeMethods {

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
		static extern IntPtr GetFocus();

		public static Control GetFocusedControl() {
			IntPtr focusedHandle = GetFocus();
			if (focusedHandle != IntPtr.Zero)
				return Control.FromHandle(focusedHandle);
			return null;
		}
	}
}
