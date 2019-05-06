using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Input;

namespace Nez.UI
{
    public class ComboBoxEntry<T>
    {
        public T[] subItems;
        public T title;

        public ComboBoxEntry(T value)
        {
            title = value;
        }

        public ComboBoxEntry(T valueHeader, params T[] values)
        {
            this.title = valueHeader;
            this.subItems = values;
        }


        public ComboBoxEntry<T>[] toEntries()
        {
            ComboBoxEntry<T>[] entries = new ComboBoxEntry<T>[subItems.Length];
            for(int i = 0; i < subItems.Length; i++)
            {
                entries[i] = new ComboBoxEntry<T>(subItems[i]);
            }
            return entries;
        }
    }

    public class ComboBox<T> : Element, IInputListener where T : class
    {
        public Action<T> onChanged;
        public Action<T> onClicked;

        SelectBoxStyle style;
        List<ComboBoxEntry<T>> _items = new List<ComboBoxEntry<T>>();
        ArraySelection<ComboBoxEntry<T>> _selection;
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

            _selection = new ArraySelection<ComboBoxEntry<T>>( _items );
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
        {
            
        }


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
            Insist.isNotNull( style, "style cannot be null" );
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
        public void setItems( params ComboBoxEntry<T>[] newItems )
        {
            setItems( newItems );
        }

        /// <summary>
        /// Set the backing Array that makes up the choices available in the SelectBox
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="newItems">New items.</param>
        public void setItems( params T[] newItems)
        {
            List<ComboBoxEntry<T>> listItems = new List<ComboBoxEntry<T>>();
            foreach (T item in newItems) listItems.Add(new ComboBoxEntry<T>(item));
            setItems( listItems);
        }

        public void addItem( ComboBoxEntry<T> item )
        {
            _items.Add(item);

            float oldPrefWidth = preferredWidth;
            _selection.validate();
            _selectBoxList.listBox.setItems(_items);

            invalidate();
            validate();
            if (oldPrefWidth != _prefWidth)
            {
                invalidateHierarchy();
                setSize(_prefWidth, _prefHeight);
            }
            //setItems(_items);
        }


