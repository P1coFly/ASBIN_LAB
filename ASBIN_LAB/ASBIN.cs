using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Timers;
using System.Threading;
using System.Text;
using System.Globalization;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Linq;

namespace ASBIN_LAB
{
    class ASBIN
    {
        public static INS ins = new INS();
        public static SNS sns = new SNS();
        static Mutex mtx = new Mutex();
        public static uint phi = 190, lamba = 190;
        const int port = 12346;
        const string adr = "127.0.0.1";
        const string filename = @"C:\Users\Art\Desktop\road.kml";
        const string Endfilename = @"C:\Users\Art\Desktop\end.kml";

        const double g = 9.8;

        public static UdpClient UDPSender;
        //пакет для отправки
        public static byte[] packetSend = new byte[4]; //массив для отправки;
        public static IPEndPoint SendEndPoint;

        //таймер подготовки ИНС
        public static System.Timers.Timer Prepare_ins = new System.Timers.Timer();

        public static double start_abs = DateTime.UtcNow.Millisecond;

        public static List<double> angle = new List<double>();

        //создаём экземпляр класса ЛА
        public static LA la = new LA(filename);

        public static List<PPM> point = new List<PPM>();

        public static string err;

        //ins start
        static void INS_Start()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            Console.WriteLine($"{timestamp} - Запуск тест-контроля ИНС 20 сек...");

            System.Timers.Timer TestControl = new System.Timers.Timer(2000);//20sek
            TestControl.Elapsed += Ins_TestControl_Elapsed;
            TestControl.Enabled = true;
            TestControl.AutoReset = false;
            TestControl.Start();
    }

        private static void Ins_TestControl_Elapsed(object sender, ElapsedEventArgs e)
        {
            mtx.WaitOne();
            ins.dis = new Discrete(address: 184, SDI: 1, preparation: 0, control: 0, navigation: 0, hypercomp: 0, re_start: 0,
                scale_preparation: 1, heating: 1, thermostating: 1, no_data: 1, no_reception: 0, serviceability_ins: 1, willingness_f: 0,
                willingness: 0, matrix: 0, p: 0);
            mtx.ReleaseMutex();

            Prepare_ins = new System.Timers.Timer(2.5);//400 Гц
            Prepare_ins.Elapsed += Prepare_ins_Elapsed;
            Prepare_ins.Enabled = true;
            Prepare_ins.AutoReset = true;
            Prepare_ins.Start();
        }

        private static void Prepare_ins_Elapsed(object sender, ElapsedEventArgs e)
        {
            if ((phi > 180) || (lamba > 180))
            {
                mtx.WaitOne();
                ins.dis = new Discrete(address: 184, SDI: 1, preparation: 1, control: 0, navigation: 0, hypercomp: 0, re_start: 0,
                    scale_preparation: 1, heating: 1, thermostating: 1, no_data: 1, no_reception: 0, serviceability_ins: 1, willingness_f: 0,
                    willingness: 0, matrix: 0, p: 0);
                mtx.ReleaseMutex();
            }

            else
            {

                mtx.WaitOne();
                ins.dis = new Discrete(address: 184, SDI: 1, preparation: 1, control: 0, navigation: 0, hypercomp: 0, re_start: 0,
                    scale_preparation: 1, heating: 1, thermostating: 1, no_data: 0, no_reception: 0, serviceability_ins: 1, willingness_f: 0,
                    willingness: 1, matrix: 0, p: 0);
                mtx.ReleaseMutex();

                Prepare_ins.Stop();

                //ждём 2 минуты чтобы включить режим навигация
                System.Timers.Timer waiter_ins = new System.Timers.Timer(2000); //2min
                waiter_ins.Elapsed += Waiter_ins_Elapsed;
                waiter_ins.Enabled = true;
                waiter_ins.AutoReset = false;
                waiter_ins.Start();

            }
        }

