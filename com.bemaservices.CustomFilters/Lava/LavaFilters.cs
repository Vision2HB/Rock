// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Entity;
using System.Drawing;
//using System.Drawing.Imaging;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Linq.Dynamic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
//using System.Web.UI.HtmlControls;
//using DDay.iCal;
using DotLiquid;
using DotLiquid.Util;
//using Humanizer;
//using Humanizer.Localisation;
//using ImageResizer;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Rock;
using Rock.Lava;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Security;
using Rock.Web.UI;
//using UAParser;
using Rock.Utility;

namespace com.bemaservices.CustomFilters.Lava
{
    /// <summary>
    ///
    /// </summary>
    public class LavaFilters : IRockStartup
    {
        /// <summary>
        /// All IRockStartup classes will be run in order by this value. If class does not depend on an order, return zero.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int StartupOrder { get { return 0; } }

        /// <summary>
        /// Method that will be run at Rock startup
        /// </summary>
        public virtual void OnStartup()
        {
            Template.RegisterFilter( GetType() );
        }

        public static List<object> BEMA_Array_CreateList( string input )
        {
            var result = new List<object>();

            return result;
        }

        public static object BEMA_Array_Add( object input, object addedObject )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( ( IEnumerable ) input ) )
                {
                    result.Add( value );
                }
                result.Add( addedObject );

                return result;
            }

            return input;
        }

        public static object BEMA_Array_Remove( object input, object removedObject )
        {
            if ( input == null )
            {
                return input;
            }

            if ( input is IEnumerable )
            {
                var result = new List<object>();

                foreach ( var value in ( ( IEnumerable ) input ) )
                {
                    var condition = DotLiquid.Condition.Operators["=="];

                    if ( !condition( value, removedObject ) )
                    {
                        result.Add( value );
                    }
                }

                return result;
            }

            return input;
        }

        public static object BEMA_Array_RemoveAt( object input, object index )
        {
            if ( input == null || index == null )
            {
                return input;
            }

            if ( !( input is IList ) )
            {
                return input;
            }

            var inputList = input as IList;
            var indexInt = index.ToString().AsIntegerOrNull();
            if ( !indexInt.HasValue || indexInt.Value < 0 || indexInt.Value >= inputList.Count )
            {
                return input;
            }
            else
            {
                inputList.RemoveAt( indexInt.Value );
                return inputList;
            }
        }
    }
}

