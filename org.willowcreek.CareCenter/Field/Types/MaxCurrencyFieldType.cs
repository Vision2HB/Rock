using System.Collections.Generic;
using System.Web.UI;

using org.willowcreek.CareCenter.Web.UI.Controls;

using Rock;
using Rock.Field;

namespace org.willowcreek.CareCenter.Field.Types
{
    class MaxCurrencyFieldtype : Rock.Field.FieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            if ( !string.IsNullOrWhiteSpace( value ) )
            {
                var maxAndValue = MaxCurrency.MaxAndValue.FromJson( value );
                if ( maxAndValue != null && maxAndValue.Value.HasValue )
                {
                    formattedValue = maxAndValue.Value.Value.FormatAsCurrency();
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new MaxCurrency { ID = id };
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var itemControl = control as MaxCurrency;
            if ( itemControl != null )
            {
                return itemControl.Selection.ToJson();
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var itemControl = control as MaxCurrency;
            if ( itemControl != null )
            {
                itemControl.Selection = MaxCurrency.MaxAndValue.FromJson( value );
            }
        }

        #endregion

    }
}
