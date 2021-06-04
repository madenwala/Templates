using AppFramework.Core.Models;
using System;

namespace Contoso.Core.Models
{
    /// <summary>
    /// Sample model class.
    /// </summary>
    public sealed class ItemModel : BaseUniqueModel
    {
        #region Properties

        private string _lineOne;
        public string LineOne
        {
            get { return _lineOne; }
            set { this.SetProperty(ref _lineOne, value); }
        }

        private string _lineTwo;
        public string LineTwo
        {
            get { return _lineTwo; }
            set { this.SetProperty(ref _lineTwo, value); }
        }

        private string _lineThree;
        public string LineThree
        {
            get { return _lineThree; }
            set { this.SetProperty(ref _lineThree, value); }
        }

        private string _LineFour;
        public string LineFour
        {
            get { return _LineFour; }
            set { this.SetProperty(ref _LineFour, value); }
        }

        private string _PhoneNumber;
        public string PhoneNumber
        {
            get { return _PhoneNumber; }
            set { this.SetProperty(ref _PhoneNumber, value); }
        }

        private DateTime _LastUpdated = DateTime.Now;
        public DateTime LastUpdated
        {
            get { return _LastUpdated; }
            set { this.SetProperty(ref _LastUpdated, value); }
        }

        #endregion
    }
}