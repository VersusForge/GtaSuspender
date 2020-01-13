using GtaSuspend.Properties;
using System;
using System.Windows.Forms;

namespace GtaSuspend {

  public class MyCustomApplicationContext : ApplicationContext {

    #region Constructors

    public MyCustomApplicationContext() {
      // Initialize Tray Icon
      trayIcon = new NotifyIcon() {
        Icon = Resources.AppIcon,
        ContextMenu = new ContextMenu(new MenuItem[] {
                new MenuItem("Exit", Exit)
            }),
        Visible = true
      };
    }

    #endregion Constructors

    #region Fields

    private NotifyIcon trayIcon;

    #endregion Fields

    #region Methods

    private void Exit(object sender, EventArgs e) {
      // Hide tray icon, otherwise it will remain shown until user mouses over it
      trayIcon.Visible = false;
      HotKeyManager.UnregisterAllHotKeys();
      Application.Exit();
    }

    #endregion Methods
  }
}