        /// <summary>
        /// Sets the items visible in the select box
        /// </summary>
        /// <returns>The items.</returns>
        /// <param name="newItems">New items.</param>
        public void setItems( List<ComboBoxEntry<T>> newItems )
        {
            Insist.isNotNull( newItems, "newItems cannot be null" );
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
        public List<ComboBoxEntry<T>> getItems()
        {
            return _items;
        }


        /// <summary>
        /// Get the set of selected items, useful when multiple items are selected returns a Selection object containing the
        /// selected elements
        /// </summary>
        /// <returns>The selection.</returns>
        public ArraySelection<ComboBoxEntry<T>> getSelection()
        {
            return _selection;
        }


        /// <summary>
        /// Returns the first selected item, or null. For multiple selections use SelectBox#getSelection()
        /// </summary>
        /// <returns>The selected.</returns>
        public ComboBoxEntry<T> getSelected()
        {
            return _selection.first();
        }


        /// <summary>
        /// Sets the selection to only the passed item, if it is a possible choice, else selects the first item.
        /// </summary>
        /// <returns>The selected.</returns>
        /// <param name="item">Item.</param>
        public void setSelected( ComboBoxEntry<T> item )
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
        public ComboBoxListBox<T> getListBox()
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
        public ComboBoxListBox<T> listBox;

        ComboBox<T> _selectBox;
        Element _previousScrollFocus;
        Vector2 _screenPosition;
        bool _isListBelowSelectBox;

        //public ComboBoxList<T> subList;

        public ComboBoxList( ComboBox<T> selectBox ) : base( null, selectBox.getStyle().scrollStyle )
        {
            _selectBox = selectBox;

            setOverscroll( false, false );
            setFadeScrollBars( false );
            setScrollingDisabled( true, false );

            listBox = new ComboBoxListBox<T>( selectBox.getStyle().listStyle );
            listBox.setTouchable( Touchable.Disabled );
            setWidget( listBox );

            listBox.onChanged += item =>
            {
                selectBox.getSelection().choose( item );
                if ( selectBox.onChanged != null )
                    selectBox.onChanged( item.title );
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
            var point = stage.getMousePosition();
            point = screenToLocalCoordinates(point);

            if ( Input.isKeyPressed( Keys.Escape ) )
            {
                Core.schedule( 0f, false, this, t => ( (ComboBoxList<T>)t.context ).hide() );
                return;
            }

            if ( Input.leftMouseButtonPressed )
            {
                //var point = stage.getMousePosition();
                //point = screenToLocalCoordinates( point );

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

    /// <summary>
    /// displays textual items and highlights the currently selected item
    /// </summary>
    public class ComboBoxListBox<T> : Element, IInputListener where T : class
    {
        public event Action<ComboBoxEntry<T>> onChanged;
        public event Action<ComboBoxEntry<T>> onClicked;

        ListBoxStyle _style;
        List<ComboBoxEntry<T>> _items = new List<ComboBoxEntry<T>>();
        ArraySelection<ComboBoxEntry<T>> _selection;
        float _prefWidth, _prefHeight;
        float _itemWidth, _itemHeight;
        float _textOffsetX, _textOffsetY;
        Rectangle? _cullingArea;
        int _hoveredItemIndex = -1;
        bool _isMouseOverList;

        public ComboBoxListBox<T> subBox;

        public bool keepSelection = false;

        public ComboBoxListBox( Skin skin, string styleName = null ) : this( skin.get<ListBoxStyle>( styleName ) )
        { }


        public ComboBoxListBox( ListBoxStyle style )
        {
            _selection = new ArraySelection<ComboBoxEntry<T>>( _items );
            _selection.setElement( this );
            _selection.setRequired( true );

            setStyle( style );
            setSize( preferredWidth, preferredHeight );
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
            _isMouseOverList = true;
        }


        void IInputListener.onMouseExit()
        {
            _isMouseOverList = false;
            _hoveredItemIndex = -1;
        }


        bool IInputListener.onMousePressed( Vector2 mousePos )
        {
            if ( _selection.isDisabled() || _items.Count == 0 )
                return false;

            var lastSelectedItem = _selection.getLastSelected();
            var index = getItemIndexUnderMousePosition( mousePos );
            index = Math.Max( 0, index );
            index = Math.Min( _items.Count - 1, index );

            _selection.choose( _items[ index ] );

            if (onClicked != null) onClicked(_items[index]);

            if (((keepSelection && lastSelectedItem != _items[index]) || (!keepSelection)) && onChanged != null)
            {
                onChanged(_items[index]);
            }

            return true;
        }


        void IInputListener.onMouseMoved( Vector2 mousePos )
        {

        }


        void IInputListener.onMouseUp( Vector2 mousePos )
        { }


        bool IInputListener.onMouseScrolled( int mouseWheelDelta )
        {
            return false;
        }


        public int getItemIndexUnderMousePosition( Vector2 mousePos )
        {
            if ( _selection.isDisabled() || _items.Count == 0 )
                return -1;

            var top = 0f;
            if ( _style.background != null )
            {
                top += _style.background.topHeight + _style.background.bottomHeight;
                mousePos.Y += _style.background.bottomHeight;
            }

            var index = (int)( ( top + mousePos.Y ) / _itemHeight );
            if ( index < 0 || index > _items.Count - 1 )
                return -1;

            return index;
        }

        #endregion


        public override void layout()
        {
            if (subBox != null) subBox.layout();

            var font = _style.font;
            IDrawable selectedDrawable = _style.selection;

            _itemHeight = /*font.getCapHeight()*/ font.lineHeight - font.descent * 2;
            _itemHeight += selectedDrawable.topHeight + selectedDrawable.bottomHeight;

            _textOffsetX = selectedDrawable.leftWidth;
            _textOffsetY = selectedDrawable.topHeight - font.descent;

            _prefWidth = 0;
            for ( var i = 0; i < _items.Count; i++ )
                _prefWidth = Math.Max( font.measureString( _items[ i ].title.ToString() ).X, _prefWidth );

            _prefWidth += selectedDrawable.leftWidth + selectedDrawable.rightWidth;
            _prefHeight = _items.Count * _itemHeight;

            //if (subBox != null) _prefWidth += subBox.getWidth() * 10;

            width = _prefWidth + 100;
            //setSize(_prefWidth, _prefHeight);
            

            var background = _style.background;
            if ( background != null )
            {
                _prefWidth += background.leftWidth + background.rightWidth;
                _prefHeight += background.topHeight + background.bottomHeight;
            }
        }

        public override void draw( Graphics graphics, float parentAlpha )
        {
            if (subBox != null) subBox.draw(graphics, parentAlpha);

            // update our hoved item if the mouse is over the list
            if ( _isMouseOverList )
            {
                var mousePos = screenToLocalCoordinates( stage.getMousePosition() );
                int prevHoveredIndex = _hoveredItemIndex;
                _hoveredItemIndex = getItemIndexUnderMousePosition( mousePos );

                _hoveredItemIndex = Math.Max(0, _hoveredItemIndex);
                _hoveredItemIndex = Math.Min(_items.Count - 1, _hoveredItemIndex);

                if( _items[_hoveredItemIndex].subItems != null && subBox == null)
                {
                    addSubBox( _hoveredItemIndex );
                }
                else if (_items[_hoveredItemIndex].subItems == null && subBox != null)
                {
                    removeSubBox(_hoveredItemIndex);
                }
            }

            validate();

            var font = _style.font;
            var selectedDrawable = _style.selection;

            var color = getColor();
            color = new Color( color, (int)( color.A * parentAlpha ) );

            float x = getX(), y = getY(), width = getWidth(), height = getHeight();
            var itemY = 0f;

            var background = _style.background;
            if ( background != null )
            {
                background.draw( graphics, x, y, width, height, color );
                var leftWidth = background.leftWidth;
                x += leftWidth;
                itemY += background.topHeight;
                width -= leftWidth + background.rightWidth;
            }

            var unselectedFontColor = new Color( _style.fontColorUnselected, (int)( _style.fontColorUnselected.A * parentAlpha ) );
            var selectedFontColor = new Color( _style.fontColorSelected, (int)( _style.fontColorSelected.A * parentAlpha ) );
            var hoveredFontColor = new Color( _style.fontColorHovered, (int)( _style.fontColorHovered.A * parentAlpha ) );

            if ( keepSelection == false ) selectedFontColor = unselectedFontColor;

            Color fontColor;
            for ( var i = 0; i < _items.Count; i++ )
            {
                if ( !_cullingArea.HasValue || ( itemY - _itemHeight <= _cullingArea.Value.Y + _cullingArea.Value.Height && itemY >= _cullingArea.Value.Y ) )
                {
                    var item = _items[ i ];
                    var selected = _selection.contains( item );
                    if ( selected && keepSelection)
                    {
                        selectedDrawable.draw( graphics, x, y + itemY, width, _itemHeight, color );
                        fontColor = selectedFontColor;
                    }
                    else if ( i == _hoveredItemIndex && _style.hoverSelection != null )
                    {
                        _style.hoverSelection.draw( graphics, x, y + itemY, width, _itemHeight, color );
                        fontColor = hoveredFontColor;
                    }
                    else
                    {
                        fontColor = unselectedFontColor;
                    }

                    var textPos = new Vector2( x + _textOffsetX, y + itemY + _textOffsetY );
                    graphics.batcher.drawString( font, item.title.ToString(), textPos, fontColor );

                    if( item.subItems != null)
                    {
                        graphics.batcher.drawString(font, ">", textPos + new Vector2( width - 12,0), fontColor);
                    }

                    graphics.batcher.drawString(font, "test", textPos + new Vector2(width - 12, 0), fontColor);

                }
                else if ( itemY < _cullingArea.Value.Y )
                {
                    break;
                }

                itemY += _itemHeight;
            }
        }

        private void removeSubBox(int hoveredItemIndex)
        {
            subBox = null;
            invalidate();
        }

        private void addSubBox(int index)
        {
            subBox = new ComboBoxListBox<T>(_style);
            subBox.x = x + _prefWidth;
            subBox.y = y + index * _itemHeight;
            subBox.setItems(_items[index].toEntries());
            
            invalidate();
        }


        #region config

        public ComboBoxListBox<T> setStyle( ListBoxStyle style )
        {
            Insist.isNotNull( style, "style cannot be null" );
            _style = style;
            invalidateHierarchy();
            return this;
        }


        /// <summary>
        /// Returns the list's style. Modifying the returned style may not have an effect until setStyle(ListStyle) is called
        /// </summary>
        /// <returns>The style.</returns>
        public ListBoxStyle getStyle()
        {
            return _style;
        }


        public ArraySelection<ComboBoxEntry<T>> getSelection()
        {
            return _selection;
        }


        /// <summary>
        /// Returns the first selected item, or null
        /// </summary>
        /// <returns>The selected.</returns>
        public ComboBoxEntry<T> getSelected()
        {
            return _selection.first();
        }


        /// <summary>
        /// Sets the selection to only the passed item, if it is a possible choice.
        /// </summary>
        /// <param name="item">Item.</param>
        public ComboBoxListBox<T> setSelected( ComboBoxEntry<T> item )
        {
            if ( _items.Contains( item ) )
                _selection.set( item );
            else if ( _selection.getRequired() && _items.Count > 0 )
                _selection.set( _items[ 0 ] );
            else
                _selection.clear();

            return this;
        }


        /// <summary>
        /// gets the index of the first selected item. The top item has an index of 0. Nothing selected has an index of -1.
        /// </summary>
        /// <returns>The selected index.</returns>
        public int getSelectedIndex()
        {
            var selected = _selection.items();
            return selected.Count == 0 ? -1 : _items.IndexOf( selected[ 0 ] );
        }


        /// <summary>
        /// Sets the selection to only the selected index
        /// </summary>
        /// <param name="index">Index.</param>
        public ComboBoxListBox<T> setSelectedIndex( int index )
        {
            Insist.isFalse( index < -1 || index >= _items.Count, "index must be >= -1 and < " + _items.Count + ": " + index );

            if ( index == -1 )
                _selection.clear();
            else
                _selection.set( _items[ index ] );

            return this;
        }


        public ComboBoxListBox<T> setItems( params ComboBoxEntry<T>[] newItems )
        {
            setItems( new List<ComboBoxEntry<T>>( newItems ) );
            return this;
        }


        /// <summary>
        /// Sets the items visible in the list, clearing the selection if it is no longer valid. If a selection is
        /// ArraySelection#getRequired(), the first item is selected.
        /// </summary>
        /// <param name="newItems">New items.</param>
        public ComboBoxListBox<T> setItems( IList<ComboBoxEntry<T>> newItems )
        {
            Insist.isNotNull( newItems, "newItems cannot be null" );
            float oldPrefWidth = _prefWidth, oldPrefHeight = _prefHeight;

            _items.Clear();
            _items.AddRange( newItems );
            _selection.validate();

            invalidate();
            validate();
            if ( oldPrefWidth != _prefWidth || oldPrefHeight != _prefHeight )
            {
                invalidateHierarchy();
                setSize( _prefWidth, _prefHeight );
            }
            return this;
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
        /// Returns the internal items array. If modified, {@link #setItems(Array)} must be called to reflect the changes.
        /// </summary>
        /// <returns>The items.</returns>
        public List<ComboBoxEntry<T>> getItems()
        {
            return _items;
        }


        public float getItemHeight()
        {
            return _itemHeight;
        }


        public ComboBoxListBox<T> setCullingArea( Rectangle cullingArea )
        {
            _cullingArea = cullingArea;
            return this;
        }

        #endregion

    }

}


