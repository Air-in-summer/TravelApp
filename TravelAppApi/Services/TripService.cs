using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Repository;

namespace TravelAppApi.Services
{
    // Service xử lý các nghiệp vụ liên quan đến Trip (chuyến đi)
    // Bao gồm: tạo mới, lấy danh sách, cập nhật, xóa trip
    public class TripService
    {
        // Repositories để tương tác với database
        private readonly TripRepository _tripRepository;
        private readonly DestinationRepository _destinationRepository;
        private readonly StopRepository _stopRepository;

        // Constructor - Dependency Injection các repository cần thiết

        public TripService(TripRepository tripRepository, DestinationRepository destinationRepository, StopRepository stopRepository)
        {
            _tripRepository = tripRepository;
            _destinationRepository = destinationRepository;
            _stopRepository = stopRepository;
        }

        // Tạo một chuyến đi mới
        // Tham số: dto - Thông tin chuyến đi từ client
        // Trả về: Chuyến đi vừa tạo (dạng TripModelFrontend)
        public async Task<TripModelFrontend> CreateTripAsync(AddTripDto dto)
        {
            // 1. Kiểm tra Destination tồn tại chưa
            var destinationExists = await _destinationRepository.GetDestinationAsync(dto.DestinationId);
            if(destinationExists == null)
            {
                await _destinationRepository.AddDestinationAsync(new Destinations
                {
                    Id = dto.DestinationId,
                    Name = dto.DestinationName,
                    MinLatitude = dto.MinLat,
                    MinLongitude = dto.MinLon,
                    MaxLatitude = dto.MaxLat,
                    MaxLongitude = dto.MaxLon
                });
            }

            // 2. Tạo Trip mới
            var newTrip = new Trip
            {
                Title = dto.Title,
                Description = dto.Description,
                DestinationId = dto.DestinationId,
                StartDate = dto.StartDate.Date, 
                EndDate = dto.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                Budget = dto.Budget,
                UserId = dto.UserId
            };
            newTrip = await _tripRepository.AddTripAsync(newTrip);
            

            // 3. Map sang TripModelFrontend
            var tripModel = new TripModelFrontend
            {
                Id = newTrip.Id,
                Title = newTrip.Title,
                Description = newTrip.Description,
                DestinationId = newTrip.DestinationId,
                DestinationName = dto.DestinationName, // hoặc lấy từ DB nếu muốn chắc chắn
                MinLat = dto.MinLat,
                MinLon = dto.MinLon,
                MaxLat = dto.MaxLat,
                MaxLon = dto.MaxLon,
                StartDate = newTrip.StartDate, // nếu StartDate là DateOnly
                EndDate = newTrip.EndDate,
                Budget = newTrip.Budget,
                UserId = newTrip.UserId,
                Stops = new ObservableCollection<StopModelFrontend>() // chưa có stop, nên rỗng
            };

            return tripModel;
        }

        // Lấy tất cả các chuyến đi của một user
        // Tham số: ownerId - ID của user cần lấy danh sách trip
        // Trả về: Danh sách các chuyến đi (TripModelFrontend)
        public async Task<IEnumerable<TripModelFrontend>> GetTripsByUserIdAsync(int ownerId)
        {
            List<TripModelFrontend> tripModels = new List<TripModelFrontend>();
            // Lấy tất cả trip của user từ database
            var trips = await _tripRepository.GetTripsByUserIdAsync(ownerId);

            // lấy destID từ mỗi trip, sau đó lấy thông tin dest từ DestinationRepository và map sang TripModelFrontend
            foreach (var trip in trips)
            {
                var destId = trip.DestinationId;
                var destination = await _destinationRepository.GetDestinationAsync(destId);

                var tripModel = new TripModelFrontend
                {
                    Id = trip.Id,
                    Title = trip.Title,
                    Description = trip.Description,
                    DestinationId = trip.DestinationId,
                    DestinationName = destination.Name,
                    MinLat = destination.MinLatitude,
                    MinLon = destination.MinLongitude,
                    MaxLat = destination.MaxLatitude,
                    MaxLon = destination.MaxLongitude,
                    StartDate = trip.StartDate,
                    EndDate = trip.EndDate,
                    Budget = trip.Budget,
                    UserId = trip.UserId,
                    Stops = new ObservableCollection<StopModelFrontend>()
                };

                tripModels.Add(tripModel);
            }

            return tripModels;
        }

