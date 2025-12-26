using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelApplication.Models;
using TravelApplication.Services;
namespace TravelApplication.ViewModels
{
    public partial class SuggestViewModel : BaseViewModel
    {
        int TripId { get; set; }
        public ObservableCollection<SelectableStop> Stops { get; } = new();
        private readonly TripService tripService;
        private readonly StopService stopService;
        private readonly SuggestService suggestService;
        [ObservableProperty]
        Trip currentTrip = default!;

        [ObservableProperty]
        bool useBudgetConstraint = true; // mặc định bật
        public SuggestViewModel(int tripId)
        {
            TripId = tripId;
            tripService = ServiceHelper.GetService<TripService>();
            stopService = ServiceHelper.GetService<StopService>();
            suggestService = ServiceHelper.GetService<SuggestService>();
        }

        public async Task LoadTripAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                CurrentTrip = await tripService.GetTripAsync(TripId);
                Stops.Clear();
                if (CurrentTrip?.Stops != null)
                {
                    foreach (var stop in CurrentTrip.Stops.OrderBy(s => s.ArrivalDate))
                    {
                        Stops.Add(new SelectableStop { Stop = stop, IsSelectedForSorting = true });
                    }
                }
                Title = CurrentTrip?.Title ?? "Trip Details";

            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load trip details: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task LoadSuggestAsync(List<Stop> stops)
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                //CurrentTrip = await tripService.GetTripAsync(TripId);
                Stops.Clear();

