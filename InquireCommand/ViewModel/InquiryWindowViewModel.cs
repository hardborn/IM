using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Nova.NovaWeb.Common;
using Nova.Net.Http;
using System.Collections;
using Nova.NovaWeb.Protocol;
using Nova.NovaWeb.OperationCommon;
using System.Windows;
using System.Windows.Input;
using Nova.NovaWeb.Windows.Command;
using Nova.NovaWeb.Windows.View;
using Microsoft.Win32;
using Nova.NovaWeb.Windows.Utilities;
using System.Data;
using System.ComponentModel;
using System.IO;
using Nova.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Threading;

namespace Nova.NovaWeb.Windows.ViewModels
{
    public class InquiryWindowViewModel : BaseViewModel, IDataErrorInfo
    {
        private bool _isInquiryType = true;
        private bool _isExcuteTime = true;
        private bool _isSetTime = false;
        private bool _isExcuteType = false;
        private bool _isRightNow = true;
        private bool _isRegular = false;
        private bool _isPeriod = false;
        private bool _isDaily = true;
        private bool _isWeek = false;
        private bool _isMonth = false;

        private List<Site> _queryTerminals = new List<Site>();
        private Hashtable _infoTable;
        private string _cultureName;
        private string _languagePath;
        private bool _hasPlayList;
        private UserTypes _userType = UserTypes.commander;
        private string _requireCmdListUrl;
        private string _requireCmdRecordUrl;
        //private ObservableCollection<CmdTypes> _commands = new ObservableCollection<CmdTypes>();
        //private ObservableCollection<CmdTypes> _selectedCommands = new ObservableCollection<CmdTypes>();

        private Dictionary<string, CmdTypes> _commands ;
        private Dictionary<string, CmdTypes> _selectedCommands ;

        private Dictionary<string, CmdInfo> _terminalCmdInfoDic = new Dictionary<string, CmdInfo>();
        private ObservableCollection<DisplayCommandInfo> _inquiryResults = new ObservableCollection<DisplayCommandInfo>();
        private ObservableCollection<DisplayCommandInfo> _selectedInquiryResults = new ObservableCollection<DisplayCommandInfo>();
        private List<CmdPhaseTypes> _cmdPhaseList = new List<CmdPhaseTypes>();
        private CollectionView _inquiryResultsView;


