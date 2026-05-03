using MediaApp2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MediaApp2.Services;

public interface IDataService
{
    Task<User?> AuthenticateAsync(string login, string password);
    Task<List<Equipment>> GetEquipmentAsync();
    Task<bool> CheckoutAsync(int equipmentId, int userId);
    Task<bool> ReturnAsync(int equipmentId);
    Task<Equipment> AddEquipmentAsync(string name);  // старый метод (для совместимости)
    Task<Equipment> AddEquipmentAsync(Equipment equipment);  // новый метод с объектом
    
    // Методы для бронирования помещений
    Task<List<RoomBooking>> GetRoomBookingsAsync();
    Task<bool> BookRoomAsync(RoomBooking booking);
    Task<bool> ApproveBookingAsync(int bookingId);
    Task<bool> RejectBookingAsync(int bookingId);
}