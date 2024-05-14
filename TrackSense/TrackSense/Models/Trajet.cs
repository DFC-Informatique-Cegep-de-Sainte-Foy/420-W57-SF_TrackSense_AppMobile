using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackSense.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Trajet
    {
        public string ride_id { get; set; }

        public string nom { get; set; }

        public double distance { get; set; }

        public double vitesse_moyenne { get; set; }

        public string dateBegin { get; set; }

        public string dateEnd { get; set; }
        public long duration { get; set; }

        public bool estComplete;
        public bool estReadyToSave;


        List<LocationPoint> points { get; set; }
        List<LocationPoint> pointsdInteret { get; set; }
        List<LocationPoint> pointsdDanger { get; set; }

        public Trajet(string p_id, string p_nom, double p_distance, double p_vitesse, string p_begin, string p_end, long p_duration, List<LocationPoint> p_points, List<LocationPoint> p_pointsInteret, List<LocationPoint> p_pointsDanger)
        {
            ride_id = p_id;
            nom = p_nom;
            distance = p_distance;
            vitesse_moyenne = p_vitesse;
            dateBegin = p_begin;
            dateEnd = p_end;
            duration = p_duration;
            points = p_points;
            pointsdInteret = p_pointsInteret;
            pointsdDanger = p_pointsDanger;
            estComplete = false;
            estReadyToSave = false;
        }


        public static Trajet fromPlannedRide2Trajet()
        {
            //TODO
            return null;
        }

        public string fromTrajet2Json()
        {
            //TODO
            return "";
        }





    }
}
