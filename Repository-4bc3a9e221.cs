using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using Dapper;
using System.Data.SQLite;
using YaotouPartyManager.Models;

namespace YaotouPartyManager.Data
{
    /// <summary>
    /// 党员信息数据访问类
    /// </summary>
    public class PartyMemberRepository
    {
        private readonly string _connectionString;

        public PartyMemberRepository()
        {
            _connectionString = DatabaseInitializer.GetConnectionString();
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        private IDbConnection GetConnection()
        {
            return new SQLiteConnection(_connectionString);
        }

        /// <summary>
        /// 添加党员信息
        /// </summary>
        public int Insert(PartyMember member, string operatorName)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                INSERT INTO party_members 
                (name, branch, team, gender, ethnicity, education, id_card, age, join_date, regular_date, 
                 phone, occupation, work_unit, father_name, mother_name, village_position, remarks, status, 
                 is_deleted, created_by, created_time, updated_by, updated_time)
                VALUES 
                (@Name, @Branch, @Team, @Gender, @Ethnicity, @Education, @IdCard, @Age, @JoinDate, @RegularDate,
                 @Phone, @Occupation, @WorkUnit, @FatherName, @MotherName, @VillagePosition, @Remarks, @Status,
                 0, @CreatedBy, @CreatedTime, @UpdatedBy, @UpdatedTime);

                SELECT last_insert_rowid();";

            var memberId = connection.ExecuteScalar<int>(sql, new
            {
                member.Name,
                member.Branch,
                member.Team,
                member.Gender,
                member.Ethnicity,
                member.Education,
                member.IdCard,
                member.Age,
                JoinDate = member.JoinDate?.ToString("yyyy-MM-dd"),
                RegularDate = member.RegularDate?.ToString("yyyy-MM-dd"),
                member.Phone,
                member.Occupation,
                member.WorkUnit,
                member.FatherName,
                member.MotherName,
                member.VillagePosition,
                member.Remarks,
                member.Status,
                CreatedBy = operatorName,
                CreatedTime = DateTime.Now,
                UpdatedBy = operatorName,
                UpdatedTime = DateTime.Now
            });

            // 记录历史
            SaveHistory(memberId, "新增", JsonSerializer.Serialize(member), operatorName);

            return memberId;
        }

