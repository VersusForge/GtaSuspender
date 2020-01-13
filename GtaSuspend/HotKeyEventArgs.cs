using System;
using System.Windows.Forms;

namespace GtaSuspend {

  public class HotKeyEventArgs : EventArgs {

    #region Fields

    public readonly Keys Key;
    public readonly KeyModifiers Modifiers;

    #endregion Fields

    #region Constructors

    public HotKeyEventArgs(Keys key, KeyModifiers modifiers) {
      this.Key = key;
      this.Modifiers = modifiers;
    }

    public HotKeyEventArgs(IntPtr hotKeyParam) {
      uint param = (uint)hotKeyParam.ToInt64();
      Key = (Keys)((param & 0xffff0000) >> 16);
      Modifiers = (KeyModifiers)(param & 0x0000ffff);
    }

    #endregion Constructors
  }
}