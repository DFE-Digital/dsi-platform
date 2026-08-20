using BullMQ;
using BullMqPublisherSpike.Comparer;
using BullMqPublisherSpike.Contracts;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

Console.WriteLine("==================================================");
Console.WriteLine("🚀 .NET BullMQ Redis Publisher Spike");
Console.WriteLine("==================================================");

// Build configuration: appsettings.json + Environment Variables
var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();

var redisConnectionString = 
    config["REDIS_CONNECTION_STRING"] ??
    config["Redis:ConnectionString"] ?? 
    config["QueueStorage:ConnectionString"] ?? 
    "127.0.0.1:6379";

var queueName = 
    config["QUEUE_NAME"] ?? 
    config["Queue:Name"] ?? 
    "approveraccessrequest_v1";

Console.WriteLine($"Config File : {Path.Combine(AppContext.BaseDirectory, "appsettings.json")}");
Console.WriteLine($"Redis Host  : {redisConnectionString}");
Console.WriteLine($"Target Queue: {queueName}\n");

Console.WriteLine("Select an action:");
Console.WriteLine("  1. Publish sample job via .NET BullMQ");
Console.WriteLine("  2. Inspect Redis Queue State");
Console.WriteLine("  3. Publish & then Inspect (Default)");
Console.Write("\nEnter choice [1-3] (default 3): ");

var choice = Console.ReadLine()?.Trim();
if (string.IsNullOrEmpty(choice))
{
    choice = "3";
}

if (choice is "1" or "3")
{
    Console.WriteLine($"\n[1/2] Initializing BullMQ Queue '{queueName}'...");

    var queue = new Queue(queueName, new QueueOptions
    {
        Connection = new ConnectionOptions
        {
            ConnectionString = redisConnectionString
        }
    });

    var samplePayload = new ApproverAccessRequestPayload
    {
        Recipients = new List<ApproverRecipient>
        {
            new()
            {
                Email = "approver.one@education.gov.uk",
                FirstName = "Alex",
                LastName = "Taylor"
            },
            new()
            {
                Email = "approver.two@education.gov.uk",
                FirstName = "Sam",
                LastName = "Smith"
            }
        },
        OrgName = "St. Mary's Primary School",
        UserName = "Jordan Doe",
        UserEmail = "jordan.doe@education.gov.uk",
        OrgId = "93128913-9E2B-450F-A886-B94B1B761E01",
        RequestId = "REQ-2026-0820-001"
    };

    Console.WriteLine("[2/2] Publishing job to Redis...");

    var jobOptions = new JobsOptions
    {
        RemoveOnComplete = new KeepJobs { Age = 3600, Count = 50 },
        RemoveOnFail = new KeepJobs { Age = 12 * 3600 }
    };

    var job = await queue.AddAsync(queueName, samplePayload, jobOptions);

    Console.WriteLine("\n✅ Job successfully enqueued by .NET BullMQ!");
    Console.WriteLine($"  Job ID    : {job.Id}");
    Console.WriteLine($"  Job Name  : {job.Name}");
    Console.WriteLine($"  Timestamp : {job.Timestamp} ({DateTimeOffset.FromUnixTimeMilliseconds(job.Timestamp):O})");

    await queue.CloseAsync();
}

if (choice is "2" or "3")
{
    await RedisStateInspector.InspectQueueStateAsync(redisConnectionString, queueName);
}

Console.WriteLine("\nSpike execution complete.");
