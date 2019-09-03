using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Core.Ocelot.OsInformations
{
    public class RamInformation
    {

        [DllImport("SystemInfoCpp.dll", CallingConvention = CallingConvention.Cdecl)]
        extern static long getAvailableSystemMemory_Windows64();


        [DllImport("SystemInfoCpp.so", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getAvailableSystemMemory_Linux64")]
        extern static long getAvailableSystemMemory_Linux64();

        /// <summary>
        /// Get system available memory in MB
        /// </summary>
        /// <returns>Value is in MB</returns>
        public static long GetAvailableSystemMemory()
        {
            long availableRam = 0;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                availableRam = getAvailableSystemMemory_Windows64();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                availableRam = getAvailableSystemMemory_Linux64();

            return availableRam;
        }

    }
}
