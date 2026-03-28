using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using YaotouPartyManager.Data;
using YaotouPartyManager.Models;
using YaotouPartyManager.Services;

namespace YaotouPartyManager.UI
{
    /// <summary>
    /// 主窗体 - 窑头社区党员管理系统
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly PartyMemberService _partyMemberService;
        private BindingSource _bindingSource;
        private string _currentUser = "管理员";

        public MainForm()
        {
            InitializeComponent();
            _partyMemberService = new PartyMemberService();
            _bindingSource = new BindingSource();
            InitializeDatabase();
            LoadData();
            UpdateStatistics();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();

            // 窗体基本设置
            this.Text = "窑头社区党员管理系统 v1.0";
            this.Size = new Size(1200, 750);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(1000, 600);
            this.Font = new Font("微软雅黑", 9F, FontStyle.Regular);
            this.IsMdiContainer = true;
            this.Icon = SystemIcons.Application;

            // 主菜单栏
            var menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("文件(&F)");
            var manageMenu = new ToolStripMenuItem("党员管理(&M)");
            var queryMenu = new ToolStripMenuItem("查询统计(&Q)");
            var reportMenu = new ToolStripMenuItem("报表打印(&R)");
            var systemMenu = new ToolStripMenuItem("系统(&S)");
            var helpMenu = new ToolStripMenuItem("帮助(&H)");

            // 文件菜单
            fileMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("数据备份", null, BackupData_Click),
                new ToolStripMenuItem("数据恢复", null, RestoreData_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("退出", Keys.Control | Keys.Q, (s, e) => this.Close())
            });

