-- GitHub Copilot Usage Metrics Dashboard - Database Schema

CREATE TABLE users (
    user_login               NVARCHAR(100) NOT NULL PRIMARY KEY,
    display_name             NVARCHAR(200),
    team                     NVARCHAR(100),
    has_seat                 BIT NOT NULL DEFAULT 1,
    seat_assigned_date       DATE,
    last_synced_at           DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE daily_usage (
    user_login                  NVARCHAR(100) NOT NULL,
    date                        DATE NOT NULL,
    is_active                   BIT NOT NULL DEFAULT 0,
    is_engaged                  BIT NOT NULL DEFAULT 0,
    completions_suggestions     INT NOT NULL DEFAULT 0,
    completions_acceptances     INT NOT NULL DEFAULT 0,
    completions_lines_suggested INT NOT NULL DEFAULT 0,
    completions_lines_accepted  INT NOT NULL DEFAULT 0,
    chat_engaged                BIT NOT NULL DEFAULT 0,
    agent_engaged               BIT NOT NULL DEFAULT 0,
    cli_engaged                 BIT NOT NULL DEFAULT 0,
    primary_editor              NVARCHAR(50),
    primary_language            NVARCHAR(50),
    loc_suggested_to_add        INT NOT NULL DEFAULT 0,
    loc_suggested_to_delete     INT NOT NULL DEFAULT 0,
    loc_added                   INT NOT NULL DEFAULT 0,
    loc_deleted                 INT NOT NULL DEFAULT 0,
    interaction_count           INT NOT NULL DEFAULT 0,
    code_generation_count       INT NOT NULL DEFAULT 0,
    code_acceptance_count       INT NOT NULL DEFAULT 0,
    used_chat                   BIT NOT NULL DEFAULT 0,
    used_agent                  BIT NOT NULL DEFAULT 0,
    used_cli                    BIT NOT NULL DEFAULT 0,
    chat_agent_mode_count       INT NOT NULL DEFAULT 0,
    chat_ask_mode_count         INT NOT NULL DEFAULT 0,
    chat_edit_mode_count        INT NOT NULL DEFAULT 0,
    cli_session_count           INT NOT NULL DEFAULT 0,
    cli_request_count           INT NOT NULL DEFAULT 0,
    cli_prompt_tokens           INT NOT NULL DEFAULT 0,
    cli_output_tokens           INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_daily_usage PRIMARY KEY (user_login, date),
    CONSTRAINT FK_daily_usage_user FOREIGN KEY (user_login) REFERENCES users(user_login)
);

CREATE INDEX IX_daily_usage_date ON daily_usage(date);
CREATE INDEX IX_daily_usage_active ON daily_usage(date, is_active) WHERE is_active = 1;

CREATE TABLE daily_aggregate (
    date                 DATE NOT NULL PRIMARY KEY,
    total_active_users   INT NOT NULL DEFAULT 0,
    total_engaged_users  INT NOT NULL DEFAULT 0,
    total_suggestions    INT NOT NULL DEFAULT 0,
    total_acceptances    INT NOT NULL DEFAULT 0,
    acceptance_rate      DECIMAL(5,4) NOT NULL DEFAULT 0
);

CREATE TABLE daily_usage_detail (
    user_login      NVARCHAR(100) NOT NULL,
    date            DATE NOT NULL,
    editor_name     NVARCHAR(50) NOT NULL,
    language_name   NVARCHAR(50) NOT NULL,
    suggestions     INT NOT NULL DEFAULT 0,
    acceptances     INT NOT NULL DEFAULT 0,
    lines_suggested INT NOT NULL DEFAULT 0,
    lines_accepted  INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_daily_usage_detail PRIMARY KEY (user_login, date, editor_name, language_name),
    CONSTRAINT FK_detail_user FOREIGN KEY (user_login) REFERENCES users(user_login)
);
