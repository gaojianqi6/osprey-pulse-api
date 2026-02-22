This is the **Osprey Pulse Database Schema v1.7 (Full Edition)**. It includes all infrastructure, hierarchical sports data, the "Hupu-style" rating/comment system, the automated point trigger, and performance indexes.

---

### 1. Database Extensions & Types

```sql
-- Required for high-performance deep comment threading
CREATE EXTENSION IF NOT EXISTS ltree;

-- Required for UUID generation
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
```

### 2. Infrastructure & Hierarchy

```sql
CREATE TABLE channels (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    description TEXT,
    slug VARCHAR(20) UNIQUE NOT NULL -- 'nba', 'rugby', 'gaming'
);

CREATE TABLE leagues (
    id SERIAL PRIMARY KEY,
    channel_id INT REFERENCES channels(id),
    name VARCHAR(100) NOT NULL,
    logo_url TEXT
);

CREATE TABLE seasons (
    id SERIAL PRIMARY KEY,
    league_id INT REFERENCES leagues(id),
    label VARCHAR(50) NOT NULL, -- e.g. '2025-26 Regular Season', 'Spring Split'
    is_current BOOLEAN DEFAULT FALSE
);
```

### 3. Teams & Players

```sql
CREATE TABLE teams (
    id SERIAL PRIMARY KEY,
    league_id INT REFERENCES leagues(id),
    name VARCHAR(100) NOT NULL,
    logo_url TEXT,
    description TEXT
);

CREATE TABLE players (
    id SERIAL PRIMARY KEY,
    full_name VARCHAR(100) NOT NULL,
    nationality VARCHAR(50),
    avatar_url TEXT,
    default_position VARCHAR(20)
);

-- Tracks current team assignment; historical assignment is stored in competition_rosters
CREATE TABLE player_team_assignments (
    player_id INT REFERENCES players(id),
    team_id INT REFERENCES teams(id),
    is_active BOOLEAN DEFAULT TRUE,
    joined_date DATE,
    PRIMARY KEY (player_id, team_id)
);
```

### 4. Competitions & Universal Rosters

```sql
CREATE TABLE competitions (
    id SERIAL PRIMARY KEY,
    season_id INT REFERENCES seasons(id),
    home_team_id INT REFERENCES teams(id),
    away_team_id INT REFERENCES teams(id),
    start_time TIMESTAMPTZ,
    status SMALLINT DEFAULT 0, -- 0: Scheduled, 1: Live, 2: Finished, 3: Delayed
    home_score INT DEFAULT 0,
    away_score INT DEFAULT 0,
    metadata JSONB -- League-specific info (e.g. Map Name for LoL, Venue for Rugby)
);

CREATE TABLE competition_rosters (
    id SERIAL PRIMARY KEY,
    competition_id INT REFERENCES competitions(id),
    player_id INT REFERENCES players(id), -- Nullable for Coaches/Referees
    team_id INT REFERENCES teams(id), -- Links player to team for THIS specific game
    role_type SMALLINT, -- 1: Player, 2: Coach, 3: Referee
    lineup_status SMALLINT, -- 1: Starter, 2: Bench (替补), 3: Substituted In
    team_side SMALLINT, -- 1: Home, 2: Away

    -- DYNAMIC STATS: JSONB handles Basketball, Rugby, or LoL stats uniquely
    stats JSONB,

    rating_avg DECIMAL(3,1) DEFAULT 0.0
);
```

### 5. Users & The Economy (Trigger Included)

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    username VARCHAR(50) UNIQUE NOT NULL,
    email VARCHAR(100) UNIQUE NOT NULL,
    osprey_points BIGINT DEFAULT 1000,
    avatar_url TEXT,
    is_admin BOOLEAN DEFAULT FALSE,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

CREATE TABLE point_transaction_types (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL, -- 'DAILY_REWARD', 'BET_WIN', 'BET_WAGER'
    display_name VARCHAR(100)
);

CREATE TABLE point_transactions (
    id BIGSERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(id) NOT NULL,
    type_id INT REFERENCES point_transaction_types(id) NOT NULL,
    amount INT NOT NULL, -- Positive for rewards, negative for wagers
    balance_after BIGINT,
    created_at TIMESTAMPTZ DEFAULT NOW()
);

-- THE POINT GUARDIAN TRIGGER
CREATE OR REPLACE FUNCTION sync_user_points() RETURNS TRIGGER AS $$
BEGIN
    UPDATE users
    SET osprey_points = osprey_points + NEW.amount
    WHERE id = NEW.user_id;

    IF (SELECT osprey_points FROM users WHERE id = NEW.user_id) < 0 THEN
        RAISE EXCEPTION 'Insufficient Osprey Points';
    END IF;

    SELECT osprey_points INTO NEW.balance_after FROM users WHERE id = NEW.user_id;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_sync_points
BEFORE INSERT ON point_transactions
FOR EACH ROW EXECUTE FUNCTION sync_user_points();
```

### 6. Social: Posts, Ratings & Nested Comments

```sql
CREATE TABLE posts (
    id SERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    channel_id INT REFERENCES channels(id),
    title VARCHAR(255) NOT NULL,
    content TEXT,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    last_bumped_at TIMESTAMPTZ DEFAULT NOW() -- For NBA feed sorting
);

CREATE TABLE user_ratings (
    id SERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    roster_item_id INT REFERENCES competition_rosters(id),
    score INT CHECK (score >= 1 AND score <= 10),
    comment_text TEXT, -- The "Top Level" post of the rating
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW(),
    UNIQUE(user_id, roster_item_id)
);

CREATE TABLE comments (
    id SERIAL PRIMARY KEY,
    user_id UUID REFERENCES users(id),
    post_id INT REFERENCES posts(id), -- If replying to a post
    rating_id INT REFERENCES user_ratings(id), -- If replying to a rating
    parent_id INT REFERENCES comments(id),
    path LTREE, -- For deep, fast threaded replies
    content TEXT NOT NULL,
    deleted_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT NOW()
);
```

### 7. Performance Indexes

```sql
-- Feed Performance: Home (Recommend)
CREATE INDEX idx_posts_recommend ON posts (created_at DESC) WHERE deleted_at IS NULL;

-- Feed Performance: NBA (Bump logic)
CREATE INDEX idx_posts_nba_bump ON posts (last_bumped_at DESC)
WHERE channel_id = (SELECT id FROM channels WHERE slug = 'nba') AND deleted_at IS NULL;

-- Comment Threading Performance (GIST index for LTREE)
CREATE INDEX idx_comments_path_gist ON comments USING GIST (path);

-- Competition Lookup for 'Today'
CREATE INDEX idx_competitions_start ON competitions (start_time);

-- Rating lookups per game
CREATE INDEX idx_user_ratings_roster ON user_ratings (roster_item_id);

-- User lookups for Me profile
CREATE INDEX idx_users_username ON users (username);
```

### 8. Key Logic Notes for this Schema:

1. **Universal Stats:** The `competition_rosters.stats` (JSONB) allows you to store NBA points, LoL Kills, or Rugby Tries without changing the table.
2. **The "Hupu" Bump:** The `posts.last_bumped_at` is updated whenever someone comments. This keeps the active NBA debates at the top of the feed.
3. **Threaded Conversations:** The `ltree` in `comments` allows you to fetch an entire "tree" of arguments under a 1/10 rating with a single efficient query.
4. **Points Integrity:** The `trg_sync_points` trigger ensures that no points are added or spent without a permanent record in `point_transactions`, protecting your "Gambling" system.