using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nova.NovaWeb.OperationCommon;

namespace Nova.NovaWeb.McGo.DAL
{
    public class PhpServiceInterface
    {
        private string _userIdentifierName;
        private string _accVerifyPhp = string.Empty;
        private string _terListPhp = string.Empty;
        private string _terminalStatusInquirePhp = string.Empty;
        private string _sslinquirePhp = string.Empty;
        private string _playProgramInquirePhp = string.Empty;
        private string _mediaInquirePhp = string.Empty;
        private string _mediaUpdatePhp = string.Empty;
        private string _playProgramUpdatePhp = string.Empty;
        private string _publishPhp = string.Empty;
        private string _accountModifyPasswordPhp = string.Empty;
        private string _playlistinquirePhp = string.Empty;
        private string _playlistupdatePhp = string.Empty;
        //private string _playproginquirePhp = string.Empty;
        //private string _playprogupdatePhp = string.Empty;



        private PhpConfigParser _phpConfigParser;

        public PhpServiceInterface(PhpConfigParser phpConfigParser)
        {
            _phpConfigParser = phpConfigParser;
            Initilize(phpConfigParser);
        }

        public string UserIdentifierName
        {
            get { return _phpConfigParser.UserIdentifierName; }
        }
        public string PublishPhp
        {
            get { return _publishPhp; }
            set
            {
                if (_publishPhp != value)
                {
                    _publishPhp = value;
                }
            }
        }
        public string TerListPhp
        {
            get { return _terListPhp; }
            set
            {
                if (_terListPhp != value)
                {
                    if (_phpConfigParser.SetTerListPhp(value))
                    {
                        _terListPhp = value;
                    }
                }
            }
        }

        public string TerminalStatusInquirePhp
        {
            get { return _terminalStatusInquirePhp; }
            set
            {
                if (_terminalStatusInquirePhp != value)
                {
                    if (_phpConfigParser.SetTerminalstatusinquirePhp(value))
                    {
                        _terminalStatusInquirePhp = value;
                    }
                }
            }
        }

        public string AccVerifyPhp
        {
            get { return _accVerifyPhp; }
            set
            {
                if (_accVerifyPhp != value)
                {
                    if (_phpConfigParser.SetAccVerifyPhp(value))
                    {
                        _accVerifyPhp = value;
                    }
                }
            }
        }

        public string SSLInquirePhp
        {
            get { return _sslinquirePhp; }
            set
            {
                if (_sslinquirePhp != value)
                {
                    if (_phpConfigParser.SetSslinquirePhp(value))
                    {
                        _sslinquirePhp = value;
                    }
                }
            }
        }

        public string PlayProgramInquirePhp
        {
            get { return _playProgramInquirePhp; }
            private set
            {
                if (value == _playProgramInquirePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetPlaylistinquirePhp(value))
                {
                    _playProgramInquirePhp = value;
                }
            }
        }

        public string MediaInquirePhp
        {
            get { return _mediaInquirePhp; }
            private set
            {
                if (value == _mediaInquirePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetMediainquirePhp(value))
                {
                    _mediaInquirePhp = value;
                }
            }
        }

        public string MediaUpdatePhp
        {
            get { return _mediaUpdatePhp; }
            private set
            {
                if (value == _mediaUpdatePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetMediaupdatePhp(value))
                {
                    _mediaUpdatePhp = value;
                }
            }
        }

        public string PlayProgramUpdatePhp
        {
            get { return _playProgramUpdatePhp; }
            private set
            {
                if (value == _playProgramUpdatePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetPlayprogupdatePhp(value))
                {
                    _playProgramUpdatePhp = value;
                }
            }
        }

        public string AccountModifyPasswordPhp
        {
            get { return _accountModifyPasswordPhp; }
            private set
            {
                if (value == _accountModifyPasswordPhp)
                {
                    return;
                }
                if (_phpConfigParser.SetAccModifyPassPhp(value))
                {
                    _accountModifyPasswordPhp = value;
                }
            }
        }

        public string TerDataTransInfoPhp
        {
            get { return "cgi-bin/identifier/TerDataTransInfo.php"; }
        }

        public string PlatformDataTransInfoPhp
        {
            get { return "cgi-bin/identifier/PlatformDataTransInfo.php"; }
        }

        public string TerAlarmInfoPhp
        {
            get { return "cgi-bin/identifier/TerAlarmInfo.php"; }
        }


        public string PlaylistInquirePhp
        {
            get { return _playlistinquirePhp; }
            private set
            {
                if (value == _playlistinquirePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetPlaylistinquirePhp(value))
                {
                    _playlistinquirePhp = value;
                }
            }
        }

        public string PlaylistUpdatePhp
        {
            get { return _playlistupdatePhp; }
            private set
            {
                if (value == _playlistupdatePhp)
                {
                    return;
                }
                if (_phpConfigParser.SetPlaylistupdatePhp(value))
                {
                    _playlistupdatePhp = value;
                }
            }
        }




        private void Initilize(PhpConfigParser phpConfigParser)
        {
            if (phpConfigParser == null)
            {
                _userIdentifierName="identifier";

                _accVerifyPhp = "cgi-bin/identifier/verifyUser.php";
                _terListPhp = "cgi-bin/identifier/terminallistxml.php";
                _terminalStatusInquirePhp="cgi-bin/identifier/terminalstatusinquire.php";
                _sslinquirePhp = "cgi-bin/identifier/sslinquire.php";
                _publishPhp = "cgi-bin/identifier/commandpublish.php";
                _accountModifyPasswordPhp = "cgi-bin/identifier/accmodipass.php";
                
                _playlistinquirePhp = "cgi-bin/identifier/playlistinquire.php";
                _playlistupdatePhp = "cgi-bin/identifier/playlistupdate.php";
                _playProgramInquirePhp = "cgi-bin/identifier/playproginquire.php";
                _playProgramUpdatePhp = "cgi-bin/identifier/playprogupdate.php";
                _mediaInquirePhp = "cgi-bin/identifier/mediainquire.php";
                _mediaUpdatePhp = "cgi-bin/identifier/mediaupdate.php";                
            }
            else
            {
                _phpConfigParser.GetAccVerifyPhp(out _accVerifyPhp);
                _phpConfigParser.GetTerListPhp(out _terListPhp);
                _phpConfigParser.GetTerminalstatusinquirePhp(out _terminalStatusInquirePhp);
                _phpConfigParser.GetSslinquirePhp(out _sslinquirePhp);

                _phpConfigParser.GetPublishPhp(out _publishPhp);
                _phpConfigParser.GetAccModiPassPhp(out _accountModifyPasswordPhp);

                _phpConfigParser.GetPlaylistinquirePhp(out _playlistinquirePhp);
                _phpConfigParser.GetPlaylistupdatePhp(out _playlistupdatePhp);
                _phpConfigParser.GetMediainquirePhp(out _mediaInquirePhp);
                _phpConfigParser.GetMediaupdatePhp(out _mediaUpdatePhp);
                _phpConfigParser.GetPlayproginquirePhp(out _playProgramInquirePhp);
                _phpConfigParser.GetPlayprogupdatePhp(out _playProgramUpdatePhp);
            }
            
        }

    }
}
