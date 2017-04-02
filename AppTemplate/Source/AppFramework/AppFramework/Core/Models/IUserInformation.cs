namespace AppFramework.Core.Models
{
    public interface IUserInformation : IModel
    {
        string UserID { get; }
        string DisplayName { get; }
        string Email { get; }
    }
}