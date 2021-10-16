using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KeyboardHookMonitor
{
    /// <summary>
    /// グローバルキーフッククラス
    /// cf. https://aonasuzutsuki.hatenablog.jp/entry/2018/10/15/170958 
    /// </summary>
    public static class KeyboardHook
    {
        private const int WH_KEYBOARD_LL = 0x000D;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            KEYEVENTF_EXTENDEDKEY = 0x0001,
            KEYEVENTF_KEYUP = 0x0002,
            KEYEVENTF_UNICODE = 0x0004,
            KEYEVENTF_SCANCODE = 0x0008,
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr KeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static KeyboardProc keyboardProc;
        private static IntPtr hookId = IntPtr.Zero;

        public static void Hook()
        {
            if (hookId == IntPtr.Zero) {
                keyboardProc = HookProcedure;
                using (var curProcess = Process.GetCurrentProcess()) {
                    using (ProcessModule curModule = curProcess.MainModule) {
                        hookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, GetModuleHandle(curModule.ModuleName), 0);
                    }
                }
            }
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hookId);
            hookId = IntPtr.Zero;
        }

        public enum WM_KEY_STATUS {
            None,
            WmKeyDown,
            WmSysKeyDown,
            WmKeyUp,
            WmSysKeyUp
        }

        public delegate void DelegateOnKeyboardEvent(WM_KEY_STATUS wmKeyStatus, uint vkey, uint scanCode, uint flags, uint time, uint extraInfo);

        public static DelegateOnKeyboardEvent OnKeyboardEvent { get; set; }

        public static IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            WM_KEY_STATUS wmKeyStatus =
                (wParam == (IntPtr)WM_KEYDOWN) ? WM_KEY_STATUS.WmKeyDown :
                (wParam == (IntPtr)WM_SYSKEYDOWN) ? WM_KEY_STATUS.WmSysKeyDown :
                (wParam == (IntPtr)WM_KEYUP) ? WM_KEY_STATUS.WmKeyUp :
                (wParam == (IntPtr)WM_SYSKEYUP) ? WM_KEY_STATUS.WmSysKeyUp :
                WM_KEY_STATUS.None;

            if (nCode >= 0 && wmKeyStatus != WM_KEY_STATUS.None) {
                var kb = (KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));
                OnKeyboardEvent?.Invoke(wmKeyStatus, (uint)kb.vkCode, (uint)kb.scanCode, (uint)kb.flags, (uint)kb.time, (uint)kb.dwExtraInfo);
            }

            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