        private static void Waiter_ins_Elapsed(object sender, ElapsedEventArgs e)
        {
            //запускаем режим навигация и формируем данные с частотой 400Гц
            System.Timers.Timer ins_navigation = new System.Timers.Timer(2.5); //400Гц
            ins_navigation.Elapsed += Ins_navigation_Elapsed;
            ins_navigation.Enabled = true;
            ins_navigation.AutoReset = false;
            ins_navigation.Start();
        }

        private static void Ins_navigation_Elapsed(object sender, ElapsedEventArgs e)
        {
            mtx.WaitOne();
            ins.latitude = new BNR(address: 200, h: la.Lat_m, matrix: 1, p: 1);
            ins.longitude = new BNR(address: 201, h: la.Lon_m, matrix: 1, p: 1);
            ins.h = new BNR(address: 241, h: la.H, matrix: 1, p: 1);
            ins.coure_truth = new BNR(address: 204, h: la.alpha, matrix: 1, p: 1);
            ins.pitch_angle = new BNR(address: 212, h: la.pitch * 180/Math.PI, matrix: 1, p: 1);
            ins.roll_angle = new BNR(address: 213, h: 45, matrix: 1, p: 1);
            ins.speed_NS= new BNR(address: 246, h: 370, matrix: 1, p: 1);
            ins.speed_EW = new BNR(address: 247, h: 370, matrix: 1, p: 1);
            ins.speed_vertical = new BNR(address: 245, h: la.V * Math.Sin(la.pitch), matrix: 1, p: 1);
            ins.acceleration_ax = new BNR(address: 217, h: 2, matrix: 1, p: 1);
            ins.acceleration_az = new BNR(address: 218, h: 10, matrix: 1, p: 1);
            ins.acceleration_ay = new BNR(address: 219, h: 10, matrix: 1, p: 1);
            ins.dis = new Discrete(address: 184, SDI: 1, preparation: 1, control: 0, navigation: 1, hypercomp: 0, re_start: 0,
                    scale_preparation: 1, heating: 1, thermostating: 1, no_data: 0, no_reception: 0, serviceability_ins: 1, willingness_f: 0,
                    willingness: 1, matrix: 0, p: 0);
            mtx.ReleaseMutex();
        }

        //ins end
        /*
         * ----------------------------------------
         */
        //sns start
        static void SNS_Start()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            Console.WriteLine($"{timestamp} - Запуск тест-контроля СНС 20 сек...");

