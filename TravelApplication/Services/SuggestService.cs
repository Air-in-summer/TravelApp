using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TravelApplication.Models;
namespace TravelApplication.Services
{
    public class SuggestService
    {
        public SuggestService()
        {

        }

        private double CalculateDistance(Stop a, Stop b)
        {
            const double R = 6371; // Bán kính Trái Đất (km)
            var φ1 = a.Latitude * Math.PI / 180;
            var φ2 = b.Latitude * Math.PI / 180;
            var Δφ = (b.Latitude - a.Latitude) * Math.PI / 180;
            var Δλ = (b.Longitude - a.Longitude) * Math.PI / 180;

            var haversine = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                            Math.Cos(φ1) * Math.Cos(φ2) *
                            Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(haversine), Math.Sqrt(1 - haversine));
            return R * c; // km
        }

        // Tiền tính toán ma trận khoảng cách
        private double[,] PrecomputeDistanceMatrix(List<Stop> stops)
        {
            int n = stops.Count;
            var dist = new double[n, n];
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                    dist[i, j] = i == j ? 0 : CalculateDistance(stops[i], stops[j]);
            return dist;
        }

        private List<int> NearestNeighbor(int n, double[,] dist, int start = 0)
        {
            var visited = new bool[n];
            var path = new List<int> { start };
            visited[start] = true;
            int current = start;

            for (int i = 1; i < n; i++)
            {
                int next = -1;
                double minDist = double.MaxValue;
                for (int j = 0; j < n; j++)
                {
                    if (!visited[j] && dist[current, j] < minDist)
                    {
                        minDist = dist[current, j];
                        next = j;
                    }
                }
                if (next == -1) break;
                path.Add(next);
                visited[next] = true;
                current = next;
            }
            return path;
        }

        private (List<int> path, double cost) TwoOpt(List<int> path, double[,] dist)
        {
            var bestPath = new List<int>(path);
            double bestCost = CalculatePathCost(bestPath, dist);
            bool improved = true;

            while (improved)
            {
                improved = false;
                for (int i = 0; i < bestPath.Count - 2; i++)
                {
                    for (int j = i + 2; j < bestPath.Count; j++)
                    {
                        // Đảo đoạn [i+1, j]
                        var newPath = new List<int>(bestPath);
                        newPath.Reverse(i + 1, j - i);
                        double newCost = CalculatePathCost(newPath, dist);

                        if (newCost < bestCost)
                        {
                            bestPath = newPath;
                            bestCost = newCost;
                            improved = true;
                            // thoát sớm để restart vòng while (tránh local minima)
                            goto NextIteration;
                        }
                    }
                }   
                NextIteration:;
            }

            return (bestPath, bestCost);
        }

        private double CalculatePathCost(List<int> path, double[,] dist)
        {
            double cost = 0;
            for (int i = 0; i < path.Count - 1; i++)
                cost += dist[path[i], path[i + 1]];
            return cost;
        }

        public List<Stop> OptimizeSpecialized(List<Stop> stops)
        {
            if (stops.Count <= 1) return stops;
            if (stops.Count == 2) return stops;

            int n = stops.Count;
            var dist = PrecomputeDistanceMatrix(stops);

            List<int> bestPath = null;
            double bestCost = double.MaxValue;

            // 🔁 Chạy Nearest Neighbor từ MỌI điểm làm điểm bắt đầu
            for (int start = 0; start < n; start++)
            {
                var nnPath = NearestNeighbor(n, dist, start);
                var (optimizedPath, cost) = TwoOpt(nnPath, dist); // dùng TwoOpt thuần (chỉ khoảng cách)

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestPath = optimizedPath;
                }
            }

