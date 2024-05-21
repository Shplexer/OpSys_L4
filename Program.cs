using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

class BillingSystem {
    public Dictionary<int, double> clientAccounts;
    private Mutex mutex;
    private readonly int withdrawalAmount;
    private readonly int operationDuration;
    private readonly int numberOfWithdrawals;
    private readonly int threadStartDelay;
    public double totalTime;
    public BillingSystem(
        int withdrawalAmount,
        int numberOfWithdrawals,
        int operationDuration,
        int threadStartDelay,
        Dictionary<int, double> clientAccounts
        ) {
        this.withdrawalAmount = withdrawalAmount;
        this.numberOfWithdrawals = numberOfWithdrawals;
        this.operationDuration = operationDuration;
        this.threadStartDelay = threadStartDelay;
        this.clientAccounts = clientAccounts;

        mutex = new Mutex();
    }
    // Метод для запуска процесса снятия средств с синхронизацией
    public void StartWithdrawalWithSync() {
        ProcessWithdrawals(); // Вызов существующего метода с синхронизацией
    }

    // Метод для запуска процесса снятия средств без синхронизации
    public void StartWithdrawalWithoutSync() {
        mutex = null; // Установка мьютекса в null, чтобы отключить синхронизацию
        ProcessWithdrawals(); // Повторный вызов того же метода, который теперь будет работать без синхронизации
    }

    // Метод ProcessWithdrawals не изменяется, т.к. он уже использует мьютекс
    public void ProcessWithdrawals() {
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<Thread> threads = new List<Thread>();
        for (int i = 0; i < numberOfWithdrawals; i++) {
            Thread thread = new Thread(PerformWithdrawals);
            threads.Add(thread);
            thread.Start();
            Thread.Sleep(threadStartDelay);
        }

        foreach (Thread thread in threads) {
            thread.Join(); // Ожидание завершения всех потоков
        }

        stopwatch.Stop();
        Console.WriteLine("Результат работы без синхронизаци: ");
        foreach (var clientId1 in clientAccounts.Keys) {
            Console.WriteLine($"Клиент {clientId1}: конечный счёт {clientAccounts[clientId1]}");
        }
        Console.WriteLine($"Время работы: {totalTime}ms");

        Console.WriteLine($"Время завершения всех операций: {stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalSeconds}s");
        totalTime = stopwatch.Elapsed.TotalMilliseconds;

    }