            System.Timers.Timer sns_testcontrol = new System.Timers.Timer(2000); //20sec
            sns_testcontrol.Elapsed += Sns_testcontrol_Elapsed;
            sns_testcontrol.Enabled = true;
            sns_testcontrol.AutoReset = false;
            sns_testcontrol.Start();
        }
        private static void Sns_testcontrol_Elapsed(object sender, ElapsedEventArgs e)
        {
            mtx.WaitOne();
            sns.srns = new SRNS(address: 187, data: 1, type: 1, GPS: 1, Glonass: 1, type_work: 2, sub_modes: 1, time: 1, dif: 1, refusing: 0,
                sig: 1, coordinates: 0, SM: 0, P: 0);
            mtx.ReleaseMutex();

            //ждём 2 минуты чтобы включить режим навигация
            System.Timers.Timer waiter_sns = new System.Timers.Timer(2000); //2min
            waiter_sns.Elapsed += Waiter_sns_Elapsed;
            waiter_sns.Enabled = true;
            waiter_sns.AutoReset = false;
            waiter_sns.Start();

        }

        private static void Waiter_sns_Elapsed(object sender, ElapsedEventArgs e)
        {
            //запускаем режим навигация и формируем данные с частотой 10Гц
            System.Timers.Timer sns_navigation = new System.Timers.Timer(100); //10Гц
            sns_navigation.Elapsed += Sns_navigation_Elapsed;
            sns_navigation.Enabled = true;
            sns_navigation.AutoReset = false;
            sns_navigation.Start();
        }

        private static void Sns_navigation_Elapsed(object sender, ElapsedEventArgs e)
        {
            mtx.WaitOne();
            sns.h = new BNR(address: 62, h: la.H, matrix: 1, p: 1);
            sns.HDOP = new BNR(address: 65, h: 1024, matrix: 1, p: 1);
            sns.VDOP = new BNR(address: 66, h: 1024, matrix: 1, p: 1);
            sns.course_angle = new BNR(address: 67, h: la.alpha, matrix: 1, p: 1);
            sns.curr_latitude= new BNR(address: 72, h: la.Lat_m, matrix: 1, p: 1);
            sns.curr_latitude_exactly = new BNR(address: 80, h: la.Lat_m, matrix: 1, p: 1);
            sns.curr_longitude = new BNR(address: 73, h: la.Lon_m, matrix: 1, p: 1);
            sns.curr_longitude_exactly = new BNR(address: 81, h: la.Lon_m, matrix: 1, p: 1);
            sns.delay = new BNR(address: 75, h: 1, matrix: 1, p: 1);
            sns.UTC_high = new BNR(address: 103, h: 90, matrix: 1, p: 1);
            sns.UTC_low = new BNR(address: 96, h: 90, matrix: 1, p: 1);
            sns.speed_vertical = new BNR(address: 117, h: la.V * Math.Sin(la.pitch), matrix: 1, p: 1);
            sns.data = new Data(address: 176, y: (uint)DateTime.UtcNow.Year, m: (uint)DateTime.UtcNow.Month, d: (uint)DateTime.UtcNow.Day, SM: 1, P: 1);
            sns.srns = new SRNS(address: 187, data: 1, type: 1, GPS: 1, Glonass: 1, type_work: 2, sub_modes: 1, time: 1, dif: 1, refusing: 0,
                sig: 1, coordinates: 0, SM: 0, P: 0);
            mtx.ReleaseMutex();
            lamba = 50;
            phi = 50;
        }
        //sns end
        

        static void Reciever()
        {
            UdpClient udpReciever = new UdpClient();
            IPEndPoint recieverEndPoint = null;
            
                try
                {
                    udpReciever = new UdpClient(port);
                }
                catch
                {
                    Console.WriteLine("erorr");
                }
            while(true)
            { 
                byte[] packetRecive;
                packetRecive = udpReciever.Receive(ref recieverEndPoint);

                uint recive_ins_uint=0;
                uint recive_sns_uint = 0;
                try
                {
                    //сохроняем полученные данные
                    recive_ins_uint = BitConverter.ToUInt32(packetRecive, 48);
                    recive_sns_uint = BitConverter.ToUInt32(packetRecive, 52);
                }
                catch
                {

                }
                
                //сравниваем адресса и декодируем
                if ((recive_ins_uint & 0xFF) == 184)
                {
                    mtx.WaitOne();
                    INS recive_ins = ins.Get_Ins(packetRecive);
                    string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",CultureInfo.InvariantCulture);

                    Console.WriteLine($"{timestamp} - Получен пакет ИНС");

                    Console.WriteLine($"{timestamp} - Широта");
                    ins.latitude.show();

                    Console.WriteLine($"{timestamp} - Долгота");
                    ins.longitude.show();

                    Console.WriteLine($"{timestamp} - Высота");
                    ins.h.show();

                    Console.WriteLine($"{timestamp} - Курс истинный");
                    ins.coure_truth.show();

                    Console.WriteLine($"{timestamp} - Угол тангажа");
                    ins.pitch_angle.show();

                    Console.WriteLine($"{timestamp} - Угол крена");
                    ins.roll_angle.show();

                    Console.WriteLine($"{timestamp} - Скорость Север/Юг");
                    ins.speed_NS.show();

                    Console.WriteLine($"{timestamp} - Скорость Восток/Запад");
                    ins.speed_EW.show();

                    Console.WriteLine($"{timestamp} - Скорость вертикальная инерциальная");
                    ins.speed_vertical.show();

                    Console.WriteLine($"{timestamp} - Ускорение продольное, ax");
                    ins.acceleration_ax.show();

                    Console.WriteLine($"{timestamp} - Ускорение поперечное, az");
                    ins.acceleration_az.show();

                    Console.WriteLine($"{timestamp} - Ускорение нормальное, aY");
                    ins.acceleration_ay.show();

                    Console.WriteLine($"{timestamp} - Слово состояния ИНС");
                    ins.dis.show();

                    mtx.ReleaseMutex();
                }
                else if ((recive_sns_uint & 0xFF) == 187)
                {
                    mtx.WaitOne();

                    SNS recive_sns = sns.Get_Sns(packetRecive);

                    string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);

                    Console.WriteLine($"{timestamp} - Получен пакет СНС");

                    Console.WriteLine($"{timestamp} - Высота");
                    recive_sns.h.show();

                    Console.WriteLine($"{timestamp} - HDOP(горизонтальный)");
                    recive_sns.HDOP.show();

                    Console.WriteLine($"{timestamp} - VDOP(вертикальный)");
                    recive_sns.VDOP.show();

                    Console.WriteLine($"{timestamp} - Путевой угол");
                    recive_sns.course_angle.show();

                    Console.WriteLine($"{timestamp} - Текущая широта");
                    recive_sns.curr_latitude.show();

                    Console.WriteLine($"{timestamp} - Текущая широта (точно)");
                    recive_sns.curr_latitude_exactly.show();

                    Console.WriteLine($"{timestamp} - Текущая долгота");
                    recive_sns.curr_longitude.show();

                    Console.WriteLine($"{timestamp} - Текущаядолгота (точно)");
                    recive_sns.curr_longitude_exactly.show();

                    Console.WriteLine($"{timestamp} - Задержка выдачи обновленных НП относительно МВ");
                    recive_sns.delay.show();

                    Console.WriteLine($"{timestamp} - Текущее время UTC(старшие разряды)");
                    recive_sns.UTC_high.show();

                    Console.WriteLine($"{timestamp} - Текущее время UTC(младшие разряды)");
                    recive_sns.UTC_low.show();

                    Console.WriteLine($"{timestamp} - Высота");
                    recive_sns.h.show();

                    Console.WriteLine($"{timestamp} - Вертикальная скорость");
                    recive_sns.speed_vertical.show();

                    Console.WriteLine($"{timestamp} - Дата");
                    recive_sns.data.show();

                    Console.WriteLine($"{timestamp} - Структура слова признаки СРНС");
                    recive_sns.srns.show();

                    mtx.ReleaseMutex();
                }

            }
        }
        private static void Send_INS()
        {
            //Запускаем таймер для отправки ИНС
            System.Timers.Timer timer_sendIns = new System.Timers.Timer(10); //100 ГЦ
            timer_sendIns.Elapsed += Timer_sendIns_Elapsed;
            timer_sendIns.Enabled = true;
            timer_sendIns.AutoReset = true;
            timer_sendIns.Start();

        }
        private static void Timer_sendIns_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Отправляем данные ИНС с частотой 100Гц
            byte[] packetSend_ins = new byte[52];
            packetSend_ins = ins.Get_Bytes(ins);
            UDPSender.Send(packetSend_ins, packetSend_ins.Length, SendEndPoint);

        }
        private static void Send_SNS()
        {
            //Запускаем таймер для отправки СНС
            System.Timers.Timer timer_sendSns = new System.Timers.Timer(1000); //1 ГЦ
            timer_sendSns.Elapsed += Timer_sendSns_Elapsed;
            timer_sendSns.Enabled = true;
            timer_sendSns.AutoReset = true;
            timer_sendSns.Start();
        }
        private static void Timer_sendSns_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Отправляем данные СНС с частотой 1Гц
            byte[] packetSend_ins = new byte[56];
            packetSend_ins = sns.Get_Bytes(sns);
            UDPSender.Send(packetSend_ins, packetSend_ins.Length, SendEndPoint);
        }

        static void Fly()
        {
            while(ins.dis.GetNavig() != 1)
            {

            }

            System.Timers.Timer Fly_timer = new System.Timers.Timer(1);
            Fly_timer.Elapsed += Fly_timer_Elapsed;
            Fly_timer.Enabled = true;
            Fly_timer.AutoReset = true;
            Fly_timer.Start();

        }

        private static void Fly_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            mtx.WaitOne();
            double t = 0.001; //1 мс

            //равноускоренное движение
            la.Lon_m += ((la.a * Math.Pow(t * la.k, 2) / 2) + (la.V * t * la.k)) * Math.Cos(la.pitch) * Math.Sin(la.alpha);
            la.Lat_m += ((la.a * Math.Pow(t * la.k, 2) / 2) + (la.V * t * la.k)) * Math.Cos(la.pitch) * Math.Cos(la.alpha);

            if (la.V < 90)
            {
                la.V += la.a * t* la.k;
            }
            else
            {
                la.a = 0;
            }
            //набор высоты
            if (la.H < 4500)
            {
                la.H += ((la.a * Math.Pow(t*la.k, 2) / 2) + (la.V * t * la.k)) * Math.Sin(la.pitch);
            }
            else
            {
                la.pitch = 0;
            }

            mtx.ReleaseMutex();
        }



        static void OSP()
        {
            while (ins.dis.GetNavig() != 1)
            {

            }

            System.Timers.Timer OSP_timer = new System.Timers.Timer(1);
            OSP_timer.Elapsed += OSP_timer_Elapsed;
            OSP_timer.Enabled = true;
            OSP_timer.AutoReset = true;
            OSP_timer.Start ();

        }

        private static void OSP_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            

            mtx.WaitOne();
            double D = 0; //дальность
            double AB, AC, BC; //стороны треугольника для поиска угла
            double alpha_ideal = 0; //угол на ппм


            if (la.Curent_PPM < la.ppm.Length)
            {
                D = Math.Sqrt(Math.Pow((la.Lat_m - la.ppm[la.Curent_PPM].lat), 2) + Math.Pow((la.Lon_m - la.ppm[la.Curent_PPM].lon), 2));

                //ищем угол
                AB = Math.Pow(la.Lat_m - Coordinates.Degrees2km(90) * 1000, 2);
                AC = Math.Pow(la.Lat_m - la.ppm[la.Curent_PPM].lat, 2) + Math.Pow(la.Lon_m - la.ppm[la.Curent_PPM].lon, 2);
                BC = Math.Pow(Coordinates.Degrees2km(90) * 1000 - la.ppm[la.Curent_PPM].lat, 2) + Math.Pow(la.Lon_m - la.ppm[la.Curent_PPM].lon, 2);
                alpha_ideal = Math.Acos((AB + AC - BC) / (2 * Math.Sqrt(AB) * Math.Sqrt(AC)));

                if (la.Lon_m > la.ppm[la.Curent_PPM].lon)
                {
                    alpha_ideal *= -1;
                }

                //определяем в какую сторону надо довернуть
                if((alpha_ideal < la.alpha) && (2 * Math.PI + alpha_ideal - la.alpha <= la.alpha - alpha_ideal))
                {
                    alpha_ideal = 2 * Math.PI + alpha_ideal;

                    if(alpha_ideal> 2 * Math.PI)
                    {
                        alpha_ideal -= 2 * Math.PI;
                    }
                }
                else if ((alpha_ideal > la.alpha) && (2 * Math.PI - alpha_ideal + la.alpha < Math.Abs(la.alpha - alpha_ideal)))
                {
                    alpha_ideal = -2 * Math.PI + alpha_ideal;
                    if (alpha_ideal < -2 * Math.PI)
                    {
                        alpha_ideal += 2 * Math.PI;
                    }
                }
            }

            if ((alpha_ideal > la.alpha - la.viz_line_r) && (alpha_ideal < la.alpha + la.viz_line_r))
            {
                //поворачиваем визирную линиюв сторону ппм
                if ((Math.Abs(la.viz_line_y - alpha_ideal) < 0.1) || (Math.Abs(2 * Math.PI + la.viz_line_y - la.alpha)) < 0.1)
                {
                    la.viz_line_y = alpha_ideal;
                }
                else if ((la.viz_line_y + 0.1 <= alpha_ideal))
                {
                    la.viz_line_y += 0.02;

                    if (la.viz_line_y > 2 * Math.PI)
                    {
                        la.viz_line_y -= 2 * Math.PI;
                    }
                }
                else
                {
                    la.viz_line_y -= 0.02;

                    if (la.viz_line_y < -2 * Math.PI)
                    {
                        la.viz_line_y += 2 * Math.PI;
                    }
                }

                AB = Math.Pow(la.H - 0, 2) + Math.Pow(la.Lon_m - (la.ppm[la.Curent_PPM].lon - 10), 2); 
                AC = Math.Pow(la.Lon_m - la.ppm[la.Curent_PPM].lon, 2);
                BC = Math.Pow(la.H - 0, 2) + Math.Pow(la.ppm[la.Curent_PPM].lon - (la.ppm[la.Curent_PPM].lon - 10), 2); ;
                double alpha_z = Math.Acos((AB + AC - BC) / (2 * Math.Sqrt(AB) * Math.Sqrt(AC)));

                if (la.viz_line_z - 0.00314 > alpha_z)
                {
                    la.viz_line_z -= 0.00314; 
                }
                else if (la.viz_line_z + 0.00314 < alpha_z)
                {
                    la.viz_line_z += 0.00314;
                }
                else 
                {
                    la.viz_line_z = alpha_z;
                }

            }
            else
            {
                //поворачиваем самолёт в сторону ппм
                if ((Math.Abs(la.alpha - alpha_ideal) < 0.1) || (Math.Abs(2 * Math.PI + alpha_ideal - la.alpha)) < 0.1)
                {
                    la.alpha = alpha_ideal;
                }
                else if ((la.alpha + 0.1 <= alpha_ideal))
                {
                    la.alpha += 0.1;
                    la.viz_line_y += 0.1;

                    if (la.alpha > 2 * Math.PI)
                    {
                        la.alpha -= 2 * Math.PI;
                    }
                }
                else
                {
                    la.alpha -= 0.1;
                    la.viz_line_y -= 0.1;

                    if (la.alpha < -2 * Math.PI)
                    {
                        la.alpha += 2 * Math.PI;
                    }
                }
            }

            la.CalculateTHandA(); //считаем штилевой относ и время падения бомбы
            mtx.ReleaseMutex();
            if ((D < 10) && (la.Curent_PPM < la.ppm.Length) && (la.Curent_PPM > 0))
            {
                mtx.WaitOne();
                //сбрасываем бомбу
                //la.boombs[la.Curent_PPM - 1] = new Boombs(la);
                // la.boombs[la.Curent_PPM - 1].drop = true;

                Console.WriteLine($"{System.Environment.NewLine}PPM{la.Curent_PPM}");
                Console.WriteLine($"Angle: {la.viz_line_z * 180 / Math.PI}");
                Console.WriteLine($"D: {D}м ");
                //переключаемся на следующую ппм
                la.Curent_PPM++;

                mtx.ReleaseMutex();
            }
            
            


            if (la.Curent_PPM < la.ppm.Length)
            {
                //добавляем точку траектории ЛА
                point.Add(new PPM(la.Lon_m, la.Lat_m, la.H));
            }
            else
            {
                mtx.WaitOne();
                point.Add(new PPM(la.Lon_m, la.Lat_m, la.H));
                KML.AddRoad("fly", point, 'r',true); //добавляем путь в kml
                //la.Curent_PPM = 0;
                KML.CreateKml(Endfilename); //создаём файл kml
                Environment.Exit(0);
                mtx.ReleaseMutex();

            }

            

        }

        static void DropBoomb()
        {
            while (ins.dis.GetNavig() != 1)
            {

            }


            
            System.Timers.Timer Boomb_timer = new System.Timers.Timer(1);
            Boomb_timer.Elapsed += Boomb_timer_Elapsed;
            Boomb_timer.Enabled = true;
            Boomb_timer.AutoReset = true;
            Boomb_timer.Start();
        }

        private static void Boomb_timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           
            double t = 0.001; //1 мс;
            
            for (int i = 0; i < la.boombs.Length; i++)
            {
                
                if ((la.boombs[i].exploded == false) && (la.boombs[i].drop == true))
                {
                    
                    la.boombs[i].points.Add(new PPM(la.boombs[i].Lon_m, la.boombs[i].Lat_m, la.boombs[i].H));//добавляем точку траектории бомбы

                    //полёт бомбы
                    la.boombs[i].Lon_m += (la.boombs[i].V0 * t * la.k)  * Math.Sin(la.boombs[i].alpha);
                    la.boombs[i].Lat_m += (la.boombs[i].V0 * t * la.k)  * Math.Cos(la.boombs[i].alpha);
                    la.boombs[i].H += ((g * Math.Pow(t* la.k, 2) / 2) + (la.boombs[i].Vz * t * la.k)) * -1;

                    la.boombs[i].Vz += g * t * la.k;

                    if(la.boombs[i].H <= 0)
                    {
                        mtx.WaitOne();
                        la.boombs[i].exploded = true;//бомба взорвалась
                        KML.AddRoad($"Boomb{i}", la.boombs[i].points, 'w', true);//добавляем путь бомбы в kml
                        mtx.ReleaseMutex();

                    }
                    

                }
                else if (la.boombs[i].exploded)
                {
                    la.boombs[i].drop = false;
                    la.boombs[i].exploded = false;
                    mtx.WaitOne();
                    KML.AddPoint(new PPM(la.boombs[i].Lon_m, la.boombs[i].Lat_m, la.boombs[i].H), $"boomb{i+1}"); //добавляем точку падения бомбы в kml
                    //считаем ошибку
                    err += $"Error boomb{i + 1}: {Math.Sqrt(Math.Pow((la.boombs[i].Lat_m - la.ppm[i + 1].lat), 2) + Math.Pow((la.boombs[i].Lon_m - la.ppm[i + 1].lon), 2))}m{System.Environment.NewLine}";
                    err += $"Angle: {angle[i]}{System.Environment.NewLine}";
                    mtx.ReleaseMutex();

                    
                    if (i == la.boombs.Length - 1)
                    {
                        mtx.WaitOne();
                        Console.WriteLine(err);
                        KML.CreateKml(Endfilename); //создаём файл kml
                        Environment.Exit(0); //закрываем программу
                        mtx.ReleaseMutex();
                    }
                }
               
            }
        }

        public static void Main(string[] args)
        {
            UDPSender = new UdpClient();
            IPAddress ip = IPAddress.Parse(adr);
            //соединяем адрес и порт куда отправляем
            SendEndPoint = new IPEndPoint(ip, port);

            //поток для формирования данных ИНС
            ThreadStart t_ins = new ThreadStart(INS_Start);
            Thread thread_ins = new Thread(t_ins);
            thread_ins.Start();

            //поток для формирования данных СНС
            ThreadStart t_sns = new ThreadStart(SNS_Start);
            Thread thread_sns = new Thread(t_sns);
            thread_sns.Start();

            //поток для отправки инс
            ThreadStart t_send_ins = new ThreadStart(Send_INS);
            Thread thread_send_ins = new Thread(t_send_ins);
            thread_send_ins.Start();
            
            //поток для отправки снс
            ThreadStart t_send_sns = new ThreadStart(Send_SNS);
            Thread thread_send_sns = new Thread(t_send_sns);
            thread_send_sns.Start();

            //поток для полёта
            ThreadStart t_fly = new ThreadStart(Fly);
            Thread thread_fly= new Thread(t_fly);
            thread_fly.Start();

            //поток для ОСП
            ThreadStart t_osp = new ThreadStart(OSP);
            Thread thread_osp = new Thread(t_osp);
            thread_osp.Start();

            //поток для сброса бомб
            ThreadStart t_boom = new ThreadStart(DropBoomb);
            Thread thread_boom = new Thread(t_boom);
            thread_boom.Start();

            //запускаем функцию для приёма данных
           // Reciever();
            Console.ReadLine();
        }

    }
}