        /// <summary>
        /// 更新党员信息
        /// </summary>
        public bool Update(PartyMember member, string operatorName)
        {
            using var connection = GetConnection();
            connection.Open();

            // 获取旧数据
            var oldMember = GetById(member.Id);
            if (oldMember == null)
                return false;

            var sql = @"
                UPDATE party_members SET
                    name = @Name,
                    branch = @Branch,
                    team = @Team,
                    gender = @Gender,
                    ethnicity = @Ethnicity,
                    education = @Education,
                    id_card = @IdCard,
                    age = @Age,
                    join_date = @JoinDate,
                    regular_date = @RegularDate,
                    phone = @Phone,
                    occupation = @Occupation,
                    work_unit = @WorkUnit,
                    father_name = @FatherName,
                    mother_name = @MotherName,
                    village_position = @VillagePosition,
                    remarks = @Remarks,
                    status = @Status,
                    updated_by = @UpdatedBy,
                    updated_time = @UpdatedTime
                WHERE id = @Id";

            var rowsAffected = connection.Execute(sql, new
            {
                member.Name,
                member.Branch,
                member.Team,
                member.Gender,
                member.Ethnicity,
                member.Education,
                member.IdCard,
                member.Age,
                JoinDate = member.JoinDate?.ToString("yyyy-MM-dd"),
                RegularDate = member.RegularDate?.ToString("yyyy-MM-dd"),
                member.Phone,
                member.Occupation,
                member.WorkUnit,
                member.FatherName,
                member.MotherName,
                member.VillagePosition,
                member.Remarks,
                member.Status,
                UpdatedBy = operatorName,
                UpdatedTime = DateTime.Now,
                member.Id
            });

            if (rowsAffected > 0)
            {
                // 记录历史
                var changeContent = new
                {
                    Old = oldMember,
                    New = member
                };
                SaveHistory(member.Id, "修改", JsonSerializer.Serialize(changeContent), operatorName);
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// 软删除党员信息
        /// </summary>
        public bool SoftDelete(int id, string operatorName)
        {
            using var connection = GetConnection();
            connection.Open();

            var member = GetById(id);
            if (member == null)
                return false;

            var sql = @"
                UPDATE party_members SET
                    is_deleted = 1,
                    updated_by = @UpdatedBy,
                    updated_time = @UpdatedTime
                WHERE id = @Id";

            var rowsAffected = connection.Execute(sql, new
            {
                UpdatedBy = operatorName,
                UpdatedTime = DateTime.Now,
                Id = id
            });

            if (rowsAffected > 0)
            {
                // 记录历史
                SaveHistory(id, "删除", JsonSerializer.Serialize(member), operatorName);
            }

            return rowsAffected > 0;
        }

        /// <summary>
        /// 根据ID获取党员信息
        /// </summary>
        public PartyMember? GetById(int id)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                SELECT id, name, branch, team, gender, ethnicity, education, id_card, age, 
                       join_date as JoinDate, regular_date as RegularDate, phone, occupation, work_unit,
                       father_name, mother_name, village_position, remarks, status, is_deleted as IsDeleted,
                       created_by, created_time as CreatedTime, updated_by, updated_time as UpdatedTime
                FROM party_members
                WHERE id = @Id AND is_deleted = 0";

            return connection.QueryFirstOrDefault<PartyMember>(sql, new { Id = id });
        }

        /// <summary>
        /// 获取所有党员信息
        /// </summary>
        public List<PartyMember> GetAll()
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                SELECT id, name, branch, team, gender, ethnicity, education, id_card, age, 
                       join_date as JoinDate, regular_date as RegularDate, phone, occupation, work_unit,
                       father_name, mother_name, village_position, remarks, status, is_deleted as IsDeleted,
                       created_by, created_time as CreatedTime, updated_by, updated_time as UpdatedTime
                FROM party_members
                WHERE is_deleted = 0
                ORDER BY branch, team, name";

            return connection.Query<PartyMember>(sql).ToList();
        }

        /// <summary>
        /// 多条件查询党员信息
        /// </summary>
        public List<PartyMember> Query(string? name = null, string? branch = null, string? team = null,
            string? gender = null, string? education = null, string? status = null)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                SELECT id, name, branch, team, gender, ethnicity, education, id_card, age, 
                       join_date as JoinDate, regular_date as RegularDate, phone, occupation, work_unit,
                       father_name, mother_name, village_position, remarks, status, is_deleted as IsDeleted,
                       created_by, created_time as CreatedTime, updated_by, updated_time as UpdatedTime
                FROM party_members
                WHERE is_deleted = 0";

            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(name))
            {
                sql += " AND name LIKE @Name";
                parameters.Add("Name", $"%{name}%");
            }

            if (!string.IsNullOrWhiteSpace(branch))
            {
                sql += " AND branch = @Branch";
                parameters.Add("Branch", branch);
            }

            if (!string.IsNullOrWhiteSpace(team))
            {
                sql += " AND team = @Team";
                parameters.Add("Team", team);
            }

            if (!string.IsNullOrWhiteSpace(gender))
            {
                sql += " AND gender = @Gender";
                parameters.Add("Gender", gender);
            }

            if (!string.IsNullOrWhiteSpace(education))
            {
                sql += " AND education = @Education";
                parameters.Add("Education", education);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                sql += " AND status = @Status";
                parameters.Add("Status", status);
            }

            sql += " ORDER BY branch, team, name";

