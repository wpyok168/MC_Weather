using MC_SDK.Interface;
using MC_Weather;
using Unity;


namespace MengCRobot
{
    public static class RegisterCore
    {
        public static void Register(IUnityContainer unityContainer)
        {
            //unityContainer.RegisterType<IEnable, AppEnable>();
            unityContainer.RegisterType<IPrivateMsg, RecPrivateMsg>();
            unityContainer.RegisterType<IGroupMsg, RecGroupMsg>();
            //unityContainer.RegisterType<ISetting, OpenRobotMenu>();
            //unityContainer.RegisterType<IEventMsg, RobotEventcallBack>();
            //unityContainer.RegisterType<IDisable, RobotDisable>();
            //unityContainer.RegisterType<IUninit, RobotUninit>();
            //unityContainer.RegisterType<IGuildMsg, SendGuildMsg>();
        }
    }
}
