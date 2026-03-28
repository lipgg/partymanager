using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YaotouPartyManager.Data;
using YaotouPartyManager.Models;

namespace YaotouPartyManager.Services
{
    /// <summary>
    /// 党员信息业务逻辑服务类
    /// </summary>
    public class PartyMemberService
    {
        private readonly PartyMemberRepository _repository;

        public PartyMemberService()
        {
            _repository = new PartyMemberRepository();
        }

        /// <summary>
        /// 添加党员信息
        /// </summary>
        public (bool Success, string Message, int MemberId) AddMember(PartyMember member, string operatorName)
        {
            // 验证基本信息
            var validationResults = ValidateMember(member);
            if (validationResults.Count > 0)
            {
                return (false, string.Join("\n", validationResults), 0);
            }

            // 检查身份证号是否已存在
            if (!string.IsNullOrWhiteSpace(member.IdCard) &&
                _repository.IsIdCardExists(member.IdCard))
            {
                return (false, "身份证号已存在,请检查", 0);
            }

            // 自动计算年龄
            if (!string.IsNullOrWhiteSpace(member.IdCard))
            {
                member.Age = PartyMember.CalculateAgeFromIdCard(member.IdCard);
            }

            try
            {
                var memberId = _repository.Insert(member, operatorName);
                LogOperation("党员管理", "新增党员", $"新增党员: {member.Name}", operatorName);
                return (true, "添加成功", memberId);
            }
            catch (Exception ex)
            {
                return (false, $"添加失败: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// 更新党员信息
        /// </summary>
        public (bool Success, string Message) UpdateMember(PartyMember member, string operatorName)
        {
            // 验证基本信息
            var validationResults = ValidateMember(member);
            if (validationResults.Count > 0)
            {
                return (false, string.Join("\n", validationResults));
            }

            // 检查身份证号是否与其他党员重复
            if (!string.IsNullOrWhiteSpace(member.IdCard) &&
                _repository.IsIdCardExists(member.IdCard, member.Id))
            {
                return (false, "身份证号与其他党员重复,请检查");
            }

            // 自动计算年龄
            if (!string.IsNullOrWhiteSpace(member.IdCard))
            {
                member.Age = PartyMember.CalculateAgeFromIdCard(member.IdCard);
            }

            try
            {
                var success = _repository.Update(member, operatorName);
                if (success)
                {
                    LogOperation("党员管理", "修改党员", $"修改党员信息: {member.Name}", operatorName);
                    return (true, "更新成功");
                }
                else
                {
                    return (false, "更新失败:未找到该党员信息");
                }
            }
            catch (Exception ex)
            {
                return (false, $"更新失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 删除党员信息(软删除)
        /// </summary>
        public (bool Success, string Message) DeleteMember(int memberId, string operatorName)
        {
            var member = _repository.GetById(memberId);
            if (member == null)
            {
                return (false, "未找到该党员信息");
            }

            try
            {
                var success = _repository.SoftDelete(memberId, operatorName);
                if (success)
                {
                    LogOperation("党员管理", "删除党员", $"删除党员: {member.Name}", operatorName);
                    return (true, "删除成功");
                }
                else
                {
                    return (false, "删除失败");
                }
            }
            catch (Exception ex)
            {
                return (false, $"删除失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 根据ID获取党员信息
        /// </summary>
        public PartyMember? GetMemberById(int id)
        {
            return _repository.GetById(id);
        }

        /// <summary>
        /// 获取所有党员信息
        /// </summary>
        public List<PartyMember> GetAllMembers()
        {
            return _repository.GetAll();
        }

        /// <summary>
        /// 多条件查询党员信息
        /// </summary>
        public List<PartyMember> QueryMembers(string? name = null, string? branch = null,
            string? team = null, string? gender = null, string? education = null, string? status = null)
        {
            return _repository.Query(name, branch, team, gender, education, status);
        }

        /// <summary>
        /// 获取党员总数
        /// </summary>
        public int GetTotalCount()
        {
            return _repository.GetCount();
        }

        /// <summary>
        /// 获取不重复的支部列表
        /// </summary>
        public List<string> GetBranchList()
        {
            return _repository.GetBranches();
        }

        /// <summary>
        /// 获取不重复的村民组列表
        /// </summary>
        public List<string> GetTeamList()
        {
            return _repository.GetTeams();
        }

        /// <summary>
        /// 批量导入党员信息
        /// </summary>
        public (bool Success, string Message, int ImportedCount) BatchImportMembers(
            List<PartyMember> members, string operatorName)
        {
            if (members == null || members.Count == 0)
            {
                return (false, "导入数据为空", 0);
            }

            // 验证所有数据
            foreach (var member in members)
            {
                var validationResults = ValidateMember(member);
                if (validationResults.Count > 0)
                {
                    return (false, $"党员 {member.Name} 数据验证失败: {string.Join(", ", validationResults)}", 0);
                }

                // 检查身份证号重复
                if (!string.IsNullOrWhiteSpace(member.IdCard) &&
                    _repository.IsIdCardExists(member.IdCard))
                {
                    return (false, $"党员 {member.Name} 的身份证号已存在", 0);
                }

                // 自动计算年龄
                if (!string.IsNullOrWhiteSpace(member.IdCard))
                {
                    member.Age = PartyMember.CalculateAgeFromIdCard(member.IdCard);
                }
            }

            try
            {
                var importedCount = _repository.BatchInsert(members, operatorName);
                LogOperation("党员管理", "批量导入", $"批量导入 {importedCount} 名党员", operatorName);
                return (true, $"成功导入 {importedCount} 名党员", importedCount);
            }
            catch (Exception ex)
            {
                return (false, $"导入失败: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// 获取党员统计信息
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            var allMembers = _repository.GetAll();
            var statistics = new Dictionary<string, int>();

            // 总人数
            statistics["总人数"] = allMembers.Count;

            // 按性别统计
            statistics["男党员数"] = allMembers.Count(m => m.Gender == "男");
            statistics["女党员数"] = allMembers.Count(m => m.Gender == "女");

            // 按状态统计
            statistics["正式党员数"] = allMembers.Count(m => m.Status == "正式党员");
            statistics["预备党员数"] = allMembers.Count(m => m.Status == "预备党员");

            // 按年龄段统计
            statistics["35岁以下"] = allMembers.Count(m => m.Age < 35);
            statistics["35-50岁"] = allMembers.Count(m => m.Age >= 35 && m.Age <= 50);
            statistics["51-60岁"] = allMembers.Count(m => m.Age > 50 && m.Age <= 60);
            statistics["60岁以上"] = allMembers.Count(m => m.Age > 60);

            // 按学历统计
            statistics["大学本科以上"] = allMembers.Count(m =>
                m.Education.Contains("本科") || m.Education.Contains("硕士") || m.Education.Contains("博士"));
            statistics["大专"] = allMembers.Count(m => m.Education.Contains("大专"));
            statistics["高中及以下"] = allMembers.Count(m =>
                m.Education.Contains("高中") || m.Education.Contains("中专") ||
                m.Education.Contains("初中") || m.Education.Contains("小学"));

            return statistics;
        }

        /// <summary>
        /// 验证党员信息
        /// </summary>
        private List<string> ValidateMember(PartyMember member)
        {
            var errors = new List<string>();

            // 验证姓名
            if (string.IsNullOrWhiteSpace(member.Name))
            {
                errors.Add("姓名不能为空");
            }

            // 验证性别
            if (string.IsNullOrWhiteSpace(member.Gender))
            {
                errors.Add("性别不能为空");
            }
            else if (member.Gender != "男" && member.Gender != "女")
            {
                errors.Add("性别必须为'男'或'女'");
            }

            // 验证身份证号格式
            if (!string.IsNullOrWhiteSpace(member.IdCard))
            {
                if (!PartyMember.ValidateIdCard(member.IdCard))
                {
                    errors.Add("身份证号格式不正确");
                }
            }

            // 验证手机号码格式
            if (!string.IsNullOrWhiteSpace(member.Phone))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(member.Phone, @"^1[3-9]\d{9}$"))
                {
                    errors.Add("手机号码格式不正确");
                }
            }

            // 验证入党日期和转正日期的逻辑关系
            if (member.JoinDate.HasValue && member.RegularDate.HasValue)
            {
                if (member.RegularDate.Value < member.JoinDate.Value)
                {
                    errors.Add("转正日期不能早于入党日期");
                }
            }

            return errors;
        }

        /// <summary>
        /// 记录操作日志
        /// </summary>
        private void LogOperation(string module, string operationType, string detail, string operatorName)
        {
            try
            {
                using var connection = new System.Data.SQLite.SQLiteConnection(
                    DatabaseInitializer.GetConnectionString());
                connection.Open();

                var sql = @"
                    INSERT INTO operation_logs (user_name, operation_type, operation_module, operation_detail)
                    VALUES (@UserName, @OperationType, @OperationModule, @OperationDetail)";

                connection.Execute(sql, new
                {
                    UserName = operatorName,
                    OperationType = operationType,
                    OperationModule = module,
                    OperationDetail = detail
                });
            }
            catch
            {
                // 日志记录失败不影响主业务
            }
        }

        /// <summary>
        /// 搜索党员
        /// </summary>
        public List<PartyMember> SearchMembers(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<PartyMember>();

            return _repository.Query(name: keyword);
        }
    }
}
