using System;

namespace AppFramework.Core.Models
{
    public abstract class BaseUniqueLocationModel : BaseLocationModel, IUniqueLocationModel, IEquatable<IUniqueModel>
    {
        #region Properties

        private string _ID;
        public virtual string ID
        {
            get { return _ID; }
            set { this.SetProperty(ref _ID, value); }
        }

        #endregion

        #region Methods

        #region Equatable Functions

        public bool Equals(IUniqueModel other)
        {
            return other != null && this.ID?.Equals(other.ID) == true;
        }

        public override bool Equals(object obj)
        {
            if (obj is IUniqueModel)
                return this.Equals((IUniqueModel)obj);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.ID.GetHashCode();
        }

        public static bool operator ==(BaseUniqueLocationModel obj1, IUniqueModel obj2)
        {
            return (object)obj1 == null || (object)obj2 == null ? Equals(obj1, obj2) : obj1.Equals(obj2);
        }

        public static bool operator !=(BaseUniqueLocationModel obj1, IUniqueModel obj2)
        {
            return obj1 == null || obj2 == null ? !Equals(obj1, obj2) : !obj1.Equals(obj2);
        }

        #endregion

        #endregion
    }
}