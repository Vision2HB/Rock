﻿using System.Collections.Generic;
using System.Web.UI.WebControls;

using com.bemaservices.HrManagement.Model;

using Rock;
using Rock.Web.UI.Controls;

namespace com.bemaservices.HrManagement.Web.UI.Controls.Pickers
{
    public class PtoTierPicker : RockDropDownList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PtoTypePicker" /> class.
        /// </summary>
        public PtoTierPicker() : base()
        {
            Label = "PTO Tier";
        }

        /// <summary>
        /// Gets or sets the Pto Types.
        /// </summary>
        /// <value>
        /// The Pto Types.
        /// </value>
        public List<PtoTier> PtoTiers
        {
            set
            {
                this.Items.Clear();
                this.Items.Add( new ListItem() );

                foreach ( PtoTier ptoTier in value )
                {
                    this.Items.Add( new ListItem( ptoTier.Name, ptoTier.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected Pto Type identifier.
        /// </summary>
        /// <value>
        /// The selected Pto Type identifier.
        /// </value>
        public int? SelectedPtoTierId
        {
            get
            {
                return this.SelectedValueAsId();
            }

            set
            {
                int id = value.HasValue ? value.Value : 0;
                var li = this.Items.FindByValue( id.ToString() );
                if( li != null )
                {
                    li.Selected = true;
                }
            }
        }
    }
}
