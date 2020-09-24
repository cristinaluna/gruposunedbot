using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GruposUnedBot
{
    public static class StringExtensionMethods
    {
        public static String ReplaceAny(this String _this, IEnumerable<char> oldValues, String newValue)
        {
            String result = _this;
            foreach(var val in oldValues)
                result = result.Replace(val.ToString(), newValue);
            
            return result;
        }
    }
}
