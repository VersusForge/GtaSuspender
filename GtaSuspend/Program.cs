using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GtaSuspend {

  static partial class Program {

    #region Fields

    private static volatile bool IsSuspended;

    #endregion Fields

    #region Methods

    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    private static void Main() {
      if (!Process.GetProcessesByName("GTA5").Any()) {
        Process.Start("steam://rungameid/271590");
      }
      HotKeyManager.HotKeyPressed += HotKeyManager_HotKeyPressed;
      HotKeyManager.RegisterHotKey(Keys.F8, KeyModifiers.Control);
      HotKeyManager.RegisterHotKey(Keys.F9, KeyModifiers.Control);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

      Application.Run(new MyCustomApplicationContext());
    }

    private async static void HotKeyManager_HotKeyPressed(object sender, HotKeyEventArgs e) {
      if (Process.GetProcessesByName("GTA5").FirstOrDefault() is Process process) {
        if (e.Key == Keys.F8) {
          if (!IsSuspended) {
            IsSuspended = true;
            process.Suspend();
            await Task.Delay(TimeSpan.FromSeconds(8));
            process.Resume();
            IsSuspended = false;
          }
        }
        else if (e.Key == Keys.F9) {
          if (IsSuspended) {
            process.Resume();
            IsSuspended = false;
          }
          else {
            process.Suspend();
            IsSuspended = true;
          }
        }
      }
    }

    #endregion Methods
  }
}