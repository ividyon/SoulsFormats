using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulsFormats.Formats.Havok.HavokTypes
{
    internal class HavokTypeRegistry
    {
        public static Dictionary<string, Func<HavokTagObject, HavokTypeBase>> registry = new() {
            { "hkArray", (a) => new hkArray(a) },
            { "hkRootLevelContainer::NamedVariant", (a) => new hkNamedVariant(a) },
            { "hkaAnimationContainer", (a) => new hkaAnimationContainer(a) },
        };


        /// <summary>
        /// Tries to instantiate the object described by the parameter, returning the parameter
        /// in the case of a failure.
        /// </summary>
        public static object? TryInstantiateObject(HavokTagObject o)
        {
            if (registry.TryGetValue(o.Type.name, out var cons)) {
                return cons(o);
            }
            return o;
        }
    }
}
