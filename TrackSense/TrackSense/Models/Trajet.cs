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
        public string ride_id { get; set; }

        public string nom { get; set; }

        public double? distance { get; set; }

        public double? vitesse_moyenne { get; set; }

        public string dateBegin { get; set; }

        public string dateEnd { get; set; }
        public long duration { get; set; }

        public bool estComplete { get; set; }
        public bool estReadyToSave { get; set; }

        public List<LocationPoint> points { get; set; }
        public List<LocationPoint> pointsdInteret { get; set; }
        public List<LocationPoint> pointsdDanger { get; set; }

        public Trajet(string p_id, string p_nom, double? p_distance, double? p_vitesse, string p_begin, string p_end, long p_duration, List<LocationPoint> p_points, List<LocationPoint> p_pointsInteret, List<LocationPoint> p_pointsDanger)
        {
            this.ride_id = p_id;
            this.nom = p_nom;
            this.distance = p_distance;
            this.vitesse_moyenne = p_vitesse;
            this.dateBegin = p_begin;
            this.dateEnd = p_end;
            this.duration = p_duration;
            this.points = p_points;
            this.pointsdInteret = p_pointsInteret;
            this.pointsdDanger = p_pointsDanger;
            this.estComplete = false;
            this.estReadyToSave = false;
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