                foreach (var stop in stops.OrderBy(s => s.ArrivalDate))
                {
                    Stops.Add(new SelectableStop { Stop = stop, IsSelectedForSorting = true });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Failed to load trip details: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public async Task UpdateStopAsync(Stop stop)
        {
            stop.ArrivalDate = stop.ArrivalDate.ToUniversalTime();
            stop.DepartureDate = stop.DepartureDate.ToUniversalTime();
            await stopService.UpdateStopAsync(stop);
        }

        // Dành cho OnSuggestClicked gọi
        public List<Stop> GetSelectedStops() =>
            Stops.Where(s => s.IsSelectedForSorting).Select(s => s.Stop).ToList();

        public List<Stop> GetUnselectedStops() =>
            Stops.Where(s => !s.IsSelectedForSorting).Select(s => s.Stop).ToList();

        // Hàm chính để tiến hành gợi ý
        // selectedStops: những stop được chọn để sắp xếp
        // fixedStops: những stop không được sắp xếp (giữ nguyên vị trí)
        // currentTrip: chuyến đi hiện tại (để lấy ngày tháng, ngân sách, v.v.)
        // useBudget: có áp dụng giới hạn ngân sách không
        // totalBudget: tổng ngân sách (đã trừ đi chi phí cố định)
        public async Task<List<Stop>> ProceedWithSuggestion(List<Stop> selectedStops, List<Stop> fixedStops, Trip? currentTrip, bool useBudget, decimal totalBudget)
        {
            var allStops = selectedStops.Concat(fixedStops).ToList();

            // 🔹 Bước 1: Chuẩn hóa category cho mọi stop
            foreach (var stop in allStops)
            {
                stop.Category = NormalizeCategory(stop.Category); // ghi đè bằng nhóm chính
            }

            // 🔹 Bước 2: Phát hiện ý định
            var (isSpecialized, primaryCategory) = DetectTripType(allStops);

            // 🔹 Chuẩn hóa category cho selectedStops (vì stopsToSchedule sẽ từ đây)
            // → Đảm bảo stopsToSchedule dùng category đã chuẩn hóa
            foreach (var stop in selectedStops)
            {
                stop.Category = NormalizeCategory(stop.Category);
            }

            // 🔹 Bước 3: Lọc stop nếu cần (theo ngân sách)
            List<Stop> stopsToSchedule = selectedStops;

            if (useBudget && totalBudget >= 0) // totalBudget ở đây là availableForFlexible
            {
                if (totalBudget == 0)
                {
                    stopsToSchedule = new List<Stop>(); // không còn ngân sách cho flexible
                }
                else
                {
                    // Lọc stop có tổng chi phí <= totalBudget (availableForFlexible)
                    stopsToSchedule = FilterStopsWithinBudget(selectedStops, totalBudget);
                }
            }
            else
            {
                // ✅ Không dùng ngân sách → dùng toàn bộ selectedStops (đã chuẩn hóa)
                stopsToSchedule = new List<Stop>(selectedStops);
            }

            // 🔹 Bước 4: Sắp xếp thứ tự
            List<Stop> orderedFlexibleStops;
            if (!useBudget)
            {
                // 🟢 Trường hợp 1: Không có ràng buộc ngân sách
                if (isSpecialized)
                {
                    orderedFlexibleStops = suggestService.OptimizeSpecialized(stopsToSchedule);
                }
                else
                {
                    orderedFlexibleStops = suggestService.OptimizeBalanced(stopsToSchedule);
                }
            }
            else
            {
                // 🔴 (Sẽ triển khai sau) Trường hợp có ngân sách → TSP với ràng buộc
                if (isSpecialized)
                {
                    orderedFlexibleStops = suggestService.OptimizeSpecializedWithBudget(stopsToSchedule, totalBudget);
                }
                else
                {
                    orderedFlexibleStops = suggestService.OptimizeBalancedWithBudget(stopsToSchedule, totalBudget);
                }
            }

            // 🔹 Bước 5: Ghép với fixed stops 
            //var fullOrderedStops = MergeAndAssignTimeSlots(orderedFlexibleStops, fixedStops, currentTrip);
            var fullTimeline = MergeFlexibleWithFixed(orderedFlexibleStops, fixedStops, currentTrip, isSpecialized);

            return fullTimeline;
            // 🔹 Bước 7: Hiển thị preview
            //await LoadSuggestAsync(fullTimeline);
        }

        // Trong SuggestViewModel.cs
        private List<Stop> MergeFlexibleWithFixed(List<Stop> orderedFlexibleStops, List<Stop> fixedStops, Trip trip, bool isSpecialized)
        {
            // 1. Sắp xếp fixed stops theo thời gian
            var sortedFixed = fixedStops.OrderBy(s => s.ArrivalDate).ToList();

            // 2. Tạo danh sách timeline kết quả (bắt đầu bằng fixed stops)
            var timeline = new List<Stop>(sortedFixed);

            // 3. Xác định các khoảng trống (gaps) giữa các fixed stops
            var gaps = new List<(DateTime Start, DateTime End)>();

            if (sortedFixed.Count == 0)
            {
                // Không có fixed stop → toàn bộ trip là 1 gap
                gaps.Add((trip.StartDate, trip.EndDate.AddDays(1))); // +1 để bao gồm hết ngày cuối
            }
            else
            {
                // Gap trước fixed stop đầu tiên
                gaps.Add((trip.StartDate, sortedFixed[0].ArrivalDate));

                // Gap giữa các fixed stops
                for (int i = 0; i < sortedFixed.Count - 1; i++)
                {
                    gaps.Add((sortedFixed[i].DepartureDate, sortedFixed[i + 1].ArrivalDate));
                }

                // Gap sau fixed stop cuối cùng
                gaps.Add((sortedFixed.Last().DepartureDate, trip.EndDate.AddDays(1)));
            }

            // 4. Phân bổ flexible stops vào các gap hợp lệ
            var gapIndex = 0;
            var flexibleIndex = 0;

            while (flexibleIndex < orderedFlexibleStops.Count && gapIndex < gaps.Count)
            {
                var gap = gaps[gapIndex];

                // Bỏ qua gap không hợp lệ (thời gian âm)
                if (gap.End <= gap.Start)
                {
                    gapIndex++;
                    continue;
                }

                // Tính tổng thời lượng cần cho flexible stops còn lại
                var remainingFlexible = orderedFlexibleStops.Skip(flexibleIndex).ToList();
                var totalDurationNeeded = remainingFlexible.Sum(s => (s.DepartureDate - s.ArrivalDate).TotalMinutes);
                var minGapBetweenStops = 30; // phút
                var minTotalTimeNeeded = totalDurationNeeded + minGapBetweenStops * (remainingFlexible.Count - 1);

                var gapMinutes = (gap.End - gap.Start).TotalMinutes;

                if (gapMinutes >= minTotalTimeNeeded)
                {
                    // Đủ thời gian → gán toàn bộ flexible stops còn lại vào gap này
                    for (int i = flexibleIndex; i < orderedFlexibleStops.Count; i++)
                    {
                        var stop = orderedFlexibleStops[i];
                        var (newArrival, newDeparture) = AssignTimeInGap(
                            stop, gap.Start, gap.End, isSpecialized);

                        if (newArrival == DateTime.MinValue) break; // không còn chỗ

                        timeline.Add(new Stop
                        {
                            Id = stop.Id,
                            Location = stop.Location,
                            LocationId = stop.LocationId,
                            Category = stop.Category,
                            Latitude = stop.Latitude,
                            Longitude = stop.Longitude,
                            Address = stop.Address,
                            EstimatedCost = stop.EstimatedCost,
                            Notes = stop.Notes,
                            TripId = stop.TripId,
                            ArrivalDate = newArrival,
                            DepartureDate = newDeparture
                        });

                        // Cập nhật gap.Start cho stop tiếp theo
                        gap = (newDeparture.AddMinutes(minGapBetweenStops), gap.End);
                    }
                    break;
                }
                else
                {
                    // Không đủ thời gian → gán stop tiếp theo nếu có thể
                    var stop = orderedFlexibleStops[flexibleIndex];
                    var stopDuration = (stop.DepartureDate - stop.ArrivalDate).TotalMinutes;

                    if (gapMinutes >= stopDuration)
                    {
                        var (newArrival, newDeparture) = AssignTimeInGap(
                            stop, gap.Start, gap.End, isSpecialized);

                        if (newArrival != DateTime.MinValue)
                        {
                            timeline.Add(new Stop
                            {
                                Id = stop.Id,
                                Location = stop.Location,
                                LocationId = stop.LocationId,
                                Category = stop.Category,
                                Latitude = stop.Latitude,
                                Longitude = stop.Longitude,
                                Address = stop.Address,
                                EstimatedCost = stop.EstimatedCost,
                                Notes = stop.Notes,
                                TripId = stop.TripId,
                                ArrivalDate = newArrival,
                                DepartureDate = newDeparture
                            });
                            flexibleIndex++;
                            // Cập nhật gap cho lần lặp tiếp theo
                            gaps[gapIndex] = (newDeparture.AddMinutes(minGapBetweenStops), gap.End);
                        }
                        else
                        {
                            gapIndex++; // chuyển sang gap tiếp theo
                        }
                    }
                    else
                    {
                        gapIndex++; // gap quá nhỏ, chuyển sang gap tiếp
                    }
                }
            }

            return timeline.OrderBy(s => s.ArrivalDate).ToList();
        }

        private (DateTime Arrival, DateTime Departure) AssignTimeInGap(Stop stop, DateTime gapStart, DateTime gapEnd, bool isSpecialized)
        {
            var originalDuration = stop.DepartureDate - stop.ArrivalDate;

            // Lấy time window hợp lý
            var (windowStart, windowEnd) = GetTimeWindow(stop.Category, isSpecialized);

            // Điều chỉnh thời gian bắt đầu theo time window
            var candidateStart = AdjustToTimeWindow(gapStart, windowStart, windowEnd, gapEnd);

            if (candidateStart >= gapEnd)
                return (DateTime.MinValue, DateTime.MinValue); // không còn chỗ

            var candidateEnd = candidateStart + originalDuration;

            // Đảm bảo không vượt quá gap
            if (candidateEnd > gapEnd)
            {
                // Nếu duration quá dài, cố gắng cắt ngắn
                if ((gapEnd - candidateStart).TotalMinutes < 15) // ít nhất 15 phút
                    return (DateTime.MinValue, DateTime.MinValue);
                candidateEnd = gapEnd;
            }

            return (candidateStart, candidateEnd);
        }

        private (TimeOnly Start, TimeOnly End) GetTimeWindow(string category, bool isSpecialized)
        {
            return category switch
            {
                "stay" => (new(21, 0), new(9, 0)), // qua đêm
                "food" => isSpecialized
                    ? (new(9, 0), new(24, 0))
                    : (new(12, 0), new(20, 0)),
                "attraction" => (new(9, 0), new(16, 0)),
                "other" => (new(9, 0), new(16, 0)),
                "nightlife" => (new(20, 0), new(2, 0)),
                _ => (new(9, 0), new(20, 0))
            };
        }

        private DateTime AdjustToTimeWindow(DateTime candidate, TimeOnly windowStart, TimeOnly windowEnd, DateTime gapEnd)
        {
            var date = candidate.Date;

            // Xử lý time window qua đêm (stay)
            if (windowEnd < windowStart)
            {
                var tonightStart = date + windowStart.ToTimeSpan();
                if (tonightStart <= gapEnd)
                    return tonightStart;
                else
                    return date.AddDays(-1) + windowStart.ToTimeSpan();
            }

            // Time window bình thường
            var windowStartDT = date + windowStart.ToTimeSpan();
            var windowEndDT = date + windowEnd.ToTimeSpan();

            if (candidate >= windowStartDT && candidate <= windowEndDT)
                return candidate;

            if (candidate < windowStartDT)
                return windowStartDT <= gapEnd ? windowStartDT : gapEnd;

            // candidate > windowEndDT → qua ngày hôm sau
            var nextDayStart = date.AddDays(1) + windowStart.ToTimeSpan();
            return nextDayStart <= gapEnd ? nextDayStart : gapEnd;
        }

        private string NormalizeCategory(string geoapifyCategory)
        {
            if (string.IsNullOrWhiteSpace(geoapifyCategory))
                return "other";

            // Lấy phần trước dấu chấm (main category)
            string mainCategory = geoapifyCategory.Split('.')[0].ToLowerInvariant();
            if (mainCategory == "stay" || mainCategory == "food" || mainCategory == "attraction")
            {
                return mainCategory;
            }
            // Tra từ điển
            if (CategoryMapping.CategoryMapper.TryGetValue(mainCategory, out string normalized))
            {
                return normalized;
            }

            // Mặc định: coi là attraction (hoặc "other" nếu bạn muốn nghiêm ngặt hơn)
            return "other";
        }

        private (bool IsSpecialized, string PrimaryCategory) DetectTripType(List<Stop> stops)
        {
            if (stops.Count == 0)
                return (false, "any");

            // 🔹 Lọc bỏ "other"
            var relevantStops = stops.Where(s => s.Category != "other").ToList();

            if (relevantStops.Count == 0)
                return (false, "any"); // không có stop nào liên quan → coi là tổng hợp

            var totalRelevant = relevantStops.Count;

            var dominantGroup = relevantStops
                .GroupBy(s => s.Category)
                .OrderByDescending(g => g.Count())
                .First();

            bool isSpecialized = (double)dominantGroup.Count() / totalRelevant >= 0.75;
            return (isSpecialized, dominantGroup.Key);
        }

        private List<Stop> FilterStopsWithinBudget(List<Stop> stops, decimal maxBudget)
        {
            if (maxBudget < 0 || stops.Count == 0)
                return new List<Stop>();

            // ✅ CHỈ loại bỏ stop nào CÓ CHI PHÍ LỚN HƠN maxBudget
            // → Vì dù stop đó có "giá trị", nó cũng không thể được chọn nếu cost > ngân sách còn lại
            var result = stops.Where(stop => stop.EstimatedCost <= maxBudget).ToList();

            return result;
        }
    }
}