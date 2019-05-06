using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.UI.Widgets
{
    public class Tab : Group
    {
        public string name;
        public Button tabButton;

        public Tab( string name, Button tabButton )
        {
            this.name = name;
            this.tabButton = tabButton;
        }
    }

    public class TabbedPanel : Table
    {
        public List<Group> tabs = new List<Group>();

        int activeTab = 0;
        Skin skin;

        Table buttonGroup = new Table();
        Group tabHolder = new Group();

        public TabbedPanel( Skin skin )
        {
            this.skin = skin;

            Initialize();
            //table.add( new Label( "test" ) );
            //addElement( new Label( "test 2" ) );
        }

        private void Initialize()
        {
            top().left();
            pad( 2 );
            //addElement( table );

            add( buttonGroup );
            buttonGroup.top().left();

            row();

            add( tabHolder ).top().left();
        }

        public TabbedPanel( Group[] tabs, Skin skin )
        {
            this.skin = skin;
            this.tabs.AddRange( tabs );

            //addElement( table );
            add( buttonGroup );
            row();
            add( tabHolder );

            Initialize();
        }

   

        public void SwitchTab( int tabIndex )
        {
            this.activeTab = tabIndex;
            foreach ( Tab tab in tabs )
            {
                tab.setIsVisible( false );
            }

            tabs[ activeTab ].setVisible( true );

        }

        public Tab addTab( string name, Element content )
        {
            TextButton button = new TextButton( name, skin );
            button.onClicked += TabButtonClicked;
            Tab tab = new Tab( name, button );

            tab.addElement( content );
            tabs.Add( tab );

            buttonGroup.add( button );
            tabHolder.addElement( tab );
            //table.add( tab );
            //addElement( tab );
            
            if ( tabs.Count == 1 )
                tab.setVisible( true );
            else
                tab.setVisible( false );
            
            return tab;
        }

        private void TabButtonClicked( Button obj )
        {
            SwitchTab( buttonGroup.children.IndexOf( obj ) );
        }
    }
}
