using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AppFramework.Core.Models
{
    public abstract class BaseTrackedChangesModel : BaseModel
    {
        private List<string> _changedProperties = new List<string>();

        public void ClearPropertiesChangedList()
        {
            _changedProperties.Clear();
        }

        public Dictionary<string, object> GetChangedProperties()
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();
            foreach (var propertyName in _changedProperties)
            {
                PropertyInfo pi = this.GetType().GetRuntimeProperty(propertyName);

                var jsonProp = pi.CustomAttributes.FirstOrDefault(f => f.AttributeType.GetTypeInfo() == typeof(JsonPropertyAttribute).GetTypeInfo());
                if (jsonProp != null && jsonProp.ConstructorArguments.Any())
                {
                    var argument = jsonProp.ConstructorArguments.FirstOrDefault();
                    var name = argument.Value.ToString().Replace("\"", "");
                    if (!string.IsNullOrEmpty(name))
                    {
                        var value = pi.GetValue(this);
                        if (value != null)
                            dic.Add(name, value);
                    }
                }
                else
                {
                    var value = pi.GetValue(this);
                    if (value != null)
                        dic.Add(propertyName, value);
                }
            }

            return dic;
        }

        protected new void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.NotifyPropertyChanged(propertyName);
            if (_changedProperties.Contains(propertyName) == false)
                _changedProperties.Add(propertyName);
        }
    }
}
