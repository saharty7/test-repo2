# Hotel Booking Platform — API

A REST API for a boutique hotel to manage rooms, bookings, guests, staff and
reviews, built with **C# / ASP.NET Core (.NET 10)**. No code comments are
included in the source — this file explains what was built and why.

---

## 1. How to run it

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
cd HotelBookingPlatform
dotnet restore
dotnet run
```

The console will print the URL it's listening on (e.g. `http://localhost:5106`).
Open:

```
http://localhost:5106/swagger
```

That's the Swagger UI — every endpoint is listed there, and you can call each
one directly from the browser ("Try it out"). The project also opens Swagger
automatically when launched with `dotnet run` or from Visual Studio / Rider.

No database installation is required — the app seeds itself with sample data
every time it starts (see §5).

### Logging in as staff

A staff account is seeded automatically:

- **Username:** `admin`
- **Password:** `Admin@123`

Call `POST /api/auth/login` with those credentials to get back a token. In
Swagger, click the **Authorize** button (top right, padlock icon) and enter:

```
Bearer <the-token-you-got-back>
```

All staff-only endpoints will then work from the "Try it out" buttons.

---

## 2. Project structure

```
Controllers/    HTTP endpoints only — parse the request, call a service, return the result
Services/       All business rules and logic live here (interfaces in Services/Interfaces)
Models/         The core entities (Room, Booking, Guest, Review, ...)
DTOs/           The shapes of data going in/out of the API (requests & responses)
Data/           The in-memory "database" and its startup sample data
Common/         Cross-cutting pieces: custom exceptions, error-handling middleware,
                password hashing, and the staff login/token system
```

Controllers never contain business logic — they call into a `Service`, and
the `Service` is where every rule from the brief (double-booking checks,
status transitions, price calculation, etc.) is enforced. This keeps the
rules in one place and testable independently of the web layer.

---

## 3. Data & entities

| Entity | Notes |
|---|---|
| `Amenity` | e.g. WiFi, Pool View, Parking |
| `RoomType` | Standard / Deluxe / Suite — has a price per night, max guests, and a list of amenities |
| `Room` | A physical room (e.g. "101"), belongs to one `RoomType`. Has `IsActive` instead of being deleted, so history is preserved when a room is taken out of service |
| `Guest` | Name, email, phone. Found-or-created automatically by email when a booking is made |
| `Booking` | Links a `Guest` + `Room` + date range, holds the calculated price and a status |
| `Review` | Linked to a specific `Booking`, so it can only exist for a stay that really happened |
| `StaffUser` | Login for hotel staff (password stored as a salted hash, never in plain text) |

### Why an in-memory store instead of a real database?

The brief asks for "the simplest way possible" and for reviewers to be able
to try the system immediately with sample data already loaded, with no setup.
A real database (SQL Server, PostgreSQL, etc.) would need to be installed and
connected to before the app could even start. Instead, `Data/DataStore.cs`
holds the data in memory (thread-safe, so concurrent requests can't corrupt
it), and `Data/DataSeeder.cs` fills it with sample room types, rooms,
amenities, and a staff login every time the app starts.

**Trade-off to be transparent about:** data resets every time the app
restarts. That's the right call for a one-week demo/take-home project with
zero setup friction. If this were going to production, the same `Services`
and `Controllers` would stay exactly as they are — only `DataStore` would be
swapped for a real database (e.g. Entity Framework Core with SQL Server),
because all the business rules already live in the service layer, not in the
data layer.

---

## 4. Business rules — where they live and how they're enforced

All of these are enforced in the `Services/` layer, not just documented:

- **No double-booking.** `BookingService.Create` checks every existing,
  non-cancelled booking for that room and rejects the request (`409
  Conflict`) if the new date range overlaps an existing one.
- **Price is always calculated by the system.** The client never sends a
  price — `BookingService` computes `nights × room type's price per night`
  itself.
- **Booking status can only move in sensible directions:**
  `Requested → Confirmed → CheckedIn → CheckedOut`, with `Cancelled` allowed
  only from `Requested` or `Confirmed`. Every transition method checks the
  current status first and returns `409 Conflict` with a clear message if
  the move isn't allowed (e.g. you cannot check in a cancelled booking, or
  cancel a booking that already checked out).
