/**
 * Full Lifecycle Simulation Script:
 * 1. Publishes a job to Redis (Producer)
 * 2. Starts a BullMQ Worker to process & complete the job (Consumer)
 * 3. Inspects and prints the exact Redis Hash fields (Matching RedisInsight)
 */
const fs = require("fs");
const path = require("path");
const { Queue, Worker } = require("bullmq");

let fileConfig = {};
const configPath = path.join(__dirname, "config.json");
if (fs.existsSync(configPath)) {
  try {
    fileConfig = JSON.parse(fs.readFileSync(configPath, "utf-8"));
  } catch (err) {}
}

const connectionUrl =
  process.env.REDIS_CONNECTION_STRING ||
  fileConfig.redis?.connectionString ||
  "redis://127.0.0.1:6379";

const queueName =
  process.env.QUEUE_NAME ||
  fileConfig.queue?.name ||
  "approveraccessrequest_v1";

const bullQueueTtl = {
  removeOnComplete: {
    age: 3600,
    count: 50,
  },
  removeOnFail: {
    age: 12 * 3600,
  },
};

const samplePayload = {
  recipients: [
    {
      email: "approver.one@education.gov.uk",
      firstName: "Alex",
      lastName: "Taylor",
    },
  ],
  orgName: "St. Mary's Primary School",
  userName: "Jordan Doe",
  userEmail: "jordan.doe@education.gov.uk",
  orgId: "93128913-9E2B-450F-A886-B94B1B761E01",
  requestId: "REQ-2026-0820-001",
};

async function runLifecycleSimulation() {
  console.log("==================================================");
  console.log("🔄 BullMQ Full Lifecycle Simulation");
  console.log("==================================================");
  console.log(`Redis URL   : ${connectionUrl}`);
  console.log(`Queue Name  : ${queueName}\n`);

  const queue = new Queue(queueName, { connection: { url: connectionUrl } });

  // 1. Publish Job
  console.log("[1/3] Publishing job (Producer)...");
  const job = await queue.add(queueName, samplePayload, bullQueueTtl);
  console.log(`  ✅ Job enqueued with ID: ${job.id}`);
  console.log(`     Initial state: waiting in 'bull:${queueName}:wait'`);

  // 2. Start Worker and Consume Job
  console.log("\n[2/3] Starting Worker (Consumer)...");
  let workerFinishedPromiseResolve;
  const workerFinishedPromise = new Promise((resolve) => {
    workerFinishedPromiseResolve = resolve;
  });

  const worker = new Worker(
    queueName,
    async (j) => {
      console.log(`  📥 Worker processing job ${j.id}...`);
      await new Promise((r) => setTimeout(r, 150)); // simulate brief work
      return null;
    },
    { connection: { url: connectionUrl } }
  );

  worker.on("completed", async (completedJob) => {
    console.log(`  🎉 Worker completed job ${completedJob.id}!`);
    workerFinishedPromiseResolve();
  });

  await workerFinishedPromise;
  await worker.close();

  // 3. Inspect the Job Hash State in Redis
  console.log("\n[3/3] Inspecting Redis Hash State (Post-Worker Completion):");
  const completedJob = await queue.getJob(job.id);
  const client = await queue.client;
  const rawHash = await client.hgetall(`bull:${queueName}:${job.id}`);

  console.log(`\n📋 Hash Fields for [bull:${queueName}:${job.id}]:`);
  console.log("--------------------------------------------------");
  for (const [field, value] of Object.entries(rawHash)) {
    console.log(`  ${field.padEnd(15)} : ${value.length > 80 ? value.substring(0, 80) + "..." : value}`);
  }
  console.log("--------------------------------------------------");

  console.log("\n🔑 Associated Redis Keys for this queue:");
  const keys = await client.keys(`bull:${queueName}:*`);
  for (const key of keys.sort()) {
    const type = await client.type(key);
    console.log(`  • ${key} [${type}]`);
  }

  await queue.close();
  console.log("\n✅ End-to-end lifecycle simulation complete!");
  process.exit(0);
}

runLifecycleSimulation().catch((err) => {
  console.error("Error in simulation:", err);
  process.exit(1);
});
