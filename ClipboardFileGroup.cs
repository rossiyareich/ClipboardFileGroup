using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using Windows.Win32.UI.Shell;
using static Windows.Win32.PInvoke;

namespace ClipboardFileGroup
{
    /// <summary>
    ///     The static class containing methods for basic clipboard functionality with CF_HDROP
    /// </summary>
    public static class ClipboardFileGroup
    {
        /// <summary>
        ///     Takes ownership of the clipboard and sends paths to it
        /// </summary>
        /// <param name="paths">Path of files to copy to clipboard</param>
        /// <param name="shouldMove">True: Performs a Delete-on-Paste Operation, False: Performs a normal copy operation</param>
        public static unsafe void SetClipboardPaths(IEnumerable<string> paths, bool shouldMove)
        {
            HANDLE hDropEffect = (HANDLE)GlobalAlloc(GLOBAL_ALLOC_FLAGS.GHND, (UIntPtr)4);
            IntPtr byteStream = (IntPtr)GlobalLock(hDropEffect);
            byte[] dragDropData = BitConverter.GetBytes(shouldMove ? 2 : 1);
            Marshal.Copy(dragDropData, 0, byteStream, 4);
            GlobalUnlock(hDropEffect);

            int clpSize = sizeof(DROPFILES) + sizeof(char); // for arr-terminating \0
            foreach (string path in paths)
            {
                clpSize += sizeof(char) * (path.Length + 1);
            }

            HANDLE hDrop = (HANDLE)GlobalAlloc(GLOBAL_ALLOC_FLAGS.GHND, (UIntPtr)clpSize);
            DROPFILES* df = (DROPFILES*)GlobalLock(hDrop);
            df->pFiles = (uint)sizeof(DROPFILES); // arr offset
            df->fWide = true; // unicode

            char* pathHead = (char*)&df[1];
            foreach (string path in paths)
            {
                byte[] chars = Encoding.Unicode.GetBytes(path + '\0');
                Marshal.Copy(chars, 0, (IntPtr)pathHead, chars.Length);
                pathHead += path.Length + 1;
            }

            *pathHead = '\0';
            GlobalUnlock(hDrop);

            OpenClipboard((HWND)(IntPtr)0);
            EmptyClipboard();
            SetClipboardData(15, hDrop);
            SetClipboardData(RegisterClipboardFormat("Preferred DropEffect"), hDropEffect);
            CloseClipboard();
        }

        /// <summary>
        ///     Gets the path to files currently in clipboard
        /// </summary>
        /// <returns>Full qualified path to files currently in clipboard</returns>
        /// <exception cref="OutOfMemoryException"></exception>
        public static unsafe IEnumerable<string> GetClipboardPaths()
        {
            OpenClipboard((HWND)(IntPtr)0);
            HANDLE hMem = GetClipboardData(15);
            DROPFILES* df = (DROPFILES*)GlobalLock(hMem);

            HDROP hDrop = (HDROP)(IntPtr)df;
            uint pathCount = DragQueryFile(hDrop, uint.MaxValue, null!, 0);
            string[] paths = new string[pathCount];
            for (int i = 0; i < pathCount; i++)
            {
                uint lengthWithoutTerm = DragQueryFile(hDrop, (uint)i, null!, 0);
                IntPtr strRes = Marshal.AllocHGlobal(sizeof(char) * ((int)lengthWithoutTerm + 1));
                uint success = DragQueryFile(hDrop, (uint)i, (char*)strRes, lengthWithoutTerm + 1);
                if (success == 0)
                {
                    throw new OutOfMemoryException();
                }

                paths[i] = Marshal.PtrToStringUni(strRes, (int)lengthWithoutTerm);
                Marshal.FreeHGlobal(strRes);
            }

            GlobalUnlock(hMem);
            CloseClipboard();

            return paths;
        }

        /// <summary>
        ///     Clears the clipboard
        /// </summary>
        public static void ClearClipboard()
        {
            OpenClipboard((HWND)(IntPtr)0);
            EmptyClipboard();
            CloseClipboard();
        }
    }
}