            return bestPath.Select(idx => stops[idx]).ToList();
        }

        private double CalculateBalancedCost(List<int> path, double[,] dist, List<Stop> stops, double avgDistance)
        {
            double totalDistance = 0;
            double categoryPenalty = 0;
            double PENALTY_SAME_CATEGORY = avgDistance * 0.5; // ~50km penalty cho mỗi cặp cùng loại liên tiếp

            for (int i = 0; i < path.Count - 1; i++)
            {
                int u = path[i], v = path[i + 1];
                totalDistance += dist[u, v];

                if (stops[u].Category == stops[v].Category)
                    categoryPenalty += PENALTY_SAME_CATEGORY;
            }

            return totalDistance + categoryPenalty;
        }

        private (List<int> path, double cost) TwoOptBalanced(List<int> path, double[,] dist, List<Stop> stops, double avgDistance)
        {
            var bestPath = new List<int>(path);
            double bestCost = CalculateBalancedCost(bestPath, dist, stops, avgDistance);
            bool improved = true;

            while (improved)
            {
                improved = false;
                for (int i = 0; i < bestPath.Count - 2; i++)
                {
                    for (int j = i + 2; j < bestPath.Count; j++)
                    {
                        var newPath = new List<int>(bestPath);
                        newPath.Reverse(i + 1, j - i);
                        double newCost = CalculateBalancedCost(newPath, dist, stops, avgDistance);

                        if (newCost < bestCost)
                        {
                            bestPath = newPath;
                            bestCost = newCost;
                            improved = true;
                            goto NextIteration;
                        }
                    }
                }
                NextIteration:;
            }

            return (bestPath, bestCost);
        }

        public List<Stop> OptimizeBalanced(List<Stop> stops)
        {
            if (stops.Count <= 1) return stops;
            if (stops.Count == 2) return stops;

            int n = stops.Count;
            var dist = PrecomputeDistanceMatrix(stops);

            double avgDistance = 0;
            
            int count = 0;
            for (int i = 0; i < n; i++)
                for (int j = i + 1; j < n; j++)
                {
                    avgDistance += dist[i, j];
                    count++;
                }
            avgDistance /= count;

            List<int> bestPath = null;
            double bestCost = double.MaxValue;

            // 🔁 Chạy NN từ MỌI điểm bắt đầu
            for (int start = 0; start < n; start++)
            {
                var nnPath = NearestNeighbor(n, dist, start);
                var (optimizedPath, cost) = TwoOptBalanced(nnPath, dist, stops, avgDistance);

                if (cost < bestCost)
                {
                    bestCost = cost;
                    bestPath = optimizedPath;
                }
            }

            return bestPath.Select(idx => stops[idx]).ToList();
        }

        private (List<Stop> candidates, double[] costs) PrepareBudgetData(List<Stop> stopsToSchedule, decimal totalBudget)
        {
            // Loại bỏ stop có chi phí > totalBudget (không thể chọn)
            var candidates = stopsToSchedule
                .Where(s => s.EstimatedCost <= totalBudget)
                .ToList();

            var costs = candidates.Select(s => (double)s.EstimatedCost).ToArray();

            return (candidates, costs);
        }

        private List<Stop> RunBudgetGA(List<Stop> candidates, double[] costs, decimal totalBudget, bool isSpecialized, int populationSize = 100, int generations = 200)
        {
            var random = new Random();
            int n = candidates.Count;
            double budget = (double)totalBudget;

            // Hàm tạo cá thể hợp lệ
            List<int> CreateIndividual()
            {
                var indices = new List<int>();
                double totalCost = 0;
                var available = Enumerable.Range(0, n).ToList();
                while (available.Count > 0)
                {
                    int idx = available[random.Next(available.Count)];
                    if (totalCost + costs[idx] <= budget)
                    {
                        indices.Add(idx);
                        totalCost += costs[idx];
                    }
                    available.Remove(idx);
                }
                return indices;
            }

            // Hàm đánh giá: tổng khoảng cách (đã sắp xếp)
            double Evaluate(List<int> indices)
            {
                if (indices.Count == 0) return double.MaxValue;

                var subStops = indices.Select(i => candidates[i]).ToList();
                var orderedStops = isSpecialized
                    ? OptimizeSpecialized(subStops)
                    : OptimizeBalanced(subStops);

                // Tính tổng khoảng cách
                double totalDist = 0;
                for (int i = 0; i < orderedStops.Count - 1; i++)
                {
                    totalDist += CalculateDistance(orderedStops[i], orderedStops[i + 1]);
                }
                return totalDist;
            }

            // Khởi tạo quần thể
            var population = new List<List<int>>();
            for (int i = 0; i < populationSize; i++)
            {
                population.Add(CreateIndividual());
            }

            List<int> bestIndividual = null;
            double bestFitness = double.MaxValue;

            // Tiến hóa
            for (int gen = 0; gen < generations; gen++)
            {
                // Đánh giá & chọn elite
                population = population
                    .Where(ind => ind.Count > 0)
                    .OrderBy(ind => Evaluate(ind))
                    .ToList();

                if (population.Count == 0) break;

                var currentBest = population[0];
                double currentFitness = Evaluate(currentBest);
                if (currentFitness < bestFitness)
                {
                    bestFitness = currentFitness;
                    bestIndividual = new List<int>(currentBest);
                }

                // Tạo thế hệ mới
                var newPopulation = new List<List<int>>();
                int eliteSize = Math.Max(1, populationSize / 10);
                for (int i = 0; i < eliteSize; i++)
                {
                    newPopulation.Add(new List<int>(population[i]));
                }

                while (newPopulation.Count < populationSize)
                {
                    // Chọn cha mẹ (từ elite)
                    var parent1 = population[random.Next(eliteSize)];
                    var parent2 = population[random.Next(eliteSize)];

                    // Lai ghép: hợp nhất + lọc theo ngân sách
                    var childSet = new HashSet<int>(parent1);
                    childSet.UnionWith(parent2);
                    var child = childSet.ToList();

                    // Đột biến: xóa hoặc thêm
                    // Xóa ngẫu nhiên
                    if (child.Count > 0 && random.NextDouble() < 0.3)
                    {
                        child.RemoveAt(random.Next(child.Count));
                    }

                    // Thêm ngẫu nhiên (nếu còn ngân sách)
                    double currentCost = child.Sum(i => costs[i]);
                    var canAdd = Enumerable.Range(0, n)
                        .Where(i => !child.Contains(i) && currentCost + costs[i] <= budget)
                        .ToList();
                    if (canAdd.Count > 0 && random.NextDouble() < 0.4)
                    {
                        child.Add(canAdd[random.Next(canAdd.Count)]);
                    }

                    // Đảm bảo hợp lệ
                    if (child.Count > 0)
                    {
                        newPopulation.Add(child);
                    }
                }

                population = newPopulation;
            }

            return bestIndividual?.Select(i => candidates[i]).ToList() ?? new List<Stop>();
        }

        public List<Stop> OptimizeSpecializedWithBudget(List<Stop> stopsToSchedule, decimal totalBudget)
        {
            if (stopsToSchedule.Count == 0 || totalBudget <= 0)
                return new List<Stop>();

            var (candidates, costs) = PrepareBudgetData(stopsToSchedule, totalBudget);
            if (candidates.Count == 0)
                return new List<Stop>();

            return RunBudgetGA(candidates, costs, totalBudget, isSpecialized: true);
        }

        public List<Stop> OptimizeBalancedWithBudget(List<Stop> stopsToSchedule, decimal totalBudget)
        {
            if (stopsToSchedule.Count == 0 || totalBudget <= 0)
                return new List<Stop>();

            var (candidates, costs) = PrepareBudgetData(stopsToSchedule, totalBudget);
            if (candidates.Count == 0)
                return new List<Stop>();

            return RunBudgetGA(candidates, costs, totalBudget, isSpecialized: false);
        }
    }
}
