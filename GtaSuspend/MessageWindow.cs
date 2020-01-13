using System.Windows.Forms;

namespace GtaSuspend {

  public static partial class HotKeyManager {

    #region Classes

    public class MessageWindow : Form {

      #region Constructors

      public MessageWindow() {
        _wnd = this;
        _hwnd = this.Handle;
        _windowReadyEvent.Set();
      }

      #endregion Constructors

      #region Methods

      protected override void WndProc(ref Message m) {
        if (m.Msg == WM_HOTKEY) {
          HotKeyEventArgs e = new HotKeyEventArgs(m.LParam);
          HotKeyManager.OnHotKeyPressed(e);
        }

        base.WndProc(ref m);
      }

      protected override void SetVisibleCore(bool value) {
        // Ensure the window never becomes visible
        base.SetVisibleCore(false);
      }

      #endregion Methods

      #region Fields

      private const int WM_HOTKEY = 0x312;

      #endregion Fields
    }

    #endregion Classes
  }
}