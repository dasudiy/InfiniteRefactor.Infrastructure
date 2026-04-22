using System;
using System.Runtime.InteropServices;

namespace AirMaster.Infrastructure.Serializer
{
    public class MarshalSerializer
    {
        /// <summary>
        /// Serializes the specified object into a byte array.
        /// </summary>
        /// <param name="nativeObject">The object to serialize.</param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            Type objectType = obj.GetType();
            int objectSize = Marshal.SizeOf(obj);
            IntPtr buffer = Marshal.AllocHGlobal(objectSize);
            Marshal.StructureToPtr(obj, buffer, false);
            byte[] array = new byte[objectSize];
            Marshal.Copy(buffer, array, 0, objectSize);
            Marshal.FreeHGlobal(buffer);
            return array;
        }
    }
}