        // Lấy chi tiết một chuyến đi theo ID (bao gồm cả danh sách stops)
        // Tham số: id - ID của chuyến đi
        // Trả về: Thông tin chi tiết chuyến đi
        public async Task<TripModelFrontend> GetTripsByIdAsync(int id)
        {
            // Lấy trip từ database kèm theo danh sách stops
            var trip = await _tripRepository.GetTripByIdAsync(id);
            var stopModels = new ObservableCollection<StopModelFrontend>();
            foreach (var stop in trip.Stops)
            {
                stopModels.Add(new StopModelFrontend
                {
                    Id = stop.Id,
                    Location = stop.Location.Name,
                    LocationId = stop.LocationId,
                    Category = stop.Location.Category,
                    Latitude = stop.Location.Latitude,
                    Longitude = stop.Location.Longitude,
                    Address = stop.Location.FormattedAddress,
                    ArrivalDate = stop.ArrivalDate,
                    DepartureDate = stop.DepartureDate,
                    EstimatedCost = stop.EstimatedCost,
                    Notes = stop.Notes,
                    TripId = stop.TripId
                });
            }
            if (trip == null)
            {
                return null;
            }
            var destId = trip.DestinationId;
            var destination = await _destinationRepository.GetDestinationAsync(destId);
            var tripModel = new TripModelFrontend
            {
                Id = trip.Id,
                Title = trip.Title,
                Description = trip.Description,
                DestinationId = trip.DestinationId,
                DestinationName = destination.Name,
                MinLat = destination.MinLatitude,
                MinLon = destination.MinLongitude,
                MaxLat = destination.MaxLatitude,
                MaxLon = destination.MaxLongitude,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Budget = trip.Budget,
                UserId = trip.UserId,
                Stops = stopModels
            };

            return tripModel;
        }

        // Cập nhật thông tin chuyến đi
        // Nếu StartDate thay đổi, tất cả stops sẽ được tịnh tiến theo
        // Tham số: id - ID của chuyến đi cần cập nhật, addTripDto - Thông tin mới của chuyến đi
        // Trả về: Chuyến đi sau khi cập nhật
        public async Task<TripModelFrontend> PutTripAsync(int id, AddTripDto addTripDto)
        {
            // Lấy trip hiện có từ database (bao gồm cả stops)
            var existingTrip = await _tripRepository.GetTripByIdAsync(id);
            if (existingTrip == null)
            {
                throw new Exception("Trip not found");
            }

            var oldStartDate = existingTrip.StartDate;
            var newStartDate = addTripDto.StartDate;

            // Nếu StartDate thay đổi → Tịnh tiến tất cả stops
            if (oldStartDate != newStartDate && existingTrip.Stops != null && existingTrip.Stops.Any())
            {
                var timeShift = newStartDate - oldStartDate;

                foreach (var stop in existingTrip.Stops)
                {
                    // Dịch chuyển cả ArrivalDate và DepartureDate cùng một khoảng
                    // Giữ nguyên thời lượng của stop
                    stop.ArrivalDate = stop.ArrivalDate.Add(timeShift).ToUniversalTime();
                    
                    stop.DepartureDate = stop.DepartureDate.Add(timeShift).ToUniversalTime();
                }

                // Cập nhật tất cả stops (EF Core sẽ track changes)
            }

            // Cập nhật thông tin trip
            existingTrip.Title = addTripDto.Title;
            existingTrip.Description = addTripDto.Description;
            existingTrip.StartDate = newStartDate;
            existingTrip.EndDate = addTripDto.EndDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            existingTrip.Budget = addTripDto.Budget;

            await _tripRepository.UpdateTripAsync(id, existingTrip);

            // Trả về TripModelFrontend đã cập nhật
            return await GetTripsByIdAsync(id);
        }

        // Xóa một chuyến đi
        // Tham số: id - ID của chuyến đi cần xóa
        public async Task DeleteTripAsync(int id)
        {
            await _tripRepository.DeleteTripAsync(id);
        }
    }
}
