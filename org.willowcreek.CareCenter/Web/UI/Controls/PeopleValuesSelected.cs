using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;

using Newtonsoft.Json;
using Rock.Web.UI.Controls;

namespace org.willowcreek.CareCenter.Web.UI.Controls
{
    public class PeopleValuesSelected : CompositeControl, IRockControl
    {
        #region Controls

        private HiddenFieldWithClass _hfSelection;

        #endregion

        #region Properties
        
        public PeopleAndValues Selection
        {
            get
            {
                EnsureChildControls();
                return PeopleAndValues.FromJson( _hfSelection.Value );
            }
            set
            {
                EnsureChildControls();
                _hfSelection.Value = value != null ? value.ToJson() : string.Empty;
            }
        }

        #endregion

        #region IRockControl implementation

        /// <summary>
        /// Gets or sets the label text.
        /// </summary>
        /// <value>
        /// The label text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The text for the label." )
        ]
        public string Label
        {
            get
            {
                return ViewState["Label"] as string ?? string.Empty;
            }

            set
            {
                ViewState["Label"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the form group class.
        /// </summary>
        /// <value>
        /// The form group class.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        Description( "The CSS class to add to the form-group div." )
        ]
        public string FormGroupCssClass
        {
            get { return ViewState["FormGroupCssClass"] as string ?? string.Empty; }
            set { ViewState["FormGroupCssClass"] = value; }
        }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        /// <value>
        /// The help text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The help block." )
        ]
        public string Help
        {
            get
            {
                return HelpBlock != null ? HelpBlock.Text : string.Empty;
            }

            set
            {
                if ( HelpBlock != null )
                {
                    HelpBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the warning text.
        /// </summary>
        /// <value>
        /// The warning text.
        /// </value>
        [
        Bindable( true ),
        Category( "Appearance" ),
        DefaultValue( "" ),
        Description( "The warning block." )
        ]
        public string Warning
        {
            get
            {
                return WarningBlock != null ? WarningBlock.Text : string.Empty;
            }

            set
            {
                if ( WarningBlock != null )
                {
                    WarningBlock.Text = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RockTextBox"/> is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        [
        Bindable( true ),
        Category( "Behavior" ),
        DefaultValue( "false" ),
        Description( "Is the value required?" )
        ]
        public bool Required
        {
            get
            {
                return ViewState["Required"] as bool? ?? false;
            }

            set
            {
                ViewState["Required"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the required error message.  If blank, the LabelName name will be used
        /// </summary>
        /// <value>
        /// The required error message.
        /// </value>
        public string RequiredErrorMessage
        {
            get
            {
                return RequiredFieldValidator != null ? RequiredFieldValidator.ErrorMessage : string.Empty;
            }

            set
            {
                if ( RequiredFieldValidator != null )
                {
                    RequiredFieldValidator.ErrorMessage = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsValid
        {
            get
            {
                return !Required || RequiredFieldValidator == null || RequiredFieldValidator.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the help block.
        /// </summary>
        /// <value>
        /// The help block.
        /// </value>
        public HelpBlock HelpBlock { get; set; }

        /// <summary>
        /// Gets or sets the warning block.
        /// </summary>
        /// <value>
        /// The warning block.
        /// </value>
        public WarningBlock WarningBlock { get; set; }

        /// <summary>
        /// Gets or sets the required field validator.
        /// </summary>
        /// <value>
        /// The required field validator.
        /// </value>
        public RequiredFieldValidator RequiredFieldValidator { get; set; }

        #endregion

        public string ValidationGroup
        {
            get { return ViewState["ValidationGroup"] != null ? ViewState["ValidationGroup"].ToString() : string.Empty; }
            set { ViewState["ValidationGroup"] = value; }
        }

        #region Constructors

        public PeopleValuesSelected() : base()
        {
            RockControlHelper.Init( this );
        }

        #endregion

        #region Base Control Methods

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            Controls.Clear();

            _hfSelection = new HiddenFieldWithClass();
            _hfSelection.ID = this.ID + "_hfSelection";
            _hfSelection.CssClass = "js-people-values-selected-hf";
            Controls.Add( _hfSelection );
        }

        public override void RenderControl( HtmlTextWriter writer )
        {
            if ( this.Visible )
            {
                RockControlHelper.RenderControl( this, writer );
            }
        }

        public void RenderBaseControl( HtmlTextWriter writer )
        {
            _hfSelection.RenderControl( writer );
            var selection = Selection;

            if ( this.Visible && selection != null )
            {
                writer.AddAttribute( "style", "border-collapse:separate;border-spacing:3px" );
                writer.RenderBeginTag( HtmlTextWriterTag.Table );

                writer.RenderBeginTag( HtmlTextWriterTag.Thead );
                writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                // name
                writer.RenderBeginTag( HtmlTextWriterTag.Th );
                writer.RenderEndTag();

                foreach ( var attribute in Selection.Values )
                {
                    writer.AddAttribute( "style", "text-align:center" );
                    writer.RenderBeginTag( HtmlTextWriterTag.Th );
                    writer.Write( attribute.Value );
                    writer.RenderEndTag();
                }

                writer.RenderEndTag();  // tr
                writer.RenderEndTag();  // thead

                writer.RenderBeginTag( HtmlTextWriterTag.Tbody );

                foreach ( var person in selection.People )
                {
                    writer.RenderBeginTag( HtmlTextWriterTag.Tr );

                    writer.RenderBeginTag( HtmlTextWriterTag.Td );
                    writer.Write( person.PersonName );
                    writer.RenderEndTag();

                    foreach ( var attribute in selection.Values )
                    {
                        writer.AddAttribute( "style", "text-align:center" );
                        writer.RenderBeginTag( HtmlTextWriterTag.Td );

                        writer.AddAttribute( "type", "checkbox" );
                        writer.AddAttribute( "data-person-guid", person.PersonGuid.ToString() );
                        writer.AddAttribute( "data-attribute-key", attribute.Key );
                        writer.AddAttribute( "class", "js-people-values-selected-checkbox" );
                        if ( person != null && person.SelectedValues != null && person.SelectedValues.Contains( attribute.Key ) )
                        {
                            writer.AddAttribute( "checked", "checked" );
                        }
                        writer.RenderBeginTag( HtmlTextWriterTag.Input );
                        writer.RenderEndTag();

                        writer.RenderEndTag();
                    }

                    writer.RenderEndTag();
                }

                writer.RenderEndTag();  // tbody

                writer.RenderEndTag();  // table
            }

            RegisterClientScript();
        }

        private void RegisterClientScript()
        {
            string script = @"
    $(document).on('click', 'input.js-people-values-selected-checkbox', function (e) {
        var $cb = $(this);
        var personGuid = $cb.attr('data-person-guid');
        var attributeGuid = $cb.attr('data-attribute-key');
        var selected = $cb.is(':checked');
        var $hf = $cb.closest('div.control-wrapper').find('input.js-people-values-selected-hf');
        var obj = jQuery.parseJSON($hf.val());

        if ( obj != null ) {
            for( var i = 0; i < obj.People.length; i++ ) {
                var person = obj.People[i];
                if ( person.PersonGuid === personGuid ) {
                    if ( person.SelectedValues == null ) {
                        person.SelectedValues = [];
                    }
                    if ( selected ) {
                        person.SelectedValues.push(attributeGuid);
                    } else {
                        for (var j = person.SelectedValues.length-1; j >= 0; j--) {
                            if ( person.SelectedValues[j] === attributeGuid ) {
                                person.SelectedValues.splice(j, 1);
                            }
                        }
                    }
                }
            }
            $hf.val( JSON.stringify(obj) );
        } 
    });
";
            ScriptManager.RegisterStartupScript( this, this.GetType(), "people-values-selected", script, true );
        }

        #endregion

        [Serializable]
        public class PeopleAndValues
        {
            public Dictionary<string, string> Values { get; set; }
            public List<PersonSelection> People { get; set; }

            public PeopleAndValues()
            {
                Values = new Dictionary<string, string>();
                People = new List<PersonSelection>();
            }

            public string ToJson()
            {
                return JsonConvert.SerializeObject( this );
            }

            public static PeopleAndValues FromJson( string jsonString )
            {
                if ( !string.IsNullOrWhiteSpace( jsonString ) )
                {
                    return JsonConvert.DeserializeObject<PeopleAndValues>( jsonString );
                }
                return new PeopleAndValues();
            }

        }

        [Serializable]
        public class PersonSelection
        {
            public Guid PersonGuid { get; set; }
            public string PersonName { get; set; }
            public List<string> SelectedValues { get; set; }

            public PersonSelection()
            {
                SelectedValues = new List<string>();
            }
        }
    }
}
