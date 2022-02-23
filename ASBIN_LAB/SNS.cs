using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ASBIN_LAB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct Data
    {
        uint data;

        public Data(uint address, uint y, uint m, uint d, uint SM,uint P)
        {
            data = address; //адрес
            data += y << 10; //год
            data += m << 14; //месяц
            data += d << 23; //день
            data += SM << 29; //матрица состояния
            data += P << 31; //четность
        }

        public void show()
        {
            uint address = data & 0xFF;
            uint y = (data & 0x1FFFFFFF) >> 10;
            uint m = (data & 0x3FFFFFFF) >> 14;
            uint d = (data & 0x3FFFFFFF) >> 23;
            uint matrix = (data & 0x7FFFFFFF) >> 29;
            uint p = (data & 0xFFFFFFFF) >> 31;
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Адрес: {address}");
            Console.WriteLine($"Год: {y}");
            Console.WriteLine($"Месяц: {m}");
            Console.WriteLine($"День: {d}");
            Console.WriteLine($"SM: {matrix}");
            Console.WriteLine($"P: {p}");
            Console.WriteLine("-----------------------------------");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SRNS
    {
        uint srns;

        public SRNS(uint address, uint data, uint type, uint GPS, uint Glonass, uint type_work, uint sub_modes, uint time, uint dif,
            uint refusing, uint sig, uint coordinates, uint SM, uint P)
        {
            srns = address; //адрес
            srns += data << 8; //Запрос начальных данных
            srns += type << 9; //Тип рабочей срнс
            srns += GPS << 12; // Альманах GPS
            srns += Glonass << 13; // Альманх ГЛОНАСС
            srns += type_work << 14; // Режим работы
            srns += sub_modes << 16; // Подрежимы работы по сигналам СРНС
            srns += time << 17; // Признак времени
            srns += dif << 20;// Диффер режим измерений
            srns += refusing << 22; //Отказ изделия
            srns += sig << 23; //Порого сигнализации
            srns += coordinates << 24;//Система координат
            srns += SM << 29;//Матрица состояния 
            srns += P << 31;//Четность
        }
        public void show()
        {
            uint address = srns & 0xFF;
            uint data = (srns & 0x1FF) >> 8;
            uint type = (srns & 0x3FF) >> 9;
            uint GPS = (srns & 0x1FFF) >> 12;
            uint Glonass = (srns & 0x3FFF) >> 13;
            uint type_work = (srns & 0x7FFF) >> 14;
            uint sub_mode = (srns & 0x1FFFF) >> 16;
            uint time = (srns & 0x3FFFF) >> 17;
            uint dif = (srns & 0x3FFFFF) >> 20;
            uint refusing = (srns & 0xFFFFFF) >> 22;
            uint sig = (srns & 0x1FFFFFF) >> 23;
            uint coordinates = (srns & 0x3FFFFFF) >> 24;
            uint SM= (srns & 0x7FFFFFFF) >> 29;
            uint p = (srns & 0xFFFFFFFF) >> 31;
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Адрес: {address}");
            Console.WriteLine($"Запрос начальных данных: {data}");
            Console.WriteLine($"Тип рабочей СРНС: {type}");
            Console.WriteLine($"Альманах GPS: {GPS}");
            Console.WriteLine($"Альманах ГЛОНАСС: {Glonass}");
            Console.WriteLine($"Режим работы: {type_work}");
            Console.WriteLine($"Подрежимы работы по сигналам СРНС: {sub_mode}");
            Console.WriteLine($"Признак времени: {time}");
            Console.WriteLine($"Дифференциальный режим измерений: {dif}");
            Console.WriteLine($"Отказ изделия: {refusing}");
            Console.WriteLine($"Порого сигнализации: {sig}");
            Console.WriteLine($"Система координат: {coordinates}");
            Console.WriteLine($"SM: {SM}");
            Console.WriteLine($"P: {p}");
            Console.WriteLine("-----------------------------------");
        }
        public uint Get_SRNS()
        {
            return srns;
        }
        public void Set_SRNS(uint srns)
        {
            this.srns = srns;
        }
    }

    struct SNS
    {
        public BNR h; // широта
        public BNR HDOP;
        public BNR VDOP;
        public BNR course_angle; // путевой угол
        public BNR curr_latitude; // текущая широта
        public BNR curr_latitude_exactly; //текущая широта (точно)
        public BNR curr_longitude; // текущая долгта
        public BNR curr_longitude_exactly; //текущая долгата (точно)
        public BNR delay; //Задержка выдачи обновленных НП относительно МВ
        public BNR UTC_high; // текущее время UTC (старшие разряды)
        public BNR UTC_low; //текущее время UTC (младшие разряды)
        public BNR speed_vertical;
        public Data data; //ДАТА
        public SRNS srns; //Структура слова признаки СРНС

        public byte[] Get_Bytes(SNS str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public SNS Get_Sns(byte[] arr)
        {
            SNS str = new SNS();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(arr, 0, ptr, size);

            str = (SNS)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            return str;
        }
    }
}
