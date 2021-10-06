using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COVIDCheckinWebApp.Models
{
    public class VenueDetail
    {
        public int venueId { get; set; }
        public string venueName { get; set; }
        public string venueAddress { get; set; }
        public string venuePhone { get; set; }
        public string ownerName { get; set; }
        public string ownerPhone { get; set; }
        public CustomerDetail personAffected { get; set; }
        public DateTime logDate { get; set; }
    }
}
