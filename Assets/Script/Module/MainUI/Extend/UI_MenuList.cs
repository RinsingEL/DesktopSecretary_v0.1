using Com.Module.Chat;
using Core.Framework.FGUI;
using FairyGUI;

namespace Com.Module.MainUI
{
    public partial class UI_MenuList : GComponent
    {
        public void Init()
        {
            m_MenuList.itemRenderer = RenderListItem;
            m_MenuList.numItems = 1;
        }
        public void RenderListItem(int index, GObject Menubtn)
        {
           switch(index)
            {
                case 0:
                    Menubtn.asButton.onClick.Set(OpenSettingPanel);
                    break;
                case 1:
                    Menubtn.asButton.onClick.Set(OpenCalendarPanel);
                    break;
                default:
                    break;
            }
        }

        private void OpenCalendarPanel(EventContext context)
        {
            
        }

        public void OpenSettingPanel(EventContext context)
        {
            GUIManager.Instance.ShowWindow<SettingWindow>();
        }
    }

}

