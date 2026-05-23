using System.Diagnostics;
using System.Globalization;

// Список значень N для обчислення
long[] iterations = { 
    1_000_000L, 
    10_000_000L, 
    100_000_000L, 
    1_000_000_000L, 
    10_000_000_000L, 
    100_000_000_000L 
};

string directoryPath = "results";
string csvPath = Path.Combine(directoryPath, "pi_monte_carlo_results.csv");

if (!Directory.Exists(directoryPath)) Directory.CreateDirectory(directoryPath);

using (StreamWriter sw = new StreamWriter(csvPath))
{
    sw.WriteLine("N,pi_estimated,accuracy,execution_time_sec,time_per_point_sec");
    Console.WriteLine($"{"N",15} | {"Estimated Pi",12} | {"Accuracy",10} | {"Time (s)",10}");
    Console.WriteLine(new string('-', 55));

    foreach (long n in iterations)
    {
        long pointsInsideCircle = 0;
        Stopwatch timer = Stopwatch.StartNew();

        // Використовуємо всі ядра процесора
        Parallel.For(0, Environment.ProcessorCount, i =>
        {
            long localCount = 0;
            long iterationsPerThread = n / Environment.ProcessorCount;
            Random rand = new Random(Guid.NewGuid().GetHashCode());

            for (long j = 0; j < iterationsPerThread; j++)
            {
                double x = rand.NextDouble();
                double y = rand.NextDouble();
                if (x * x + y * y <= 1.0) localCount++;
            }
            Interlocked.Add(ref pointsInsideCircle, localCount);
        });

        timer.Stop();

        double piEst = 4.0 * pointsInsideCircle / n;
        double accuracy = Math.Abs(piEst - Math.PI);
        double seconds = timer.Elapsed.TotalSeconds;
        double timePerPoint = seconds / n;

        // Запис у файл
        sw.WriteLine($"{n},{piEst.ToString(CultureInfo.InvariantCulture)},{accuracy.ToString(CultureInfo.InvariantCulture)},{seconds.ToString(CultureInfo.InvariantCulture)},{timePerPoint.ToString("E", CultureInfo.InvariantCulture)}");
        
        // Вивід у консоль
        Console.WriteLine($"{n,15:N0} | {piEst,12:F8} | {accuracy,10:F8} | {seconds,10:F4}");
    }
}

Console.WriteLine($"\nГотово! Результати збережено в {csvPath}");
//dev гілка 