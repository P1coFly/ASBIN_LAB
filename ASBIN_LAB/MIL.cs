using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ASBIN_LAB
{
    //Командное слово
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Command_Word
    {
        uint com;
        public Command_Word(uint signal, uint address, uint k, uint subaddress, uint SD, uint P)
        {
            com = signal; //Синхро-сигнал
            com += address << 3; //адрес ОУ
            com += k << 8; //k
            com += subaddress << 9; //Подадрес Режим управления
            com += SD << 14; //Число СД Код команд
            com += P << 19; //P
        }

    }
    //Слово данных
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Data_Word
    {
        uint data;
        public Data_Word(uint signal, uint data, uint P)
        {
            this.data = signal; //Синхро-сигнал
            this.data += data << 3; //Данные 
            this.data += P << 19; //p
        }
    }

    //Ответное слово
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Reply_Word
    {
        uint reply;
        public Reply_Word(uint siganl, uint address, uint error, uint broadcast, uint request, uint reserve, uint command,
            uint a_busy, uint a_defect, uint interface_control, uint OY_defect, uint P)
        {
            reply = siganl; //Синхро-сигнал
            reply += address << 3; //адрес Оу
            reply += error << 8; //Ошибка в сообщение
            reply += broadcast << 9; //Передача ОС
            reply += request << 10; //Запрос на обслуживанеи
            reply += reserve << 11; //Резерв
            reply += command << 14; //Принята групповая команда
            reply += a_busy << 15; //Абонимент занят
            reply += a_defect << 16; //Неисправность абонимента
            reply += interface_control << 17; //Принято управление интерфесом
            reply += OY_defect << 18; //Неисправность Оу
            reply += P << 19; //P
        }
    }

    [System.Runtime.InteropServices.StructLayout(LayoutKind.Explicit)]
    public struct MIL
    {
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Command_Word com;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Data_Word data;
        [System.Runtime.InteropServices.FieldOffset(0)]
        public Reply_Word reply;
        [System.Runtime.InteropServices.FieldOffset(0)]
        uint mil;

        public uint Get_MIL()
        {
            return mil;
        }

    }
}
