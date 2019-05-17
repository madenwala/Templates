using System;

namespace AppFramework.Core.Models
{
    /// <summary>
    /// Interface that all model objects should implement.
    /// </summary>
    public interface IModel
    {
    }

    public interface IUniqueModel : IModel, IEquatable<IUniqueModel>
    {
        string ID { get; set; }
    }

    /// <summary>
    /// Interface that any model containing latitude/longitude should implement.
    /// </summary>
    public interface ILocationModel : IModel
    {
        double Latitude { get; set; }

        double Longitude { get; set; }

        string LocationDisplayName { get; set; }

        void SetDistanceAway(ILocationModel loc);
    }

    public interface IUniqueLocationModel : IUniqueModel, ILocationModel
    {
    }
}