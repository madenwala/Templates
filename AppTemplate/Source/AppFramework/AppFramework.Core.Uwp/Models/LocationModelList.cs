using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace AppFramework.Core.Models
{

    /// <summary>
    /// Collection for holding LocationModel instance.
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    public class LocationModelList<TItem> : ModelList<TItem> where TItem : ILocationModel
    {
        #region Constructors

        public LocationModelList() : base()
        {
        }

        public LocationModelList(IEnumerable<TItem> items) : base(items)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the distance away for all location model instances in this collection.
        /// </summary>
        /// <param name="loc"></param>
        public void SetDistancesAway(ILocationModel loc)
        {
            foreach (var item in this)
                item.SetDistanceAway(loc);
        }

        #endregion
    }

    public class UniqueLocationModelList<T> : UniqueModelList<T> where T : IUniqueLocationModel
    {
        #region Constructors

        public UniqueLocationModelList()
        {
        }

        public UniqueLocationModelList(IEnumerable<T> items)
        {
            this.AddRange(items);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Set the distance away for all location model instances in this collection.
        /// </summary>
        /// <param name="loc"></param>
        public void SetDistancesAway(ILocationModel loc)
        {
            foreach (var item in this)
                item.SetDistanceAway(loc);
        }

        #endregion
    }
}