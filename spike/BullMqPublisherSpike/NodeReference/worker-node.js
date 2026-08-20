/**
 * Node.js Reference Worker
 * Mirrors MonitorBull.js in login.dfe.jobs
 */
const fs = require("fs");
const path = require("path");
const { Worker } = require("bullmq");

// Load from config.json if available
let fileConfig = {};
const configPath = path.join(__dirname, "config.json");
if (fs.existsSync(configPath)) {
  try {
    fileConfig = JSON.parse(fs.readFileSync(configPath, "utf-8"));
  } catch (err) {
    console.warn(`[Node Worker] Warning: Could not parse config.json, using defaults.`);
  }
}

const connectionUrl =
  process.env.REDIS_CONNECTION_STRING ||
  fileConfig.redis?.connectionString ||
  "redis://127.0.0.1:6379";

const queueName =
  process.env.QUEUE_NAME ||
  fileConfig.queue?.name ||
  "approveraccessrequest_v1";

console.log("==================================================");
console.log("👷 Node.js Reference Worker (MonitorBull Simulation)");
console.log("==================================================");
console.log(`Redis URL   : ${connectionUrl}`);
console.log(`Queue Name  : ${queueName}`);
console.log("Status      : Listening for jobs... (Press Ctrl+C to stop)\n");

const worker = new Worker(
  queueName,
  async (job) => {
    console.log(`\n📥 [Worker] Received job ID: ${job.id} (name: "${job.name}")`);
    console.log(`   Job Data:`, JSON.stringify(job.data, null, 2));

    // Simulate processing time
    await new Promise((resolve) => setTimeout(resolve, 250));

    console.log(`✅ [Worker] Finished processing job ID: ${job.id}`);
    return { success: true, processedAt: new Date().toISOString() };
  },
  {
    connection: { url: connectionUrl },
  }
);

worker.on("completed", (job, result) => {
  console.log(`🎉 [Worker] Job ${job.id} has completed successfully!`);
  console.log(`   Job moved from 'wait' -> 'completed' Sorted Set in Redis.`);
  console.log(`   Hash updated with 'processedOn', 'finishedOn', 'returnvalue', 'atm'.\n`);
});

worker.on("failed", (job, err) => {
  console.error(`❌ [Worker] Job ${job?.id} failed:`, err.message);
});

// Handle graceful shutdown
const shutdown = async () => {
  console.log("\n[Worker] Shutting down...");
  await worker.close();
  process.exit(0);
};

process.on("SIGINT", shutdown);
process.on("SIGTERM", shutdown);
