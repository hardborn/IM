using Nova.NovaWeb.McGo.Common;
using Nova.NovaWeb.OperationCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System.Configuration;
using GalaSoft.MvvmLight.Messaging;

namespace Nova.NovaWeb.McGo.DAL
{
    [Serializable]
    public class PlatformConfig
    {
        public static readonly string NebulaDir = String.Format(@"{0}\MC-go", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));

        public static readonly string CopyRightFilePath = String.Format(@"{0}\Resources\CopyRight.xml", AppDomain.CurrentDomain.BaseDirectory);

        public static readonly string UserConfigPath = String.Format(@"{0}\UserConfig.xml", NebulaDir);

        public static readonly string PlatformConfigFilePath = String.Format(@"{0}\PlatformConfig", NebulaDir);

        //默认的工作路径，OEM的时候只改这个就行，AppData下的东西是隐藏的
        public static readonly string DefaultWorkspacePath = String.Format(@"{0}\MC-go\", Environment.GetFolderPath(Environment.SpecialFolder.Personal));

        public static readonly string LanguageResourcePath = String.Format(@"{0}Lang\", AppDomain.CurrentDomain.BaseDirectory);

        /// <summary>
        /// 工作区二级目录（Local）
        /// </summary>
        public static readonly string LocalAssignationDirectory = "Local";
        /// <summary>
        /// 工作区二级目录（Remote）
        /// </summary>
        public static readonly string RemoteAssignationDrectory = "Remote";
        
        /// <summary>
        /// 工作区三级目录（Log）
        /// </summary>
        public static readonly string LogAssignationDirectory = "Log";
        /// <summary>
        /// 工作区三级目录（ImageMonitor）
        /// </summary>
        public static readonly string ImageMonitorAssignationDirectory = "ImageMonitor";
        /// <summary>
        /// 工作区三级目录（EmergencyList）
        /// </summary>
        public static readonly string EmergencyListAssignationDirectory = "EmergencyList";
  
        /// <summary>
        /// 工作区三级目录（Schedule List）
        /// </summary>
        public static readonly string ScheduleListAssignationDirectory = "Schedule List";
        /// <summary>
        /// 工作区三级目录（Publication List）
        /// </summary>
        public static readonly string PublicationListAssignationDirectory = "Publication List";
        /// <summary>
        /// 工作区四级目录（Pluto）
        /// </summary>
        public static readonly string PlutoScheduleAssignationDirectory = "Pluto";
        /// <summary>
        /// 工作区四级目录（Cloud）
        /// </summary>
        public static readonly string CloudScheduleAssignationDirectory = "Cloud";

     //   public static readonly string ServerPlayAssignationDirectory = "ServerPlay";

      //  public static readonly string EditedPlayAssignationDirectory = "Edited";

        public static string LocalDirectoryPath;
        public static string RemoteDirectoryPath;

        public static string LogDirectoryPath;
        public static string ImageMonitorDirectoryPath;
        public static string EmergencyListDirectoryPath;

        public static string LocalScheduleDirectoryPath;
        public static string LocalPublicationDirectoryPath;
        public static string LocalPlutoPublicationDirectoryPath;
        public static string LocalCloudPublicationDirectoryPath;

        public static string RemoteScheduleDirectoryPath;
        public static string RemotePublicationDirectoryPath;
        public static string RemotePlutoPublicationDirectoryPath;
        public static string RemoteCloudPublicationDirectoryPath;
    



        //public static string LocalScheduleDirectoryPath;

        //public static string RemoteScheduleDirectoryPath;

        //public static string PlayCloudDirectoryPath;

        //public static string PlayPlutoDirectoryPath;

        //public static string ServerCloudDirectoryPath;

        //public static string ServerPlutoDirectoryPath;

        //public static string ServerEditDirectoryPath;


        //  public PlatformConfig() { }


        public PlatformConfig()
        {
            //InitializeConfig();
        }

        public void InitializeConfig()
        {
            if (!LoadLocalConfig() && !ImportOldLocalConfig())
            {
                this.PlatformLanguage = "zh-cn";
                this.PlatformAccount = new Account(string.Empty, string.Empty);
                this.ServerInfo = new ServerInfo();
                this.WorkspacePath = DefaultWorkspacePath;
                this.AppDataInfo = new AppData() { Token = string.Empty, ScheduleScreenSize = new System.Drawing.Size(400, 400), TerminalSortField = "GroupName", IsEnableFTPS = false, RefreshFrequency = 30.0, IsAutoStatusRefresh = true };
                this.TimeZoneTable = new TimeZoneTable();
            }
            InitializeWorkspace();
        }
        private bool LoadLocalConfig()
        {
            if (File.Exists(PlatformConfigFilePath))
            {
                try
                {
                    using (FileStream stream = new FileStream(PlatformConfigFilePath, FileMode.Open))
                    {
                        BinaryFormatter deserializer = new BinaryFormatter();
                        object obj = deserializer.Deserialize(stream);
                        PlatformConfig platformConfig = obj as PlatformConfig;
                        if (platformConfig == null)
                        {
                            return false;
                        }

                        if (platformConfig.PlatformAccount == null)
                            this.PlatformAccount = new Account();
                        else
                            this.PlatformAccount = platformConfig.PlatformAccount;

                        if (platformConfig.ServerInfo == null)
                            this.ServerInfo = new ServerInfo();
                        else
                            this.ServerInfo = platformConfig.ServerInfo;

                        if (string.IsNullOrEmpty(platformConfig.PlatformLanguage))
                            this.PlatformLanguage = "zh-cn";
                        else
                            this.PlatformLanguage = platformConfig.PlatformLanguage;

                        if (string.IsNullOrEmpty(platformConfig.WorkspacePath))
                            WorkspacePath = DefaultWorkspacePath;
                        else
                            WorkspacePath = platformConfig.WorkspacePath;

                        if (platformConfig.AppDataInfo == null)
                            this.AppDataInfo = new AppData() { Token = string.Empty, ScheduleScreenSize = new System.Drawing.Size(400, 400), TerminalSortField = "GroupName", IsEnableFTPS = false, RefreshFrequency = 30.0, IsAutoStatusRefresh = true };
                        else
                            this.AppDataInfo = platformConfig.AppDataInfo;

                        this.TimeZoneTable = new TimeZoneTable();
                    }
                    return true;
                }
                catch (Exception e)
                {
                    File.Delete(PlatformConfigFilePath);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool ImportOldLocalConfig()
        {
            bool bUserSuccess;

            UserConfigParser userConfigParser = new UserConfigParser(UserConfigPath, out bUserSuccess);
            if (bUserSuccess)
            {
                string language;
                bool isEnableSSL;
                string userName;
                string userIdentifier;
                string webUrl;

                userConfigParser.GetLanguage(out language);
                userConfigParser.GetFtpSSL(out isEnableSSL);
                userConfigParser.GetUserName(out userName);
                userConfigParser.GetWebUrl(out webUrl);
                userConfigParser.GetUserIdentifier(out userIdentifier);

                this.PlatformLanguage = language;
                this.PlatformAccount = new Account(userName, string.Empty);
                this.ServerInfo = new ServerInfo() { ServerAddress = webUrl, CustomerId = userIdentifier };

                string workPath = GetAppConfig("WorkPath");
                if (string.IsNullOrEmpty(workPath))
                {
                    WorkspacePath = DefaultWorkspacePath;
                }
                else
                {
                    WorkspacePath = workPath;
                }

                this.AppDataInfo = new AppData() { Token = string.Empty, ScheduleScreenSize = new System.Drawing.Size(400, 400), TerminalSortField = "GroupName", IsEnableFTPS = false, RefreshFrequency = 30.0, IsAutoStatusRefresh = true };


                this.TimeZoneTable = new TimeZoneTable();



                return true;
            }
            else
            {
                return false;
            }
        }
        public Account PlatformAccount { get; set; }

        public ServerInfo ServerInfo { get; set; }

        private string _platformLanguage;
        public string PlatformLanguage
        {
            get
            {
                return _platformLanguage;
            }
            set
            {
                if (value == _platformLanguage)
                {
                    return;
                }
                _platformLanguage = value;

                AppMessages.ChangeLanguageMessage.Send(_platformLanguage);
            }
        }

        private string _workspacePath;
        public string WorkspacePath
        {
            get
            {
                return _workspacePath;
            }
            set
            {
                if (_workspacePath == value)
                {
                    return;
                }
                _workspacePath = value;
                InitializeWorkspace();
            }
        }

        public AppData AppDataInfo { get; set; }

        [System.Xml.Serialization.XmlIgnore]
        [System.Web.Script.Serialization.ScriptIgnore]
        public TimeZoneTable TimeZoneTable { get; set; }

        public void Save()
        {
            if (!Directory.Exists(NebulaDir))
                Directory.CreateDirectory(NebulaDir);
            BinaryFormatter writer = new BinaryFormatter();
            using (FileStream file = new FileStream(PlatformConfigFilePath, FileMode.OpenOrCreate))
            {
                writer.Serialize(file, this);
                file.Close();
            }
            
        }


        public void InitializeWorkspace()
        {
            if (string.IsNullOrEmpty(this.WorkspacePath))
                return;

            InitializeDirectory(this.WorkspacePath);

            LocalDirectoryPath = Path.Combine(this.WorkspacePath, LocalAssignationDirectory);
            InitializeDirectory(LocalDirectoryPath);

            RemoteDirectoryPath = Path.Combine(this.WorkspacePath, RemoteAssignationDrectory);
            InitializeDirectory(RemoteDirectoryPath);

            EmergencyListDirectoryPath = Path.Combine(this.WorkspacePath, EmergencyListAssignationDirectory);
            InitializeDirectory(EmergencyListDirectoryPath);

            LogDirectoryPath = Path.Combine(this.WorkspacePath, LogAssignationDirectory);
            InitializeDirectory(LogDirectoryPath);

            ImageMonitorDirectoryPath = Path.Combine(this.WorkspacePath, ImageMonitorAssignationDirectory);
            InitializeDirectory(ImageMonitorDirectoryPath);

            LocalScheduleDirectoryPath = Path.Combine(LocalDirectoryPath, ScheduleListAssignationDirectory);
            InitializeDirectory(LocalScheduleDirectoryPath);

            LocalPublicationDirectoryPath = Path.Combine(LocalDirectoryPath, PublicationListAssignationDirectory);
            InitializeDirectory(LocalPublicationDirectoryPath);

            LocalPlutoPublicationDirectoryPath = Path.Combine(LocalPublicationDirectoryPath, PlutoScheduleAssignationDirectory);
            InitializeDirectory(LocalPlutoPublicationDirectoryPath);

            LocalCloudPublicationDirectoryPath = Path.Combine(LocalPublicationDirectoryPath, CloudScheduleAssignationDirectory);
            InitializeDirectory(LocalCloudPublicationDirectoryPath);

            RemoteScheduleDirectoryPath = Path.Combine(RemoteDirectoryPath, ScheduleListAssignationDirectory);
            InitializeDirectory(RemoteScheduleDirectoryPath);

            RemotePublicationDirectoryPath = Path.Combine(RemoteDirectoryPath, PublicationListAssignationDirectory);
            InitializeDirectory(RemotePublicationDirectoryPath);

            RemotePlutoPublicationDirectoryPath = Path.Combine(RemotePublicationDirectoryPath, PlutoScheduleAssignationDirectory);
            InitializeDirectory(RemotePlutoPublicationDirectoryPath);

            RemoteCloudPublicationDirectoryPath = Path.Combine(RemotePublicationDirectoryPath, CloudScheduleAssignationDirectory);
            InitializeDirectory(RemoteCloudPublicationDirectoryPath);

            //PlayCloudDirectoryPath = Path.Combine(PlayDirectoryPath, CloudPlayAssignationDirectory);
            //InitializeDirectory(PlayCloudDirectoryPath);

            //PlayPlutoDirectoryPath = Path.Combine(PlayDirectoryPath, PlutoPlayAssignationDirectory);
            //InitializeDirectory(PlayPlutoDirectoryPath);

            //ServerEditDirectoryPath = Path.Combine(ServerPlayDirectoryPath,EditedPlayAssignationDirectory);
            //InitializeDirectory(ServerEditDirectoryPath);

            //ServerCloudDirectoryPath = Path.Combine(ServerPlayDirectoryPath, CloudPlayAssignationDirectory);
            //InitializeDirectory(ServerCloudDirectoryPath);

            //ServerPlutoDirectoryPath = Path.Combine(ServerPlayDirectoryPath, PlutoPlayAssignationDirectory);
            //InitializeDirectory(ServerPlutoDirectoryPath);
        }

        private void InitializeDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }


        //private static Configuration _config;
        private static string GetAppConfig(string strKey)
        {
            string configPath = Path.Combine(NebulaDir, "AppConfig");
            if (!File.Exists(configPath))
            {
                File.Create(configPath);
            }
            Configuration roamingConfig = ConfigurationManager.OpenExeConfiguration(configPath);

            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();
            configFileMap.ExeConfigFilename = roamingConfig.FilePath;

            Configuration _config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);
            _config.Save(ConfigurationSaveMode.Full);

            AppSettingsSection currentSection = null;
            string sectionName = "appSettings";

            currentSection = (AppSettingsSection)_config.GetSection("appSettings");
            if (!_config.AppSettings.Settings.AllKeys.Contains(strKey))
            {
                _config.AppSettings.Settings.Add(strKey, string.Empty);
                _config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(sectionName);
                return string.Empty;
            }
            else
            {
                foreach (KeyValueConfigurationElement key in _config.AppSettings.Settings)
                {
                    if (key.Key == strKey)
                    {
                        return key.Value;
                    }
                }
            }
            return null;
        }
    }
}
