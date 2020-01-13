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

  public static partial class HotKeyManager {

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

    public static int RegisterHotKey(Keys key, KeyModifiers modifiers) {
      _windowReadyEvent.WaitOne();
      int id = System.Threading.Interlocked.Increment(ref _id);
      _wnd.Invoke(new RegisterHotKeyDelegate(RegisterHotKeyInternal), _hwnd, id, (uint)modifiers, (uint)key);
      RegistedIds.Add(id);
      return id;
    }

    public static void UnregisterHotKey(int id) {
      _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
    }

    public static void UnregisterAllHotKeys() {
      foreach (var id in RegistedIds) {
        _wnd.Invoke(new UnRegisterHotKeyDelegate(UnRegisterHotKeyInternal), _hwnd, id);
      }
      RegistedIds.Clear();
    }

    #endregion Methods

    #region Fields

    private static List<int> RegistedIds = new List<int>();
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
      var invokedMethod = (EventHandler<HotKeyEventArgs>)ar.AsyncDelegate;

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
      if (HotKeyManager.HotKeyPressed != null) {
        HotKeyManager.HotKeyPressed.BeginInvoke(null, e, EndAsyncEvent, null);
      }
    }

    //public static event Action<HotKeyEventArgs> OnHotKeyPressed;
    [DllImport("user32", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
  }
}