using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GtaSuspend {

  [Flags]
  public enum KeyModifiers {
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8,
    NoRepeat = 0x4000
  }

  public static class HotKeyManager {

    #region Constructors

    static HotKeyManager() {
      Thread messageLoop = new Thread(delegate () {
        Application.Run(new MessageWindow());
      });
      messageLoop.Name = "MessageLoopThread";
      messageLoop.IsBackground = true;
      messageLoop.Start();
    }

    #endregion Constructors

    #region Events

    public static event EventHandler<HotKeyEventArgs> HotKeyPressed;

    #endregion Events

    #region Methods

    public static int RegisterHotKey(Keys key, KeyModifiers modifiers, Action<HotKeyEventArgs> action = null) {
      _windowReadyEvent.WaitOne();
      int id = System.Threading.Interlocked.Increment(ref _id);
      _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)modifiers, (uint)key);
      Hotkeys.Add(id, action);
      return id;
    }

    public static void UnregisterHotKey(int id) {
      _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
    }

    public static void UnregisterAllHotKeys() {
      foreach (var id in Hotkeys.Keys) {
        _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
      }
      Hotkeys.Clear();
    }

    #endregion Methods

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
          HotKeyEventArgs e = new HotKeyEventArgs(ref m);
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

    #region Fields

    private static Dictionary<int, Action<HotKeyEventArgs>> Hotkeys = new Dictionary<int, Action<HotKeyEventArgs>>();
    private static volatile MessageWindow _wnd;

    private static volatile IntPtr _hwnd;

    private static ManualResetEvent _windowReadyEvent = new ManualResetEvent(false);

    private static int _id = 0;

    #endregion Fields

    #region Delegates

    private delegate void RegisterHotKeyDelegate(IntPtr hwnd, int id, uint modifiers, uint key);

    private delegate void UnRegisterHotKeyDelegate(IntPtr hwnd, int id);

    #endregion Delegates

    private static void EndAsyncEvent(IAsyncResult iar) {
      var ar = (System.Runtime.Remoting.Messaging.AsyncResult)iar;
      var invokedMethod = (Action<HotKeyEventArgs>)ar.AsyncDelegate;

      try {
        invokedMethod.EndInvoke(iar);
      }
      catch {
        // Handle any exceptions that were thrown by the invoked method
        Console.WriteLine("An event listener went kaboom!");
      }
    }

    private static void RegisterHotKeyInternal(IntPtr hwnd, int id, uint modifiers, uint key) {
      RegisterHotKey(hwnd, id, modifiers, key);
    }

    private static void UnRegisterHotKeyInternal(IntPtr hwnd, int id) {
      UnregisterHotKey(_hwnd, id);
    }

    private static void OnHotKeyPressed(HotKeyEventArgs e) {
      if (Hotkeys.TryGetValue(e.ID, out var action) && action != null) {
        action.BeginInvoke(e, EndAsyncEvent, null);
      }
      HotKeyManager.HotKeyPressed?.BeginInvoke(null, e, EndAsyncEvent, null);
    }

    //public static event Action<HotKeyEventArgs> OnHotKeyPressed;
    [DllImport("user32", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
  }

  public class HotKeyEventArgs : EventArgs {

    #region Properties

    public int ID { get; }

    #endregion Properties

    #region Fields

    public readonly Keys Key;
    public readonly KeyModifiers Modifiers;

    #endregion Fields

    #region Constructors

    public HotKeyEventArgs(Keys key, KeyModifiers modifiers, int id) {
      this.Key = key;
      this.Modifiers = modifiers;
      this.ID = id;
    }

    public HotKeyEventArgs(ref Message message) {
      uint param = (uint)message.LParam.ToInt64();
      ID = message.WParam.ToInt32();
      Key = (Keys)((param & 0xffff0000) >> 16);
      Modifiers = (KeyModifiers)(param & 0x0000ffff);
    }

    #endregion Constructors
  }
}