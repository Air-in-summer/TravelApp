using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TravelAppApi.Data;
using TravelAppApi.Models;
using TravelAppApi.Repository;

namespace TravelAppApi.Services
{
    // Service xử lý các nghiệp vụ liên quan đến Stop (điểm dừng)
    public class StopService
    {
        private readonly StopRepository _stopRepository;
        private readonly PlaceRepository _placeRepository;
        public StopService(StopRepository stopRepository, PlaceRepository placeRepository)
        {
            _stopRepository = stopRepository;
            _placeRepository = placeRepository;
        }

        // Tạo một điểm dừng mới
        // Kiểm tra Place tồn tại, nếu chưa có thì tạo mới
        public async Task<StopModelFrontend> CreateStopAsync(AddStopDto addStopDto)
        {
            //1. Kiểm tra Place tồn tại chưa
            var placeExists = await _placeRepository.GetPlaceAsync(addStopDto.LocationId);
            if (placeExists == null)
            {
                await _placeRepository.AddPlaceAsync(new Places
                {
                    Id = addStopDto.LocationId,
                    Name = addStopDto.Location,
                    Category = addStopDto.Category,
                    Latitude = addStopDto.Latitude,
                    Longitude = addStopDto.Longitude,
                    FormattedAddress = addStopDto.Address
                });
            }

            //2. Tạo Stop mới
            var newStop = new Stop
            {
                LocationId = addStopDto.LocationId,               
                ArrivalDate = addStopDto.ArrivalDate,
                DepartureDate = addStopDto.DepartureDate,
                EstimatedCost = addStopDto.EstimatedCost,
                Notes = addStopDto.Notes,
                TripId = addStopDto.TripId
            };

            newStop = await _stopRepository.AddStopAsync(newStop);

            //3. Map sang StopModelFrontend
            var stopModel = new StopModelFrontend
            {
                Id = newStop.Id,
                Location = addStopDto.Location,
                LocationId = newStop.LocationId,
                Category = addStopDto.Category,
                Latitude = addStopDto.Latitude,
                Longitude = addStopDto.Longitude,
                Address = addStopDto.Address,
                ArrivalDate = newStop.ArrivalDate,
                DepartureDate = newStop.DepartureDate,
                EstimatedCost = newStop.EstimatedCost,
                Notes = newStop.Notes,
                TripId = newStop.TripId
            };

            return stopModel;

        }

        // Lấy tất cả các điểm dừng của một chuyến đi
        public async Task<IEnumerable<StopModelFrontend>> GetStopsByTripIdAsync(int tripId)
        {
            List<StopModelFrontend> stopModels = new List<StopModelFrontend>();
            // Lấy danh sách Stop từ repository
            var stops = await _stopRepository.GetStopsByTripIdAsync(tripId);

            // lấy thông tin Place cho mỗi Stop và map sang StopModelFrontend
            foreach (var stop in stops)
            {
                var placeId = stop.LocationId;
                var place = await _placeRepository.GetPlaceAsync(placeId);

                var stopModel = new StopModelFrontend
                {
                    Id = stop.Id,
                    Location = place.Name,
                    LocationId = stop.LocationId,
                    Category = place.Category,
                    Latitude = place.Latitude,
                    Longitude = place.Longitude,
                    Address = place.FormattedAddress,
                    ArrivalDate = stop.ArrivalDate,
                    DepartureDate = stop.DepartureDate,
                    EstimatedCost = stop.EstimatedCost,
                    Notes = stop.Notes,
                    TripId = stop.TripId
                };
                stopModels.Add(stopModel);
            }

            return stopModels;

        }

        // Lấy thông tin chi tiết một điểm dừng theo ID
        public async Task<StopModelFrontend> GetStopByIdAsync(int id)
        {
            var stop = await _stopRepository.GetStopByIdAsync(id);
            if (stop == null)
            {
                throw new Exception("Điểm dừng không tồn tại");
            }
            var place = await _placeRepository.GetPlaceAsync(stop.LocationId);
            var stopModel = new StopModelFrontend
            {
                Id = stop.Id,
                Location = place.Name,
                LocationId = stop.LocationId,
                Category = place.Category,
                Latitude = place.Latitude,
                Longitude = place.Longitude,
                Address = place.FormattedAddress,
                ArrivalDate = stop.ArrivalDate,
                DepartureDate = stop.DepartureDate,
                EstimatedCost = stop.EstimatedCost,
                Notes = stop.Notes,
                TripId = stop.TripId
            };
            return stopModel;
        }

        // Cập nhật thông tin điểm dừng
        public async Task<StopModelFrontend> PutStopAsync(int id, AddStopDto addStopDto)
        {
            var stopModel = await GetStopByIdAsync(id);
            stopModel.ArrivalDate = addStopDto.ArrivalDate;
            stopModel.DepartureDate = addStopDto.DepartureDate;
            stopModel.EstimatedCost = addStopDto.EstimatedCost;
            stopModel.Notes = addStopDto.Notes;
            await _stopRepository.UpdateStopAsync(id, stopModel);
            return stopModel;
        }

        // Xóa một điểm dừng
        public async Task DeleteStopAsync(int id)
        {
            await _stopRepository.DeleteStopAsync(id);
        }
    }
}
