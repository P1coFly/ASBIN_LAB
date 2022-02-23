using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;

namespace ASBIN_LAB
{
    public struct INS
    {
        public BNR latitude; // широта
        public BNR longitude; // долгота
        public BNR h; // высота
        public BNR coure_truth; // курс истины
        public BNR pitch_angle; // угол тангажа
        public BNR roll_angle; // угол крена
        public BNR speed_NS; // скорость Север/Юг
        public BNR speed_EW; // скорость Восток/Запад
        public BNR speed_vertical; // скорость вертикальная
        public BNR acceleration_ax; // Ускорение продольное ax
        public BNR acceleration_az; // Ускорение поперечное az
        public BNR acceleration_ay; //Ускорение нормальное ay
        public Discrete dis; //Слово состояния

        public byte[] Get_Bytes(INS str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public INS Get_Ins(byte[] arr)
        {
            INS str = new INS();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (INS)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }

  
}
