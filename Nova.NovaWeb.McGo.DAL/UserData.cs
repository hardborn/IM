using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.NovaWeb.OperationCommon;
//using Nova.MD5;

namespace Nova.NovaWeb.McGo.DAL
{
    public class UserData
    {
        private UserConfigParser _userConfigPaser;

        private string _language;
        private int _displayMode;
        private string _playListLibPath;
        private bool _isShowAskDialog;
        private bool _isEnableFTPS;
        //private ProductType _md5Algorithm;
        private VersionInfo _versionType;
        private string _webURL;
        private string _serverAddress;
        private string _vpnServerAddress;
        private string _userName;
        private string _userIdentity;
        private bool _isEnableVPN = false;
        private int _operationId;
        private string _userPassword;

        public UserData(UserConfigParser userConfigParser)
        {
            _userConfigPaser = userConfigParser;
            Initilize(_userConfigPaser);
        }

        public UserData(string language, int displayMode, string playListLibPath, bool isShowAskDialog, bool isEnableFTPS,
                              VersionInfo versionType, string webAddress, string serverAddress, string vpnServerAddress,
                              string userName, string userIdentity, int operationId, bool isEnableVPN)
        {
            Initilize(language, displayMode, playListLibPath, isShowAskDialog, isEnableFTPS,
                      versionType, webAddress, serverAddress, vpnServerAddress,
                      userName, userIdentity, isEnableVPN);
        }

        public string Language
        {
            get { return _language; }
            set
            {
                if (_language != value)
                {
                    if (_userConfigPaser.SetLanguage(value))
                    {
                        _language = value;
                    }
                }
            }
        }
        public int DisplayMode
        {
            get { return _displayMode; }
            set
            {
                if (_displayMode != value)
                {
                    if (_userConfigPaser.SetDisplayMode(value))
                    {
                        _displayMode = value;
                    }
                }
            }
        }
        public string PlayListLibPath
        {
            get { return _playListLibPath; }
            set
            {
                if (_playListLibPath != value)
                {
                    if (_userConfigPaser.SetPlaylistLibPath(value))
                    {
                        _playListLibPath = value;
                    }
                }
            }
        }
        public bool IsShowAskDialog
        {
            get { return _isShowAskDialog; }
            set
            {
                if (_isShowAskDialog != value)
                {
                    if (_userConfigPaser.SetIfShowAskDiag(value))
                    {
                        _isShowAskDialog = value;
                    }
                }
            }
        }
        public bool IsEnableFTPS
        {
            get { return _isEnableFTPS; }
            set
            {
                if (_isEnableFTPS != value)
                {
                    if (_userConfigPaser.SetFtpSSL(value))
                    {
                        _isEnableFTPS = value;
                    }
                }
            }
        }
        //public ProductType MD5Algorithm
        //{
        //    get { return _md5Algorithm; }
        //    set
        //    {
        //        if (_md5Algorithm != value)
        //        {
        //            if (_userConfigPaser.SetMD5Algorithm(value))
        //            {
        //                _md5Algorithm = value;
        //            }
        //        }
        //    }
        //}
        public VersionInfo VersionType
        {
            get { return _versionType; }
            set
            {
                if (_versionType != value)
                {
                    if (_userConfigPaser.SetVersionType(value))
                    {
                        _versionType = value;
                    }
                }
            }
        }
        public string WebURL
        {
            get { return _webURL; }
            set
            {
                if (_webURL != value)
                {
                    if (_userConfigPaser.SetWebUrl(value))
                    {
                        _webURL = value;
                    }
                }
            }
        }
        public string ServerAddress
        {
            get { return _serverAddress; }
            set
            {
                if (_serverAddress != value)
                {
                    if (_userConfigPaser.SetSelectedWebAddress(value))
                    {
                        _serverAddress = value;
                    }
                }
            }
        }
        public string VPNServerAddress
        {
            get { return _vpnServerAddress; }
            set
            {
                if (_vpnServerAddress != value)
                {
                    if (_userConfigPaser.SetVpnServerAddr(value))
                    {
                        _vpnServerAddress = value;
                    }
                }
            }
        }
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (_userName != value)
                {
                    if (_userConfigPaser.SetUserName(value))
                    {
                        _userName = value;
                    }
                }
            }
        }
        public string UserIdentity
        {
            get { return _userIdentity; }
            set
            {
                if (_userIdentity != value)
                {
                    if (_userConfigPaser.SetUserIdentifier(value))
                    {
                        _userIdentity = value;
                    }
                }
            }
        }
        public bool IsEnableVPN
        {
            get { return _isEnableVPN; }
            private set
            {
                if (_isEnableVPN != value)
                {
                    if (_userConfigPaser.SetVpnServerEnable(value))
                    {
                        _isEnableVPN = value;
                    }
                }
            }
        }
        public int OperationId
        {
            get { return _operationId; }
        }

        public string UserPassword { get { return _userPassword; } set { _userPassword = value; } }

        public UserConfigParser ConfigParser
        {
            get { return _userConfigPaser; }
            set
            {
                _userConfigPaser = value;
            }
        }


        private void Initilize(UserConfigParser userConfigParser)
        {
            int operationId;

            userConfigParser.GetLanguage(out _language);
            userConfigParser.GetDisplayMode(out _displayMode);
            userConfigParser.GetPlaylistLibPath(out _playListLibPath);
            userConfigParser.GetIfShowAskDiag(out _isShowAskDialog);
            userConfigParser.GetFtpSSL(out _isEnableFTPS);
            //userConfigParser.GetMD5Algorithm(out _md5Algorithm);
            userConfigParser.GetVersionInfo(out _versionType);
            userConfigParser.GetWebUrl(out _webURL);
            userConfigParser.GetSelectedWebAddress(out _serverAddress);
            userConfigParser.GetVpnServerAddr(out _vpnServerAddress);
            userConfigParser.GetUserName(out _userName);
            userConfigParser.GetUserIdentifier(out _userIdentity);
           // userConfigParser.GetVpnServerSetting(out _isEnableVPN);


            if (!string.IsNullOrEmpty(_serverAddress) && _serverAddress.Length >= 1)
            {
                if (Int32.TryParse(_serverAddress.Substring(_serverAddress.Length - 1, 1), out operationId))
                {
                    _operationId = operationId;
                }
                else
                {
                    _operationId = 0;
                }
            }

        }

        private void Initilize(string language, int displayMode, string playListLibPath, bool isShowAskDialog, bool isEnableFTPS,
                               VersionInfo versionType, string webAddress, string serverAddress, string vpnServerAddress,
                               string userName, string userIdentity, bool isEnableVPN)
        {
            int operationId;
            if (!string.IsNullOrEmpty(_serverAddress) && _serverAddress.Length >= 1)
            {
                if (Int32.TryParse(_serverAddress.Substring(_serverAddress.Length - 1, 1), out operationId))
                {
                    _operationId = operationId;
                }
                else
                {
                    _operationId = 0;
                }
            }
            if (isEnableVPN)
            {
                _vpnServerAddress = vpnServerAddress;
            }
            _language = language;
            _displayMode = displayMode;
            _playListLibPath = playListLibPath;
            _isShowAskDialog = isShowAskDialog;
            _isEnableFTPS = isEnableFTPS;
            //_md5Algorithm = md5Algorithm;
            _versionType = versionType;
            _webURL = webAddress.Replace("https://",string.Empty).TrimEnd('/');
            _serverAddress = serverAddress;
            _userName = userName;
            _userIdentity = userIdentity;
           // _isEnableVPN = isEnableVPN;
        }
    }
}
