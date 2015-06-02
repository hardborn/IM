using GalaSoft.MvvmLight.Messaging;
using Nova.NovaWeb.McGo.BLL;
using Nova.NovaWeb.McGo.DAL;
using Nova.NovaWeb.McGo.Platform.ViewModel;
using Nova.NovaWeb.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nova.NovaWeb.McGo.Platform.Utilities
{
    public class PreviewHelper : IDisposable
    {
        private PlayHelper _playHelper;

        private static PreviewHelper uniqueInstance;

        public bool IsShowCompleted = false;
        private static readonly object locker = new object();

        // 定义私有构造函数，使外界不能创建该类实例
        private PreviewHelper()
        {
            //var screenLocationInfos = new List<ScreenLocationInfo>() { new ScreenLocationInfo(0, 0, 100, 100) };
            
            //_playHelper = new PlayHelper(screenLocationInfos, string.Empty);
            //_playHelper.SetScreenHide(0, false);
           
            //_playHelper.IsShortcutsHidden = false;
            //_playHelper.SetScreenTopMost(0, false);
            //_playHelper.IsShowContextMenuStrip = true;
            //_playHelper.IsShowMouseInScreen = true;

            //_playHelper.Key_EscKey_EnterEvent += StopPreviewHandler;
            //_playHelper.StopPreview_ContrexMenuStrip_Event += StopPreviewHandler;
            //_playHelper.ScreenShow_CompletedEvent+=_playHelper_ScreenShow_CompletedEvent;
            //var _platformService = AppEnvionment.Default.Get<IPlatformService>();
            //if (_platformService != null)
            //    _playHelper.UpdateLanguage(PlatformConfig.LanguageResourcePath, _platformService.GetPlatformLanguage());
        }

        private void _playHelper_ScreenShow_CompletedEvent(object sender, PlayHelper.ScreenShowArgs args)
        {
            if(args.ScreenIndex != 0)
                return;
            IsShowCompleted = true;
        }


        void StopPreviewHandler(object sender, EventArgs e)
        {
            StopPreview();
        }

        public void StopPreview()
        {
            if (_playHelper == null)
                return;
            _playHelper.SetScreenHide(0, false);
            _playHelper.StopPlay(0);
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>("StopPreview", "StopPreview"), "StopPreview");
        }


        public void StartPreview(ScheduleViewModel scheduleViewModel)
        {
            _playHelper.SetScreenLocationAndSize(0, scheduleViewModel.ScreenX, scheduleViewModel.ScreenY, scheduleViewModel.ScreenWidth, scheduleViewModel.ScreenHeight);
            _playHelper.SetScreenHide(0, true);
            _playHelper.StartPlay(0, scheduleViewModel.FilePath, false, string.Empty);
            Messenger.Default.Send<NotificationMessage<string>>(new NotificationMessage<string>("StartPreview", "StartPreview"), "StartPreview");
        }

        public void Initialize(ScheduleViewModel scheduleViewModel)
        {
            if(_playHelper != null)
                return;
            var screenLocationInfos = new List<ScreenLocationInfo>() { 
                new ScreenLocationInfo(scheduleViewModel.ScreenX, scheduleViewModel.ScreenY, scheduleViewModel.ScreenWidth, scheduleViewModel.ScreenHeight) };

            _playHelper = new PlayHelper(screenLocationInfos, string.Empty);
            _playHelper.SetScreenHide(0, false);

            _playHelper.IsShortcutsHidden = false;
            _playHelper.SetScreenTopMost(0, false);
            _playHelper.IsShowContextMenuStrip = true;
            _playHelper.IsShowMouseInScreen = true;

            _playHelper.Key_EscKey_EnterEvent += StopPreviewHandler;
            _playHelper.StopPreview_ContrexMenuStrip_Event += StopPreviewHandler;
            _playHelper.SingleScreenShow_CompletedEvent += _playHelper_ScreenShow_CompletedEvent;
            var _platformService = AppEnvionment.Default.Get<IPlatformService>();
            if (_platformService != null)
                _playHelper.UpdateLanguage(PlatformConfig.LanguageResourcePath, _platformService.GetPlatformLanguage());
        }

        /// <summary>
        /// 定义公有方法提供一个全局访问点,同时你也可以定义公有属性来提供全局访问点
        /// </summary>
        /// <returns></returns>
        public static PreviewHelper GetInstance()
        {
            if (uniqueInstance == null)
            {
                lock (locker)
                {
                    if (uniqueInstance == null)
                    {
                        uniqueInstance = new PreviewHelper();
                    }
                }
            }
            return uniqueInstance;
        }

        public void Dispose()
        {
            if(_playHelper == null)
                return;
            _playHelper.Key_EscKey_EnterEvent -= StopPreviewHandler;
            _playHelper.StopPreview_ContrexMenuStrip_Event -= StopPreviewHandler;
            try
            {
                _playHelper.Dispose();
                uniqueInstance = null;
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}
