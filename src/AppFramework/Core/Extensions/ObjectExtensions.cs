using System;
using System.Reflection;

namespace AppFramework.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static void CopyFrom(this object current, object other)
        {
            if (other == null)
                return;
            if (current.GetType() != other.GetType())
                throw new ArgumentException(string.Format("Other item type ({0}) does not match this item's type ({1}).", other.GetType().FullName, current.GetType().FullName));

            var otherType = other.GetType();
            foreach (PropertyInfo currentPI in current.GetType().GetRuntimeProperties())
            {
                var propertyName = currentPI.Name;
                var otherPI = otherType.GetProperty(propertyName);
                if (otherPI != null && otherPI.CanRead)
                {
                    var otherValue = otherPI.GetValue(other);
                    current.SetPropertyValue(currentPI, otherValue);
                }
            }
        }

        public static void SetPropertyValue(this object current, PropertyInfo pi, object value)
        {
            if (pi != null && pi.CanWrite)
                pi.SetValue(current, value, null);
        }
    }
}