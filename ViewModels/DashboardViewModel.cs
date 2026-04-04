using TravelAgency_Secure.Models;

namespace TravelAgency_Secure.ViewModels
{
    public class DashboardViewModel
    {
        public List<Booking> UpcomingTrips { get; set; } = new();
        public List<Booking> PastTrips { get; set; } = new();
        public List<Booking> PendingBookings { get; set; } = new();
        public List<WaitingList> WaitingTrips { get; set; } = new();
    }
}