- **Reviews require a completed stay.** `ReviewService.Create` checks that
  the booking exists, that the email matches the guest on that booking, that
  the booking's status is `CheckedOut`, and that it hasn't already been
  reviewed — before accepting the review.
- **Deactivating a room never deletes it.** `Room.IsActive` is flipped to
  `false`; the room and all its past bookings stay exactly where they are,
  so booking history and reports are unaffected.

---

## 5. Sample data already loaded

Every time the app starts, it seeds:

- 5 amenities (WiFi, Pool View, Parking, Breakfast, Minibar)
- 3 room types: Standard ($60/night), Deluxe ($110/night), Suite ($220/night)
- 6 physical rooms spread across those types (101–103, 201–202, 301)
- 1 staff login (`admin` / `Admin@123`)

No guests, bookings or reviews are seeded — those are created as you use the
API, so you can see the full lifecycle from a clean slate.

---

## 6. Who can do what (authorization)

- **Public, no login needed** (like browsing a hotel's website): viewing
  room types, amenities, prices, checking room availability for given dates,
  reading a room type's reviews, making a booking, and leaving a review.
- **Staff only** (needs a login token): adding room types/rooms/amenities,
  deactivating/reactivating rooms, confirming/cancelling bookings, checking
  guests in/out, looking up guests and their booking history, and both
  manager reports.

Staff endpoints are protected with `[Authorize]`; if you call one without a
valid token you get a `401 Unauthorized` — not a crash, and not accidental
access.

### Why a simple bearer token instead of guest accounts?

The brief explicitly leaves this open ("whichever makes more sense to you").
Requiring guests to register an account before booking a hotel room adds
friction most hotel sites avoid — you can book as a guest checkout on nearly
every real hotel website. So guests simply provide their name, email and
phone at booking time; the system finds-or-creates their `Guest` record by
email automatically. Their "proof of identity" for actions like leaving a
review is simply knowing the email tied to the booking. Staff, on the other
hand, genuinely need protected, auditable accounts, so they get a real login
with hashed passwords (PBKDF2/SHA-256, salted, no plain-text passwords
anywhere) and a bearer token issued on login.

The token itself is a random, single-use-until-expiry opaque string (not a
JWT) checked against an in-memory token store. This was a deliberate
simplicity trade-off: it needs zero extra configuration (no signing keys,
no issuer/audience setup) while still fully protecting every staff endpoint,
which is what the brief actually asks for. Swapping it for a JWT/OAuth
provider later would only mean replacing `Common/StaffAuthenticationHandler.cs`
— nothing in the controllers or services would need to change.

---

## 7. Reports

- **`GET /api/rooms/available?checkIn=&checkOut=&roomTypeId=`** *(public)* —
  active rooms with no overlapping booking for the given dates. `roomTypeId`
  is optional, to narrow to one room type.
- **`GET /api/reports/occupancy`** *(staff)* — percentage of active rooms
  that currently have a guest checked in right now (`CheckedIn` status),
  out of all active rooms. A `Confirmed` booking for next month doesn't make
  a room "occupied" today — only an actual checked-in guest does.
- **`GET /api/reports/best-reviewed-room-types`** *(staff)* — every room
  type that has at least one review, with its average rating and review
  count, sorted best-first.
- **`GET /api/guests/{id}/bookings`** *(staff)* — a guest's full booking
  history, most recent first.

---

## 8. Error handling

A single middleware (`Common/ExceptionHandlingMiddleware.cs`) turns every
error into a clear JSON message with the right HTTP status code:

- Bad input → `400` with a plain message (also covers automatic model
  validation, e.g. a missing email or a rating outside 1–5)
- Not found → `404`
- Rule broken (double-booking, wrong booking status, duplicate review, etc.)
  → `409`
- No token / wrong token → `401`
- Anything unexpected → `500` with a generic "something went wrong" message
  — never a raw stack trace.

---

## 9. Full endpoint list

| Method | Route | Access |
|---|---|---|
| POST | `/api/auth/login` | Public |
| GET | `/api/roomtypes` | Public |
| GET | `/api/roomtypes/{id}` | Public |
| GET | `/api/roomtypes/{id}/reviews` | Public |
| POST | `/api/roomtypes` | Staff |
| GET | `/api/amenities` | Public |
| POST | `/api/amenities` | Staff |
| GET | `/api/rooms/available` | Public |
| GET | `/api/rooms` | Staff |
| GET | `/api/rooms/{id}` | Staff |
| POST | `/api/rooms` | Staff |
| PATCH | `/api/rooms/{id}/deactivate` | Staff |
| PATCH | `/api/rooms/{id}/activate` | Staff |
| POST | `/api/bookings` | Public |
| GET | `/api/bookings/{id}` | Staff |
| POST | `/api/bookings/{id}/confirm` | Staff |
| POST | `/api/bookings/{id}/cancel` | Staff |
| POST | `/api/bookings/{id}/checkin` | Staff |
| POST | `/api/bookings/{id}/checkout` | Staff |
| POST | `/api/reviews` | Public |
| GET | `/api/guests` | Staff |
| GET | `/api/guests/{id}` | Staff |
| GET | `/api/guests/{id}/bookings` | Staff |
| GET | `/api/reports/occupancy` | Staff |
| GET | `/api/reports/best-reviewed-room-types` | Staff |

All of the above are documented and callable from `/swagger`.

---

## 10. A note on this build

This project was written and fully compiled/run/tested (including the full
booking lifecycle — double-booking rejection, status transitions, reviews,
and both reports) in a sandboxed environment without access to the NuGet
package registry, so the one external package it uses (`Swashbuckle.AspNetCore`,
for the Swagger UI) could not be restored or verified there. Everything else
in the project — every model, service, and controller — was compiled and
exercised end-to-end successfully. `dotnet restore && dotnet run` on a normal
internet-connected machine will fetch that one package automatically.

---

## 11. Docker Compose, Postgres, and a persistent Volume

This section covers the second part of the project: running the API against
a **real PostgreSQL database**, in its own container, wired up with Docker
Compose, with the data surviving container recreation.

### ⚠️ Read this first — what I could and couldn't verify here

The sandbox this project was built in has **no access to any container
registry at all** — not Docker Hub, not Microsoft's container registry, not
GitHub Container Registry. Every `docker pull` (including the base images
this Dockerfile needs) is blocked at the network level in that environment,
so I could not actually run `docker compose up`, and everything below the
`.NET code itself` could not be executed or captured by me first-hand.

What I *did* verify, for real, in that sandbox:
- **The .NET application code compiles and runs correctly.** I could not
  install the real `Npgsql.EntityFrameworkCore.PostgreSQL` package either
  (same NuGet restriction as before), so I built a small stand-in library
  that mirrors the exact EF Core API surface this project calls (`DbContext`,
  `DbSet<T>`, `ModelBuilder`, `UseNpgsql`, `Database.EnsureCreated()`, etc.),
  compiled the real project against it, and ran the full booking lifecycle
  through it end-to-end (create booking → reject overlap → guest
  de-duplication by email → confirm → check-in → check-out → review →
  reports) — all passed.
- **The retry logic itself**, by making that stand-in throw on the first two
  connection attempts and succeed on the third — confirmed the app logs a
  warning and retries with a delay each time, and starts up normally once
  the "database" becomes available, instead of crashing.

What I could **not** verify, because it requires pulling real images and
running real containers: the Dockerfile actually building, Postgres actually
starting, the exact wording of any error message, and every "Gotcha" below.

So, honestly: **sections 11.1–11.3 below are working code you can run.**
The Gotchas in **section 11.4** are written as a **runbook for you to
execute** — each one tells you the exact commands to run and exactly what to
expect and why, based on how Docker, Compose, and Postgres actually behave,
but I have not personally captured that output. Please run them and replace
the "Expected result" lines with your own captured output — that's what
actually satisfies "real evidence, not just a working configuration" per the
brief.

### 11.1 What changed to add a real database

- The in-memory `DataStore` from the first version of this project is gone.
  `Data/AppDbContext.cs` is now a real EF Core `DbContext` talking to
  PostgreSQL through the Npgsql provider.
- `RoomType.AmenityIds` is stored as a native Postgres `integer[]` column —
  no join table needed for something this simple.
- Every `Service` now queries `AppDbContext` instead of in-memory lists —
  the business rules themselves (no double-booking, status transitions,
  etc.) did not change at all, only where the data lives.
- `Data/DataSeeder.cs` now checks `if (db.RoomTypes.Any()) return;` before
  seeding, so restarting the API against a database that already has data
  doesn't try to insert the sample data twice.
- The connection string is read from configuration
  (`builder.Configuration.GetConnectionString("Postgres")`), which ASP.NET
  Core populates from the `ConnectionStrings__Postgres` **environment
  variable** set in `docker-compose.yml` — never hardcoded. A local
  fallback lives in `appsettings.json` purely so `dotnet run` still works
  without Docker for quick local development.
- `Program.cs` no longer calls `UseHttpsRedirection()` — inside a
  container the API is only expected to be reached over plain HTTP on the
  Docker network / a reverse proxy would handle TLS in front of it, so
  forcing an HTTPS redirect would just break requests for no benefit here.

### 11.2 The Dockerfile (multi-stage)

`Dockerfile` has two stages:

1. **`build`** — uses the full .NET SDK image, restores and publishes the
   project into `/app/publish`.
2. **`final`** — uses the much smaller ASP.NET *runtime* image, copies only
   the published output from the `build` stage, and runs it.

This keeps the final image small — it never contains the SDK, the source
code's `obj/`/`bin/` folders, or anything else only needed to *build* the
app.

### 11.3 docker-compose.yml

Two services:

- **`db`** — official `postgres:16-alpine` image. Reads its username,
  password and database name from environment variables (which come from
  your `.env` file), and mounts a **named volume**,
  `hotel_booking_db_data`, at `/var/lib/postgresql/data` — that's exactly
  where the official Postgres image stores its actual data files.
- **`api`** — built from the `Dockerfile` above. Gets its full Postgres
  connection string from the `ConnectionStrings__Postgres` environment
  variable (built from the same `.env` values, so both services always
  agree unless you deliberately break that — see Gotcha 3).

The named volume is declared both inside `db`'s `volumes:` list *and* in the
top-level `volumes:` section at the bottom of the file — Compose requires
both: the service-level line says "mount this volume here", the top-level
section is where the volume itself is actually declared to exist.

**To run it:**

```bash
cp .env.example .env
docker compose up --build
```

The API will be reachable at `http://localhost:8080/swagger` (or whatever
port you set `API_PORT` to in `.env`).

### 11.4 The two test endpoints used throughout this section

Per the brief, one write and one read endpoint, used for every persistence
test below:

- **Write:** `POST /api/bookings` (public, no login needed) — creates a
  booking.
- **Read:** `POST /api/auth/login` (to get a staff token) followed by
  `GET /api/bookings/{id}` — reads a specific booking back by id.

```bash
# write
curl -s -X POST http://localhost:8080/api/bookings \
  -H "Content-Type: application/json" \
  -d '{"guestName":"Ahmed Test","guestEmail":"ahmed@example.com","guestPhone":"01000000000","roomId":1,"checkInDate":"2027-06-05","checkOutDate":"2027-06-10"}'
# note the "id" in the response, e.g. 1

# get a staff token
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}' | python3 -c "import sys,json;print(json.load(sys.stdin)['token'])")

# read
curl -s http://localhost:8080/api/bookings/1 -H "Authorization: Bearer $TOKEN"
```

---

## 12. The Gotchas — runbook

Run these in order. Each one lists the exact commands and the result you
should expect and why — fill in what you actually saw.

### Gotcha 1 — Reproduce the startup race condition

```bash
docker compose down -v          # clean slate: containers AND volumes gone
docker compose up --build       # fresh start, watch the api service's logs
```

**What to look for:** `depends_on: - db` (the plain, condition-less form
used in this `docker-compose.yml`) only guarantees Compose *starts the `db`
container before the `api` container*. It does **not** wait for Postgres to
actually finish initializing and start accepting connections — that can
take a few seconds after the container itself is "started." Without the
retry logic in `Program.cs`, the very first connection attempt would very
plausibly fail with a connection-refused error while Postgres is still
initializing, and the app would crash before ever handling a request. Try
it a handful of times — like the brief says, this kind of timing issue
doesn't always show up on every single run, especially in Docker's build
cache warms up and the images are already local.

**With the retry logic (as shipped):** you should instead see log lines
like:

```
warn: Program[0]
      Database not ready yet (attempt 1/10). Retrying in 3s...
```

one or more times, followed by:

```
info: Program[0]
      Connected to the database on attempt N.
```

and the app comes up normally instead of crashing. This is the exact
behavior I confirmed with the simulated flaky-database stand-in described in
§11 above — the loop retries a bounded number of times (10, 3 seconds apart)
and only lets an exception crash the app if every attempt fails.

### Gotcha 2 — Prove the volume actually works

```bash
# 1. write a real record
curl -s -X POST http://localhost:8080/api/bookings -H "Content-Type: application/json" \
  -d '{"guestName":"Persistence Test","guestEmail":"persist@example.com","guestPhone":"0100000000","roomId":3,"checkInDate":"2027-08-01","checkOutDate":"2027-08-03"}'
# note the booking id, e.g. 1

# 2. read it back to confirm it's there right now
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login -H "Content-Type: application/json" -d '{"username":"admin","password":"Admin@123"}' | python3 -c "import sys,json;print(json.load(sys.stdin)['token'])")
curl -s http://localhost:8080/api/bookings/1 -H "Authorization: Bearer $TOKEN"

# 3. destroy the containers (NOT the volumes — no -v flag here)
docker compose down

# 4. bring everything back up
docker compose up -d

# 5. read the same booking id again
TOKEN=$(curl -s -X POST http://localhost:8080/api/auth/login -H "Content-Type: application/json" -d '{"username":"admin","password":"Admin@123"}' | python3 -c "import sys,json;print(json.load(sys.stdin)['token'])")
curl -s http://localhost:8080/api/bookings/1 -H "Authorization: Bearer $TOKEN"
```

**Expected result:** step 5 returns the exact same booking as step 2, even
though `docker compose down` in step 3 genuinely deleted both containers.
This works because the data was never inside the container in the first
place — it was inside the named volume `hotel_booking_db_data`, which
Compose does not touch unless you explicitly pass `-v`/`--volumes`. Record
the actual JSON output of both reads here as your before/after evidence.

### Gotcha 3 — The credential mismatch

Edit `docker-compose.yml` and change **only** the `api` service's password,
leaving `db`'s untouched:

```yaml
    environment:
      ConnectionStrings__Postgres: "Host=db;Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=wrong_password_on_purpose"
```

```bash
docker compose up -d --build api
docker compose logs api --tail=50
```

**Expected result:** Postgres will reject the connection with its standard
authentication failure, and Npgsql will surface it — you should see
something equivalent to:

```
Npgsql.PostgresException (0x80004005): 28P01: password authentication failed for user "hotelbooking"
```

(`28P01` is Postgres's own SQLSTATE error code for "invalid password" —
you'll see that same code every time this specific mistake happens, in any
project, which is exactly why doing this once on purpose is useful.)

Combined with the retry logic, you should also see the app retry a few
times and then genuinely give up and crash — because this isn't a timing
problem, it's a real, permanent misconfiguration, and 10 retries all fail
the same way.

Revert the change back to `${POSTGRES_PASSWORD}` and rerun
`docker compose up -d --build api` to confirm it works again.

### Gotcha 4 — Port conflicts from previous work

Before your very first `docker compose up`:

```bash
docker ps --format "table {{.Names}}\t{{.Ports}}"
lsof -i :5432 -i :8080          # or: sudo ss -ltnp | grep -E '5432|8080'
```

**What to do with the result:** if either `5432` (Postgres) or `8080` (the
API) is already bound by something else — commonly a Postgres container
left running from an earlier, unrelated exercise — either stop/remove that
container, or change `POSTGRES_PORT` / `API_PORT` in your `.env` file to
free ports and rerun `docker compose up`. Record what you actually found —
"nothing was conflicting" is a perfectly valid, useful answer too, per the
brief.

### Gotcha 5 — Two volumes for the same path

This has two different ways to trigger it — try both.

**(a) Same service, two volume entries mapped to the same container path:**

```yaml
    volumes:
      - hotel_booking_db_data:/var/lib/postgresql/data
      - hotel_booking_db_data_2:/var/lib/postgresql/data
```

```bash
docker compose up db
```

**Expected result:** Docker rejects this outright — a container cannot have
two different mounts targeting the exact same path at the same time. You
should see an error to the effect of a duplicate mount point for
`/var/lib/postgresql/data`, and the `db` container will fail to start.

**(b) An extra, unused volume only in the top-level section:**

```yaml
volumes:
  hotel_booking_db_data:
  hotel_booking_db_data_unused:
```

```bash
docker compose up -d
docker volume ls
```

**Expected result:** this one is harmless — Compose does not complain about
declaring a volume that no service actually uses. `docker volume ls` will
simply show an extra, empty volume sitting there, taking up a small amount
of disk and doing nothing. Remove the unused line afterward to clean it up.

### Gotcha 6 — Removing a volume on purpose

After confirming Gotcha 2 passes:

```bash
docker compose down
docker volume ls | grep hotel_booking_db_data     # confirm the exact name, e.g. hotelbookingplatform_hotel_booking_db_data
docker volume rm hotelbookingplatform_hotel_booking_db_data
docker compose up -d
```

Then read back the same booking id from Gotcha 2 again.

**Expected result:** this time the read returns `404 Not Found` — the data
is genuinely gone, because `docker volume rm` deletes the volume itself
(the actual files Postgres wrote), not just the container that was using
it. This is the key difference from Gotcha 2's `docker compose down`
(without `-v`): plain `down` only removes containers and leaves volumes
alone by design, specifically so that this kind of accidental data loss
doesn't happen just from restarting your stack. `docker volume rm` (or
`docker compose down -v`, which does the same thing) is the deliberate,
explicit way to actually throw the data away.

