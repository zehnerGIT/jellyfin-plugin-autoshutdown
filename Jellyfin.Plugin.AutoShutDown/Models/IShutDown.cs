namespace Jellyfin.Plugin.AutoShutDown.Models
{
    public interface IShutDown
    {
        void ShutDown();
        /*
        bool ShutDownDelayed();
        bool Reboot();
        bool Hibernate();
        bool CustomCommand(string command);
        */
    }
}
