using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.UI.Widgets
{
    public class CollapseableTable : Table
    {
        public delegate void ItemToggled( CollapseableTable parent, Table table, bool toggled);
        public event ItemToggled OnItemToggled;

        public Table table;
        public Table container;

        public CollapseableTable( string title, Table table, Skin skin )
        {
            this.table = table;
            left().top().pad( 0 );
            //tableDebug( TableDebug.All );
            //table.tableDebug( TableDebug.All );

            add( CreateTitleLabel( title, skin ) ).setExpandX().width( table.getWidth() );
            row();
            //add( table ).width( table.getWidth() );
        }

        public void Hide()
        {
            table.remove();
            if( OnItemToggled != null ) OnItemToggled( this, table, false );
        }

        public void Show()
        {
            add( table ).width( table.getWidth() );
            if( OnItemToggled != null ) OnItemToggled( this, table, true );
        }

        public void Toggle( bool val )
        {
            if ( val )
                Show();
            else
                Hide();
        }

        protected TextButton CreateTitleLabel( string title, Skin skin )
        {
            //create titleLabel
            var label = new TextButton( title, skin );
            label.onClicked += Label_onClicked;
            label.setTouchable( Touchable.Enabled );

            // set a width on the cell so long labels dont cause issues if we have a leftCellWidth set
            //if ( leftCellWidth > 0 )
              //  label.setEllipsis( "..." ).setWidth( leftCellWidth );

            /*var tooltipAttribute = getFieldOrPropertyAttribute<TooltipAttribute>();
            if ( tooltipAttribute != null )
            {
                var tooltip = new TextTooltip( tooltipAttribute.tooltip, label, skin );
                table.getStage().addElement( tooltip );
            }*/

            return label;
        }

        private void Label_onClicked( Button obj )
        {
            Toggle( !table.hasParent() );
        }
    }
}
