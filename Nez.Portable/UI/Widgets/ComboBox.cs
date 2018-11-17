using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Input;

namespace Nez.UI
{
    public class ComboBox<T> : Element, IInputListener where T : class
    {
        public Action<T> onChanged;

        SelectBoxStyle style;
        List<T> _items = new List<T>();
        ArraySelection<T> _selection;
        ComboBoxList<T> _selectBoxList;
        float _prefWidth, _prefHeight;
        bool _isDisabled;
        bool _isMouseOver;

        public string title;

        public ComboBox( string title, Skin skin ) : this( title, skin.get<SelectBoxStyle>() )
        {
            this.title = title;
        }


        public ComboBox( string title, Skin skin, string styleName = null ) : this( title, skin.get<SelectBoxStyle>( styleName ) )
        {
            this.title = title;
        }


        public ComboBox( string title, SelectBoxStyle style )
        {
            this.title = title;

            _selection = new ArraySelection<T>( _items );
            setStyle( style );
            setSize( preferredWidth, preferredHeight );

            _selection.setElement( this );
            _selection.setRequired( true );
            _selectBoxList = new ComboBoxList<T>( this );
        }


        public override void layout()
        {
            var bg = style.background;
            var font = style.font;

            if ( bg != null )
                _prefHeight = Math.Max( bg.topHeight + bg.bottomHeight + font.lineHeight - font.descent * 2f, bg.minHeight );
            else
                _prefHeight = font.lineHeight - font.descent * 2;

            float maxItemWidth = font.measureString( title ).X;


            _prefWidth = maxItemWidth;
            if ( bg != null )
                _prefWidth += bg.leftWidth + bg.rightWidth;

            var listStyle = style.listStyle;
            var scrollStyle = style.scrollStyle;
            float listWidth = maxItemWidth + listStyle.selection.leftWidth + listStyle.selection.rightWidth;
            if ( scrollStyle.background != null )
                listWidth += scrollStyle.background.leftWidth + scrollStyle.background.rightWidth;
            if ( _selectBoxList == null || !_selectBoxList.isScrollingDisabledY() )
                listWidth += Math.Max( style.scrollStyle.vScroll != null ? style.scrollStyle.vScroll.minWidth : 0,
                                      style.scrollStyle.vScrollKnob != null ? style.scrollStyle.vScrollKnob.minWidth : 0 );
            _prefWidth = Math.Max( _prefWidth, listWidth );
        }


        public override void draw( Graphics graphics, float parentAlpha )
        {
            validate();

            IDrawable background;
            if ( _isDisabled && style.backgroundDisabled != null )
                background = style.backgroundDisabled;
            else if ( _selectBoxList.hasParent() && style.backgroundOpen != null )
                background = style.backgroundOpen;
            else if ( _isMouseOver && style.backgroundOver != null )
                background = style.backgroundOver;
            else if ( style.background != null )
                background = style.background;
            else
                background = null;

            var font = style.font;
            var fontColor = _isDisabled ? style.disabledFontColor : style.fontColor;

            var color = getColor();
            color = new Color( color, (int)( color.A * parentAlpha ) );
            float x = getX();
            float y = getY();
            float width = getWidth();
            float height = getHeight();

            if ( background != null )
                background.draw( graphics, x, y, width, height, color );

            var selected = _selection.first();
            if ( selected != null )
            {
                var str = title;
                if ( background != null )
                {
                    width -= background.leftWidth + background.rightWidth;
                    height -= background.bottomHeight + background.topHeight;
                    x += background.leftWidth;
                    y += (int)( height / 2 + background.bottomHeight - font.lineHeight / 2 );
                }
                else
                {
                    y += (int)( height / 2 + font.lineHeight / 2 );
                }

                fontColor = new Color( fontColor, (int)( fontColor.A * parentAlpha ) );
                graphics.batcher.drawString( font, str, new Vector2( x, y ), fontColor );
            }
        }


        #region ILayout

        public override float preferredWidth
        {
            get
            {
                validate();
                return _prefWidth;
            }
        }

        public override float preferredHeight
        {
            get
            {
                validate();
                return _prefHeight;
            }
        }

        #endregion


        #region IInputListener

        void IInputListener.onMouseEnter()
        {
            _isMouseOver = true;
        }


        void IInputListener.onMouseExit()
        {
            _isMouseOver = false;
        }


        bool IInputListener.onMousePressed( Vector2 mousePos )
        {
            if ( _isDisabled )
                return false;

            if ( _selectBoxList.hasParent() )
                hideList();
            else
                showList();

            return true;
        }


        void IInputListener.onMouseMoved( Vector2 mousePos )
        { }


        void IInputListener.onMouseUp( Vector2 mousePos )
        { }


        bool IInputListener.onMouseScrolled( int mouseWheelDelta )
        {
            return false;
        }

        #endregion


        #region config

