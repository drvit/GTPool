using System.Reflection;

namespace NewsSearch.Infrastructure.Utils
{
    public class DynamicUtils
    {
        public static void SetPropertyValue(object instance, string name, object value)
        {
            if (!string.IsNullOrEmpty(name))
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var prop = instance.GetType()
                        .GetProperty(
                            name,
                            BindingFlags.SetProperty |
                            BindingFlags.IgnoreCase |
                            BindingFlags.Public |
                            BindingFlags.Instance);

                    if (prop != null)
                    {
                        prop.SetValue(instance, value, null);
                    }
                }
            }
        }
    }
}