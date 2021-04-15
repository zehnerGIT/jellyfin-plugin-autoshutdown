namespace Jellyfin.Plugin.AutoShutDown.Models.Entities
{
    public struct CancelResult
    {
        public string Message { get; init; }

        public bool Cancel { get; init; }
    }
}
