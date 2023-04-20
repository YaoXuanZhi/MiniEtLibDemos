using AppClientGUI;

namespace ET.Client
{
    [FriendOf(typeof(MainWindowComponent))]
    public static partial class MainWindowComponentSystem
    {
        public static void Binder(this MainWindowComponent self, MainWindowProxy mainWindow)
        {
            self.proxy = mainWindow.owner;
            self.btn_login = mainWindow.Button_Login;
            self.btn_logout = mainWindow.Button_Logout;
            self.btn_gm = mainWindow.Button_GmApply;
            self.textbox_message = mainWindow.Text_Board;
            self.input_gm = mainWindow.Input_Gm;
        }
        [ObjectSystem]
        public class PnlHelpComponentAwakeSystem : AwakeSystem<MainWindowComponent, MainWindowProxy>
        {
            protected override void Awake(MainWindowComponent self, MainWindowProxy mainWindow)
            {
                self.Binder(mainWindow);
                self.AddListener();
                self.OnInit();
            }
        }

        [ObjectSystem]
        public class PnlHelpComponentDestroySystem : DestroySystem<MainWindowComponent>
        {
            protected override void Destroy(MainWindowComponent self)
            {
                self.RemoveListener();
                self.OnDispose();
            }
        }

        [ObjectSystem]
        public class PnlHelpComponentLoadSystem : LoadSystem<MainWindowComponent>
        {
            protected override void Load(MainWindowComponent self)
            {
                self.Load();
            }
        }

        public static void Load(this MainWindowComponent self)
        {
            self.AddListener();
        }
        
        [ObjectSystem]
        public class PnlHelpComponentUnloadSystem : UnloadSystem<MainWindowComponent>
        {
            protected override void Unload(MainWindowComponent self)
            {
                self.RemoveListener();
            }
        }

    }
}
