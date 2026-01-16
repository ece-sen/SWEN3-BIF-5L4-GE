GitHub Link: [https://github.com/ece-sen/SWEN3-BIF-5L4-GE.git](https://github.com/ece-sen/SWEN3-BIF-5L4-GE.git)



### Paperless – Critical Aspects Documentation



#### Architecture



* Layered architecture (Controller, Service, Repository)
* Clear separation of concerns
* Dependency Injection for loose coupling



#### Business Logic



* Validation and orchestration handled in service layer
* Controllers only handle HTTP concerns
* External systems accessed via interfaces



#### Persistence



* Entity Framework Core for data access
* In-memory database used for integration tests
* Database migrations only applied for relational databases



#### External Infrastructure



* Object storage, message queue, and search treated as infrastructure concerns
* Infrastructure execution controlled via environment configuration
* Disabled in integration test environment



#### Testing Strategy



* Unit tests with mocked dependencies
* Integration tests with full HTTP pipeline
* Custom test host using WebApplicationFactory

---

## Configuration & Startup Guide

This document explains how to configure and start the Paperless system locally using Docker, including GenAI integration and the scheduled batch process.

---

## Prerequisites

Make sure the following tools are installed:

* Docker
* Docker Compose
* Git

---

## Environment Configuration (GenAI – Google Gemini)

The GenAI worker uses **Google Gemini** to generate document summaries.

Create a `.env` file in the **project root directory** (same level as `docker-compose.yml`) if it does not exist.

### `.env` example

```env
GEMINI_API_KEY=<YOUR_API_KEY>
GEMINI_MODEL=gemini-2.5-flash
```

Notes:

* The `.env` file is **not committed** to the repository
* If this file is missing or invalid:

  * The GenAI worker will start
  * But summary generation will fail

---

## Starting the Application (Docker)

From the project root directory, run:

```bash
docker compose up -d --build
```

This starts all required services:

* REST API
* PostgreSQL
* RabbitMQ
* MinIO
* OCR Worker
* GenAI Worker
* Elasticsearch
* Batch Processor
* Frontend

To stop all services:

```bash
docker compose down
```

---

## Batch Processing (Scheduled Access Log Import)

The batch process reads **daily XML access log files** from external systems and stores **per-document access counts** in the PostgreSQL database.

---

### Folder Structure (Important)

At the beginning, **only the input folder exists**.

```text
accesslogs/
 └── input/
```

* `input/`
  Place incoming daily XML files here.

The `archive/` folder **does not exist initially**.

After the first successful batch run, the batch process will **automatically create** the archive folder and move processed files there:

```text
accesslogs/
 ├── input/
 └── archive/
```

* `archive/`
  Contains already processed XML files to prevent duplicate processing.

> If the `accesslogs` folder itself does not exist, create it manually before starting the batch container.

---

### XML Demo File

A demo XML file is used to demonstrate batch functionality.

Example filename:

```text
access-2026-01-13.xml
```

The batch process will:

1. Scan `accesslogs/input`
2. Read all matching XML files
3. Persist daily access counts per document in PostgreSQL
4. Move processed files to `accesslogs/archive`

If no XML files are found, the batch process logs that nothing was processed. Please add a demo XML file in accesslogs/input folder.

## XML Example (Access Log Demo)

The following is a **minimal demo XML file** used by the batch process to import daily document access statistics.

### Example File: `access-2026-01-13.xml`

```xml
<?xml version="1.0" encoding="UTF-8"?>
<accessLogs date="2026-01-13">
  <documentAccess>
    <documentId>1</documentId>
    <accessCount>5</accessCount>
  </documentAccess>
</accessLogs>
```

### Explanation

* `date`
  The day the access statistics belong to

* `documentId`
  ID of an existing document in the database

* `accessCount`
  Number of accesses for that document on the given day

### Processing Behavior

When this file is placed in:

```text
accesslogs/input/
```

The batch process will:

1. Parse the XML file
2. Match `documentId` values to existing documents
3. Store the daily access counts in PostgreSQL
4. Move the processed file to:

```text
accesslogs/archive/
```

> If a `documentId` does not exist in the database, the batch process will fail for that entry.


---

## Batch Scheduling via Cron (Docker)

The batch processor is **scheduled via cron inside its Docker container**.

The cron job is configured directly in the Dockerfile:

```dockerfile
RUN echo "0 1 * * * dotnet /app/Paperless.BatchProcess.dll | tee -a /var/log/batch.log >> /proc/1/fd/1 2>&1" \
    > /etc/cron.d/batch-cron
```

### What this means

* The batch process runs **every day at 01:00 AM**
* Output is:

  * written to `/var/log/batch.log`
  * forwarded to Docker container logs (visible via `docker compose logs`)

The container runs cron in foreground mode:

```dockerfile
CMD ["cron", "-f"]
```

---

## Testing the Batch Process (Recommended)

For development and testing, you can temporarily change the cron schedule to run **every minute**.

### Change cron expression

Replace:

```text
0 1 * * *
```

with:

```text
* * * * *
```

### Example testing configuration
After saving the changes in Dockerfile (BatchProcess) run these commands in order:
```
docker compose stop paperless_batch
docker compose rm -f paperless_batch
docker compose build paperless_batch
docker compose up -d paperless_batch
```

```dockerfile
RUN echo "* * * * * dotnet /app/Paperless.BatchProcess.dll | tee -a /var/log/batch.log >> /proc/1/fd/1 2>&1" \
    > /etc/cron.d/batch-cron
```

This allows you to:

* Drop an XML file into `accesslogs/input`
* See processing within **one minute**
* Verify:

  * database updates
  * automatic archive folder creation
  * file movement

---

## Batch Logs

Batch logs can be viewed via Docker:

```bash
docker compose logs -f paperless_batch
```

They are also stored inside the container at:

```text
/var/log/batch.log
```
---

## API Documentation (Swagger)

The REST API is documented using **Swagger / OpenAPI**.

After starting the application with Docker, Swagger can be accessed at:

```text
http://localhost:8081/swagger/index.html
```

Swagger provides:

* An overview of **all available REST endpoints**
* Request and response schemas
* The ability to **execute API calls directly from the browser**
* Easy testing of document upload, retrieval, update, and deletion ad more.

> All REST endpoints implemented in the Paperless system are available and documented in Swagger.

---

## Verifying Database Changes (PostgreSQL)

To verify that the batch process has correctly persisted access statistics, you can connect directly to the PostgreSQL database running in Docker.

Run the following command:

```bash
docker exec -it paperless_postgres psql -U dms -d dmsdb
```

Once connected, you can execute SQL queries to inspect the stored data, for example:

```sql
SELECT * FROM "DocumentDailyAccesses";
```

or

```sql
SELECT * FROM "Documents";
```

Type `\q` to exit the PostgreSQL shell.

> This is useful to confirm that the batch process has successfully processed the XML file and stored the daily access counts as expected.

---

## CI/CD Pipeline

> The CI/CD pipeline runs on github via workflows automatically on every push and pull request (to  main) to ensure the application builds and tests successfully.

## Important Notes & Troubleshooting

* At least one document must already exist in the database
  (XML files reference existing document IDs)
* Processed XML files are **not reprocessed** due to archiving
* If nothing happens:

  * Check container logs
  * Verify XML filename pattern
  * Verify document IDs in XML
* If GenAI summaries are missing:

  * Check `.env`
  * Check GenAI worker logs

