using Core.Framework.FGUI;
using FairyGUI;
using Schedule;
namespace Com.Module.Schedule
{
    public class CalendarWindow : GUIWindow
    {
        CalendarData CalendarData;
        UI_CalendarWindow rootWindow;
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
            
        }

    }
}


