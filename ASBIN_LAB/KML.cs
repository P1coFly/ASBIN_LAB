using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;
using System.IO;
using System.Xml;
using System.Linq;

namespace ASBIN_LAB
{
    public static class Coordinates // перевод градусы в км и обратно
    {
        const double R = 6378.1; // Радиус Земли
        public static double Degrees2km(double x)
        {
            double L = Math.PI * R * x / 180; // длина дуги окружности в км
            return L;
        }

        public static double km2Degress(double L)
        {
            double x = L * 180 / (R * Math.PI);
            return x;
        }

    }

    public static class KML
    {
        public static string kmlstruct;
        public static PPM[] GetsPoitsKMLToPPM(string filename)
        {
            double[] point;
            KmlFile file;
            FileStream stream;
            using (stream = File.Open(filename, FileMode.Open))
            {
                file = KmlFile.Load(stream);
            }
            var split = StyleResolver.SplitStyles(file.Root);
            var serializer = new Serializer();
            serializer.Serialize(split);
            string pmm = serializer.Xml;
            kmlstruct = pmm;
            kmlstruct = kmlstruct.Substring(0, kmlstruct.IndexOf("<Placemark>"));
            kmlstruct += $" {System.Environment.NewLine}";
            int pos = pmm.LastIndexOf("</coordinates>");
            int pos1 = pmm.IndexOf("s>");
            pmm = pmm.Substring(0, pos);
            pmm = pmm.Substring(pos1 + 2);
            pmm = pmm.Replace(Environment.NewLine, ",");

            point = pmm.Split(',').Select(n => Convert.ToDouble(n)).ToArray();

            PPM[] ppm = new PPM[(int)point.Length / 3];

            int l = 0;
            for (int i = 0; i < point.Length; i+=3)
            {
                ppm[l] = new PPM(Coordinates.Degrees2km(point[i]) * 1000, Coordinates.Degrees2km(point[i + 1]) * 1000,point[i+2]* 1000); //km2m
                l++;
            }
            stream.Close();
            
            return ppm;
        }

        public static void CreateKml(string filename)
        {
            kmlstruct = kmlstruct.Replace("test.kml", "end.kml");
            kmlstruct = kmlstruct.Replace("ff00ffff", "ff0000ff");
            kmlstruct += $"</Document>{System.Environment.NewLine}</kml>";
            System.IO.File.WriteAllText(filename, kmlstruct);
        }

        public static void AddRoad(string name, List<PPM> points, char color, bool relativeToGround)
        {
            kmlstruct += $"{System.Environment.NewLine}<Placemark> {System.Environment.NewLine}";
            kmlstruct += $"<name>{name}</name> {System.Environment.NewLine}";

            if(color == 'r')
            {
                kmlstruct += $"<styleUrl>#m_ylw-pushpin</styleUrl> {System.Environment.NewLine}";
            }
            else 
            {
                kmlstruct += $"<styleUrl>#m_ylw-pushpin123</styleUrl> {System.Environment.NewLine}";
            }

            kmlstruct += $"<LineString> {System.Environment.NewLine}<tessellate> 1 </tessellate> {System.Environment.NewLine}";
            if(relativeToGround)
            {
              // kmlstruct+= $"<altitudeMode>relativeToGround</altitudeMode>{System.Environment.NewLine}";
            }
            kmlstruct += $"<coordinates> {System.Environment.NewLine}";

            for (int i = points.Count - 1; i > -1; i--)
            {
                kmlstruct += $"{Coordinates.km2Degress(points[i].lon / 1000)},{Coordinates.km2Degress(points[i].lat / 1000)},{points[i].h}{System.Environment.NewLine}";
            }

            kmlstruct += $"</coordinates> {System.Environment.NewLine} </LineString> {System.Environment.NewLine}</Placemark>";
        }

        public static void AddPoint (PPM ppm, string name)
        {
            kmlstruct += $"{System.Environment.NewLine}<Placemark> {System.Environment.NewLine}";
            kmlstruct += $"<name>{name}</name> {System.Environment.NewLine}";
            kmlstruct += $"<styleUrl>#m_ylw-pushpin123</styleUrl>";
            kmlstruct += $"<Point> {System.Environment.NewLine}<coordinates>{Coordinates.km2Degress(ppm.lon/1000)},{Coordinates.km2Degress(ppm.lat/1000)},0</coordinates>{System.Environment.NewLine}";
            kmlstruct += $"</Point>{System.Environment.NewLine}</Placemark>";
        }
       
    }
}
