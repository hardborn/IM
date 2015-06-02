using Nova.NovaWeb.Player;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
        }
        private PlayHelper _playHelper;
        void Form1_Load(object sender, EventArgs e)
        {
            ScreenLocationInfo info = new ScreenLocationInfo(0, 0, 100, 100);
            List<ScreenLocationInfo> scInfo = new List<ScreenLocationInfo>();
            scInfo.Add(info);
            _playHelper = new PlayHelper(scInfo, "");
            _playHelper.SetScreenHide(0, false);
            //_playHelper.UpdateLanguage(_langResDir, _curLangStr);
            _playHelper.IsShowContextMenuStrip = true;
            _playHelper.IsShowMouseInScreen = true;
            _playHelper.SetScreenTopMost(0, false);
            _playHelper.StopPreview_ContrexMenuStrip_Event += new EventHandler(PlayHelper_StopPreview_ContrexMenuStrip_Event);
        }

        private void PlayHelper_StopPreview_ContrexMenuStrip_Event(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
