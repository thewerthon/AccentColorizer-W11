using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

namespace AccentColorizer_W11 {

	class AccentColorListener : Form {

		[DllImport("user32.dll")]
		static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		const int WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
		const int WM_THEMECHANGED = 0x031A;
		const int WM_WTSSESSION_CHANGE = 0x02B1;
		const int WTS_SESSION_UNLOCK = 0x8;

		[DllImport("wtsapi32.dll", SetLastError = true)]
		static extern bool WTSRegisterSessionNotification(IntPtr hWnd, [MarshalAs(UnmanagedType.U4)] int dwFlags);
		const int NOTIFY_FOR_THIS_SESSION = 0;

		private readonly GlyphColorizer Colorizer;
		private Color? LastAccent = null;

		public AccentColorListener(GlyphColorizer colorizer) {

			this.Colorizer = colorizer;
			ShowInTaskbar = false;
			Visible = false;

		}

		public void ApplyColorization() {

			var color = AccentColors.GetColorByTypeName("ImmersiveSystemAccent");
			if (LastAccent != null && LastAccent.Equals(color)) { return; }

			LastAccent = color;
			Colorizer.ApplyColorization();

		}

		protected override void OnHandleCreated(EventArgs e) {

			base.OnHandleCreated(e);
			WTSRegisterSessionNotification(Handle, NOTIFY_FOR_THIS_SESSION);

		}

		protected override void SetVisibleCore(bool value) {

			if (value && !IsHandleCreated) CreateHandle();

			value = false;
			base.SetVisibleCore(value);

		}

		protected override void WndProc(ref Message m) {

			if (m.Msg == WM_DWMCOLORIZATIONCOLORCHANGED || m.Msg == WM_THEMECHANGED || m.Msg == WM_WTSSESSION_CHANGE && m.WParam.ToInt32() == WTS_SESSION_UNLOCK) {

				if (m.Msg != WM_DWMCOLORIZATIONCOLORCHANGED) { LastAccent = null; }

				ApplyColorization();

			}

			base.WndProc(ref m);

		}

	}

}