        private DateTime _executionStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private DateTime _executionEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        private DateTime _settingStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private DateTime _settingEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);
        private DateTime _executionModeStartTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        private DateTime _executionModeEndTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59);

        private HttpClient _httpClient;
        private AuthorizeConfig _authorizeConfig;
        private CommandInquireRequestProtocolParser _inquireProtocolParser;
        private CommandInquireReplyProtocolParser _replyProtocolParser;
        private bool _isMultiSelect = false;
        private bool _isBusy = false;

        private ICommand _viewCommandLogCommand;
        private ICommand _importCommand;
        private ICommand _inquireCmdListCommand;


        private List<CmdTypes> _queryCommandTypes;

        public InquiryWindowViewModel(List<Site> queryTerminals, List<CmdTypes> commandTypes, List<CmdTypes> queryCommandTypes, string languagePath, string cultureName, bool isEnableExecutionMode)
            :this(queryTerminals,commandTypes,queryCommandTypes,languagePath,cultureName)
        {
            _isEnableExecutionMode = isEnableExecutionMode;
        }

        public InquiryWindowViewModel(List<Site> queryTerminals, List<CmdTypes> commandTypes, List<CmdTypes> queryCommandTypes, string languagePath, string cultureName)
        {
            _authorizeConfig = new AuthorizeConfig(CommDef.SysConfigPath);
            _queryTerminals = queryTerminals;
            _queryCommandTypes = queryCommandTypes;
            _requireCmdListUrl = GetRequireCMDListURL();
            _requireCmdRecordUrl = GetRequireCMDRecordURL();


            _languagePath = languagePath;
            _cultureName = cultureName;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(_cultureName);
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(_cultureName);

            _httpClient = new HttpClient();
            GetCommandTypeList(commandTypes, _queryCommandTypes);

            if (commandTypes.Count > 1)
            {
                IsMultiSelect = true;
            }

            InquiryResultsView = CollectionViewSource.GetDefaultView(_inquiryResults) as CollectionView;
            InquiryResultsView.SortDescriptions.Clear();
            InquiryResultsView.SortDescriptions.Add(new SortDescription("ReplyCommand.SetDateTime", ListSortDirection.Descending));
            //MessageBox.Show(_queryCommandTypes[0].ToString());

            _cmdPhaseList.Add(CmdPhaseTypes.commit);
            _cmdPhaseList.Add(CmdPhaseTypes.distributed);
            _cmdPhaseList.Add(CmdPhaseTypes.excuting);
            _cmdPhaseList.Add(CmdPhaseTypes.expired);
            _cmdPhaseList.Add(CmdPhaseTypes.finished);
            _cmdPhaseList.Add(CmdPhaseTypes.finish_ok);
            _cmdPhaseList.Add(CmdPhaseTypes.finish_failed);
        }

        //public void GetBussinessResources(ResourceDictionary resources)
        //{
        //    SystemLangParser.GetInstant().UpdateLanguage(_languagePath, _cultureName, resources);
        //}

        public string GetLanguageResourcePath()
        {
            return Path.Combine(_languagePath, _cultureName, "InquireCommandModule." + _cultureName + ".xaml");
        }

        private string GetRequireCMDListURL()
        {
            bool bPhpSuccess;
            string requireCMDListURL = string.Empty;
            PhpConfigParser _phpCFParser = new PhpConfigParser(CommDef.PhpConfigPath, out bPhpSuccess);
            if (bPhpSuccess && _phpCFParser.GetInquireCmdListPhp(out requireCMDListURL))
            {
                string userIdentify;
                string webURL;
                if (_authorizeConfig.GetUserIdentifier(out userIdentify) &&
                    _authorizeConfig.GetWebAddr(out webURL))
                {
                    requireCMDListURL = requireCMDListURL.Replace(_phpCFParser.UserIdentifierName, userIdentify);
                    requireCMDListURL = webURL + "/" + requireCMDListURL;
                }
            }
            return requireCMDListURL;
        }

        private string GetRequireCMDRecordURL()
        {
            bool bPhpSuccess;
            string requireCMDRecordURL = string.Empty;
            PhpConfigParser _phpCFParser = new PhpConfigParser(CommDef.PhpConfigPath, out bPhpSuccess);
            if (bPhpSuccess && _phpCFParser.GetInquireCmdRecordPhp(out requireCMDRecordURL))
            {
                string userIdentify;
                string webURL;
                if (_authorizeConfig.GetUserIdentifier(out userIdentify) &&
                    _authorizeConfig.GetWebAddr(out webURL))
                {
                    requireCMDRecordURL = requireCMDRecordURL.Replace(_phpCFParser.UserIdentifierName, userIdentify);
                    requireCMDRecordURL = webURL + "/" + requireCMDRecordURL;
                }
            }
            return requireCMDRecordURL;
        }

        private void GetCommandTypeList(List<CmdTypes> commandTypes, List<CmdTypes> queryCommandTypes)
        {
            if (queryCommandTypes != null && commandTypes != null)
            {
                Commands = new Dictionary<string, CmdTypes>();
                foreach (var item in commandTypes)
                {
                    string itemResult;
                    if (MultiLanguageUtils.GetLanguageString(item.ToString(), out itemResult))
                    {
                        Commands.Add(itemResult, item);
                    }
                    else
                    {
                        Commands.Add(item.ToString(), item);
                    }
                }
                SelectedCommands = new Dictionary<string, CmdTypes>();
                foreach (var item in queryCommandTypes)
                {
                    string itemResult;
                    if (MultiLanguageUtils.GetLanguageString(item.ToString(), out itemResult))
                    {
                        SelectedCommands.Add(itemResult, item);
                    }
                    else
                    {
                        SelectedCommands.Add(item.ToString(), item);
                    }
                }

            }
        }

        private bool _isEnableExecutionMode = true;
        public bool IsEnableExecutionMode
        {
            get { return _isEnableExecutionMode; }
            set
            {
                if (value == _isEnableExecutionMode)
                {
                    return;
                }
                _isEnableExecutionMode = value;
                OnPropertyChanged("IsEnableExecutionMode");
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (_isBusy == value)
                {
                    return;
                }
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        public bool IsInquiryType
        {
            get { return _isInquiryType; }
            set
            {
                if (_isInquiryType != value)
                {
                    _isInquiryType = value;
                    OnPropertyChanged("IsInquiryType");
                }
            }
        }

        public bool IsExcuteTime
        {
            get { return _isExcuteTime; }
            set
            {
                if (_isExcuteTime != value)
                {
                    _isExcuteTime = value;
                    OnPropertyChanged("IsExcuteTime");
                }
            }
        }

        public bool IsSetTime
        {
            get { return _isSetTime; }
            set
            {
                if (_isSetTime != value)
                {
                    _isSetTime = value;
                    OnPropertyChanged("IsSetTime");
                }
            }
        }

        public bool IsExcuteType
        {
            get { return _isExcuteType; }
            set
            {
                if (_isExcuteType != value)
                {
                    _isExcuteType = value;
                    OnPropertyChanged("IsExcuteType");
                }
            }
        }

        public bool IsRightNow
        {
            get { return _isRightNow; }
            set
            {
                if (_isRightNow != value)
                {
                    _isRightNow = value;
                    OnPropertyChanged("IsRightNow");
                }
            }
        }

        public bool IsRegular
        {
            get { return _isRegular; }
            set
            {
                if (_isRegular != value)
                {
                    _isRegular = value;
                    OnPropertyChanged("IsRegular");
                }
            }
        }

        public bool IsPeriod
        {
            get { return _isPeriod; }
            set
            {
                if (_isPeriod != value)
                {
                    _isPeriod = value;
                    OnPropertyChanged("IsPeriod");
                }
            }
        }

        public bool IsDaily
        {
            get { return _isDaily; }
            set
            {
                if (_isDaily != value)
                {
                    _isDaily = value;
                    OnPropertyChanged("IsDaily");
                }
            }
        }

        public bool IsWeek
        {
            get { return _isWeek; }
            set
            {
                if (_isWeek != value)
                {
                    _isWeek = value;
                    OnPropertyChanged("IsWeek");
                }
            }
        }

        public bool IsMonth
        {
            get { return _isMonth; }
            set
            {
                if (_isMonth != value)
                {
                    _isMonth = value;
                    OnPropertyChanged("IsMonth");
                }
            }
        }

        public Dictionary<string, CmdTypes> Commands
        {
            get { return _commands; }
            set
            {
                _commands = value;
                OnPropertyChanged("Commands");
            }
        }

        public Dictionary<string, CmdTypes> SelectedCommands
        {
            get
            {
                //if (_selectedCommands == null)
                //{
                //    _selectedCommands = new Dictionary<string, CmdTypes>();

                //    _selectedCommands. +=
                //        (s, e) =>
                //        {
                //            //MessageBox.Show(e.NewItems.Count.ToString());
                //            OnPropertyChanged("SelectedCommands");
                //        };
                //}
                return _selectedCommands;
            }
            set
            {
                _selectedCommands = value;
                OnPropertyChanged("SelectedCommands");
            }
        }


        public DateTime ExecutionStartTime
        {
            get { return _executionStartTime; }
            set
            {
                _executionStartTime = value;
                OnPropertyChanged("ExecutionStartTime");
                OnPropertyChanged("ExecutionEndTime");
            }
        }

        public DateTime ExecutionEndTime
        {
            get { return _executionEndTime; }
            set
            {
                _executionEndTime = value;
                OnPropertyChanged("ExecutionEndTime");
                OnPropertyChanged("ExecutionStartTime");
            }
        }

        public DateTime SettingStartTime
        {
            get { return _settingStartTime; }
            set
            {
                _settingStartTime = value;
                OnPropertyChanged("SettingStartTime");
                OnPropertyChanged("SettingEndTime");
            }
        }

        public DateTime SettingEndTime
        {
            get { return _settingEndTime; }
            set
            {
                _settingEndTime = value;
                OnPropertyChanged("SettingEndTime");
                OnPropertyChanged("SettingStartTime");
            }
        }

        public DateTime ExecutionModeStartTime
        {
            get { return _executionModeStartTime; }
            set
            {
                _executionModeStartTime = value;
                OnPropertyChanged("ExecutionModeStartTime");
                OnPropertyChanged("ExecutionModeEndTime");
            }
        }

        public DateTime ExecutionModeEndTime
        {
            get { return _executionModeEndTime; }
            set
            {
                _executionModeEndTime = value;
                OnPropertyChanged("ExecutionModeEndTime");
                OnPropertyChanged("ExecutionModeStartTime");
            }
        }

        public ObservableCollection<DisplayCommandInfo> InquiryResults
        {
            get { return _inquiryResults; }
            set
            {
                _inquiryResults = value;
                OnPropertyChanged("InquiryResults");
            }
        }

        public CollectionView InquiryResultsView
        {
            get
            { return _inquiryResultsView; }
            private set
            {
                _inquiryResultsView = value;
                OnPropertyChanged("InquiryResultsView");
            }
        }

        public bool IsMultiSelect
        {
            get { return _isMultiSelect; }
            set
            {
                if (_isMultiSelect == value)
                {
                    return;
                }
                _isMultiSelect = value;
                OnPropertyChanged("IsMultiSelect");
            }
        }

        public ICommand ViewCommandLogCommand
        {
            get
            {
                if (_viewCommandLogCommand == null)
                {
                    _viewCommandLogCommand = new RelayCommand<object>(ViewCommandLog,
                        (s) =>
                        {
                            IList commandInfos = s as IList;
                            if (commandInfos == null)
                                return false;
                            if (commandInfos.Count == 1)
                                return true;
                            else
                                return false;
                        });
                }
                return _viewCommandLogCommand;
            }
        }
        public ICommand ImportCommand
        {
            get
            {
                if (_importCommand == null)
                {
                    _importCommand = new RelayCommand<object>(ExportCmdExcute,
                        (s) =>
                        {
                            if (_inquiryResults != null && _inquiryResults.Count > 0)
                                return true;
                            else
                                return false;
                        });
                }
                return _importCommand;
            }
        }

        public ICommand InquireCmdListCommand
        {
            get
            {
                if (_inquireCmdListCommand == null)
                {
                    _inquireCmdListCommand = new RelayCommand<object>(InquireCmdList,
                        (s) =>
                        {
                            if (SelectedCommands != null && SelectedCommands.Count > 0 && CanApply)
                                return true;
                            else
                                return false;
                        });
                }
                return _inquireCmdListCommand;
            }
        }
        #region Method

        private void ViewCommandLog(object selectedItems)
        {
            if (selectedItems == null)
                return;
            IList commandInfos = selectedItems as IList;

            if (commandInfos == null)
                return;

            DisplayCommandInfo commandInfo = commandInfos[0] as DisplayCommandInfo;

            //System.Diagnostics.Trace.WriteLine(commandInfo);

            if (commandInfo == null)
                return;

            InquireReplyCommand currentReplyCommand = commandInfo.ReplyCommand;
            DateTime beginTime = currentReplyCommand.SetDateTime.AddDays(-1);
            DateTime endTime = currentReplyCommand.OverDueDateTime.AddDays(1);


            string xmlData;
            if (!GetInquireCmdRecordXmlString(commandInfo.GroupId, currentReplyCommand.CommandId, beginTime, endTime, out xmlData))
            {
                MessageBox.Show("查询失败");
                return;
            }

            string replyData = string.Empty;
            string errorData = string.Empty;

            System.Diagnostics.Trace.WriteLine("命令日志请求数据：\r\n" + xmlData);
            if (!HttpClientHelper.Post(_requireCmdRecordUrl, xmlData, out replyData))
            {
                System.Diagnostics.Trace.WriteLine("查询失败");
                return;
            }
            System.Diagnostics.Trace.WriteLine("命令日志响应数据：\r\n" + replyData);
            HandInquireCmdRecordRelply(replyData);
        }

        private void HandInquireCmdRecordRelply(string replyData)
        {

            bool bSuccess;
            CommandRecordInquireReplyProtocolParser replyProtocolParser = new CommandRecordInquireReplyProtocolParser(replyData, out bSuccess);
            if (!bSuccess)
            {

                MessageBox.Show("回复数据格式有误，查询失败!");
                return;
            }

            int cmdId = -1;
            if (!replyProtocolParser.GetCmdId(out cmdId))
            {

                MessageBox.Show("获取命令ID时出错，查询失败!");
                return;
            }
            cmdOperateCodeTypes[] codeTypes;
            string[] cmdRecordInfo;
            DateTime[] dateTimeArr;
            if (!replyProtocolParser.GetRecordList(out codeTypes, out cmdRecordInfo, out dateTimeArr))
            {

                MessageBox.Show("提取回复数据时出错，查询失败!");
                return;
            }

            if (cmdRecordInfo == null)
            {
                //MessageBox.Show("提取回复数据时出错，查询失败!");
                return;
            }

            for (int i = 0; i < cmdRecordInfo.Length; i++)
            {
                if (cmdRecordInfo[i] != "")
                {
                    CmdResInfo cmdInfo;
                    if (Enum.TryParse<CmdResInfo>(cmdRecordInfo[i], true, out cmdInfo))
                    {
                        cmdRecordInfo[i] = cmdInfo.ToString();
                    }
                    else
                    {
                        cmdRecordInfo[i] = "获取操作信息失败！";
                    }
                }
            }


            System.Collections.ObjectModel.ObservableCollection<CommandLog> commandLogList = new ObservableCollection<CommandLog>();
            for (int i = 0; i < codeTypes.Length; i++)
            {
                commandLogList.Add(new CommandLog(dateTimeArr[i], codeTypes[i], cmdRecordInfo[i]));
            }

            CommandLogView commandLogView = new CommandLogView(_languagePath, _cultureName);
            commandLogView.DataContext = commandLogList;
            commandLogView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            commandLogView.ShowDialog();
        }

        /// <summary>
        /// 添加列头
        /// </summary>
        /// <param name="dataTable">数据表对象</param>
        /// <param name="columnKey">列头关键字</param>
        /// <param name="defaultColumnName">列头默认名称</param>
        private void CreateColumn(DataTable dataTable,string columnKey,string defaultColumnName)
        {
            if (dataTable == null)
            {
                return;
            }            
            dataTable.Columns.Add(GetLocalizationBussnissInfo(columnKey, defaultColumnName));
        }

        /// <summary>
        /// 导出到Excel
        /// </summary>
        /// <param name="param"></param>
        private void ExportCmdExcute(object param)
        {
            ExcelHelper _excelHelper = new ExcelHelper();
            DataTable dataTable = new DataTable();
            IFormatProvider culture = new System.Globalization.CultureInfo(_cultureName, true);

            CreateColumn(dataTable, "InquireCommandModule_UI_GroupName","GroupName");
            CreateColumn(dataTable, "InquireCommandModule_UI_TerminalName", "TerminalName");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandType", "CommandType");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandParameter", "CommandParameter");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandPhase", "CommandPhase");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandMode", "CommandMode");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandExecutionTime", "CommandExecutionTime");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandSettingTime", "CommandSettingTime");
            CreateColumn(dataTable, "InquireCommandModule_UI_CommandExpires", "CommandExpires");
            //dataTable.Columns.Add("分组");
            //dataTable.Columns.Add("终端");
            //dataTable.Columns.Add("命令类型");
            //dataTable.Columns.Add("命令参数");
            //dataTable.Columns.Add("命令阶段");
            //dataTable.Columns.Add("执行方式");
            //dataTable.Columns.Add("执行时间");
            //dataTable.Columns.Add("设置时间");
            //dataTable.Columns.Add("过期时间");
            for (int i = 0; i < _inquiryResults.Count; i++)
            {
                DataRow row = dataTable.NewRow();
                dataTable.Rows.Add(row);
                dataTable.Rows[i][0] = _inquiryResults[i].GroupName;
                dataTable.Rows[i][1] = _inquiryResults[i].TerminalName;
                dataTable.Rows[i][2] = GetLocalizationBussnissInfo(_inquiryResults[i].ReplyCommand.CommandType.ToString(),_inquiryResults[i].ReplyCommand.CommandType.ToString());
                dataTable.Rows[i][3] = GetCommandPara(_inquiryResults[i].ReplyCommand);
                dataTable.Rows[i][4] = GetLocalizationBussnissInfo(_inquiryResults[i].ReplyCommand.CmdPhase.ToString(),_inquiryResults[i].ReplyCommand.CmdPhase.ToString());
                dataTable.Rows[i][5] = GetLocalizationBussnissInfo(_inquiryResults[i].ReplyCommand.CommandModeType.ToString(), _inquiryResults[i].ReplyCommand.CommandModeType.ToString());
                dataTable.Rows[i][6] = _inquiryResults[i].ReplyCommand.ExecuteDateTime.GetDateTimeFormats('f', culture)[0].ToString();
                dataTable.Rows[i][7] = _inquiryResults[i].ReplyCommand.SetDateTime.GetDateTimeFormats('f', culture)[0].ToString();
                dataTable.Rows[i][8] = _inquiryResults[i].ReplyCommand.OverDueDateTime.GetDateTimeFormats('f', culture)[0].ToString();
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Excel (*.XLS)|*.xls"; ;
            if ((bool)(saveFileDialog.ShowDialog()))
            {
                try
                {
                    if (File.Exists(saveFileDialog.FileName))
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                    if (!_excelHelper.SaveToExcel(saveFileDialog.FileName, dataTable))
                    {
                        if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Export Failure");
                            return;
                        }
                        else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("导出失败");
                            return;
                        }
                    }
                    else
                    {
                        if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("Export Success");
                            return;
                        }
                        else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("导出成功");
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Export Failure：" + ex.Message);
                        return;
                    }
                    else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("导出失败：" + ex.Message);
                        return;
                    }
                    
                }
                //if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                //{
                //    MessageBox.Show("Export Successful！");
                //}
                //else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                //{
                //    MessageBox.Show("导出成功");
                //}


            }
        }

        private void InquireCmdList(object obj)
        {
            //List<string> seletedCmdTypes = obj as List<string>;
            InquireCmdList(_selectedCommands);
        }

        private string GetLocalizationBussnissInfo(string key, string defaultInfo)
        {
            string itemResult;
            if (MultiLanguageUtils.GetLanguageString(key, out itemResult))
            {
                return itemResult;
            }
            else
            {
                return itemResult = defaultInfo;
            }
        }

        public void InquireCmdList(Dictionary<string, CmdTypes> seletedCmdTypes)
        {
            InquiryResults.Clear();

            SelectedCommands = seletedCmdTypes;
           
            var uiDispatcher = System.Windows.Application.Current.Dispatcher;

            BackgroundWorker worker = new BackgroundWorker();

            worker.DoWork += (o, ea) =>
            {

                bool bRequireSucc = true;
                for (int i = 0; i < _queryTerminals.Count; i++)
                {

                    int groupId = Convert.ToInt32(_queryTerminals[i].GroupId);

                    string pwd = string.Empty;
                    if (!_authorizeConfig.GetCommanderPwd(groupId, out pwd))
                    {
                        string showInfo = GetLocalizationBussnissInfo("InquireCommandModule_Business_PasswordWrong", "密码错误，查询失败！");
                        MessageBox.Show(showInfo);
                    }

                    SetRequireCmdProtocolParser(_queryTerminals[i].Mac, groupId, pwd);

                    string replyData = string.Empty;
                    string errorData = string.Empty;

                    //_httpClient.Post(_requireCmdListUrl, _inquireProtocolParser.GetXmlString(), out replyData, out errorData);

                    if (!HttpClientHelper.Post(_requireCmdListUrl, _inquireProtocolParser.GetXmlString(), out replyData))
                    {
                        //bRequireSucc = false;
                        break;
                    }

                    if (replyData.Contains("<"))
                    {
                        replyData = replyData.Remove(0, replyData.IndexOf('<'));
                    }

                    if (!IsReplyRight(replyData, errorData))
                    {
                        bRequireSucc = false;
                        break;
                    }

                    GetCmdListList(replyData, _queryTerminals[i].Name, groupId, _queryTerminals[i].GroupName);
                }
                if (!bRequireSucc)
                {
                    return;
                }
                if (!HandleFinishedCmd())
                {
                    MessageBox.Show("处理失败！");
                    return;
                }
                uiDispatcher.BeginInvoke((Action)(() =>
                {
                    GetInquiryCmdResult();
                }));
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                IsBusy = false;
            };
            IsBusy = true;
            worker.RunWorkerAsync();
        }

        private List<CmdTypes> ConverterCmdTypeList(List<string> seletedCmdTypes)
        {
            if (seletedCmdTypes == null)
            {
                return null;
            }

            List<CmdTypes> commandTypeList = new List<CmdTypes>();
            for (int i = 0; i < seletedCmdTypes.Count; i++)
            {
                CmdTypes parseResult;
                if (CmdTypes.TryParse(seletedCmdTypes[i], false, out parseResult) && !commandTypeList.Contains(parseResult))
                {
                    commandTypeList.Add(parseResult);
                }
            }
            return commandTypeList;
        }

        private void GetInquiryCmdResult()
        {
            foreach (string terminalName in _terminalCmdInfoDic.Keys)
            {
                List<InquireReplyCommand> oneTerminalCmdList = _terminalCmdInfoDic[terminalName].CmdList;
                for (int i = 0; i < oneTerminalCmdList.Count; i++)
                {
                    DisplayCommandInfo commandInfo = new DisplayCommandInfo(_terminalCmdInfoDic[terminalName].GroupId, _terminalCmdInfoDic[terminalName].GroupName, terminalName, oneTerminalCmdList[i]);

                    InquiryResults.Add(commandInfo);
                }
            }
            InquiryResultsView.Refresh();
        }

        //将回复数据中的命令添加到字典中
        private void GetCmdListList(string replyData, string terminalName, int groupId, string groupName)
        {
            List<InquireReplyCommand> replyCmdList = new List<InquireReplyCommand>();
            _replyProtocolParser.GetCommandList(out replyCmdList);
            _terminalCmdInfoDic[terminalName] = new CmdInfo(groupId, groupName, replyCmdList);
        }

        private bool IsReplyRight(string replyData, string errorData)
        {

            //if (!string.IsNullOrEmpty(errorData))
            //{
            //    MessageBox.Show(errorData);
            //    return false;
            //}
            //if (IsSysError(replyData))
            //{
            //    return false;
            //}

            bool bSuccess;
            _replyProtocolParser = new CommandInquireReplyProtocolParser(replyData, out bSuccess);
            if (!bSuccess)
            {
                string errorInfo = GetLocalizationBussnissInfo("InquireCommandModule_Business_ResolveInquireReplyFailed", "查询协议解析失败！");
                MessageBox.Show(errorInfo, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            List<InquireReplyCommand> replyCmdList = new List<InquireReplyCommand>();
            try
            {
                string mac = string.Empty;
                if (!_replyProtocolParser.GetMac(out mac))
                {
                    string errorInfo = GetLocalizationBussnissInfo("InquireCommandModule_Business_GetMacFailed", "提取回复数据中的mac失败，查询失败！");
                    MessageBox.Show(errorInfo, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (!_replyProtocolParser.GetCommandList(out replyCmdList))
                {
                    string errorInfo = GetLocalizationBussnissInfo("InquireCommandModule_Business_GetCmdListFailed", "提取命令列表时出现错误，查询失败!");
                    MessageBox.Show(errorInfo, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            }
            catch (System.Exception e)
            {
                string errorInfo = GetLocalizationBussnissInfo("InquireCommandModule_Business_ExceptionRelove", "解析回复数据时出现异常，查询失败！");
                MessageBox.Show(errorInfo, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        //判断回复的数据是否为系统错误
        private bool IsSysError(string replyData)
        {

            bool bSysError = false;
            SystemError systemError;
            systemError = new SystemError(replyData, out bSysError);
            SysErrorCode errCode;
            string errorInfo;
            if (systemError.GetErrCode(out errCode))
            {

                errorInfo = GetLocalizationBussnissInfo(errCode.ToString(), errCode.ToString());
                return true;
                //switch (errCode)
                //{
                //    case SysErrorCode.Error_CommandInquire_MissConstraintField:
                //        string cmdNotComplete = "查询条件不完整，查询失败!";
                //        MessageBox.Show(cmdNotComplete, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_GroupNotExists:
                //        string groupNotExist = "该组不存在，查询失败!";
                //        MessageBox.Show(groupNotExist, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_IllegalAccount:
                //        string pwdWrong = "密码错误，查询失败!";
                //        MessageBox.Show(pwdWrong, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_IllegalXml:
                //        string invalidFormat = "查询指令格式错误，查询失败!";
                //        MessageBox.Show(invalidFormat, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_TerminalNotBelongToGroup:
                //        string terminalNotBelongToGroup = "查询终端不属于该组，查询失败!";
                //        MessageBox.Show(terminalNotBelongToGroup, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_TerminalNotExists:
                //        string terminalNotExist = "该终端不存在，查询失败!";
                //        MessageBox.Show(terminalNotExist, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //    case SysErrorCode.Error_CommandInquire_DataFormatError:
                //        string inquireDataIllegal = "数据格式错误，查询失败";
                //        MessageBox.Show(inquireDataIllegal, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                //        return true;
                //}
            }
            return false;
        }

        private bool HandleFinishedCmd()
        {
            foreach (string terminalName in _terminalCmdInfoDic.Keys)
            {
                CmdInfo oneTerminalCmdInfo = _terminalCmdInfoDic[terminalName];
                List<InquireReplyCommand> cmdList = oneTerminalCmdInfo.CmdList;
                for (int i = 0; i < cmdList.Count; i++)
                {

                    if ((cmdList[i].CmdPhase == CmdPhaseTypes.finished) && !InquireFinishedCmdOperateCode(terminalName, oneTerminalCmdInfo.GroupId, cmdList[i], i))
                    {
                        return false;
                    }
                }
            }
            //MessageBox.Show("Go On...");
            return true;
        }

        private bool InquireFinishedCmdOperateCode(string terminalName, int groupId, InquireReplyCommand cmd, int cmdIndex)
        {
            string inquireRecordXmlString;
            GetInquireCmdRecordXmlString(groupId, cmd.CommandId, cmd.SetDateTime.AddDays(-1), cmd.OverDueDateTime.AddDays(1), out inquireRecordXmlString);

            string replyData;
            string errorData;
            //_httpClient.Post(_requireCmdRecordUrl, inquireRecordXmlString, out replyData, out errorData);

            //if (!IsInquireCmdRecordReplyRight(replyData, errorData))
            //{
            //    MessageBox.Show("响应失败！！");
            //    return false;
            //}


            if (!HttpClientHelper.Post(_requireCmdRecordUrl, inquireRecordXmlString, out replyData))
            {
                return false;
            }

            cmdOperateCodeTypes[] codeTypes;
            string[] cmdRecordInfo;
            DateTime[] dateTimeArr;

            bool bSuccess;
            CommandRecordInquireReplyProtocolParser replyProtocolParser = new CommandRecordInquireReplyProtocolParser(replyData, out bSuccess);
            replyProtocolParser.GetRecordList(out codeTypes, out cmdRecordInfo, out dateTimeArr);
            for (int i = 0; i < codeTypes.Length; i++)
            {
                if ((codeTypes[i] == cmdOperateCodeTypes.excuteOk) || (codeTypes[i] == cmdOperateCodeTypes.excuteFailed))
                {
                    _terminalCmdInfoDic[terminalName].CmdList[cmdIndex].CmdOperateType = codeTypes[i];
                    return true;
                }
            }
            return true;
        }

        private bool IsInquireCmdRecordReplyRight(string replyData, string errData)
        {
            string notice = "提示";
            if (_infoTable != null && _infoTable.Contains("notice".ToLower()))
            {
                notice = _infoTable["notice".ToLower()].ToString();
            }
            if (errData != "")
            {
                string communicateFailed = "与服务器通讯出错，查询失败!";
                if (_infoTable != null && _infoTable.Contains("communicateFailed".ToLower()))
                {
                    communicateFailed = _infoTable["communicateFailed".ToLower()].ToString();
                }
                return false;
            }

            if (IsInquireRecordSysError(replyData))
            {
                return false;
            }

            bool bSuccess;
            CommandRecordInquireReplyProtocolParser replyProtocolParser = new CommandRecordInquireReplyProtocolParser(replyData, out bSuccess);
            //MessageBox.Show(string.Format("{0}\r\n{1}", bSuccess.ToString(), replyData));
            if (!bSuccess)
            {
                string replyIllegal = "回复数据格式有误，查询失败!";
                if (_infoTable != null && _infoTable.Contains("illegalReply".ToLower()))
                {
                    replyIllegal = _infoTable["illegalReply".ToLower()].ToString();
                }
                return false;
            }

            int cmdId = -1;
            if (!replyProtocolParser.GetCmdId(out cmdId))
            {
                string getIdErr = "获取命令ID时出错，查询失败!";
                if (_infoTable != null && _infoTable.Contains("getCmdIdFailed".ToLower()))
                {
                    getIdErr = _infoTable["getCmdIdFailed".ToLower()].ToString();
                }
                return false;
            }
            cmdOperateCodeTypes[] codeTypes;
            string[] cmdRecordInfo;
            DateTime[] dateTimeArr;
            if (!replyProtocolParser.GetRecordList(out codeTypes, out cmdRecordInfo, out dateTimeArr))
            {
                string resolveErr = "提取回复数据时出错，查询失败!";
                if (_infoTable != null && _infoTable.Contains("resolveReplyFailed".ToLower()))
                {
                    resolveErr = _infoTable["resolveReplyFailed".ToLower()].ToString();
                }
                return false;
            }

            return true;
        }

        private bool IsInquireRecordSysError(string replyData)
        {
            bool bSysError = false;
            SystemError sysErr = new SystemError(replyData, out bSysError);
            SysErrorCode errCode = SysErrorCode.Operation_OK;
            if (sysErr.GetErrCode(out errCode))
            {
                string notice = "提示";
                if (_infoTable != null && _infoTable.Contains("notice".ToLower()))
                {
                    notice = _infoTable["notice".ToLower()].ToString();
                }
                switch (errCode)
                {
                    case SysErrorCode.Error_CommandlLog_MissConstraintField:
                        string conditionNotComplete = "查询条件不完整，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquireCmdNotComplete".ToLower()))
                        {
                            conditionNotComplete = _infoTable["inquireCmdNotComplete".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_CommandNotBelongToGroup:
                        string notBelongToGroup = "终端不属于该组，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquireTerminalNotBelongToGroup".ToLower()))
                        {
                            notBelongToGroup = _infoTable["inquireTerminalNotBelongToGroup".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_EmptyPassword:
                        string emptyPwd = "查询密码为空，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquirePwdIsNull".ToLower()))
                        {
                            emptyPwd = _infoTable["inquirePwdIsNull".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_GroupNotExists:
                        string groupNotExist = "终端所属组不存在，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquireGroupNotExist".ToLower()))
                        {
                            groupNotExist = _infoTable["inquireGroupNotExist".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_IllegalAccount:
                        string pwdWrong = "密码错误，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquirePwdWrong".ToLower()))
                        {
                            pwdWrong = _infoTable["inquirePwdWrong".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_IllegalAdminType:
                        string illegalUserType = "当前用户无法执行此操作，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("illegalUserType".ToLower()))
                        {
                            illegalUserType = _infoTable["illegalUserType".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_IllegalXml:
                        string illegalXml = "查询数据格式错误，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquireDataIllegal".ToLower()))
                        {
                            illegalXml = _infoTable["inquireDataIllegal".ToLower()].ToString();
                        }
                        return true;
                    case SysErrorCode.Error_CommandLog_DataFormatError:
                        string inquireDataIllegal = "查询数据格式错误，查询失败!";
                        if (_infoTable != null && _infoTable.Contains("inquireDataIllegal".ToLower()))
                        {
                            inquireDataIllegal = _infoTable["inquireDataIllegal".ToLower()].ToString();
                        }
                        return true;
                }
            }
            return false;
        }



        private bool GetInquireCmdRecordXmlString(int groupId, int cmdId, DateTime beginDateTime, DateTime endDateTime, out string xmlString)
        {
            xmlString = null;
            bool bSuccess;
            CommandRecordInquireRequestProtocolParser _cmdRecordInquireRequest = new CommandRecordInquireRequestProtocolParser(out bSuccess);
            _cmdRecordInquireRequest.SetGroupId(groupId);
            _cmdRecordInquireRequest.SetUserType(_userType);

            string pwd = "";
            if (_userType == UserTypes.publisher)
            {
                _authorizeConfig.GetPublishPwd(groupId, out pwd);
            }
            else
            {
                _authorizeConfig.GetCommanderPwd(groupId, out pwd);
            }

            _cmdRecordInquireRequest.SetPassword(pwd);
            _cmdRecordInquireRequest.SetRecordDateTimeRange(beginDateTime, endDateTime);

            _cmdRecordInquireRequest.SetCmdId(cmdId);

            xmlString = _cmdRecordInquireRequest.GetXmlString();
            return true;
        }


        private void SetRequireCmdProtocolParser(string terminalMac, int groupId, string pwd)
        {
            bool bSucc;
            _inquireProtocolParser = new CommandInquireRequestProtocolParser(out bSucc);

            #region 设置属性
            _inquireProtocolParser.SetPassword(pwd);
            if (_isExcuteTime)
            {
                _inquireProtocolParser.SetExecuteDateTime(_executionStartTime, _executionEndTime);
            }
            if (_isSetTime)
            {
                _inquireProtocolParser.SetDateTime(_settingStartTime, _settingEndTime);
            }


            _inquireProtocolParser.SetGroupId(groupId.ToString());


            _inquireProtocolParser.SetMac(terminalMac);

            _inquireProtocolParser.SetUserType(_userType);


            _inquireProtocolParser.SetCommandType(GetCmdTypeList());
            _inquireProtocolParser.SetStatusRange(GetStatusList());
            //_inquireProtocolParser.SetPhaseRange(_cmdPhaseList);
            //_inquireProtocolParser.SetCommandMode(CmdModeTypes.immediate);
            if (_isExcuteType)
            {
                if (_isRegular)
                {
                    _inquireProtocolParser.SetCommandMode(CmdModeTypes.timing);
                    _inquireProtocolParser.SetExecuteDateTime(_executionModeStartTime, _executionModeEndTime);
                }
                else if (_isPeriod)
                {
                    if (_isDaily)
                    {
                        _inquireProtocolParser.SetCommandMode(CmdModeTypes.daily);
                    }
                    else if (_isWeek)
                    {
                        _inquireProtocolParser.SetCommandMode(CmdModeTypes.weekly);
                    }
                    else
                    {
                        _inquireProtocolParser.SetCommandMode(CmdModeTypes.monthly);
                    }
                }
                else
                {
                    _inquireProtocolParser.SetCommandMode(CmdModeTypes.immediate);
                }
            }
            #endregion
        }

        private ICommand _selectionChangedCommand;
        public ICommand SelectionChangedCommand
        {
            get
            {
                if (_selectionChangedCommand == null)
                {
                    _selectionChangedCommand = new RelayCommand<object>(T => ResultSelectionChanged(T), null);
                }
                return _selectionChangedCommand;
            }
        }

        private void ResultSelectionChanged(object sender)
        {
            SelectionChangedEventArgs e = sender as SelectionChangedEventArgs;
            if (e == null)
            {
                return;
            }
            DataGrid dataGrid = e.Source as DataGrid;
            if (dataGrid == null)
            {
                return;
            }

            if (dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count > 1)
            {
                dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            }
            else
            {
                dataGrid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
            }

        }



        private List<CmdStatus> GetStatusList()
        {
            List<CmdStatus> statusList = new List<CmdStatus>();
            statusList.Add(CmdStatus.active);
            return statusList;
        }

        private List<CmdTypes> GetCmdTypeList()
        {
            return _selectedCommands.Values.ToList();
        }


        /// <summary>
        /// 获取命令参数的显示数据
        /// </summary>
        /// <param name="replyCmd"></param>
        /// <returns></returns>
        private string GetCommandPara(InquireReplyCommand replyCmd)
        {
            if (
                replyCmd.CommandType == CmdTypes.downloadPlaylist ||
                replyCmd.CommandType == CmdTypes.downloadSchedule ||
                replyCmd.CommandType == CmdTypes.downLoadPlayProgam ||
                replyCmd.CommandType == CmdTypes.uploadDirSet)
            {
                string tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                tempPara = tempPara.Substring(tempPara.LastIndexOf('/') + 1);
                tempPara = RemoveMD5InFileName(tempPara);
                return tempPara;
            }
            else if (
                replyCmd.CommandType == CmdTypes.dviEncrypt ||
                replyCmd.CommandType == CmdTypes.modifyDviEncryptPwd ||
                replyCmd.CommandType == CmdTypes.enableDviEncrypt ||
                replyCmd.CommandType == CmdTypes.readDviEncryptStatus ||
                replyCmd.CommandType == CmdTypes.readDviEncryptConfig)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.emergencyMessage)
            {
                return GetEmergencyText(replyCmd);
            }
            //else if (replyCmd.CommandType == CmdTypes.switchLed)//参数有变化,添加类型CmdTypes.plutoPowerPlanSet,CmdTypes.displayModeSet,autoRestartTerminalSet,restartTeminal,downLoadPlayProgam,lockUnlockTC(无参数)
            //{
            //    SwitchLedType switchType;
            //    if (!IsSwitchLedParaValid(replyCmd, out switchType))
            //    {
            //        return replyCmd.CommandPara;
            //    }

            //    return replyCmd.CommandPara;
            //}
            else if (replyCmd.CommandType == CmdTypes.lockUnlockTC)
            {
                try
                {
                    string lockText = "";
                    if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempPara = replyCmd.CommandPara;
                        //if (replyCmd.CommandPara == "0")
                        //{
                        //    tempPara = replyCmd.CommandPara;
                        //}
                        //else
                        //{
                        //    tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                        //}

                        if (tempPara == "0")
                        {
                            lockText = "lockTerminal";
                        }
                        else
                        {
                            lockText = "unLockTerminal";
                        }
                        return lockText;
                    }
                    else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempPara = replyCmd.CommandPara;
                        //if (replyCmd.CommandPara == "0")
                        //{
                        //    tempPara = replyCmd.CommandPara;
                        //}
                        //else
                        //{
                        //    tempPara = replyCmd.CommandPara.Substring(0, replyCmd.CommandPara.IndexOf('+'));
                        //}
                        if (tempPara == "0")
                        {
                            lockText = "锁定";
                        }
                        else
                        {
                            lockText = "解锁";
                        }
                        return lockText;
                    }
                    return lockText;
                }
                catch (Exception)
                {
                    return string.Empty;
                }

            }
            else if (replyCmd.CommandType == CmdTypes.soundSchSet ||
                replyCmd.CommandType == CmdTypes.lightSchSet)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.uploadLog)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 5)
                {
                    return "";
                }
                return paraArr[3] + " -> " + paraArr[4];
            }
            else if (replyCmd.CommandType == CmdTypes.terminalConfig)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 4)
                {
                    return "";
                }
                return paraArr[0] + " | " + paraArr[1];
            }
            else if (replyCmd.CommandType == CmdTypes.cleanMediasPeriodSet)
            {
                string cmdPara = string.Empty;
                if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                {

                    if (replyCmd.CommandPara == "0")
                    {
                        cmdPara = "One day previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "1")
                    {
                        cmdPara = "Three day previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "2")
                    {
                        cmdPara = "One week previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "3")
                    {
                        cmdPara = "One month previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "4")
                    {
                        cmdPara = "Two months previously downloaded media files";
                    }
                    else if (replyCmd.CommandPara == "5")
                    {
                        cmdPara = "Three months previously uploaded media files";
                    }
                    else
                    {
                        cmdPara = string.Empty;
                    }
                }
                else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                {

                    if (replyCmd.CommandPara == "0")
                    {
                        cmdPara = "一天以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "1")
                    {
                        cmdPara = "三天以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "2")
                    {
                        cmdPara = "一周以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "3")
                    {
                        cmdPara = "一个月以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "4")
                    {
                        cmdPara = "两个月以前下载的媒体文件";
                    }
                    else if (replyCmd.CommandPara == "5")
                    {
                        cmdPara = "三个月以前下载的媒体文件";
                    }
                    else
                    {
                        cmdPara = string.Empty;
                    }


                }
                return cmdPara;

            }
            else if (replyCmd.CommandType == CmdTypes.updateSoftware)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr.Length < 4)
                {
                    return string.Empty;
                }
                return paraArr[3];
            }
            else if (replyCmd.CommandType == CmdTypes.screenShotPeriodSet)
            {
                string cmdPara = "";
                if (_cultureName.Equals("en", StringComparison.OrdinalIgnoreCase))
                {
                    string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);

                    if (paraArr.Length < 1)
                    {
                        return cmdPara;
                    }
                    if (paraArr[0] == "0")
                    {
                        cmdPara = "Close Snapshot";
                    }
                    else
                    {
                        cmdPara = "Open Snapshot，Set a period of:";
                        string unit = "min";
                        cmdPara = cmdPara + paraArr[0] + unit;
                    }


                }
                else if (_cultureName.Equals("zh-cn", StringComparison.OrdinalIgnoreCase))
                {
                    string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                    if (paraArr.Length < 1)
                    {
                        return cmdPara;
                    }
                    if (paraArr[0] == "0")
                    {
                        cmdPara = "关闭快照";
                    }
                    else
                    {
                        cmdPara = "开启快照，并周期为：";
                        string unit = "分钟";
                        cmdPara = cmdPara + paraArr[0] + unit;
                    }
                }
                return cmdPara;

            }
            else if (replyCmd.CommandType == CmdTypes.emergencyPlaylist)
            {
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                string cmdPara = "", playlistNameText = "清单名称：", startTimeText = "起始播放时间：", duringText = "播放时长：";
                string itemResult1;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_playlistName", out itemResult1);
                string itemResult2;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_startTime", out itemResult2);
                string itemResult3;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_during", out itemResult3);

                if (paraArr.Length >= 8)
                {
                    cmdPara = itemResult1 + Path.GetFileNameWithoutExtension(paraArr[0]) + itemResult2 + paraArr[6] + itemResult3 + paraArr[7];
                    cmdPara = string.Format("{0}{1}  {2}{3}  {4}{5}", itemResult1, Path.GetFileNameWithoutExtension(paraArr[0]), itemResult2, paraArr[6], itemResult3, paraArr[7]);
                }
                return cmdPara;
            }
            else if (replyCmd.CommandType == CmdTypes.switchLed)
            {
                return string.Empty;
            }
            else if (replyCmd.CommandType == CmdTypes.uploadLogParaSet ||
                replyCmd.CommandType == CmdTypes.displayModeSet ||
                replyCmd.CommandType == CmdTypes.mediaSync)
            {
                string cmdPara = string.Empty;
                if (replyCmd.CommandPara == "1")
                {
                    string result;
                    MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Enable", out result);
                    cmdPara = result;
                }
                else if (replyCmd.CommandPara == "0")
                {
                    string result;
                    MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Disabled", out result);
                    cmdPara = result;
                }
                return cmdPara;
            }
            else if (replyCmd.CommandType == CmdTypes.autoRestartTerminalSet)
            {
                string cmdPara = string.Empty;
                string[] paraArr = replyCmd.CommandPara.Split(new string[] { "+" }, StringSplitOptions.None);
                if (paraArr[0] == "1")
                {
                    string result;
                    MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Enable", out result);
                    cmdPara = result;
                    cmdPara += "(" + paraArr[1] + ")";
                }
                else if (paraArr[0] == "0")
                {
                    string result;
                    MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_Disabled", out result);
                    cmdPara = result;
                }
                return cmdPara;
            }
            else
            {
                return replyCmd.CommandPara;
            }
        }


        private string RemoveMD5InFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return string.Empty;
            }
            string[] nameSplits = fileName.Split(new Char[] { '.' });
            if (nameSplits.Length == 2)
            {
                return fileName;
            }
            else if (nameSplits.Length == 3)
            {
                return nameSplits[0] + "." + nameSplits[2];
            }
            else
            {
                return nameSplits[0] + "." + nameSplits[nameSplits.Length - 1];
            }
        }

        private string GetEmergencyText(InquireReplyCommand replyCmd)
        {
            string msgTypeStr = replyCmd.CommandPara.Substring(0, 1);
            int msgTypeInt = Int32.Parse(msgTypeStr);
            MsgTypeEnum msgType = (MsgTypeEnum)msgTypeInt;
            bool bSuccess;
            EmergencyMsgBase emergMsgClass = null;
            string InvalidCmdPara = "命令参数格式错误";


            switch (msgType)
            {
                case MsgTypeEnum.MerryGoRound:
                    emergMsgClass = new EmergencyMsgMerryGoRound(replyCmd.CommandPara, out bSuccess);
                    break;
                case MsgTypeEnum.SingleLine:
                    emergMsgClass = new EmergencyMsgSingleLine(replyCmd.CommandPara, out bSuccess);
                    break;
                case MsgTypeEnum.Static:
                    emergMsgClass = new EmergencyMsgStatic(replyCmd.CommandPara, out bSuccess);
                    break;
                default:
                    return InvalidCmdPara;
            }
            if (!bSuccess)
            {
                return  InvalidCmdPara;
            }
            return emergMsgClass.MsgText;
        }

        private bool IsSwitchLedParaValid(InquireReplyCommand cmd, out SwitchLedType switchType)
        {
            switchType = SwitchLedType.normal;
            try
            {
                int intSwithcType = Int32.Parse(cmd.CommandPara.Substring(0, 1));
                switchType = (SwitchLedType)intSwithcType;
            }
            catch
            {
                return false;
            }
            return true;
        }

        #endregion

        #region Validate 验证

        private bool _canApply;
        /// <summary>
        /// 是否通过验证并使能确定按钮
        /// </summary>
        public bool CanApply
        {
            get { return _canApply; }
            set
            {
                if (_canApply != value)
                {
                    _canApply = value;
                    OnPropertyChanged("CanApply");
                }
            }
        }

        static readonly string[] ValidatedProperties = 
        { 
            "ExecutionStartTime",
            "ExecutionEndTime",
            "SettingEndTime",
            "SettingStartTime",
            "ExecutionModeEndTime",
            "ExecutionModeStartTime"
        };

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get { return GetValidationError(columnName); }
        }

        protected string[] _error;

        private string GetValidationError(string propertyName)
        {
            if (Array.IndexOf(ValidatedProperties, propertyName) < 0)
                return null;

            string result = null;

            if (_error == null)
                _error = new string[6];

            switch (propertyName)
            {
                case "ExecutionEndTime":
                    result = this.ValidateExecutionEndTime(this.ExecutionEndTime);
                    _error[0] = result;
                    break;
                case "ExecutionStartTime":
                    result = this.ValidateExecutionStartTime(this.ExecutionStartTime);
                    _error[1] = result;
                    break;
                case "SettingEndTime":
                    result = this.ValidateSettingEndTime(this.SettingEndTime);
                    _error[2] = result;
                    break;
                case "SettingStartTime":
                    result = this.ValidateSettingStartTime(this.SettingStartTime);
                    _error[3] = result;
                    break;
                case "ExecutionModeEndTime":
                    result = this.ValidateExecutionModeEndTime(this.ExecutionModeEndTime);
                    _error[4] = result;
                    break;
                case "ExecutionModeStartTime":
                    result = this.ValidateExecutionModeStartTime(this.ExecutionModeStartTime);
                    _error[5] = result;
                    break;

                default:
                    break;
            }

            CanApply = ValidateFields(_error);

            return result;
        }

        internal static bool ValidateFields(string[] value)
        {
            foreach (string s in value)
                if (!string.IsNullOrEmpty(s))
                    return false;
            return true;
        }

        private string ValidateExecutionEndTime(DateTime endTime)
        {
            if (endTime.CompareTo(this.ExecutionStartTime) < 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_EndTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateExecutionStartTime(DateTime startTime)
        {
            if (startTime.CompareTo(this.ExecutionEndTime) > 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_StartTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateSettingEndTime(DateTime endTime)
        {
            if (endTime.CompareTo(this.SettingStartTime) < 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_EndTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateSettingStartTime(DateTime startTime)
        {
            if (startTime.CompareTo(this.SettingEndTime) > 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_StartTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateExecutionModeEndTime(DateTime endTime)
        {
            if (endTime.CompareTo(this.ExecutionModeStartTime) < 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_EndTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        private string ValidateExecutionModeStartTime(DateTime startTime)
        {
            if (startTime.CompareTo(this.ExecutionModeEndTime) > 0)
            {
                string errorInfo;
                MultiLanguageUtils.GetLanguageString("InquireCommandModule_Business_StartTimeError", out errorInfo);
                return errorInfo;
            }
            return null;
        }

        #endregion
    }

    public class CmdInfo
    {
        private int _groupId;
        public int GroupId
        {
            get
            {
                return _groupId;
            }
        }

        private string _groupName;
        public string GroupName
        {
            get
            {
                return _groupName;
            }
        }

        private List<InquireReplyCommand> _cmdList;
        public List<InquireReplyCommand> CmdList
        {
            get
            {
                return _cmdList;
            }
        }

        public CmdInfo(int groupId, string groupName, List<InquireReplyCommand> cmdList)
        {
            _groupId = groupId;
            _groupName = groupName;
            _cmdList = cmdList;
        }
    }

    public class DisplayCommandInfo : BaseViewModel
    {
        private string _groupName;
        private string _terminalName;
        private int _groupId;
        private InquireReplyCommand _replyCommand;

        public DisplayCommandInfo(int groupId, string groupName, string terminalName, InquireReplyCommand replyCommand)
        {
            _groupName = groupName;
            _terminalName = terminalName;
            _replyCommand = replyCommand;
            _groupId = groupId;
        }

        public int GroupId
        {
            get { return _groupId; }
            set { _groupId = value; OnPropertyChanged("GroupId"); }
        }

        public string GroupName
        {
            get { return _groupName; }
            set { _groupName = value; OnPropertyChanged("GroupName"); }
        }

        public string TerminalName
        {
            get { return _terminalName; }
            set { _terminalName = value; OnPropertyChanged("TerminalName"); }
        }

        public InquireReplyCommand ReplyCommand
        {
            get { return _replyCommand; }
            set { _replyCommand = value; OnPropertyChanged("ReplyCommand"); }
        }
    }

    public class CommandLog : BaseViewModel
    {
        private DateTime _operationTime;
        private cmdOperateCodeTypes _operationType;
        private string _operationInfo;

        public CommandLog(DateTime time, cmdOperateCodeTypes type, string info)
        {
            _operationTime = time;
            _operationType = type;
            _operationInfo = info;
        }

        public DateTime OperationTime
        {
            get { return _operationTime; }
            set
            {
                if (_operationTime != value)
                {
                    _operationTime = value;
                    OnPropertyChanged("OperationTime");
                }
            }
        }
        public cmdOperateCodeTypes OperationType
        {
            get { return _operationType; }
            set
            {
                if (_operationType != value)
                {
                    _operationType = value;
                    OnPropertyChanged("OperationType");
                }
            }
        }
        public string OperationInfo
        {
            get { return _operationInfo; }
            set
            {
                if (_operationInfo != value)
                {
                    _operationInfo = value;
                    OnPropertyChanged("OperationInfo");
                }
            }
        }
    }
   
}
