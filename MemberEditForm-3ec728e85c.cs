using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YaotouPartyManager.Models;

namespace YaotouPartyManager.UI
{
    /// <summary>
    /// 党员信息编辑窗体
    /// </summary>
    public partial class MemberEditForm : Form
    {
        public PartyMember Member { get; private set; }
        private bool _isEditMode;

        public MemberEditForm()
        {
            InitializeComponent();
            Member = new PartyMember();
            _isEditMode = false;
            this.Text = "新增党员";
        }

        public MemberEditForm(PartyMember member)
        {
            InitializeComponent();
            Member = member;
            _isEditMode = true;
            this.Text = "编辑党员";
            LoadMemberData();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 窗体设置
            this.Size = new Size(750, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Font = new Font("微软雅黑", 9F);

            // 主面板
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // 标题标签
            var titleLabel = new Label
            {
                Text = _isEditMode ? "编辑党员信息" : "新增党员信息",
                Font = new Font("微软雅黑", 14F, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.FromArgb(0, 102, 204)
            };

            // 分组框 - 基本信息
            var basicGroupBox = new GroupBox
            {
                Text = "基本信息",
                Location = new Point(20, 55),
                Size = new Size(690, 320),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };

            // 创建输入控件
            _txtName = CreateTextBox(20, 30, 150, "姓名 *", true);
            _cboGender = CreateComboBox(190, 30, 80, "性别 *", new[] { "男", "女" });
            _cboEthnicity = CreateComboBox(290, 30, 100, "民族", new[] { "汉族", "回族", "满族", "蒙古族", "藏族", "维吾尔族", "其他" });
            _cboEducation = CreateComboBox(410, 30, 250, "学历", new[] { "小学", "初中", "高中", "中专", "大专", "本科", "硕士", "博士" });

            _txtIdCard = CreateTextBox(20, 80, 200, "身份证号", true);
            _lblAge = CreateLabel(230, 85, 50, "年龄: 0");
            _txtPhone = CreateTextBox(300, 80, 150, "手机号码", true);
            _cboStatus = CreateComboBox(470, 80, 100, "党员状态", new[] { "正式党员", "预备党员" });

            _txtBranch = CreateTextBox(20, 130, 200, "隶属支部 *", true);
            _txtTeam = CreateTextBox(240, 130, 120, "隶属村民组 *", true);
            _txtOccupation = CreateTextBox(380, 130, 280, "职业", true);

            _txtWorkUnit = CreateTextBox(20, 180, 350, "工作单位", true);
            _cboVillagePosition = CreateComboBox(390, 180, 270, "村内职务", new[] { "", "支部书记", "支部副书记", "组织委员", "支部委员", "监委会主任" });

            _dtpJoinDate = CreateDatePicker(20, 230, 150, "入党日期");
            _dtpRegularDate = CreateDatePicker(190, 230, 150, "转正日期");

            // 分组框 - 家庭信息
            var familyGroupBox = new GroupBox
            {
                Text = "家庭信息",
                Location = new Point(20, 385),
                Size = new Size(690, 110),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };

            _txtFatherName = CreateTextBox(20, 30, 310, "父亲姓名", true);
            _txtMotherName = CreateTextBox(350, 30, 310, "母亲姓名", true);

            // 备注
            var remarksGroupBox = new GroupBox
            {
                Text = "备注",
                Location = new Point(20, 505),
                Size = new Size(690, 90),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold)
            };

            _txtRemarks = new TextBox
            {
                Location = new Point(10, 25),
                Size = new Size(670, 55),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("微软雅黑", 9F)
            };

            remarksGroupBox.Controls.Add(_txtRemarks);

            // 按钮
            _btnSave = new Button
            {
                Text = "保存",
                Size = new Size(100, 35),
                Location = new Point(430, 610),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnSave.Click += BtnSave_Click;

            _btnCancel = new Button
            {
                Text = "取消",
                Size = new Size(100, 35),
                Location = new Point(550, 610),
                Font = new Font("微软雅黑", 9F),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            _btnCancel.Click += BtnCancel_Click;

            // 添加控件到分组框
            basicGroupBox.Controls.AddRange(new Control[]
            {
                _txtName.Label, _txtName.TextBox,
                _cboGender.Label, _cboGender.ComboBox,
                _cboEthnicity.Label, _cboEthnicity.ComboBox,
                _cboEducation.Label, _cboEducation.ComboBox,
                _txtIdCard.Label, _txtIdCard.TextBox, _lblAge,
                _txtPhone.Label, _txtPhone.TextBox,
                _cboStatus.Label, _cboStatus.ComboBox,
                _txtBranch.Label, _txtBranch.TextBox,
                _txtTeam.Label, _txtTeam.TextBox,
                _txtOccupation.Label, _txtOccupation.TextBox,
                _txtWorkUnit.Label, _txtWorkUnit.TextBox,
                _cboVillagePosition.Label, _cboVillagePosition.ComboBox,
                _dtpJoinDate.Label, _dtpJoinDate.DateTimePicker,
                _dtpRegularDate.Label, _dtpRegularDate.DateTimePicker
            });

            familyGroupBox.Controls.AddRange(new Control[]
            {
                _txtFatherName.Label, _txtFatherName.TextBox,
                _txtMotherName.Label, _txtMotherName.TextBox
            });

            // 添加到主面板
            mainPanel.Controls.AddRange(new Control[]
            {
                titleLabel,
                basicGroupBox,
                familyGroupBox,
                remarksGroupBox,
                _btnSave,
                _btnCancel
            });

            this.Controls.Add(mainPanel);

            // 事件绑定
            _txtIdCard.TextBox.Leave += TxtIdCard_Leave;
            _txtIdCard.TextBox.TextChanged += TxtIdCard_TextChanged;

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        // 控件引用
        private TextBoxWithLabel _txtName;
        private ComboBoxWithLabel _cboGender;
        private ComboBoxWithLabel _cboEthnicity;
        private ComboBoxWithLabel _cboEducation;
        private TextBoxWithLabel _txtIdCard;
        private Label _lblAge;
        private TextBoxWithLabel _txtPhone;
        private ComboBoxWithLabel _cboStatus;
        private TextBoxWithLabel _txtBranch;
        private TextBoxWithLabel _txtTeam;
        private TextBoxWithLabel _txtOccupation;
        private TextBoxWithLabel _txtWorkUnit;
        private ComboBoxWithLabel _cboVillagePosition;
        private DatePickerWithLabel _dtpJoinDate;
        private DatePickerWithLabel _dtpRegularDate;
        private TextBoxWithLabel _txtFatherName;
        private TextBoxWithLabel _txtMotherName;
        private TextBox _txtRemarks;
        private Button _btnSave;
        private Button _btnCancel;

        #region 辅助类

        private class TextBoxWithLabel
        {
            public Label Label { get; private set; }
            public TextBox TextBox { get; private set; }

            public TextBoxWithLabel(int x, int y, int width, string labelText, bool isRequired)
            {
                Label = new Label
                {
                    Text = labelText,
                    Location = new Point(x, y),
                    Size = new Size(80, 23),
                    TextAlign = ContentAlignment.MiddleRight
                };

                TextBox = new TextBox
                {
                    Location = new Point(x + 85, y),
                    Size = new Size(width - 85, 23),
                    Font = new Font("微软雅黑", 9F)
                };
            }
        }

        private class ComboBoxWithLabel
        {
            public Label Label { get; private set; }
            public ComboBox ComboBox { get; private set; }

            public ComboBoxWithLabel(int x, int y, int width, string labelText, string[] items)
            {
                Label = new Label
                {
                    Text = labelText,
                    Location = new Point(x, y),
                    Size = new Size(80, 23),
                    TextAlign = ContentAlignment.MiddleRight
                };

                ComboBox = new ComboBox
                {
                    Location = new Point(x + 85, y),
                    Size = new Size(width - 85, 23),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Font = new Font("微软雅黑", 9F)
                };
                ComboBox.Items.AddRange(items);
            }
        }

        private class DatePickerWithLabel
        {
            public Label Label { get; private set; }
            public DateTimePicker DateTimePicker { get; private set; }

            public DatePickerWithLabel(int x, int y, int width, string labelText)
            {
                Label = new Label
                {
                    Text = labelText,
                    Location = new Point(x, y),
                    Size = new Size(80, 23),
                    TextAlign = ContentAlignment.MiddleRight
                };

                DateTimePicker = new DateTimePicker
                {
                    Location = new Point(x + 85, y),
                    Size = new Size(width - 85, 23),
                    Format = DateTimePickerFormat.Short,
                    Font = new Font("微软雅黑", 9F),
                    ShowCheckBox = true
                };
            }
        }

        #endregion

        #region 控件创建方法

        private TextBoxWithLabel CreateTextBox(int x, int y, int width, string labelText, bool isRequired)
        {
            return new TextBoxWithLabel(x, y, width, labelText, isRequired);
        }

        private ComboBoxWithLabel CreateComboBox(int x, int y, int width, string labelText, string[] items)
        {
            return new ComboBoxWithLabel(x, y, width, labelText, items);
        }

        private DatePickerWithLabel CreateDatePicker(int x, int y, int width, string labelText)
        {
            return new DatePickerWithLabel(x, y, width, labelText);
        }

        private Label CreateLabel(int x, int y, int width, string text)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(width, 23),
                Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204)
            };
        }

        #endregion

        #region 事件处理

        private void TxtIdCard_Leave(object sender, EventArgs e)
        {
            var idCard = _txtIdCard.TextBox.Text.Trim();
            if (!string.IsNullOrWhiteSpace(idCard) && PartyMember.ValidateIdCard(idCard))
            {
                var age = PartyMember.CalculateAgeFromIdCard(idCard);
                _lblAge.Text = $"年龄: {age}";
            }
        }

        private void TxtIdCard_TextChanged(object sender, EventArgs e)
        {
            var idCard = _txtIdCard.TextBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(idCard))
            {
                _lblAge.Text = "年龄: 0";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // 基本验证
            if (string.IsNullOrWhiteSpace(_txtName.TextBox.Text))
            {
                MessageBox.Show("请输入姓名", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtName.TextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtBranch.TextBox.Text))
            {
                MessageBox.Show("请输入隶属支部", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtBranch.TextBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtTeam.TextBox.Text))
            {
                MessageBox.Show("请输入隶属村民组", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                _txtTeam.TextBox.Focus();
                return;
            }

            // 填充数据
            Member.Name = _txtName.TextBox.Text.Trim();
            Member.Gender = _cboGender.ComboBox.SelectedItem?.ToString() ?? "";
            Member.Ethnicity = _cboEthnicity.ComboBox.SelectedItem?.ToString() ?? "汉族";
            Member.Education = _cboEducation.ComboBox.SelectedItem?.ToString() ?? "";
            Member.IdCard = _txtIdCard.TextBox.Text.Trim();
            Member.Age = PartyMember.CalculateAgeFromIdCard(Member.IdCard);
            Member.Phone = _txtPhone.TextBox.Text.Trim();
            Member.Status = _cboStatus.ComboBox.SelectedItem?.ToString() ?? "正式党员";
            Member.Branch = _txtBranch.TextBox.Text.Trim();
            Member.Team = _txtTeam.TextBox.Text.Trim();
            Member.Occupation = _txtOccupation.TextBox.Text.Trim();
            Member.WorkUnit = _txtWorkUnit.TextBox.Text.Trim();
            Member.VillagePosition = _cboVillagePosition.ComboBox.SelectedItem?.ToString() ?? "";
            Member.JoinDate = _dtpJoinDate.DateTimePicker.Checked ? _dtpJoinDate.DateTimePicker.Value : null;
            Member.RegularDate = _dtpRegularDate.DateTimePicker.Checked ? _dtpRegularDate.DateTimePicker.Value : null;
            Member.FatherName = _txtFatherName.TextBox.Text.Trim();
            Member.MotherName = _txtMotherName.TextBox.Text.Trim();
            Member.Remarks = _txtRemarks.Text.Trim();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #endregion

        #region 数据加载

        private void LoadMemberData()
        {
            _txtName.TextBox.Text = Member.Name ?? "";
            _cboGender.ComboBox.SelectedItem = Member.Gender ?? "男";
            _cboEthnicity.ComboBox.SelectedItem = Member.Ethnicity ?? "汉族";
            _cboEducation.ComboBox.SelectedItem = Member.Education ?? "";
            _txtIdCard.TextBox.Text = Member.IdCard ?? "";
            _lblAge.Text = $"年龄: {Member.Age}";
            _txtPhone.TextBox.Text = Member.Phone ?? "";
            _cboStatus.ComboBox.SelectedItem = Member.Status ?? "正式党员";
            _txtBranch.TextBox.Text = Member.Branch ?? "";
            _txtTeam.TextBox.Text = Member.Team ?? "";
            _txtOccupation.TextBox.Text = Member.Occupation ?? "";
            _txtWorkUnit.TextBox.Text = Member.WorkUnit ?? "";
            _cboVillagePosition.ComboBox.SelectedItem = Member.VillagePosition ?? "";

            if (Member.JoinDate.HasValue)
            {
                _dtpJoinDate.DateTimePicker.Value = Member.JoinDate.Value;
                _dtpJoinDate.DateTimePicker.Checked = true;
            }
            else
            {
                _dtpJoinDate.DateTimePicker.Checked = false;
            }

            if (Member.RegularDate.HasValue)
            {
                _dtpRegularDate.DateTimePicker.Value = Member.RegularDate.Value;
                _dtpRegularDate.DateTimePicker.Checked = true;
            }
            else
            {
                _dtpRegularDate.DateTimePicker.Checked = false;
            }

            _txtFatherName.TextBox.Text = Member.FatherName ?? "";
            _txtMotherName.TextBox.Text = Member.MotherName ?? "";
            _txtRemarks.Text = Member.Remarks ?? "";
        }

        #endregion
    }
}
