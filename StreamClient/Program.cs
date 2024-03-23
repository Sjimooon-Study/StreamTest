using HttpClient client = new();
string fileName = "message.png";
int skipCount = 20000;
int skipPosition = 50000;
string baseUrl = "https://localhost:7083/";
string requestUrl = $"file/slow/{fileName}";
CancellationTokenSource tokenSource = new();

try
{
    var task = GetFile(tokenSource.Token);

    Console.WriteLine("Hit enter to cancel at any time.");
    Console.ReadLine();
    if (!task.IsCompleted)
    {
        tokenSource.Cancel();
    }
    Console.WriteLine("Cancelled.");
    Console.WriteLine("Hit enter to exit.");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}

async Task GetFile(CancellationToken cancellationToken)
{
    var statusTimeout = TimeSpan.FromSeconds(1);
    var lastRead = DateTime.UtcNow;
    var totalByteCount = 0;
    var currentChunkByteCount = 0;
    var writeStatus = false;
    var currentByte = 0;

    Console.WriteLine("Starting stream...");
    using var stream = await client.GetStreamAsync(baseUrl + requestUrl, cancellationToken);
    while (stream.CanRead && currentByte != -1)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }
        
        Task<int> readTask = new(stream.ReadByte, cancellationToken);
        readTask.Start();
        while (!readTask.IsCompleted)
        {
            // If status timeout has passed since last read; set flat to write byte received status.
            if (lastRead.Add(statusTimeout) < DateTime.UtcNow)
            {
                writeStatus = true;

                break;
            }
        }
        if (writeStatus)
        {
            // Write byte received status.
            Console.WriteLine($"Received: {currentChunkByteCount} bytes (Current total: {totalByteCount} bytes). Waiting for more data...");
            await Task.Delay(800, cancellationToken);
            currentChunkByteCount = 0;
            writeStatus = false;
        }

        // Write byte value.
        currentByte = readTask.GetAwaiter().GetResult();
        Console.WriteLine(currentByte);

        lastRead = DateTime.UtcNow;
        currentChunkByteCount++;
        totalByteCount++;

        // Should we seek the stream here?
        if (stream.CanSeek && totalByteCount == skipPosition)
        {
            Console.WriteLine($"Received a total of {totalByteCount} bytes. Skipping {skipCount} bytes.");
            stream.Seek(skipCount, SeekOrigin.Current);
            Console.WriteLine($"Position is now {totalByteCount + skipCount}.");
            await Task.Delay(5000, cancellationToken);
        }
    }
    Console.WriteLine("End of stream.");
    Console.WriteLine($"Received a total of {totalByteCount} bytes.");
}
