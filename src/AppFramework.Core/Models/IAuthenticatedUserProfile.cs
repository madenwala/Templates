namespace AppFramework.Core.Models
{
    public interface IAuthenticatedUserProfile : IModel
    {
        string UserID { get; }
        string Username { get; }
        string DisplayName { get; }
    }
}