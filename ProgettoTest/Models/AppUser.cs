using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoTest.Models
{
    public class AppUser : IdentityUser
    {
        public int? Pace { get; set; }  // pace = ritmo
        public int? Mileage { get; set; }   // chilometraggio

        [ForeignKey("Address")]
        public int? AddressId { get; set; }
        public Address? Address { get; set; }
        public ICollection<Club> Clubs { get; set; }
        public ICollection<Race> Races { get; set; }
    }
}
