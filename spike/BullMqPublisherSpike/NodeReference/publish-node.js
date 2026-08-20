/**
 * Node.js Reference Publisher
 * Mirrors BullHelpers.js in login.dfe.jobs
 */
const fs = require("fs");
const path = require("path");
const { Queue } = require("bullmq");

// Load from config.json if available
let fileConfig = {};
const configPath = path.join(__dirname, "config.json");
if (fs.existsSync(configPath)) {
  try {
    fileConfig = JSON.parse(fs.readFileSync(configPath, "utf-8"));
  } catch (err) {
    console.warn(`[Node Publisher] Warning: Could not parse config.json, using defaults.`);
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

const bullQueueTtl = {
  removeOnComplete: {
    age: 3600, // keep up to 1 hour
    count: 50, // keep up to 50 jobs
  },
  removeOnFail: {
    age: 12 * 3600, // keep up to 12 hours
  },
};

const samplePayload = {
  recipients: [
    {
      email: "approver.one@education.gov.uk",
      firstName: "Alex",
      lastName: "Taylor",
    },
    {
      email: "approver.two@education.gov.uk",
      firstName: "Sam",
      lastName: "Smith",
    },
  ],
  orgName: "St. Mary's Primary School",
  userName: "Jordan Doe",
  userEmail: "jordan.doe@education.gov.uk",
  orgId: "93128913-9E2B-450F-A886-B94B1B761E01",
  requestId: "REQ-2026-0820-001",
};

async function main() {
  console.log(`[Node Publisher] Config file : ${fs.existsSync(configPath) ? configPath : "(None, using fallback)"}`);
  console.log(`[Node Publisher] Redis URL   : ${connectionUrl}`);
  console.log(`[Node Publisher] Target Queue: ${queueName}`);

  const queue = new Queue(queueName, {
    connection: { url: connectionUrl },
  });

  try {
    const job = await queue.add(queueName, samplePayload, bullQueueTtl);
    console.log(`\n[Node Publisher] Successfully published job!`);
    console.log(`  Job ID    : ${job.id}`);
    console.log(`  Job Name  : ${job.name}`);
    console.log(`  Timestamp : ${job.timestamp} (${new Date(job.timestamp).toISOString()})`);
    console.log(`\nExpected Redis Keys Created:`);
    console.log(`  - bull:${queueName}:id`);
    console.log(`  - bull:${queueName}:${job.id} (Hash)`);
    console.log(`  - bull:${queueName}:wait (List)`);
    console.log(`  - bull:${queueName}:events (Stream)`);
  } catch (error) {
    console.error(`[Node Publisher] Error publishing job:`, error);
  } finally {
    await queue.close();
    console.log(`\n[Node Publisher] Queue connection closed.`);
  }
}

main().catch(console.error);
