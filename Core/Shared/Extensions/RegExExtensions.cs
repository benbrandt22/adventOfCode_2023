using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Core.Shared.Extensions;

public static class RegExExtensions
{
    /// <summary>
    /// Uses Named capture groups in the RegEx Match as object properties to map it to the desired type. 
    /// </summary>
    public static T MapTo<T>(this Match match)
    {
        var constructors = typeof(T).GetConstructors();
        
        var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
        if (parameterlessConstructor != null)
        {
            // get instance of T and set properties
            var instance = (T)parameterlessConstructor.Invoke(null);
            
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var stringValue = match.Groups.Values
                    .SingleOrDefault(g => propertyInfo.Name.Equals(g.Name, StringComparison.OrdinalIgnoreCase) )?
                    .Value;

                if (stringValue != null)
                {
                    var propValue = TypeDescriptor.GetConverter(propertyInfo.PropertyType)
                        .ConvertFromInvariantString(stringValue);
                
                    propertyInfo.SetValue(instance, propValue);
                }
            }

            return instance;
        }
        else
        {
            // find the default constructor and get parameter values
            var constructor = constructors.First();
            
            var constructorParameterValues = new List<object>();
            foreach (var parameterInfo in constructor.GetParameters())
            {
                var stringValue = match.Groups.Values
                    .SingleOrDefault(g => parameterInfo.Name!.Equals(g.Name, StringComparison.OrdinalIgnoreCase) )?
                    .Value ?? "";
                var paramValue = TypeDescriptor.GetConverter(parameterInfo.ParameterType)
                    .ConvertFromInvariantString(stringValue)!;
                constructorParameterValues.Add(paramValue);
            }

            // invoke constructor with parameters
            var instance = (T)constructor.Invoke(constructorParameterValues.ToArray());
            return instance;
        }
    }

    /// <summary>
    /// Uses Named capture groups in the RegEx Matches as object properties to map each item to the desired type. 
    /// </summary>
    public static IEnumerable<T> MapEachTo<T>(this IEnumerable<Match> matchCollection) => matchCollection.Select(m => m.MapTo<T>());
}