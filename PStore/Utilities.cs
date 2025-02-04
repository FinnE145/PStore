using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32.Foundation;

namespace PStore;
internal class Utilities {
    unsafe class UIntPtr(IntPtr value) {
        public readonly uint* value = (uint*) value;
    }
    public static void Win32Function(Delegate func, params object?[]? args) {
        ArgumentNullException.ThrowIfNull(func);
        ArgumentNullException.ThrowIfNull(args);

        HRESULT res = (HRESULT) (func.DynamicInvoke(args) ?? HRESULT.S_OK);
        if (res != HRESULT.S_OK) {
            throw Marshal.GetExceptionForHR((int) res) ?? new Exception();
        }
    }

    public static bool Win32FuncRes(HRESULT res, string defaultMessage) {
        //try {
        if (res == HRESULT.S_OK) {
            return true;
        }

        Console.WriteLine($"Failed with {(uint) res:X8}");
        throw Marshal.GetExceptionForHR((int) res) ?? new Exception($"{defaultMessage} - {(uint) res:X8}");

        /*} catch (Exception ex) {
            Console.WriteLine($"Experienced an {ex.GetType()} parsing {(uint) res:X8}:\n{ex.Message}");
            return false;
        }*/
    }

    public static bool Win32FuncRes(HRESULT res) {
        return Win32FuncRes(res, $"Failed with unknown error");
    }
}