    private void PerformWithdrawals() {
        int[] clientIds = clientAccounts.Keys.OrderBy(x => Guid.NewGuid()).ToArray(); // Случайный порядок клиентов

        foreach (int clientId in clientIds) {
            Withdraw(clientId);
        }
    }
    private void Withdraw(int clientId) {
        for (int i = 0; i < numberOfWithdrawals; i++) {
            if (mutex != null) {
                mutex.WaitOne(); // Вход в критическую секцию
            }
            double currentBalance = clientAccounts[clientId];
            Thread.Sleep(operationDuration); // Задержка операции
            if (currentBalance >= withdrawalAmount) {
                clientAccounts[clientId] -= withdrawalAmount;
                //Console.WriteLine($"Клиент {clientId}: снято {withdrawalAmount}. Остаток: {clientAccounts[clientId]}");
            }
            else {
                //Console.WriteLine($"Клиент {clientId}: недостаточно средств.");
            }
            if (mutex != null) {
                mutex.ReleaseMutex(); // Выход из критической секции
            }
        }
    }
}
class Program {
    static void Main() {
        PrintLine();
        Console.WriteLine("Перемножение матриц");
        PrintLine();
        Console.WriteLine("Введите количество потоков:");
        int numThreads = int.Parse(Console.ReadLine());

        Console.WriteLine("Введите размер матрицы:");
        int size = int.Parse(Console.ReadLine());

        int[,] intMatrix1 = GenerateRandomIntMatrix(size);
        int[,] intMatrix2 = GenerateRandomIntMatrix(size);

        double[,] doubleMatrix1 = GenerateRandomDoubleMatrix(size);
        double[,] doubleMatrix2 = GenerateRandomDoubleMatrix(size);


        int[,] multithreaddedIntResult = MultiplyIntMatrixMultithreadded(intMatrix1, intMatrix2, numThreads);
        int[,] intResult = MultiplyIntMatrix(intMatrix1, intMatrix2);

        double[,] multithreaddedDoubleResult = MultiplyDoubleMatrixMultithreadded(doubleMatrix1, doubleMatrix2, numThreads);
        double[,] doubleResult = MultiplyDoubleMatrix(doubleMatrix1, doubleMatrix2);
        
        PrintLine();
        Console.WriteLine("Биллинговая система");
        PrintLine();
        
        Console.Write("Введите кол-во клиентов: ");
        int clientNumber = int.Parse(Console.ReadLine());

        Console.Write("Введите снимаемую сумму: ");
        int withdrawalAmount = int.Parse(Console.ReadLine());

        Console.Write("Введите число снятий: ");
        int numberOfWithdrawals = int.Parse(Console.ReadLine());

        Console.Write("Введите продолжительность операции снятия (мс): ");
        int operationDuration = int.Parse(Console.ReadLine());

        Console.Write("Введите интервал между потоками (мс): ");
        int threadStartDelay = int.Parse(Console.ReadLine());

        var clientAccounts = new Dictionary<int, double>();

        for (int i = 0; i < clientNumber; i++) {
            Random random = new Random();
            clientAccounts[i] = random.Next(1000, 5000);

        } // Стартовый баланс для клиентов

        var billingSystemSynced = new BillingSystem(
            withdrawalAmount: withdrawalAmount, // снимаемая сумма
            numberOfWithdrawals: numberOfWithdrawals, // количество снятий (число потоков)
            operationDuration: operationDuration, // продолжительность операции снятия (мс)
            threadStartDelay: threadStartDelay, // интервал между потоками (мс)
            clientAccounts: clientAccounts
        );
        Console.WriteLine();

        var billingSystemUnsynced = new BillingSystem(
            withdrawalAmount: withdrawalAmount, // снимаемая сумма
            numberOfWithdrawals: numberOfWithdrawals, // количество снятий (число потоков)
            operationDuration: operationDuration, // продолжительность операции снятия (мс)
            threadStartDelay: threadStartDelay, // интервал между потоками (мс)
            clientAccounts: clientAccounts
        );
        PrintLine();
        Console.WriteLine("Запуск процесса с синхронизацией:");
        PrintLine();
        Console.WriteLine("Результат работы с синхронизацией: ");    
        billingSystemSynced.StartWithdrawalWithSync(); // Запуск процесса снятия денег
        PrintLine();
        Console.WriteLine("Запуск процесса без синхронизации:");
        PrintLine();
        Console.WriteLine("Результат работы без синхронизации: ");
        billingSystemUnsynced.StartWithdrawalWithoutSync(); // Запуск процесса снятия денег
        PrintLine();

    }
    static void PrintLine() {
        Console.WriteLine("===========================================================");
    }
    static int[,] GenerateRandomIntMatrix(int size) {
        int[,] matrix = new int[size, size];
        Random random = new Random();

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                matrix[i, j] = random.Next(1000, 3001);
            }
        }

        return matrix;
    }
    static double[,] GenerateRandomDoubleMatrix(int size) {
        double[,] matrix = new double[size, size];
        Random random = new Random();

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                matrix[i, j] = random.NextDouble() * 2000 + 1000;
            }
        }

        return matrix;
    }
    static int[,] MultiplyIntMatrix(int[,] matrix1, int[,] matrix2) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        int size = matrix1.GetLength(0);
        int[,] result = new int[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                for (int k = 0; k < size; k++) {
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            }
        }
        stopwatch.Stop();
        Console.WriteLine($"Время на перемножение матрицы типа int (без многопоточности): {stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalSeconds}s");
        return result;
    }
    static double[,] MultiplyDoubleMatrix(double[,] matrix1, double[,] matrix2) {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        int size = matrix1.GetLength(0);
        double[,] result = new double[size, size];

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                for (int k = 0; k < size; k++) {
                    result[i, j] += matrix1[i, k] * matrix2[k, j];
                }
            }
        }
        stopwatch.Stop();
        Console.WriteLine($"Время на перемножение матрицы типа double (без многопоточности):  {stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalSeconds}s");
        return result;
    }
    static int[,] MultiplyIntMatrixMultithreadded(int[,] matrix1, int[,] matrix2, int numThreads) {
        int size = matrix1.GetLength(0);
        int[,] result = new int[size, size];

        // Вычисляем размер каждого фрагмента для параллельного процесса
        int chunkSize = size / numThreads;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // Создаем массив задач для выполнения умножения в параллели
        Task[] tasks = new Task[numThreads];

        // Проходим по каждому фрагменту
        for (int i = 0; i < numThreads; i++) {
            // Вычисляем начальную и конечную строку для текущего фрагмента
            int startRow = i * chunkSize;
            int endRow = (i == numThreads - 1) ? size : (i + 1) * chunkSize;

            // Создаем новую задачу для выполнения умножения для текущего фрагмента
            tasks[i] = Task.Run(() => {
                // Проходим по каждому элементу результирующей матрицы
                for (int j = 0; j < size; j++) {
                    for (int k = 0; k < size; k++) {
                        // Выполняем умножение для текущего элемента
                        for (int l = startRow; l < endRow; l++) {
                            result[j, k] += matrix1[j, l] * matrix2[l, k];
                        }
                    }
                }
            });
        }

        // Ожидаем завершения всех задач
        Task.WaitAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($"Время на перемножение матрицы типа int (с {numThreads} потоками): {stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalSeconds}s");
        return result;
    }
    static double[,] MultiplyDoubleMatrixMultithreadded(double[,] matrix1, double[,] matrix2, int numThreads) {
        int size = matrix1.GetLength(0);
        double[,] result = new double[size, size];

        // Вычисляем размер каждого фрагмента для параллельного процесса
        int chunkSize = size / numThreads;

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        // Создаем массив задач для выполнения умножения в параллели
        Task[] tasks = new Task[numThreads];

        // Проходим по каждому фрагменту
        for (int i = 0; i < numThreads; i++) {
            // Вычисляем начальную и конечную строку для текущего фрагмента
            int startRow = i * chunkSize;
            int endRow = (i == numThreads - 1) ? size : (i + 1) * chunkSize;

            // Создаем новую задачу для выполнения умножения для текущего фрагмента
            tasks[i] = Task.Run(() => {
                // Проходим по каждому элементу результирующей матрицы
                for (int j = 0; j < size; j++) {
                    for (int k = 0; k < size; k++) {
                        // Выполняем умножение для текущего элемента
                        for (int l = startRow; l < endRow; l++) {
                            result[j, k] += matrix1[j, l] * matrix2[l, k];
                        }
                    }
                }
            });
        }

        // Ожидаем завершения всех задач
        Task.WaitAll(tasks);
        stopwatch.Stop();
        Console.WriteLine($"Время на перемножение матрицы типа double (с {numThreads} потоками): {stopwatch.Elapsed.TotalMilliseconds}ms, {stopwatch.Elapsed.TotalSeconds}s");
        return result;
    }
    static void PrintMatrix(int[,] matrix) {
        int size = matrix.GetLength(0);

        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }
}