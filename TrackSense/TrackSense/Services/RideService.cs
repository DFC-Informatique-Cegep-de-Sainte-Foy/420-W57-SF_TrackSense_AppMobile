﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackSense.Data;

namespace TrackSense.Services
{
    public class RideService
    {
        private RideData _rideData;

        public RideService(RideData rideData)
        {
            _rideData = rideData;
        }

        internal void ReceiveRideData(string rideData)
        {
            //Enregistrer le rideData dans RideData
            Console.WriteLine(rideData);
            //Si connexion à internet, envoyer vers API
        }
    }
}