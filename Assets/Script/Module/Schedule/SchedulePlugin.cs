using Core.Framework.Plugin;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Schedule
{
    public class SchedulePlugin : PluginBase
    {
        CalendarData _calendarData = new CalendarData();
        protected override void OnRegister()
        {
            _calendarData.OnInit();
        }

        protected override void OnUninstall()
        {
            
        }
    }
}

