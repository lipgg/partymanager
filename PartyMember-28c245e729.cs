using System;
using System.ComponentModel.DataAnnotations;

namespace YaotouPartyManager.Models
{
    /// <summary>
    /// 党员信息实体类
    /// </summary>
    public class PartyMember
    {
        /// <summary>
        /// 党员ID
        /// </summary>
        [Display(Name = "ID")]
        public int Id { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Display(Name = "姓名")]
        [Required(ErrorMessage = "姓名不能为空")]
        [StringLength(50, ErrorMessage = "姓名长度不能超过50个字符")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 隶属支部
        /// </summary>
        [Display(Name = "隶属支部")]
        [Required(ErrorMessage = "隶属支部不能为空")]
        [StringLength(100, ErrorMessage = "隶属支部长度不能超过100个字符")]
        public string Branch { get; set; } = string.Empty;

        /// <summary>
        /// 隶属村民组(隶属队)
        /// </summary>
        [Display(Name = "隶属队")]
        [StringLength(50, ErrorMessage = "隶属队长度不能超过50个字符")]
        public string Team { get; set; } = string.Empty;

        /// <summary>
        /// 性别
        /// </summary>
        [Display(Name = "性别")]
        [Required(ErrorMessage = "性别不能为空")]
        public string Gender { get; set; } = string.Empty;

        /// <summary>
        /// 民族
        /// </summary>
        [Display(Name = "民族")]
        [StringLength(20, ErrorMessage = "民族长度不能超过20个字符")]
        public string Ethnicity { get; set; } = string.Empty;

        /// <summary>
        /// 学历
        /// </summary>
        [Display(Name = "学历")]
        [StringLength(50, ErrorMessage = "学历长度不能超过50个字符")]
        public string Education { get; set; } = string.Empty;

        /// <summary>
        /// 身份证号
        /// </summary>
        [Display(Name = "身份证号")]
        [StringLength(18, ErrorMessage = "身份证号长度必须为18位")]
        public string IdCard { get; set; } = string.Empty;

        /// <summary>
        /// 年龄(根据身份证号自动生成)
        /// </summary>
        [Display(Name = "年龄")]
        public int Age { get; set; }

        /// <summary>
        /// 入党日期
        /// </summary>
        [Display(Name = "入党日期")]
        public DateTime? JoinDate { get; set; }

        /// <summary>
        /// 转正日期
        /// </summary>
        [Display(Name = "转正日期")]
        public DateTime? RegularDate { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        [Display(Name = "手机号码")]
        [StringLength(20, ErrorMessage = "手机号码长度不能超过20个字符")]
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// 职业
        /// </summary>
        [Display(Name = "职业")]
        [StringLength(50, ErrorMessage = "职业长度不能超过50个字符")]
        public string Occupation { get; set; } = string.Empty;

        /// <summary>
        /// 工作单位
        /// </summary>
        [Display(Name = "工作单位")]
        [StringLength(100, ErrorMessage = "工作单位长度不能超过100个字符")]
        public string WorkUnit { get; set; } = string.Empty;

        /// <summary>
        /// 父亲姓名
        /// </summary>
        [Display(Name = "父亲姓名")]
        [StringLength(50, ErrorMessage = "父亲姓名长度不能超过50个字符")]
        public string FatherName { get; set; } = string.Empty;

        /// <summary>
        /// 母亲姓名
        /// </summary>
        [Display(Name = "母亲姓名")]
        [StringLength(50, ErrorMessage = "母亲姓名长度不能超过50个字符")]
        public string MotherName { get; set; } = string.Empty;

        /// <summary>
        /// 村内职务
        /// </summary>
        [Display(Name = "村内职务")]
        [StringLength(50, ErrorMessage = "村内职务长度不能超过50个字符")]
        public string VillagePosition { get; set; } = string.Empty;

        /// <summary>
        /// 备注
        /// </summary>
        [Display(Name = "备注")]
        public string? Remarks { get; set; }

        /// <summary>
        /// 状态(正式党员/预备党员)
        /// </summary>
        [Display(Name = "党员状态")]
        public string Status { get; set; } = "正式党员";

        /// <summary>
        /// 软删除标记(0-未删除,1-已删除)
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedTime { get; set; }

        /// <summary>
        /// 根据身份证号计算年龄
        /// </summary>
        public static int CalculateAgeFromIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length != 18)
                return 0;

            try
            {
                string birthDateStr = idCard.Substring(6, 8);
                DateTime birthDate = DateTime.ParseExact(birthDateStr, "yyyyMMdd", null);
                DateTime today = DateTime.Today;

                int age = today.Year - birthDate.Year;
                if (today < birthDate.AddYears(age))
                    age--;

                return age;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 验证身份证号格式
        /// </summary>
        public static bool ValidateIdCard(string idCard)
        {
            if (string.IsNullOrEmpty(idCard) || idCard.Length != 18)
                return false;

            // 简单校验:18位数字,最后一位可以是X
            for (int i = 0; i < 17; i++)
            {
                if (!char.IsDigit(idCard[i]))
                    return false;
            }

            char lastChar = idCard[17];
            if (!char.IsDigit(lastChar) && lastChar != 'X' && lastChar != 'x')
                return false;

            return true;
        }
    }
}
