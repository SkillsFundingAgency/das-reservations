﻿using System.Collections.Generic;

namespace SFA.DAS.Reservations.Web.Models
{
    public class GaData
    {
        
        public string DataLoaded { get; set; } = "dataLoaded";
        public string UserId { get; set; }

        public string Vpv { get; set; }
        public string Acc { get; set; }
        public string UkPrn { get; set; }

        public IDictionary<string, string> Extras { get; set; } = new Dictionary<string, string>();
    }
}