            // 党员管理菜单
            manageMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("新增党员", Keys.Control | Keys.N, AddMember_Click),
                new ToolStripMenuItem("编辑党员", Keys.Control | Keys.E, EditMember_Click),
                new ToolStripMenuItem("删除党员", Keys.Delete, DeleteMember_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("批量导入", Keys.Control | Keys.I, BatchImport_Click)
            });

            // 查询统计菜单
            queryMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("快速搜索", Keys.Control | Keys.F, QuickSearch_Click),
                new ToolStripMenuItem("高级查询", Keys.Control | Keys.A, AdvancedQuery_Click),
                new ToolStripMenuItem("统计概览", Keys.F5, StatisticsOverview_Click)
            });

            // 报表打印菜单
            reportMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("党员花名册", Keys.Control | Keys.P, PrintMemberList_Click),
                new ToolStripMenuItem("自定义报表", null, CustomReport_Click),
                new ToolStripMenuItem("综合打印", null, ComprehensivePrint_Click)
            });

            // 系统菜单
            systemMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("用户管理", null, UserManagement_Click),
                new ToolStripMenuItem("操作日志", null, OperationLog_Click),
                new ToolStripMenuItem("系统设置", null, SystemSettings_Click),
                new ToolStripSeparator(),
                new ToolStripMenuItem("关于", null, About_Click)
            });

            // 帮助菜单
            helpMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("使用说明", Keys.F1, Help_Click),
                new ToolStripMenuItem("检查更新", null, CheckUpdate_Click)
            });

            menuStrip.Items.AddRange(new ToolStripItem[]
            {
                fileMenu, manageMenu, queryMenu, reportMenu, systemMenu, helpMenu
            });

            this.MainMenuStrip = menuStrip;
            this.Controls.Add(menuStrip);

            // 工具栏
            var toolStrip = new ToolStrip();
            toolStrip.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip.RenderMode = ToolStripRenderMode.System;

            toolStrip.Items.Add(new ToolStripButton("新增", null, AddMember_Click));
            toolStrip.Items.Add(new ToolStripButton("编辑", null, EditMember_Click));
            toolStrip.Items.Add(new ToolStripButton("删除", null, DeleteMember_Click));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("搜索", null, QuickSearch_Click));
            toolStrip.Items.Add(new ToolStripButton("统计", null, StatisticsOverview_Click));
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton("打印", null, PrintMemberList_Click));
            toolStrip.Items.Add(new ToolStripButton("备份", null, BackupData_Click));

            this.Controls.Add(toolStrip);

            // 主面板容器
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 200,
                Orientation = Orientation.Horizontal,
                BorderStyle = BorderStyle.FixedSingle
            };

            // 统计信息面板
            var statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 248, 255),
                Padding = new Padding(10)
            };

            var statsLabel = new Label
            {
                Text = "📊 统计概览",
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                Location = new Point(10, 10),
                AutoSize = true
            };

            // 创建统计标签
            _statsLabels = new Dictionary<string, Label>();
            int x = 10, y = 45, width = 150, height = 80;
            var statsTitles = new[] { "总人数", "正式党员", "预备党员", "男党员", "女党员", "35岁以下" };

            for (int i = 0; i < statsTitles.Length; i++)
            {
                var group = new GroupBox
                {
                    Text = statsTitles[i],
                    Location = new Point(x, y),
                    Size = new Size(width, height),
                    Font = new Font("微软雅黑", 9F, FontStyle.Bold),
                    BackColor = Color.White
                };

                var valueLabel = new Label
                {
                    Name = $"lblStats_{i}",
                    Text = "0",
                    Font = new Font("Arial", 18F, FontStyle.Bold),
                    Location = new Point(10, 25),
                    AutoSize = true,
                    ForeColor = Color.FromArgb(0, 102, 204)
                };

                group.Controls.Add(valueLabel);
                statsPanel.Controls.Add(group);
                _statsLabels[statsTitles[i]] = valueLabel;

                x += width + 10;
                if ((i + 1) % 6 == 0)
                {
                    x = 10;
                    y += height + 10;
                }
            }

            statsPanel.Controls.Add(statsLabel);
            splitContainer.Panel1.Controls.Add(statsPanel);

            // 党员列表面板
            var listPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var listLabel = new Label
            {
                Text = "👥 党员列表",
                Font = new Font("微软雅黑", 11F, FontStyle.Bold),
                Location = new Point(10, 5),
                AutoSize = true
            };

            // 搜索框
            var searchTextBox = new TextBox
            {
                Name = "txtSearch",
                Location = new Point(100, 8),
                Size = new Size(200, 23),
                PlaceholderText = "输入姓名或身份证号搜索..."
            };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;

            var searchButton = new Button
            {
                Text = "搜索",
                Location = new Point(310, 5),
                Size = new Size(60, 26)
            };
            searchButton.Click += QuickSearch_Click;

            var refreshButton = new Button
            {
                Text = "刷新",
                Location = new Point(380, 5),
                Size = new Size(60, 26)
            };
            refreshButton.Click += RefreshButton_Click;

            // 党员数据表格
            var dataGridView = new DataGridView
            {
                Name = "dgvMembers",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = true,
                RowHeadersWidth = 50,
                Font = new Font("微软雅黑", 9F),
                GridColor = Color.LightGray,
                BackgroundColor = Color.White,
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(248, 248, 248)
                }
            };

            // 添加双击事件
            dataGridView.DoubleClick += DataGridView_DoubleClick;

            listPanel.Controls.AddRange(new Control[] { listLabel, searchTextBox, searchButton, refreshButton, dataGridView });
            splitContainer.Panel2.Controls.Add(listPanel);

            this.Controls.Add(splitContainer);

            // 状态栏
            var statusStrip = new StatusStrip();
            var statusLabel = new ToolStripStatusLabel
            {
                Text = $"当前用户: {_currentUser} | 就绪"
            };
            var timeLabel = new ToolStripStatusLabel
            {
                Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Spring = true,
                TextAlign = ContentAlignment.MiddleRight
            };
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, timeLabel });

            // 定时器更新时间
            var timer = new System.Windows.Forms.Timer { Interval = 1000 };
            timer.Tick += (s, e) => timeLabel.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            timer.Start();

            this.Controls.Add(statusStrip);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private Dictionary<string, Label> _statsLabels;

        /// <summary>
        /// 初始化数据库
        /// </summary>
        private void InitializeDatabase()
        {
            try
            {
                DatabaseInitializer.InitializeDatabase();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"数据库初始化失败: {ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        private void LoadData()
        {
            var members = _partyMemberService.GetAllMembers();
            _bindingSource.DataSource = members;
            var dgv = this.Controls.Find("dgvMembers", true).FirstOrDefault() as DataGridView;
            if (dgv != null)
            {
                dgv.DataSource = _bindingSource;
                dgv.Columns.Clear();

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "序号",
                    Name = "Index",
                    Width = 50,
                    DisplayIndex = 0
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "姓名",
                    DataPropertyName = "Name",
                    Width = 80,
                    DisplayIndex = 1
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "性别",
                    DataPropertyName = "Gender",
                    Width = 50,
                    DisplayIndex = 2
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "隶属支部",
                    DataPropertyName = "Branch",
                    Width = 120,
                    DisplayIndex = 3
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "隶属队",
                    DataPropertyName = "Team",
                    Width = 80,
                    DisplayIndex = 4
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "年龄",
                    DataPropertyName = "Age",
                    Width = 50,
                    DisplayIndex = 5
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "学历",
                    DataPropertyName = "Education",
                    Width = 100,
                    DisplayIndex = 6
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "手机号码",
                    DataPropertyName = "Phone",
                    Width = 110,
                    DisplayIndex = 7
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "状态",
                    DataPropertyName = "Status",
                    Width = 80,
                    DisplayIndex = 8
                });

                dgv.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "党内职务",
                    DataPropertyName = "VillagePosition",
                    Width = 100,
                    DisplayIndex = 9
                });

                // 更新序号
                foreach (DataGridViewRow row in dgv.Rows)
                {
                    row.Cells["Index"].Value = row.Index + 1;
                }
            }
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        private void UpdateStatistics()
        {
            var stats = _partyMemberService.GetStatistics();

            _statsLabels["总人数"].Text = stats["总人数"].ToString();
            _statsLabels["正式党员"].Text = stats["正式党员数"].ToString();
            _statsLabels["预备党员"].Text = stats["预备党员数"].ToString();
            _statsLabels["男党员"].Text = stats["男党员数"].ToString();
            _statsLabels["女党员"].Text = stats["女党员数"].ToString();
            _statsLabels["35岁以下"].Text = stats["35岁以下"].ToString();
        }

        // ==================== 事件处理程序 ====================

        private void AddMember_Click(object? sender, EventArgs e)
        {
            var form = new MemberEditForm();
            if (form.ShowDialog() == DialogResult.OK)
            {
                var result = _partyMemberService.AddMember(form.Member, _currentUser);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    UpdateStatistics();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void EditMember_Click(object? sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvMembers", true).FirstOrDefault() as DataGridView;
            if (dgv == null || dgv.CurrentRow == null)
            {
                MessageBox.Show("请选择要编辑的党员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var member = dgv.CurrentRow.DataBoundItem as PartyMember;
            if (member == null) return;

            var form = new MemberEditForm(member);
            if (form.ShowDialog() == DialogResult.OK)
            {
                var result = _partyMemberService.UpdateMember(form.Member, _currentUser);
                if (result.Success)
                {
                    MessageBox.Show(result.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    UpdateStatistics();
                }
                else
                {
                    MessageBox.Show(result.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void DeleteMember_Click(object? sender, EventArgs e)
        {
            var dgv = this.Controls.Find("dgvMembers", true).FirstOrDefault() as DataGridView;
            if (dgv == null || dgv.CurrentRow == null)
            {
                MessageBox.Show("请选择要删除的党员", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var member = dgv.CurrentRow.DataBoundItem as PartyMember;
            if (member == null) return;

            var result = MessageBox.Show(
                $"确定要删除党员 {member.Name} 吗?\n\n此操作将软删除该党员信息,可在系统管理中恢复。",
                "确认删除",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                var deleteResult = _partyMemberService.DeleteMember(member.Id, _currentUser);
                if (deleteResult.Success)
                {
                    MessageBox.Show(deleteResult.Message, "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    UpdateStatistics();
                }
                else
                {
                    MessageBox.Show(deleteResult.Message, "失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void QuickSearch_Click(object? sender, EventArgs e)
        {
            var txt = this.Controls.Find("txtSearch", true).FirstOrDefault() as TextBox;
            if (txt == null) return;

            var keyword = txt.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadData();
                return;
            }

            var members = _partyMemberService.SearchMembers(keyword);
            _bindingSource.DataSource = members;
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            var txt = sender as TextBox;
            if (txt == null) return;

            var keyword = txt.Text.Trim();
            if (string.IsNullOrWhiteSpace(keyword))
            {
                LoadData();
                return;
            }

            var members = _partyMemberService.SearchMembers(keyword);
            _bindingSource.DataSource = members;
        }

        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            LoadData();
            UpdateStatistics();
            MessageBox.Show("数据已刷新", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DataGridView_DoubleClick(object? sender, EventArgs e)
        {
            EditMember_Click(sender, e);
        }

        private void BatchImport_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("批量导入功能开发中...\n\n支持从Excel文件批量导入党员信息", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AdvancedQuery_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("高级查询功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void StatisticsOverview_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("统计概览功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PrintMemberList_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("报表打印功能开发中...\n\n将生成带序号的党员花名册,使用仿宋GB-2312字体,12号字号",
                "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CustomReport_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("自定义报表功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ComprehensivePrint_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("综合打印功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BackupData_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("数据备份功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void RestoreData_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("数据恢复功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UserManagement_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("用户管理功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OperationLog_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("操作日志功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void SystemSettings_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("系统设置功能开发中...", "提示",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void About_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "窑头社区党员管理系统 v1.0\n\n" +
                "开发单位:窑头社区党支部\n" +
                "技术支持:专业开发团队\n\n" +
                "本系统用于管理窑头社区党员信息,实现党员管理的数字化和规范化。",
                "关于",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void Help_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "使用说明:\n\n" +
                "1. 新增党员:点击'文件'→'党员管理'→'新增党员'\n" +
                "2. 编辑党员:选中列表中的党员,双击或点击'编辑党员'\n" +
                "3. 查询党员:在搜索框中输入姓名或身份证号\n" +
                "4. 打印报表:点击'报表打印'菜单选择相应报表\n" +
                "5. 数据备份:定期点击'文件'→'数据备份'备份数据\n\n" +
                "更多帮助请联系系统管理员。",
                "帮助",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void CheckUpdate_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("当前已是最新版本。", "检查更新",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
