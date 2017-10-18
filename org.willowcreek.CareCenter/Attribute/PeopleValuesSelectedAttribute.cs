using System;

using Rock.Field;
using Rock.Attribute;
using org.willowcreek.CareCenter.Field.Types;

namespace org.willowcreek.CareCenter.Attribute
{
    /// <summary>
    /// Field Attribute to select multiple values for multiple people
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    class PeopleValuesSelectedAttribute : FieldAttribute
    {
        private const string ATTRIBUTE_CATEGORY = "attributeCategory";

        public PeopleValuesSelectedAttribute( string attributeCategoryGuid, string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( PeopleValuesSelectedFieldType ).FullName, "org.willowcreek.CareCenter" )
        {
            var configValue = new ConfigurationValue( attributeCategoryGuid );
            FieldConfigurationValues.Add( ATTRIBUTE_CATEGORY, configValue );
        }
    }
}
