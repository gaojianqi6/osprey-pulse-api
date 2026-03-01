-- Use the role from your app's connection string (often "postgres")
GRANT ALL ON competitions.posts TO postgres;
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA competitions TO postgres;