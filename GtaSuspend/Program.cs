using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace GtaSuspend {

  static partial class Program {

    #region Fields

    private static volatile bool IsSuspended;

    public static volatile bool IsJoiningFriend;
    public static HttpClient HttpClient { get; set; } = new HttpClient();
    public static JavaScriptSerializer JavaScriptSerializer { get; set; } = new JavaScriptSerializer();

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
      HotKeyManager.RegisterHotKey(Keys.F8, KeyModifiers.Control, Suspend10Seconds);
      HotKeyManager.RegisterHotKey(Keys.F8, KeyModifiers.Alt, SuspendTrigger);
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new SystemTrayContext());
    }

    private static void SuspendTrigger(HotKeyEventArgs obj) {
      if (Process.GetProcessesByName("GTA5").FirstOrDefault() is Process process) {
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

    private async static void Suspend10Seconds(HotKeyEventArgs obj) {
      if (!IsSuspended && Process.GetProcessesByName("GTA5").FirstOrDefault() is Process process) {
        IsSuspended = true;
        process.Suspend();
        await Task.Delay(TimeSpan.FromSeconds(8));
        process.Resume();
        IsSuspended = false;
      }
    }

    #endregion Methods
  }
}