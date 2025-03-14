using Core.Framework.FGUI;
using FairyGUI;
using System;
namespace Com.Module.Schedule
{
    public class CalendarWindow : GUIWindow
    {
        private UI_CalendarWindow rootWindow;
        private CalendarViewModel _viewModel;
        public CalendarWindow()
        {
            Param.packagePath = "UI/Schedule";
            Param.packageName = "Schedule";
            Param.componentName = "CalendarWindow";
            Param.Layer = UILayer.Normal;
        }
        protected override void OnInit(GComponent com)
        {
            rootWindow = com as UI_CalendarWindow;
            _viewModel = new CalendarViewModel();
            rootWindow.Init(_viewModel);
            rootWindow.m_closeBtn.onClick.Set(Hide);
        }
        protected override void BeforeShow()
        {
            base.BeforeShow();
            rootWindow.BeforeShow();
        }
        protected override void OnHide()
        {
            base.OnHide();
            rootWindow.OnHide();
        }
    }
}