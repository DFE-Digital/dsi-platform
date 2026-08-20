# BullMQ Redis Publisher Spike (Node.js vs .NET)

This spike validates that messages published onto a Redis queue using **.NET (C#)** produce the exact same Redis keys, hash fields, and JSON payload serialization as those published by the legacy **Node.js** platform (`login.dfe.jobs`).

---

## Prerequisites & Starting Redis

You need a running Redis instance. You can either use **.NET Aspire** (recommended) or **Docker**.

### Option A: Using .NET Aspire (Recommended)

Start the Aspire AppHost:
```powershell
dotnet run --project C:\Work\Playground\dsi-workspace\codebase\dsi-platform\src\Dfe.SignIn.AppHost
```
* Aspire will spin up the `infra-redis` container along with **RedisInsight**.
* Note the port assigned to `infra-redis` in the Aspire Dashboard (e.g. `localhost:55000` or `localhost:6379`).

### Option B: Using Standalone Docker
```powershell
docker run -d --name redis-spike -p 6379:6379 redis:alpine
```

---

## Configuration Files (No `$env` required)

Both projects support dedicated JSON configuration files:

### 1. Node.js Configuration: [`NodeReference/config.json`](file:///C:/Work/Playground/dsi-workspace/codebase/dsi-scratchpad/Spikes/BullMqPublisherSpike/NodeReference/config.json)
```json
{
  "redis": {
    "connectionString": "redis://127.0.0.1:6379/4"
  },
  "queue": {
    "name": "approveraccessrequest_v1"
  }
}
```

### 2. .NET Configuration: [`DotNetPublisher/appsettings.json`](file:///C:/Work/Playground/dsi-workspace/codebase/dsi-scratchpad/Spikes/BullMqPublisherSpike/DotNetPublisher/appsettings.json)
```json
{
  "Redis": {
    "ConnectionString": "127.0.0.1:6379,defaultDatabase=4"
  },
  "Queue": {
    "Name": "approveraccessrequest_v1"
  }
}
```

---

## Directory Structure

```
BullMqPublisherSpike/
├── NodeReference/
│   ├── config.json                # Node configuration file (Redis URL & Queue name)
│   ├── package.json               # bullmq ^5.65.1
│   ├── publish-node.js            # Node reference publisher (Producer only)
│   ├── worker-node.js             # Standalone Worker (Consumer simulator)
│   └── simulate-lifecycle.js      # Full end-to-end simulation (Publish -> Consume -> Inspect)
└── DotNetPublisher/
    ├── appsettings.json           # .NET configuration file (Redis host & Queue name)
    ├── BullMqPublisherSpike.csproj # .NET 10 console app
    ├── Program.cs                 # Spike entry point (publish & inspect)
    ├── Contracts/                 # Strongly-typed C# payload DTOs
    │   ├── ApproverRecipient.cs
    │   └── ApproverAccessRequestPayload.cs
    └── Comparer/
        └── RedisStateInspector.cs  # Inspects Redis keys, hash fields & JSON
```

---

## How to Run & Simulate the Lifecycle

### Mode 1: Full End-to-End Simulation (Publish ➔ Consume ➔ Completed Hash)
Run this script to publish a job, run a worker to complete it, and view the post-completion Redis Hash (with `processedOn`, `finishedOn`, `completed` set, etc.):
```powershell
cd C:\Work\Playground\dsi-workspace\codebase\dsi-scratchpad\Spikes\BullMqPublisherSpike\NodeReference
node simulate-lifecycle.js
```

---

### Mode 2: Run .NET Publisher against Node Worker
1. **Start the Node Worker in one terminal:**
   ```powershell
   cd C:\Work\Playground\dsi-workspace\codebase\dsi-scratchpad\Spikes\BullMqPublisherSpike\NodeReference
   node worker-node.js
   ```
2. **Run the .NET Publisher in another terminal:**
   ```powershell
   cd C:\Work\Playground\dsi-workspace\codebase\dsi-scratchpad\Spikes\BullMqPublisherSpike\DotNetPublisher
   dotnet run
   ```
3. **Observe:** The Node worker will instantly receive and process the job created by .NET, outputting the completion lifecycle.

---

### Mode 3: Standalone Publisher & State Inspector
```powershell
cd C:\Work\Playground\dsi-workspace\codebase\dsi-scratchpad\Spikes\BullMqPublisherSpike\DotNetPublisher
dotnet run
```

---

## Visual Verification in RedisInsight (via Aspire)

1. Open **RedisInsight** from your Aspire Dashboard.
2. Select **Database 4 (`db4`)**.
3. Inspect `bull:approveraccessrequest_v1:*` to see the folder view, completed sorted sets, and execution timestamps.
