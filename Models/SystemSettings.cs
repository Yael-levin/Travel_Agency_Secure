using System.ComponentModel.DataAnnotations;

namespace TravelAgency_Secure.Models
{
    public class SystemSettings
    {
        public int Id { get; set; }

        public int BookingDeadlineDays { get; set; }

        public int CancellationDeadlineDays { get; set; }

        public int ReminderDaysBeforeTrip { get; set; }
    }
}
