using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace TrackSense.Models
{
    /// <summary>
    /// 
    /// </summary>
    public class Trajet
    {
        public string Ride_id { get; set; }

        public string Nom { get; set; }

        public double? Distance { get; set; }

        public double? Vitesse_moyenne { get; set; }

        public string DateBegin { get; set; }

        public string DateEnd { get; set; }
        public long Duration { get; set; }

        public bool EstComplete { get; set; }
        public bool EstReadyToSave { get; set; }

        public List<LocationPoint> Points { get; set; }
        public List<LocationPoint> PointsdInteret { get; set; }
        public List<LocationPoint> PointsdDanger { get; set; }

        public Trajet(string p_id, string p_nom, double? p_distance, double? p_vitesse, string p_begin, string p_end, long p_duration, List<LocationPoint> p_points, List<LocationPoint> p_pointsInteret, List<LocationPoint> p_pointsDanger)
        {
            Ride_id = p_id;
            Nom = p_nom;
            Distance = p_distance;
            Vitesse_moyenne = p_vitesse;
            DateBegin = p_begin;
            DateEnd = p_end;
            Duration = p_duration;
            Points = p_points;
            PointsdInteret = p_pointsInteret;
            PointsdDanger = p_pointsDanger;
            EstComplete = false;
            EstReadyToSave = false;
        }

        public static Trajet FromPlannedRide2Trajet(PlannedRide p_plannedRide)
        {
            //TODO
            string id = p_plannedRide.PlannedRideId.ToString();
            string nom = p_plannedRide.PlannedRideName;
            double? distance = p_plannedRide.Distance;
            double? vitesse = 0.0;
            string begin = "";
            string end = "";
            long duration = (long)0.0;
            List<LocationPoint> points = p_plannedRide.FromPlannedRidePoints2LocationPoints();
            List<LocationPoint> pointsInteret = new List<LocationPoint>();
            List<LocationPoint> pointsDanger = new List<LocationPoint>();
            Trajet trajet = new Trajet(id,nom,distance,vitesse,begin,end,duration,points,pointsInteret,pointsDanger);
            return trajet;
        }

        public string FromTrajet2Json()
        {
            //TODO
            string JSON = JsonConvert.SerializeObject(this);
            return JSON;
        }





    }
}
