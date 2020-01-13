using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GtaSuspend {

  public static class ProcessExtension {

    #region Methods

    public static void Suspend(this Process process) {
      foreach (ProcessThread thread in process.Threads) {
        var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
        if (pOpenThread == IntPtr.Zero) {
          break;
        }
        SuspendThread(pOpenThread);
      }
    }

    public static void Resume(this Process process) {
      foreach (ProcessThread thread in process.Threads) {
        var pOpenThread = OpenThread(ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);
        if (pOpenThread == IntPtr.Zero) {
          break;
        }
        ResumeThread(pOpenThread);
      }
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

    [DllImport("kernel32.dll")]
    private static extern uint SuspendThread(IntPtr hThread);

    [DllImport("kernel32.dll")]
    private static extern int ResumeThread(IntPtr hThread);

    #endregion Methods
  }
}