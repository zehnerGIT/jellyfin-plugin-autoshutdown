using System.Threading.Tasks;
using Jellyfin.Plugin.AutoShutDown.Models.Entities;

namespace Jellyfin.Plugin.AutoShutDown.Models
{
    public interface ICancelShutDown
    {
        Task<CancelResult> CancelShutDown(string config);
    }
}