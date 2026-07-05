namespace FlightBookingSystem.Repositories.Interfaces
{
    public interface IDeleteRepository
    {
        Task DeleteFlightAsync(int flightId);
        Task DeleteBookingAsync(int bookingId);
    }
}
