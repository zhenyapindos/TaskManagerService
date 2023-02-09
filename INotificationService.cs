using StasDiplom.Domain;
using StasDiplom.Enum;

namespace StasDiplom;

public interface INotificationService
{
    public void ProjectInvitation(string userId, int projectId);
}