        /// <summary>
        /// Set the max number of items to display when the select box is opened. Set to 0 (the default) to display as many as fit in
        /// the stage height.
        /// </summary>
        /// <returns>The max list count.</returns>
        /// <param name="maxListCount">Max list count.</param>
        public void setMaxListCount( int maxListCount )
        {
            _selectBoxList.maxListCount = maxListCount;
        }


        /// <summary>
        /// returns Max number of items to display when the box is opened, or <= 0 to display them all.
        /// </summary>
        /// <returns>The max list count.</returns>
        public int getMaxListCount()
        {
            return _selectBoxList.maxListCount;
        }


        internal override void setStage( Stage stage )
        {
            if ( stage == null )
                _selectBoxList.hide();
            base.setStage( stage );
        }


        public void setStyle( SelectBoxStyle style )
        {
            Assert.isNotNull( style, "style cannot be null" );
            this.style = style;
            invalidateHierarchy();
        }


        /// <summary>
        /// Returns the select box's style. Modifying the returned style may not have an effect until setStyle(SelectBoxStyle)
        /// is called.
        /// </summary>
        /// <returns>The style.</returns>
        public SelectBoxStyle getStyle()
        {
            return style;
        }


        /// <summary>
        /// Set the backing Array that makes up the choices available in the SelectBox
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="newItems">New items.</param>
        public void setItems( params T[] newItems )
        {
            setItems( new List<T>( newItems ) );
        }


        /// <summary>
        /// Sets the items visible in the select box
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="newItems">New items.</param>
        public void setItems( List<T> newItems )
        {
            Assert.isNotNull( newItems, "newItems cannot be null" );
            float oldPrefWidth = preferredWidth;

            _items.Clear();
            _items.AddRange( newItems );
            _selection.validate();
            _selectBoxList.listBox.setItems( _items );

            invalidate();
            validate();
            if ( oldPrefWidth != _prefWidth )
            {
                invalidateHierarchy();
                setSize( _prefWidth, _prefHeight );
            }
        }


        public void clearItems()
        {
            if ( _items.Count == 0 )
                return;

            _items.Clear();
            _selection.clear();
            invalidateHierarchy();
        }


        /// <summary>
        /// Returns the internal items array. If modified, setItems(Array) must be called to reflect the changes.
        /// </summary>
        /// <returns>The items.</returns>
        public List<T> getItems()
        {
            return _items;
        }


        /// <summary>
        /// Get the set of selected items, useful when multiple items are selected returns a Selection object containing the
        /// selected elements
        /// </summary>
        /// <returns>The selection.</returns>
        public ArraySelection<T> getSelection()
        {
            return _selection;
        }


        /// <summary>
        /// Returns the first selected item, or null. For multiple selections use SelectBox#getSelection()
        /// </summary>
        /// <returns>The selected.</returns>
        public T getSelected()
        {
            return _selection.first();
        }


        /// <summary>
        /// Sets the selection to only the passed item, if it is a possible choice, else selects the first item.
        /// </summary>
        /// <returns>The selected.</returns>
        /// <param name="item">Item.</param>
        public void setSelected( T item )
        {
            if ( _items.Contains( item ) )
                _selection.set( item );
            else if ( _items.Count > 0 )
                _selection.set( _items[ 0 ] );
            else
                _selection.clear();
        }


        /// <summary>
        /// returns The index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1.
        /// </summary>
        /// <returns>The selected index.</returns>
        public int getSelectedIndex()
        {
            var selected = _selection.items();
            return selected.Count == 0 ? -1 : _items.IndexOf( selected.First() );
        }


        /// <summary>
        /// Sets the selection to only the selected index
        /// </summary>
        /// <returns>The selected index.</returns>
        /// <param name="index">Index.</param>
        public void setSelectedIndex( int index )
        {
            _selection.set( _items[ index ] );
        }


        public void setDisabled( bool disabled )
        {
            if ( disabled && !this._isDisabled )
                hideList();
            this._isDisabled = disabled;
        }


        public bool isDisabled()
        {
            return _isDisabled;
        }


        public void showList()
        {
            if ( _items.Count == 0 )
                return;
            _selectBoxList.show( getStage() );
        }


        public void hideList()
        {
            _selectBoxList.hide();
        }


        /// <summary>
        /// Returns the ListBox shown when the select box is open
        /// </summary>
        /// <returns>The list.</returns>
        public ListBox<T> getListBox()
        {
            return _selectBoxList.listBox;
        }


        /// <summary>
        /// Disables scrolling of the list shown when the select box is open.
        /// </summary>
        /// <returns>The scrolling disabled.</returns>
        /// <param name="y">The y coordinate.</param>
        public void setScrollingDisabled( bool y )
        {
            _selectBoxList.setScrollingDisabled( true, y );
            invalidateHierarchy();
        }


        /// <summary>
        /// Returns the scroll pane containing the list that is shown when the select box is open.
        /// </summary>
        /// <returns>The scroll pane.</returns>
        public ScrollPane getScrollPane()
        {
            return _selectBoxList;
        }


        public void onShow( Element selectBoxList, bool below )
        {
            //_selectBoxList.getColor().A = 0;
            //_selectBoxList.addAction( fadeIn( 0.3f, Interpolation.fade ) );
        }


