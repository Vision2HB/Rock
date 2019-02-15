using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.bemaservices.Finance.Utility.ExtensionMethods
{
    public static class StringExtensions
    {
        public static string TrimLength( this string value, int maxLength )
        {
            if ( string.IsNullOrEmpty( value ) )
            {
                return value;
            }

            return value.Length <= maxLength ? value : value.Substring( 0, maxLength );
        }
    }
}
