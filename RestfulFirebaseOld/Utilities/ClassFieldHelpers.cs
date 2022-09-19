using System.Globalization;
using System.Reflection;

namespace RestfulFirebase.Utilities
{
    internal class ClassFieldHelpers
    {
        public static string GetPropertyName(FieldInfo fieldInfo)
        {
            string propertyName = fieldInfo.Name;

            if (propertyName.StartsWith("m_"))
            {
                propertyName = propertyName[2..];
            }
            else if (propertyName.StartsWith("_"))
            {
                propertyName = propertyName.TrimStart('_');
            }

            return $"{char.ToUpper(propertyName[0], CultureInfo.InvariantCulture)}{propertyName[1..]}";
        }
    }
}