        public void onHide( Element selectBoxList )
        {
            //selectBoxList.getColor().A = 255;
            //selectBoxList.addAction( sequence( fadeOut( 0.15f, Interpolation.fade ), removeActor() ) );
            selectBoxList.remove();
        }

        #endregion

    }

    public class ComboBoxList<T> : ScrollPane where T : class
    {
        public int maxListCount;
        public ListBox<T> listBox;

        ComboBox<T> _selectBox;
        Element _previousScrollFocus;
        Vector2 _screenPosition;
        bool _isListBelowSelectBox;


        public ComboBoxList( ComboBox<T> selectBox ) : base( null, selectBox.getStyle().scrollStyle )
        {
            _selectBox = selectBox;

            setOverscroll( false, false );
            setFadeScrollBars( false );
            setScrollingDisabled( true, false );

            listBox = new ListBox<T>( selectBox.getStyle().listStyle );
            listBox.setTouchable( Touchable.Disabled );
            setWidget( listBox );

            listBox.onChanged += item =>
            {
                selectBox.getSelection().choose( item );
                if ( selectBox.onChanged != null )
                    selectBox.onChanged( item );
                hide();
            };
        }


        public void show( Stage stage )
        {
            if ( listBox.isTouchable() )
                return;

            stage.addElement( this );

            _screenPosition = _selectBox.localToStageCoordinates( Vector2.Zero );

            // show the list above or below the select box, limited to a number of items and the available height in the stage.
            float itemHeight = listBox.getItemHeight();
            float height = itemHeight * ( maxListCount <= 0 ? _selectBox.getItems().Count : Math.Min( maxListCount, _selectBox.getItems().Count ) );
            var scrollPaneBackground = getStyle().background;
            if ( scrollPaneBackground != null )
                height += scrollPaneBackground.topHeight + scrollPaneBackground.bottomHeight;
            var listBackground = listBox.getStyle().background;
            if ( listBackground != null )
                height += listBackground.topHeight + listBackground.bottomHeight;

            float heightAbove = _screenPosition.Y;
            float heightBelow = Screen.height /*camera.viewportHeight */ - _screenPosition.Y - _selectBox.getHeight();
            _isListBelowSelectBox = true;
            if ( height > heightBelow )
            {
                if ( heightAbove > heightBelow )
                {
                    _isListBelowSelectBox = false;
                    height = Math.Min( height, heightAbove );
                }
                else
                {
                    height = heightBelow;
                }
            }

            if ( !_isListBelowSelectBox )
                setY( _screenPosition.Y - height );
            else
                setY( _screenPosition.Y + _selectBox.getHeight() );
            setX( _screenPosition.X );
            setHeight( height );
            validate();

            var width = Math.Max( preferredWidth, _selectBox.getWidth() );
            if ( preferredHeight > height && !_disableY )
                width += getScrollBarWidth();
            setWidth( width );

            validate();
            scrollTo( 0, listBox.getHeight() - _selectBox.getSelectedIndex() * itemHeight - itemHeight / 2, 0, 0, true, true );
            updateVisualScroll();

            _previousScrollFocus = null;

            listBox.getSelection().set( _selectBox.getSelected() );
            listBox.setTouchable( Touchable.Enabled );
            _selectBox.onShow( this, _isListBelowSelectBox );
        }


        public void hide()
        {
            if ( !listBox.isTouchable() || !hasParent() )
                return;

            listBox.setTouchable( Touchable.Disabled );

            if ( stage != null )
            {
                if ( _previousScrollFocus != null && _previousScrollFocus.getStage() == null )
                    _previousScrollFocus = null;
            }

            _selectBox.onHide( this );
        }


        public override void draw( Graphics graphics, float parentAlpha )
        {
            var temp = _selectBox.localToStageCoordinates( Vector2.Zero );
            if ( temp != _screenPosition )
                Core.schedule( 0f, false, this, t => ( (SelectBoxList<T>)t.context ).hide() );

            base.draw( graphics, parentAlpha );
        }


        protected override void update()
        {
            if ( Input.isKeyPressed( Keys.Escape ) )
            {
                Core.schedule( 0f, false, this, t => ( (ComboBoxList<T>)t.context ).hide() );
                return;
            }

            if ( Input.leftMouseButtonPressed )
            {
                var point = stage.getMousePosition();
                point = screenToLocalCoordinates( point );

                float yMin = 0, yMax = height;

                // we need to include the list and the select box for our click checker. if the list is above the select box we expand the
                // height to include it. If the list is below we check for positions up to -_selectBox.height
                if ( _isListBelowSelectBox )
                    yMin -= _selectBox.height;
                else
                    yMax += _selectBox.height;

                /*if( point.X < 0 || point.X > width || point.Y > yMax || point.Y < yMin )
					Core.schedule( 0f, false, this, t => ( (SelectBoxList<T>)t.context ).hide() );*/
            }

            base.update();
            toFront();
        }

    }
}


