using System;
using System.Web.UI;

using Newtonsoft.Json;

using Rock;
using Rock.Web.UI.Controls;

namespace org.willowcreek.CareCenter.Web.UI.Controls
{
    public class MaxCurrency : CurrencyBox
    {
        #region Properties

        public MaxAndValue Selection
        {
            get
            {
                EnsureChildControls();
                var maxAndValue = new MaxAndValue();
                maxAndValue.MaxValue = ViewState["Max"] as decimal?;
                maxAndValue.Value = this.Text.AsDecimalOrNull();
                return maxAndValue;
            }
            set
            {
                EnsureChildControls();
                ViewState["Max"] = value != null ? value.MaxValue : (decimal?)null;
                this.Text = value != null && value.Value.HasValue ? value.Value.Value.ToString() : string.Empty;
            }
        }

        #endregion

        #region Base Control Methods

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }

        public override void RenderBaseControl( HtmlTextWriter writer )
        {
            decimal? maxValue = Selection.MaxValue;
            this.MinimumValue = "0.0";
            this.MaximumValue = maxValue.HasValue ? maxValue.Value.ToString() : "0.0";

            base.RenderBaseControl( writer );

            //writer.RenderBeginTag( HtmlTextWriterTag.Div );
            //writer.Write( string.Format( "{0} available", this.MaximumValue.AsDecimal().FormatAsCurrency() ) );
            //writer.RenderEndTag();
        }

        #endregion

        [Serializable]
        public class MaxAndValue
        {
            public decimal? MaxValue { get; set; }
            public decimal? Value { get; set; }

            public string ToJson()
            {
                return JsonConvert.SerializeObject( this );
            }

            public static MaxAndValue FromJson( string jsonString )
            {
                if ( !string.IsNullOrWhiteSpace( jsonString ) )
                {
                    return JsonConvert.DeserializeObject<MaxAndValue>( jsonString );
                }
                return new MaxAndValue();
            }
        }
    }
}
