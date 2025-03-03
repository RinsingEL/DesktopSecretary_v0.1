/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;

namespace Com.Module.Schedule
{
    public class ScheduleBinder
    {
        public static void BindAll()
        {
            UIObjectFactory.SetPackageItemExtension(UI_CalendarDayCom.URL, typeof(UI_CalendarDayCom));
            UIObjectFactory.SetPackageItemExtension(UI_CalendarDayTaskCom.URL, typeof(UI_CalendarDayTaskCom));
            UIObjectFactory.SetPackageItemExtension(UI_detailTaskCom.URL, typeof(UI_detailTaskCom));
            UIObjectFactory.SetPackageItemExtension(UI_EditWindow.URL, typeof(UI_EditWindow));
            UIObjectFactory.SetPackageItemExtension(UI_CalendarWindow.URL, typeof(UI_CalendarWindow));
        }
    }
}