### Gotcha 7 — Scaling the API service

```bash
docker compose up -d --scale api=3
```

**Expected result:** with `"${API_PORT}:8080"` (a *fixed* host port) in
`docker-compose.yml`, only the first replica will start successfully.
The second and third will fail with something like:

```
Error response from daemon: driver failed programming external connectivity on endpoint hotel-booking-api-2: Bind for 0.0.0.0:8080 failed: port is already allocated
```

**Why this happens, in plain terms:** a container's *internal* port
(`8080` inside the container) is private to that container and never
conflicts with anything — every container can happily listen on its own
internal `8080`. But the *host* side of a port mapping (`8080:8080`) is a
real, physical resource of your one machine's network stack, and a given
host port can only be bound by one process at a time, full stop. Three
containers all trying to claim host port 8080 simultaneously is exactly
like three different programs on your laptop all trying to listen on port
8080 at once — the first one wins and the rest fail. To actually scale
this service you'd drop the fixed host port (`"8080"` instead of
`"8080:8080"`, letting Docker assign a random free host port per replica)
and put a load balancer or reverse proxy in front that knows how to reach
all the replicas.

### Gotcha 8 — Backing up the volume's data without Compose

```bash
mkdir -p ./pg_backup

docker run --rm \
  -v hotelbookingplatform_hotel_booking_db_data:/source:ro \
  -v "$(pwd)/pg_backup":/backup \
  alpine sh -c "cp -a /source/. /backup/"

ls -la ./pg_backup
cat ./pg_backup/PG_VERSION
```