            return connection.Query<PartyMember>(sql, parameters).ToList();
        }

        /// <summary>
        /// 获取党员总数
        /// </summary>
        public int GetCount()
        {
            using var connection = GetConnection();
            connection.Open();

            return connection.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM party_members WHERE is_deleted = 0");
        }

        /// <summary>
        /// 检查身份证号是否已存在
        /// </summary>
        public bool IsIdCardExists(string idCard, int excludeId = 0)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = "SELECT COUNT(*) FROM party_members WHERE id_card = @IdCard AND id != @ExcludeId AND is_deleted = 0";
            var count = connection.ExecuteScalar<int>(sql, new { IdCard = idCard, ExcludeId = excludeId });
            return count > 0;
        }

        /// <summary>
        /// 获取不重复的支部列表
        /// </summary>
        public List<string> GetBranches()
        {
            using var connection = GetConnection();
            connection.Open();

            return connection.Query<string>(
                "SELECT DISTINCT branch FROM party_members WHERE is_deleted = 0 AND branch != '' ORDER BY branch"
            ).ToList();
        }

        /// <summary>
        /// 获取不重复的村民组列表
        /// </summary>
        public List<string> GetTeams()
        {
            using var connection = GetConnection();
            connection.Open();

            return connection.Query<string>(
                "SELECT DISTINCT team FROM party_members WHERE is_deleted = 0 AND team != '' ORDER BY team"
            ).ToList();
        }

        /// <summary>
        /// 批量导入党员信息
        /// </summary>
        public int BatchInsert(List<PartyMember> members, string operatorName)
        {
            using var connection = GetConnection();
            connection.Open();

            int insertedCount = 0;
            var transaction = connection.BeginTransaction();

            try
            {
                var sql = @"
                    INSERT INTO party_members 
                    (name, branch, team, gender, ethnicity, education, id_card, age, join_date, regular_date, 
                     phone, occupation, work_unit, father_name, mother_name, village_position, remarks, status, 
                     is_deleted, created_by, created_time, updated_by, updated_time)
                    VALUES 
                    (@Name, @Branch, @Team, @Gender, @Ethnicity, @Education, @IdCard, @Age, @JoinDate, @RegularDate,
                     @Phone, @Occupation, @WorkUnit, @FatherName, @MotherName, @VillagePosition, @Remarks, @Status,
                     0, @CreatedBy, @CreatedTime, @UpdatedBy, @UpdatedTime);

                    SELECT last_insert_rowid();";

                foreach (var member in members)
                {
                    var memberId = connection.ExecuteScalar<int>(sql, new
                    {
                        member.Name,
                        member.Branch,
                        member.Team,
                        member.Gender,
                        member.Ethnicity,
                        member.Education,
                        member.IdCard,
                        member.Age,
                        JoinDate = member.JoinDate?.ToString("yyyy-MM-dd"),
                        RegularDate = member.RegularDate?.ToString("yyyy-MM-dd"),
                        member.Phone,
                        member.Occupation,
                        member.WorkUnit,
                        member.FatherName,
                        member.MotherName,
                        member.VillagePosition,
                        member.Remarks,
                        member.Status,
                        CreatedBy = operatorName,
                        CreatedTime = DateTime.Now,
                        UpdatedBy = operatorName,
                        UpdatedTime = DateTime.Now
                    }, transaction);

                    insertedCount++;
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }

            return insertedCount;
        }

        /// <summary>
        /// 保存操作历史
        /// </summary>
        private void SaveHistory(int memberId, string operationType, string operationContent, string operatorName)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                INSERT INTO party_member_history (member_id, operation_type, operation_content, operator)
                VALUES (@MemberId, @OperationType, @OperationContent, @Operator)";

            connection.Execute(sql, new
            {
                MemberId = memberId,
                OperationType = operationType,
                OperationContent = operationContent,
                Operator = operatorName
            });
        }

        /// <summary>
        /// 获取党员修改历史
        /// </summary>
        public List<dynamic> GetHistory(int memberId)
        {
            using var connection = GetConnection();
            connection.Open();

            var sql = @"
                SELECT id, member_id as MemberId, operation_type as OperationType, operation_content as OperationContent,
                       operator as Operator, operation_time as OperationTime
                FROM party_member_history
                WHERE member_id = @MemberId
                ORDER BY operation_time DESC";

            return connection.Query(sql, new { MemberId = memberId }).ToList();
        }
    }
}
