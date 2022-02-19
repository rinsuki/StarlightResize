using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Win32.Foundation;

namespace StarlightResize
{
    internal class Hook
    {
        private const int WH_KEYBOARD_LL = 13;
        private const uint WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, WPARAM wParam, LPARAM lParam);

        private IntPtr hookPtr = IntPtr.Zero;
        private LowLevelKeyboardProc proc = null;
        private Action<Keys> action;

        private bool IsHookKeyPressed(int nCode, WPARAM wParam, LPARAM lParam) => (nCode, wParam.Value, (Keys)(short)Marshal.ReadInt32(lParam)) switch
        {
            ( >= 0, WM_KEYDOWN, var key) when key == HookKey => true,
            _ => false,
        };

        private IntPtr HookKeyCallback(int nCode, WPARAM wParam, LPARAM lParam)
        {
            if (IsHookKeyPressed(nCode, wParam, lParam)) action(HookKey);
            return CallNextHookEx(hookPtr, nCode, wParam, lParam);
        }

        public Keys HookKey { get; set; }

        public Hook(Keys hookKey, Action<Keys> action)
        {
            HookKey = hookKey;
            this.action = action;
        }

        public void HookToFullScreen()
        {
            proc = new LowLevelKeyboardProc(HookKeyCallback);

            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;

            hookPtr = SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }

        public void UnHook()
        {
            if (hookPtr != IntPtr.Zero)
            {
                UnhookWindowsHookEx(hookPtr);
                hookPtr = IntPtr.Zero;
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, WPARAM wParam, LPARAM lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}