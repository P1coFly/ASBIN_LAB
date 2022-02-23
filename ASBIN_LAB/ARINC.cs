using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ASBIN_LAB
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    //двоичное число
    public struct BNR
    {
        uint bnr;
        public BNR(uint address, double h, uint matrix, uint p)
        {
            uint sign = 0;
            if (h < 0)
            {
                sign = 1;
            }
            bnr = address; //адрес
            bnr += ((uint)(h) << 8); //высота
            bnr += sign << 28; //знак
            bnr += matrix << 29; //матрица
            bnr += p << 31; //бит чётности 
        }
        public void show()
        {
            uint address = bnr & 0xFF;
            uint h = (bnr & 0x1FFFFFFF) >> 8;
            uint sign = (bnr & 0x3FFFFFFF) >> 28;
            uint matrix = (bnr & 0x7FFFFFFF) >> 29;
            uint p = (bnr & 0xFFFFFFFF) >> 31;
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Адрес: {address}");
            Console.WriteLine($"Высота: {h}");
            Console.WriteLine($"Знак: {sign}");
            Console.WriteLine($"SM: {matrix}");
            Console.WriteLine($"P: {p}");
            Console.WriteLine("-----------------------------------");
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    //несколько параметров в одном слове
    public struct BCD
    {
        uint bcd;
        public BCD(uint address, uint seconds, uint minutes, uint hours, uint matrix, uint p)
        {
            bcd = address; //адрес
            bcd += seconds << 11; //секунды +пустые биты
            bcd += minutes << 17; //минуты
            bcd += hours << 23; //часы
            bcd += matrix << 29; //матрицы+пустые биты
            bcd += p << 31; //бит чётности
        }

    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    //дискретных данные
    public struct Discrete
    {
        uint dis;
        public Discrete(uint address, uint SDI, uint preparation, uint control, uint navigation, uint hypercomp, uint re_start,
            uint scale_preparation, uint heating, uint thermostating, uint no_data, uint no_reception, uint serviceability_ins,
            uint willingness_f, uint willingness, uint matrix, uint p)
        {
            dis = address; //адрес
            dis += SDI << 8; //инс
            dis += preparation << 10; //подготова по ЗК
            dis += control << 11; //контроль
            dis += navigation << 12; //навигация
            dis += hypercomp << 13; //гиперкомпассирование
            dis += re_start << 15; //повторный старт + пустой бит
            dis += scale_preparation << 16; //гиперкомпассирование
            dis += heating << 19; //гиперкомпассирование
            dis += thermostating << 20; //термостатирование
            dis += no_data << 21; //нет начальных данных
            dis += no_reception << 22; //нет приёма
            dis += serviceability_ins << 23; //исправность ИНС
            dis += willingness_f << 24; //готовность ускоренная
            dis += willingness << 25; //готовность
            dis += matrix << 29; //матрица + пустой бит
            dis += p << 31; //матрица
        }
        public void show()
        {
            uint address = dis & 0xFF;
            uint SDI = (dis & 0x3FF) >> 8;
            uint preparation = (dis & 0x7FF) >> 10;
            uint control = (dis & 0xFFF) >> 11;
            uint navigation = (dis & 0x1FFF) >> 12;
            uint hypercomp = (dis & 0x7FFF) >> 13;
            uint re_start = (dis & 0xFFFF) >> 15;
            uint scale_preparation = (dis & 0x7FFFF) >> 16;
            uint heating = (dis & 0xFFFFF) >> 19;
            uint thermostating = (dis & 0x1FFFFF) >> 20;
            uint no_data = (dis & 0x3FFFFF) >> 21;
            uint no_reception = (dis & 0x7FFFFF) >> 22;
            uint serviceability_ins = (dis & 0xFFFFFF) >> 23;
            uint willingness_f = (dis & 0x1FFFFFF) >> 24;
            uint willingness = (dis & 0x3FFFFFF) >> 25;
            uint matrix = (dis & 0x7FFFFFFF) >> 29;
            uint p = (dis & 0xFFFFFFFF) >> 31;
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"Адрес: {address}");
            Console.WriteLine($"SDI: {SDI}");
            Console.WriteLine($"Подготовка по ЗК: {preparation}");
            Console.WriteLine($"Контроль: {control}");
            Console.WriteLine($"Навигация: {navigation}");
            Console.WriteLine($"Гирокомпассирование: {hypercomp}");
            Console.WriteLine($"Повторный Запуск: {re_start}");
            Console.WriteLine($"Шкала Подготовки: {scale_preparation}");
            Console.WriteLine($"Исправность обогрева: {heating}");
            Console.WriteLine($"Териостатированеи: {thermostating}");
            Console.WriteLine($"Нет начальных данных: {no_data}");
            Console.WriteLine($"Нет приёма Hабс: {no_reception}");
            Console.WriteLine($"Исправность ИНС: {serviceability_ins}");
            Console.WriteLine($"Готовность ускоренная: {willingness_f}");
            Console.WriteLine($"Готовность: {willingness}");
            Console.WriteLine($"SM: {matrix}");
            Console.WriteLine($"P: {p}");
            Console.WriteLine("-----------------------------------");
        }
        public uint GetNavig()
        {
            return (dis & 0x1FFF) >> 12;
        }

        public uint Get_DIS()
        {
            return dis;
        }
        public void Set_Dis(uint dis)
        {
            this.dis = dis;
        }
    }

    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct ARINC
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public BNR bnr;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public BCD bcd;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Discrete discrete;
        [System.Runtime.InteropServices.FieldOffset(0)]
        uint arinc;

        public uint Get_ARINC()
        {
            return arinc;
        }
    }
}