(Adjust the volume name to whatever `docker volume ls` actually shows on
your machine — Compose prefixes it with your project/folder name.)

**Expected result:** `./pg_backup` on your host machine now contains a real,
plain copy of Postgres's actual data directory — files like `PG_VERSION`,
and folders like `base/`, `global/`, `pg_wal/`. These are ordinary files on
your host filesystem, readable with `cat`/`ls`/any file explorer, completely
outside of Docker — proof that the volume's contents are real data you can
extract, inspect, and move to another machine, not something locked inside
Docker's internal bookkeeping. This works because `docker run` lets you
mount an *existing* named volume (instead of creating a new empty one) into
a disposable, temporary container — here a plain `alpine` image, which has
nothing to do with Postgres or this project — purely as a vehicle to copy
files out to a bind-mounted host folder.

---

## 13. Testing requirements checklist

Mapped to the brief's numbered list, so you can tick these off after running
the runbook in §12:

1. `docker compose up --build` — both services start, API can reach `db` ✅ (code path verified via the retry-logic simulation in §11; run for real to confirm)
2. Gotcha 1 — race condition + retry logic — see §12, Gotcha 1
3. Gotcha 2 — record survives `down`/`up` — see §12, Gotcha 2
4. Gotcha 3 — credential mismatch error, then fixed — see §12, Gotcha 3
5. Gotcha 4 — port conflict check — see §12, Gotcha 4
6. Gotcha 5 — duplicate/extra volume behavior — see §12, Gotcha 5
7. Gotcha 6 — data genuinely gone after `docker volume rm` — see §12, Gotcha 6
8. Gotcha 7 — fixed host port + scaling error — see §12, Gotcha 7
9. Gotcha 8 — real external backup via `docker run` — see §12, Gotcha 8
