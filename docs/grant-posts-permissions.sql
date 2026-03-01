-- Fix: "42501: permission denied for table posts"
-- Run this in Supabase SQL editor (as owner of the table or superuser).
-- Replace "postgres" with the role in your app's connection string if different.

GRANT ALL ON competitions.posts TO postgres;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA competitions TO postgres;
