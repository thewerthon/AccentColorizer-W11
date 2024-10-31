using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using Microsoft.Win32;

namespace AccentColorizer_W11 {

	class Program {

		public const string ARGUMENT_APPLY = "-" + "Apply";
		public const string LISTENER_MUTEX = "-" + "ACCENTCLRE11";

		static void Main(string[] args) {

			var paths = FindPaths();
			foreach (var path in paths) { Utility.TakeOwnership(path); }

			var key = Registry.LocalMachine.CreateSubKey(@"SOFTWARE\AccentColorizer", true);
			var colorizer = new GlyphColorizer(paths.Select(path => path + @"\theme-").ToArray(), key);

			if (args.Length == 1 && ARGUMENT_APPLY.Equals(args[0])) {

				colorizer.ApplyColorization();
				key.Close();

			} else {

				if (Mutex.TryOpenExisting(LISTENER_MUTEX, out var mutex)) {

					key.Close();

				} else {

					mutex = new Mutex(false, LISTENER_MUTEX);
					var handler = new AccentColorListener(colorizer);
					handler.ApplyColorization();
					System.Windows.Forms.Application.Run(handler);

				}

			}

		}

		private static List<string> FindPaths() {

			var paths = new List<string>();

			var sysAppsPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows) + @"\SystemApps\";

			string[] knownPackages = {
					@"MicrosoftWindows.Client.CBS_cw5n1h2txyewy\",    // 21H2
          @"MicrosoftWindows.Client.Core_cw5n1h2txyewy\",   // 22H2
          @"MicrosoftWindows.Client.FileExp_cw5n1h2txyewy\" // WASDK
      };

			foreach (var pkg in knownPackages) {

				var path = sysAppsPath + pkg + @"FileExplorerExtensions\Assets\images\contrast-standard";
				if (!Directory.Exists(path)) { continue; }

				paths.Add(path);

			}

			return paths;

		}

	}

}