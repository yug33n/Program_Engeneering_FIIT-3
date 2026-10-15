using System;
using System.Collections.Generic;

namespace TravellingSalespersonProblem
{
    public class TspResult
    {
        public double MinCost { get; set; }
        public List<int> OptimalPath { get; set; } = new();
    }

    /// <summary>
    /// Решение задачи коммивояжёра (TSP) методом динамического программирования (алгоритм Хелда — Карпа).
    /// Сложность: O(n^2 * 2^n).
    /// </summary>
    public class TspSolver
    {
        private const double Infinity = double.PositiveInfinity;

        public static TspResult Solve(double[,] distanceMatrix)
        {
            int n = distanceMatrix.GetLength(0);
            if (n <= 1)
            {
                return new TspResult
                {
                    MinCost = 0,
                    OptimalPath = n == 1 ? new List<int> { 0, 0 } : new List<int>()
                };
            }

            // memo[mask, lastCity] - наименьшая стоимость посещения множества городов (битовая маска mask),
            // заканчивая в городе lastCity. Стартовый город всегда 0.
            int totalStates = 1 << n;
            double[,] memo = new double[totalStates, n];
            int[,] parent = new int[totalStates, n];

            for (int i = 0; i < totalStates; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    memo[i, j] = Infinity;
                    parent[i, j] = -1;
                }
            }

            // Базовый случай: стартуем в городе 0 (маска 1 << 0 = 1)
            memo[1, 0] = 0;

            // Перебираем размеры подмножеств городов от 2 до n
            for (int subsetSize = 2; subsetSize <= n; subsetSize++)
            {
                // Перебираем все возможные комбинации городов заданного размера
                foreach (int mask in GetSubsetsOfSize(subsetSize, n))
                {
                    // Стартовый город 0 обязательно должен быть в маске
                    if ((mask & 1) == 0)
                        continue;

                    for (int next = 1; next < n; next++)
                    {
                        if ((mask & (1 << next)) == 0)
                            continue;

                        int prevMask = mask ^ (1 << next);
                        for (int end = 0; end < n; end++)
                        {
                            if ((prevMask & (1 << end)) == 0)
                                continue;

                            double newCost = memo[prevMask, end] + distanceMatrix[end, next];
                            if (newCost < memo[mask, next])
                            {
                                memo[mask, next] = newCost;
                                parent[mask, next] = end;
                            }
                        }
                    }
                }
            }

            // Замыкаем цикл, возвращаясь в стартовый город 0
            int fullMask = (1 << n) - 1;
            double minTourCost = Infinity;
            int lastCityInOptimalTour = -1;

            for (int i = 1; i < n; i++)
            {
                double tourCost = memo[fullMask, i] + distanceMatrix[i, 0];
                if (tourCost < minTourCost)
                {
                    minTourCost = tourCost;
                    lastCityInOptimalTour = i;
                }
            }

            // Восстановление пути
            List<int> path = new();
            int currCity = lastCityInOptimalTour;
            int currMask = fullMask;

            path.Add(0); // Финиш в городе 0
            while (currCity != -1)
            {
                path.Add(currCity);
                int prevCity = parent[currMask, currCity];
                currMask ^= (1 << currCity);
                currCity = prevCity;
            }

            path.Reverse();

            return new TspResult
            {
                MinCost = minTourCost,
                OptimalPath = path
            };
        }

        private static IEnumerable<int> GetSubsetsOfSize(int k, int n)
        {
            List<int> subsets = new();
            GenerateCombinations(0, 0, k, n, subsets);
            return subsets;
        }

        private static void GenerateCombinations(int set, int at, int k, int n, List<int> subsets)
        {
            int elementsLeftToPick = n - at;
            if (elementsLeftToPick < k)
                return;

            if (k == 0)
            {
                subsets.Add(set);
                return;
            }

            for (int i = at; i < n; i++)
            {
                set |= (1 << i);
                GenerateCombinations(set, i + 1, k - 1, n, subsets);
                set &= ~(1 << i);
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine("=== Задача коммивояжёра (TSP) на C# ===");
            Console.WriteLine("Метод: Динамическое программирование (Алгоритм Хелда — Карпа)\n");

            // Пример: 5 городов и матрица расстояний между ними
            string[] cityNames = { "Москва", "Санкт-Петербург", "Казань", "Нижний Новгород", "Самара" };
            double[,] distanceMatrix = new double[,]
            {
                { 0,   710, 820, 420, 1050 },
                { 710, 0,   1200, 1100, 1700 },
                { 820, 1200, 0,   390,  360 },
                { 420, 1100, 390, 0,    680 },
                { 1050, 1700, 360, 680, 0 }
            };

            int n = cityNames.Length;
            Console.WriteLine("Матрица расстояний:");
            Console.Write("           ");
            for (int i = 0; i < n; i++)
                Console.Write($"{cityNames[i],-16}");
            Console.WriteLine();

            for (int i = 0; i < n; i++)
            {
                Console.Write($"{cityNames[i],-10} ");
                for (int j = 0; j < n; j++)
                {
                    Console.Write($"{distanceMatrix[i, j],-16}");
                }
                Console.WriteLine();
            }

            Console.WriteLine("\nВыполняется расчёт оптимального маршрута...");
            var result = TspSolver.Solve(distanceMatrix);

            Console.WriteLine("\n--- Результат ---");
            Console.WriteLine($"Минимальная длина пути: {result.MinCost} км");
            Console.Write("Маршрут коммивояжёра: ");
            for (int i = 0; i < result.OptimalPath.Count; i++)
            {
                int cityIdx = result.OptimalPath[i];
                Console.Write(cityNames[cityIdx]);
                if (i < result.OptimalPath.Count - 1)
                    Console.Write(" -> ");
            }
            Console.WriteLine("\n");
        }
    }
}
