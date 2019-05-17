namespace AppFramework.Core.Models
{
    public interface IAuthenticatedUserProfile : IModel
    {
        string UserID { get; }
        string DisplayName { get; }
        string Email { get; }
    }
}