using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Engine.Extensions
{
    public static class ModelExtensions
    {
        public static void CopyPropertiesTo<T, TU>(this T source, TU dest)
        {
            List<PropertyInfo> sourceProps = typeof(T).GetProperties().Where(x => x.CanRead).ToList();
            List<PropertyInfo> destProps = typeof(TU).GetProperties().Where(x => x.CanWrite).ToList();

            foreach (var sourceProp in sourceProps)
            {
                if (destProps.Any(x => x.Name == sourceProp.Name))
                {
                    var p = destProps.First(x => x.Name == sourceProp.Name);
                    if (p.CanWrite)
                    {
                        // check if the property can be set or no.
                        p.SetValue(dest, sourceProp.GetValue(source, null), null);
                    }
                }
            }
        }
    }
}