using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Oven
{
    public interface IFilling
    {
        object OnMethod(Type type, MethodInfo method, object[] args);
    }
}
