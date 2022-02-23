using System;
using System.Collections.Generic;
using System.Text;

namespace ASBIN_LAB
{
    public class LA
    {
        const double g = 9.8;

        public  double k { get; private set; } = 120; //коэфициент ускорения
        public double Lat_m { get; set; } = 0; //широта в метрах
        public double Lon_m { get; set; } = 0; // долгота в метрах
        public double H { get; set; } = 0; // долгота
        public double V { get; set; } = 75; //скорость ЛА
        public double a { get; set; } = 5; //ускорение ЛА

        public double viz_line_r { get; private set; } = Math.PI / 64;
        public double viz_line_y { get; set; } = 0;
        public double viz_line_z { get; set; } = 0;
        public double alpha { get; set; } = 0; //курс
        public double pitch { get; set; } = 30 * Math.PI / 180.0;// угол тангажа

        
        public int Curent_PPM { get; set; } = 1; // 0 - нач положение ЛА

        public double TH;
        public double A;
        public PPM[] ppm;
        public Boombs[] boombs;

        public LA (string f)
        {
            ppm = KML.GetsPoitsKMLToPPM(f);
            for (int i = 0; i < ppm.Length; i++)
            {
                KML.AddPoint(ppm[i], $"ppm{i}");
            }

            boombs = new Boombs[ppm.Length - 1];
            for (int i = 0; i < boombs.Length; i++)
            {
                boombs[i] = new Boombs(this);
            }
            Lat_m = ppm[0].lat;
            Lon_m = ppm[0].lon;

        }

        public void CalculateTHandA()
        {
            A = V * Math.Sqrt(2 * H / g);
            TH = Math.Sqrt(2 * H / g);
        }

    }

    public class PPM
    {
       public double h;
       public double lon; // долгота (х)
       public double lat; // широта (у)

        public PPM(double x, double y,double h)
        {
            lon = x;
            lat = y;
            this.h = h;
        }
    }

    public class Boombs
    {
        public double Lat_m; //широта
        public double Lon_m; //долгта
        public double H; //высота
        public double V0; // нач скорость
        public double A; //относ
        public double TH;
        public double alpha;
        public double Vz=0;

        public bool drop = false; //сборс
        public bool exploded = false; //взрыв

        public List<PPM> points = new List<PPM>();

        public Boombs(LA la)
        {
            Lat_m = la.Lat_m;
            Lon_m = la.Lon_m;
            H = la.H;
            V0 = la.V;
            alpha = la.alpha;
        }

    }
}
