using System.Text.Json;
using StackExchange.Redis;

namespace BullMqPublisherSpike.Comparer;

public static class RedisStateInspector
{
    public static async Task InspectQueueStateAsync(string connectionString, string queueName)
    {
        Console.WriteLine("\n==================================================");
        Console.WriteLine($"🔍 REDIS QUEUE STATE INSPECTOR: [bull:{queueName}]");
        Console.WriteLine("==================================================");

        var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
        var db = redis.GetDatabase();
        var server = redis.GetServers().FirstOrDefault();

        if (server is null)
        {
            Console.WriteLine("⚠️ Could not locate Redis server instance.");
            return;
        }

        var pattern = $"bull:{queueName}:*";
        var keys = server.Keys(pattern: pattern).Select(k => (string)k!).ToList();

        Console.WriteLine($"\nFound {keys.Count} keys matching pattern '{pattern}':");
        foreach (var key in keys.OrderBy(k => k))
        {
            var keyType = await db.KeyTypeAsync(key);
            Console.WriteLine($"  • {key} [{keyType}]");
        }

        // Check latest ID
        var lastId = await db.StringGetAsync($"bull:{queueName}:id");
        Console.WriteLine($"\nCurrent counter (bull:{queueName}:id): {lastId}");

        if (!lastId.IsNullOrEmpty)
        {
            var jobKey = $"bull:{queueName}:{lastId}";
            var hashEntries = await db.HashGetAllAsync(jobKey);

            Console.WriteLine($"\n📋 Hash Fields for Latest Job [{jobKey}]:");
            foreach (var entry in hashEntries)
            {
                var name = entry.Name.ToString();
                var value = entry.Value.ToString();

                if (name is "data" or "opts")
                {
                    try
                    {
                        var parsedJson = JsonDocument.Parse(value);
                        var formattedJson = JsonSerializer.Serialize(parsedJson, new JsonSerializerOptions { WriteIndented = true });
                        Console.WriteLine($"\n  [{name}]:\n{formattedJson}");
                    }
                    catch
                    {
                        Console.WriteLine($"  [{name}]: {value}");
                    }
                }
                else
                {
                    Console.WriteLine($"  [{name}]: {value}");
                }
            }
        }

        // Check waiting jobs
        var waitType = await db.KeyTypeAsync($"bull:{queueName}:wait");
        if (waitType == RedisType.List)
        {
            var waitItems = await db.ListRangeAsync($"bull:{queueName}:wait");
            Console.WriteLine($"\n⏳ Wait List (bull:{queueName}:wait) - Count: {waitItems.Length}:");
            foreach (var item in waitItems)
            {
                Console.WriteLine($"  - Job ID: {item}");
            }
        }

        Console.WriteLine("==================================================\n");
    }
}
