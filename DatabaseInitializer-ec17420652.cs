using System;
using System.IO;
using System.Data.SQLite;
using Dapper;
using BCrypt.Net;

namespace YaotouPartyManager.Data
{
    /// <summary>
    /// 数据库初始化类 - 负责创建数据库、表结构和初始数据
    /// </summary>
    public static class DatabaseInitializer
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "YaotouParty.db");
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public static void Initialize()
        {
            // 确保Data目录存在
            var dataDir = Path.GetDirectoryName(DbPath);
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }

            // 创建数据库连接
            using var connection = new SQLiteConnection(ConnectionString);
            connection.Open();

            // 创建表结构
            CreateTables(connection);

            // 初始化默认数据
            InitializeDefaultData(connection);

            connection.Close();
        }

        /// <summary>
        /// 创建所有数据表
        /// </summary>
        private static void CreateTables(SQLiteConnection connection)
        {
            // 1. 用户表
            var createUsersTable = @"
                CREATE TABLE IF NOT EXISTS users (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    password_hash VARCHAR(255) NOT NULL,
                    real_name VARCHAR(50),
                    role VARCHAR(20) DEFAULT '普通用户',
                    is_active INTEGER DEFAULT 1,
                    last_login_time DATETIME,
                    created_time DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            connection.Execute(createUsersTable);

            // 2. 党员信息表
            var createPartyMembersTable = @"
                CREATE TABLE IF NOT EXISTS party_members (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name VARCHAR(50) NOT NULL,
                    branch VARCHAR(100),
                    team VARCHAR(50),
                    gender VARCHAR(2) NOT NULL,
                    ethnicity VARCHAR(20),
                    education VARCHAR(50),
                    id_card VARCHAR(18) UNIQUE,
                    age INTEGER,
                    join_date DATE,
                    regular_date DATE,
                    phone VARCHAR(20),
                    occupation VARCHAR(50),
                    work_unit VARCHAR(100),
                    father_name VARCHAR(50),
                    mother_name VARCHAR(50),
                    village_position VARCHAR(50),
                    remarks TEXT,
                    status VARCHAR(20) DEFAULT '正式党员',
                    is_deleted INTEGER DEFAULT 0,
                    created_by VARCHAR(50),
                    created_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                    updated_by VARCHAR(50),
                    updated_time DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            connection.Execute(createPartyMembersTable);

            // 3. 党员信息修改历史表
            var createPartyMemberHistoryTable = @"
                CREATE TABLE IF NOT EXISTS party_member_history (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    operation_type VARCHAR(20) NOT NULL,
                    operation_content TEXT,
                    operator VARCHAR(50),
                    operation_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (member_id) REFERENCES party_members(id)
                );";
            connection.Execute(createPartyMemberHistoryTable);

            // 4. 党费缴纳记录表
            var createPartyDuesTable = @"
                CREATE TABLE IF NOT EXISTS party_dues (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    payment_year INTEGER,
                    payment_month INTEGER,
                    amount DECIMAL(10,2),
                    payment_date DATE,
                    payment_method VARCHAR(20),
                    operator VARCHAR(50),
                    remarks TEXT,
                    created_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (member_id) REFERENCES party_members(id)
                );";
            connection.Execute(createPartyDuesTable);

            // 5. 组织关系转接记录表
            var createOrganizationTransferTable = @"
                CREATE TABLE IF NOT EXISTS organization_transfer (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    member_id INTEGER NOT NULL,
                    transfer_type VARCHAR(20),
                    original_org VARCHAR(100),
                    target_org VARCHAR(100),
                    transfer_date DATE,
                    transfer_reason TEXT,
                    operator VARCHAR(50),
                    status VARCHAR(20) DEFAULT '进行中',
                    remarks TEXT,
                    created_time DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (member_id) REFERENCES party_members(id)
                );";
            connection.Execute(createOrganizationTransferTable);

            // 6. 党内活动记录表
            var createPartyActivitiesTable = @"
                CREATE TABLE IF NOT EXISTS party_activities (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    activity_name VARCHAR(100) NOT NULL,
                    activity_type VARCHAR(50),
                    activity_date DATETIME,
                    location VARCHAR(100),
                    organizer VARCHAR(50),
                    content TEXT,
                    created_time DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            connection.Execute(createPartyActivitiesTable);

            // 7. 活动参与记录表
            var createActivityParticipationTable = @"
                CREATE TABLE IF NOT EXISTS activity_participation (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    activity_id INTEGER NOT NULL,
                    member_id INTEGER NOT NULL,
                    check_in_time DATETIME,
                    attendance_status VARCHAR(20),
                    remarks TEXT,
                    FOREIGN KEY (activity_id) REFERENCES party_activities(id),
                    FOREIGN KEY (member_id) REFERENCES party_members(id)
                );";
            connection.Execute(createActivityParticipationTable);

            // 8. 操作日志表
            var createOperationLogsTable = @"
                CREATE TABLE IF NOT EXISTS operation_logs (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    user_name VARCHAR(50),
                    operation_type VARCHAR(50),
                    operation_module VARCHAR(50),
                    operation_detail TEXT,
                    ip_address VARCHAR(50),
                    operation_time DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            connection.Execute(createOperationLogsTable);

            // 创建索引
            CreateIndexes(connection);
        }

        /// <summary>
        /// 创建索引以提高查询性能
        /// </summary>
        private static void CreateIndexes(SQLiteConnection connection)
        {
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_party_members_branch ON party_members(branch);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_party_members_team ON party_members(team);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_party_members_status ON party_members(status);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_party_dues_member ON party_dues(member_id);");
            connection.Execute("CREATE INDEX IF NOT EXISTS idx_operation_logs_time ON operation_logs(operation_time);");
        }

        /// <summary>
        /// 初始化默认数据
        /// </summary>
        private static void InitializeDefaultData(SQLiteConnection connection)
        {
            // 检查是否已有管理员用户
            var adminExists = connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM users WHERE username = 'admin';");

            if (adminExists == 0)
            {
                // 创建默认管理员账户
                // 用户名: admin
                // 密码: admin123
                var passwordHash = BCrypt.Net.BCrypt.HashPassword("admin123");

                connection.Execute(@"
                    INSERT INTO users (username, password_hash, real_name, role)
                    VALUES (@Username, @PasswordHash, @RealName, @Role);",
                    new
                    {
                        Username = "admin",
                        PasswordHash = passwordHash,
                        RealName = "系统管理员",
                        Role = "管理员"
                    });

                // 记录操作日志
                connection.Execute(@"
                    INSERT INTO operation_logs (user_name, operation_type, operation_module, operation_detail)
                    VALUES (@UserName, @OperationType, @OperationModule, @OperationDetail);",
                    new
                    {
                        UserName = "系统",
                        OperationType = "系统初始化",
                        OperationModule = "用户管理",
                        OperationDetail = "创建默认管理员账户: admin"
                    });
            }
        }

        /// <summary>
        /// 获取数据库连接字符串
        /// </summary>
        public static string GetConnectionString()
        {
            return ConnectionString;
        }

        /// <summary>
        /// 获取数据库文件路径
        /// </summary>
        public static string GetDatabasePath()
        {
            return DbPath;
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public static string BackupDatabase(string backupPath = "")
        {
            if (string.IsNullOrEmpty(backupPath))
            {
                var backupDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backup");
                if (!Directory.Exists(backupDir))
                {
                    Directory.CreateDirectory(backupDir);
                }
                backupPath = Path.Combine(backupDir, $"YaotouParty_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            }

            File.Copy(DbPath, backupPath, true);
            return backupPath;
        }

        /// <summary>
        /// 恢复数据库
        /// </summary>
        public static void RestoreDatabase(string backupPath)
        {
            if (!File.Exists(backupPath))
            {
                throw new FileNotFoundException("备份文件不存在", backupPath);
            }

            // 关闭所有数据库连接
            SQLiteConnection.ClearAllPools();

            // 备份当前数据库
            var currentBackup = Path.Combine(Path.GetDirectoryName(DbPath)!, $"Current_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
            File.Copy(DbPath, currentBackup, true);

            // 恢复数据库
            File.Copy(backupPath, DbPath, true);
        }
    }
}
