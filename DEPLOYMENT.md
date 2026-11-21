# Deploying Momentify API to Google Cloud

This project runs as a containerized ASP.NET Core API on Google Cloud Run. It uses Neon PostgreSQL for data and Google Cloud Storage for uploaded photos.

## Current deployment

- Google Cloud project: `momentify-api-56566a`
- Region: `asia-south1` (Mumbai)
- Cloud Run service: `momentify-api`
- Public API URL: `https://momentify-api-741539812410.asia-south1.run.app`
- Health endpoint: `https://momentify-api-741539812410.asia-south1.run.app/health`
- Photo bucket: `gs://momentify-photos-6fe24da7`
- Database: Neon PostgreSQL

The health endpoint should return:

```json
{"status":"healthy"}
```

## Prerequisites

Install and authenticate the Google Cloud CLI:

```bash
brew install --cask gcloud-cli
gcloud auth login
gcloud config set project momentify-api-56566a
```

A Google Cloud billing account must be linked to the project. Verify billing with:

```bash
gcloud billing projects describe momentify-api-56566a
```

`BILLING_ENABLED` should be `True`.

The project uses a Dockerfile, so Docker does not need to run locally for deployment. Cloud Build creates the container image.

## Required Google Cloud APIs

Enable the APIs needed by Cloud Run, builds, secrets, storage, and IAM:

```bash
gcloud services enable \
  run.googleapis.com \
  cloudbuild.googleapis.com \
  artifactregistry.googleapis.com \
  secretmanager.googleapis.com \
  storage.googleapis.com \
  iam.googleapis.com
```

## Runtime configuration

The Cloud Run service uses Secret Manager for sensitive values:

- `DATABASE_URL`: stored in the `database-url` secret
- `Jwt__Key`: stored in the `jwt-key` secret
- `GCS_BUCKET`: set to `momentify-photos-6fe24da7`

The service does not need credentials such as a JSON key file. Its runtime service account authenticates to Google Cloud using Application Default Credentials.

## Deploying code changes

From the project directory:

```bash
cd /Users/samthakkar/Code/momentify-backend

gcloud config set project momentify-api-56566a

gcloud run deploy momentify-api \
  --region=asia-south1 \
  --source=. \
  --allow-unauthenticated \
  --set-env-vars="GCS_BUCKET=momentify-photos-6fe24da7" \
  --set-secrets="DATABASE_URL=database-url:latest,Jwt__Key=jwt-key:latest"
```

This command:

1. Uploads the source code.
2. Builds the Docker image with Cloud Build.
3. Stores the image in Artifact Registry.
4. Creates a Cloud Run revision.
5. Routes all traffic to the new revision.

Verify the deployment:

```bash
curl --fail https://momentify-api-741539812410.asia-south1.run.app/health
```

Get the current URL if needed:

```bash
gcloud run services describe momentify-api \
  --region=asia-south1 \
  --project=momentify-api-56566a \
  --format="value(status.url)"
```

## Applying database migrations

Use the Neon direct, non-pooled connection for schema migrations. The direct URL is stored locally as `DATABASE_URL_UNPOOLED` in the ignored `.env.local` file.

```bash
cd /Users/samthakkar/Code/momentify-backend

export DATABASE_URL="$(python3 - <<'PY'
from pathlib import Path
for line in Path('.env.local').read_text().splitlines():
    if line.startswith('DATABASE_URL_UNPOOLED='):
        print(line.split('=', 1)[1].strip().strip('"\''), end='')
        break
PY
)"

export Jwt__Key="$(gcloud secrets versions access latest \
  --project=momentify-api-56566a \
  --secret=jwt-key)"

export PATH="$PATH:$HOME/.dotnet/tools"
dotnet ef database update --context AppDbContext
```

Create a migration and apply it:

```bash
dotnet ef migrations add MigrationName
dotnet ef database update --context AppDbContext
```

For production, apply migrations before deploying a backend version that depends on the new schema.

## Updating secrets

Update the database secret:

```bash
gcloud secrets versions add database-url \
  --project=momentify-api-56566a \
  --data-file=database-url.txt
```

The file must contain only the Neon connection string and should be deleted afterward:

```bash
rm database-url.txt
```

Generate and update the JWT signing key:

```bash
python3 -c 'import secrets; print(secrets.token_urlsafe(64), end="")' |
  gcloud secrets versions add jwt-key \
    --project=momentify-api-56566a \
    --data-file=-
```

Cloud Run needs the next deployment to receive the latest secret version. Redeploy afterward:

```bash
gcloud run deploy momentify-api \
  --region=asia-south1 \
  --source=. \
  --allow-unauthenticated \
  --set-env-vars="GCS_BUCKET=momentify-photos-6fe24da7" \
  --set-secrets="DATABASE_URL=database-url:latest,Jwt__Key=jwt-key:latest"
```

Rotating `Jwt__Key` invalidates JWT tokens issued with the previous key.

## CORS configuration

CORS is currently configured to allow all origins on Cloud Run because `Cors__AllowedOrigins` is empty in the deployment.

To restrict browser access to specific frontend domains:

```bash
gcloud run services update momentify-api \
  --project=momentify-api-56566a \
  --region=asia-south1 \
  --set-env-vars="GCS_BUCKET=momentify-photos-6fe24da7,Cors__AllowedOrigins=https://example.com,https://www.example.com"
```

Origins must include the protocol and must not include a path.

CORS only restricts browser-based JavaScript access. Endpoint authorization is controlled separately with JWT authentication and `[Authorize]` attributes.

## Photo storage

New photos are uploaded to:

```text
gs://momentify-photos-6fe24da7/uploads/{userId}/{generated-name}.{extension}
```

The photo object key is saved in the database as `StorageKey`. The API streams photos from Cloud Storage; storage objects are not publicly accessible by default.

The runtime service account has `roles/storage.objectAdmin` on the photo bucket.

Existing photos stored locally in `App_Data/uploads` are not migrated automatically.

## Viewing logs

```bash
gcloud run services logs read momentify-api \
  --project=momentify-api-56566a \
  --region=asia-south1
```

Open deployment details in the Cloud Console:

```text
https://console.cloud.google.com/run/detail/asia-south1/momentify-api?project=momentify-api-56566a
```

## Rollbacks

List revisions:

```bash
gcloud run revisions list \
  --project=momentify-api-56566a \
  --region=asia-south1 \
  --service=momentify-api
```

Route 100 percent of traffic to a previous revision:

```bash
gcloud run services update-traffic momentify-api \
  --project=momentify-api-56566a \
  --region=asia-south1 \
  --to-revisions=REVISION_NAME=100
```

Database migrations are not automatically reversed by a rollback. Reverse the schema change separately before routing traffic back if required.

## Secrets and files that must not be committed

Never commit:

- `.env.local`
- database credentials
- JWT signing keys
- Neon connection strings
- Google credentials or service-account JSON files
- temporary database or secret files

The `.env.local` and `.env.*` patterns are ignored by Git. The project also contains a `.gitignore` entry for database and local upload files.

## Local development

Run locally with:

```bash
dotnet run
```

The local environment loader reads `.env.local` only when `ASPNETCORE_ENVIRONMENT=Development`. If `GCS_BUCKET` or `PhotoStorage:Bucket` is not configured, local photo uploads continue to use `App_Data/uploads`.

