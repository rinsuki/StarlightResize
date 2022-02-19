using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static Windows.Win32.PInvoke;
using static Windows.Win32.Constants;
using static Windows.Win32.UI.WindowsAndMessaging.WINDOWS_HOOK_ID;

namespace StarlightResize
{
    internal class Hook : IDisposable
    {
        private UnhookWindowsHookExSafeHandle hookHandle;
        private HOOKPROC hookProc;
        private Action<Keys> hookAction;
        private bool disposedValue;

        public Keys HookKey { get; set; }
        private bool IsHookKeyPressed(int nCode, WPARAM wParam, LPARAM lParam) => (nCode, wParam.Value, (Keys)(short)Marshal.ReadInt32(lParam)) switch
        {
            ( >= 0, WM_KEYDOWN, var key) when key == HookKey => true,
            _ => false,
        };

        private LRESULT HookKeyCallback(int nCode, WPARAM wParam, LPARAM lParam)
        {
            
            if (IsHookKeyPressed(nCode, wParam, lParam)) hookAction(HookKey);
            return CallNextHookEx(hookHandle, nCode, wParam, lParam);
        }


        public Hook(Keys hookKey, Action<Keys> action)
        {
            HookKey = hookKey;
            hookAction = action;

            hookProc = new HOOKPROC(HookKeyCallback);

            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;

            hookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, GetModuleHandle(curModule.ModuleName), 0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                hookHandle?.Dispose();
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}