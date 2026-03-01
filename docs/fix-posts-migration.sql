-- Option A: If AddPostsTable is in history but posts table is missing
-- (Remove from history so "dotnet ef database update" will re-apply the migration.)
-- SELECT * FROM public."__EFMigrationsHistory" ORDER BY "MigrationId";
-- DELETE FROM public."__EFMigrationsHistory" WHERE "MigrationId" = '20260225200000_AddPostsTable';

-- Option B: If "database update" still does nothing after delete (recommended)
-- Run the full script: docs/apply-posts-migration-manually.sql
-- It creates competitions.posts and inserts the migration row so EF stays in sync.
