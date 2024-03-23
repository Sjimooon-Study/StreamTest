using Microsoft.Win32.SafeHandles;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapGet("/file/{path}", (string path) =>
{
    string mimeType = GetMimeType(path);
    var fs = File.OpenRead(path);

    return Results.File(fs, contentType: mimeType, enableRangeProcessing: true);
})
.WithName("File")
.WithOpenApi();

app.MapGet("/file/slow/{path}", (string path) =>
{
    string mimeType = GetMimeType(path);
    var fs = SlowFileStream.OpenRead(path);

    return Results.File(fs, contentType: mimeType, enableRangeProcessing: true);
})
.WithName("SlowFile")
.WithOpenApi();

static string GetMimeType(string path)
{
    string mimeType = "application/octet-stream";
    if (path.EndsWith(".mp4")) mimeType = "video/mp4";
    if (path.EndsWith(".png")) mimeType = "image/png";

    return mimeType;
}

app.Run();

internal class SlowFileStream : FileStream
{
    private readonly int _readDelay = 5000;

    #region Constructors
    public SlowFileStream(SafeFileHandle handle, FileAccess access) : base(handle, access)
    {
    }

    public SlowFileStream(nint handle, FileAccess access) : base(handle, access)
    {
    }

    public SlowFileStream(string path, FileMode mode) : base(path, mode)
    {
    }

    public SlowFileStream(string path, FileStreamOptions options) : base(path, options)
    {
    }

    public SlowFileStream(SafeFileHandle handle, FileAccess access, int bufferSize) : base(handle, access, bufferSize)
    {
    }

    public SlowFileStream(nint handle, FileAccess access, bool ownsHandle) : base(handle, access, ownsHandle)
    {
    }

    public SlowFileStream(string path, FileMode mode, FileAccess access) : base(path, mode, access)
    {
    }

    public SlowFileStream(SafeFileHandle handle, FileAccess access, int bufferSize, bool isAsync) : base(handle, access, bufferSize, isAsync)
    {
    }

    public SlowFileStream(nint handle, FileAccess access, bool ownsHandle, int bufferSize) : base(handle, access, ownsHandle, bufferSize)
    {
    }

    public SlowFileStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access, share)
    {
    }

    public SlowFileStream(nint handle, FileAccess access, bool ownsHandle, int bufferSize, bool isAsync) : base(handle, access, ownsHandle, bufferSize, isAsync)
    {
    }

    public SlowFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize) : base(path, mode, access, share, bufferSize)
    {
    }

    public SlowFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync) : base(path, mode, access, share, bufferSize, useAsync)
    {
    }

    public SlowFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) : base(path, mode, access, share, bufferSize, options)
    {
    }
    #endregion

    public static SlowFileStream OpenRead(string path) =>
        new(path, FileMode.Open, FileAccess.Read, FileShare.Read);

    //    public override int Read(byte[] buffer, int offset, int count)
    //    {
    //        Task.Delay(_readDelay).Wait();

    //        return base.Read(buffer, offset, count);
    //    }

    //    public override int Read(Span<byte> buffer)
    //    {
    //        Task.Delay(_readDelay).Wait();

    //        return base.Read(buffer);
    //    }

//    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
//    {
//        await Task.Delay(_readDelay, cancellationToken);

//#pragma warning disable CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
//        return await base.ReadAsync(buffer, offset, count, cancellationToken);
//#pragma warning restore CA1835 // Prefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'
//    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await Task.Delay(_readDelay, cancellationToken);

        return await base.ReadAsync(buffer, cancellationToken);
    }

    //public override int ReadByte()
    //{
    //    Task.Delay(_readDelay).Wait();

    //    return base.ReadByte();
    //}
}

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
