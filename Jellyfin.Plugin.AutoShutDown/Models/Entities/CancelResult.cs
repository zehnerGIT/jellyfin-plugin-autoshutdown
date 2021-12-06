namespace Jellyfin.Plugin.AutoShutDown.Models.Entities
{
    public record CancelResult
    {
        public string Message { get; init; }

        public bool Cancel { get; init; }
    }
}
