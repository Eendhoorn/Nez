using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nez.UI.Drawable
{
    public class SpriteDrawable : IDrawable
    {
        public Color? tintColor;

        public SpriteAnimation spriteAnimation;
        public SpriteEffects spriteEffects = SpriteEffects.None;

        public event Action<SpriteAnimation> onAnimationCompletedEvent;
        public bool isPlaying { get; private set; }
        public int currentFrame { get; private set; }
        public bool timeScaleIndependant = false;

        // playback state
        SpriteAnimation _currentAnimation;
        float _totalElapsedTime;
        float _elapsedDelay;
        int _completedIterations;
        bool _delayComplete;
        bool _isReversed;
        bool _isLoopingBackOnPingPong;
        protected Subtexture _subtexture;

        /// <summary>
        /// determines if the sprite should be rendered normally or flipped horizontally
        /// </summary>
        /// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
        public bool flipX
        {
            get
            {
                return ( spriteEffects & SpriteEffects.FlipHorizontally ) == SpriteEffects.FlipHorizontally;
            }
            set
            {
                spriteEffects = value ? ( spriteEffects | SpriteEffects.FlipHorizontally ) : ( spriteEffects & ~SpriteEffects.FlipHorizontally );
            }
        }

        /// <summary>
        /// determines if the sprite should be rendered normally or flipped vertically
        /// </summary>
        /// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
        public bool flipY
        {
            get
            {
                return ( spriteEffects & SpriteEffects.FlipVertically ) == SpriteEffects.FlipVertically;
            }
            set
            {
                spriteEffects = value ? ( spriteEffects | SpriteEffects.FlipVertically ) : ( spriteEffects & ~SpriteEffects.FlipVertically );
            }
        }


        public Subtexture[] subtextures
        {
            get { return spriteAnimation.frames.ToArray(); }
            set
            {
                minWidth = spriteAnimation.frames[0].sourceRect.Width;
                minHeight = spriteAnimation.frames[0].sourceRect.Height;
            }
        }


        #region IDrawable implementation

        public float leftWidth { get; set; }
        public float rightWidth { get; set; }
        public float topHeight { get; set; }
        public float bottomHeight { get; set; }
        public float minWidth { get; set; }
        public float minHeight { get; set; }


        public void setPadding( float top, float bottom, float left, float right )
        {
            topHeight = top;
            bottomHeight = bottom;
            leftWidth = left;
            rightWidth = right;
        }

        #endregion


        public SpriteDrawable( SpriteAnimation animation)
        {
            _currentAnimation = spriteAnimation = animation;
            subtextures = animation.frames.ToArray();

            animation.prepareForUse();
            isPlaying = true;
        }

        public virtual void draw( Graphics graphics, float x, float y, float width, float height, Color color )
        {
            UpdateAnimation();

            if ( tintColor.HasValue )
                color = color.multiply( tintColor.Value );

            Subtexture frame = subtextures[ currentFrame ];

            graphics.batcher.draw( subtextures[currentFrame], 
                new Rectangle( (int)(x + frame.center.X - frame.origin.X), (int)(y + frame.center.Y - frame.origin.Y), (int)width, (int)height ), frame.sourceRect, color, spriteEffects );
        }

        private void UpdateAnimation()
        {
            if ( _currentAnimation == null || !isPlaying )
                return;

            // handle delay
            if ( !_delayComplete && _elapsedDelay < _currentAnimation.delay )
            {
                _elapsedDelay += timeScaleIndependant ? Time.unscaledDeltaTime : Time.deltaTime;
                if ( _elapsedDelay >= _currentAnimation.delay )
                    _delayComplete = true;

                return;
            }

            // count backwards if we are going in reverse
            if ( _isReversed )
                _totalElapsedTime -= timeScaleIndependant ? Time.unscaledDeltaTime : Time.deltaTime;
            else
                _totalElapsedTime += timeScaleIndependant ? Time.unscaledDeltaTime : Time.deltaTime;


            _totalElapsedTime = Mathf.clamp( _totalElapsedTime, 0f, _currentAnimation.totalDuration );
            _completedIterations = Mathf.floorToInt( _totalElapsedTime / _currentAnimation.iterationDuration );
            _isLoopingBackOnPingPong = false;


            // handle ping pong loops. if loop is false but pingPongLoop is true we allow a single forward-then-backward iteration
            if ( _currentAnimation.pingPong )
            {
                if ( _currentAnimation.loop || _completedIterations < 2 )
                    _isLoopingBackOnPingPong = _completedIterations % 2 != 0;
            }


            var elapsedTime = 0f;
            if ( _totalElapsedTime < _currentAnimation.iterationDuration )
            {
                elapsedTime = _totalElapsedTime;
            }
            else
            {
                elapsedTime = _totalElapsedTime % _currentAnimation.iterationDuration;

                // if we arent looping and elapsedTime is 0 we are done. Handle it appropriately
                if ( !_currentAnimation.loop && elapsedTime == 0 )
                {
                    // the animation is done so fire our event
                    if ( onAnimationCompletedEvent != null )
                        onAnimationCompletedEvent( spriteAnimation );

                    isPlaying = false;

                    switch ( _currentAnimation.completionBehavior )
                    {
                        case AnimationCompletionBehavior.RemainOnFinalFrame:
                            return;
                        case AnimationCompletionBehavior.RevertToFirstFrame:
                            setSubtexture( _currentAnimation.frames[ 0 ] );
                            return;
                        case AnimationCompletionBehavior.HideSprite:
                            _subtexture = null;
                            _currentAnimation = null;
                            return;
                    }
                }
            }

            // if we reversed the animation and we reached 0 total elapsed time handle un-reversing things and loop continuation
            if ( _isReversed && _totalElapsedTime <= 0 )
            {
                _isReversed = false;

                if ( _currentAnimation.loop )
                {
                    _totalElapsedTime = 0f;
                }
                else
                {
                    // the animation is done so fire our event
                    if ( onAnimationCompletedEvent != null )
                        onAnimationCompletedEvent( spriteAnimation);

                    isPlaying = false;
                    return;
                }
            }

            // time goes backwards when we are reversing a ping-pong loop
            if ( _isLoopingBackOnPingPong )
                elapsedTime = _currentAnimation.iterationDuration - elapsedTime;


            // fetch our desired frame
            var desiredFrame = Mathf.floorToInt( elapsedTime / _currentAnimation.secondsPerFrame );
            if ( desiredFrame != currentFrame )
            {
                currentFrame = desiredFrame;
            
                //prevent crashes when editing animation speed in runtime
                if (currentFrame >= _currentAnimation.frames.Count || currentFrame < 0) currentFrame = 0; 

                setSubtexture( _currentAnimation.frames[ currentFrame ] );
                //handleFrameChanged();

                // ping-pong needs special care. we don't want to double the frame time when wrapping so we man-handle the totalElapsedTime
                if ( _currentAnimation.pingPong && ( currentFrame == 0 || currentFrame == _currentAnimation.frames.Count - 1 ) )
                {
                    if ( _isReversed )
                        _totalElapsedTime -= _currentAnimation.secondsPerFrame;
                    else
                        _totalElapsedTime += _currentAnimation.secondsPerFrame;
                }
            }
        }

        /// <summary>
		/// the Subtexture that should be displayed by this Sprite. When set, the origin of the Sprite is also set to match Subtexture.origin.
		/// </summary>
		/// <value>The subtexture.</value>
		public Subtexture subtexture
        {
            get { return _subtexture; }
            set { setSubtexture( value ); }
        }

        /// <summary>
        /// sets the Subtexture and updates the origin of the Sprite to match Subtexture.origin. If for whatever reason you need
        /// an origin different from the Subtexture either clone it or set the origin AFTER setting the Subtexture here.
        /// </summary>
        /// <returns>The subtexture.</returns>
        /// <param name="subtexture">Subtexture.</param>
        public void setSubtexture( Subtexture subtexture )
        {
            _subtexture = subtexture;

            /*if ( _subtexture != null )
            {
                _origin = subtexture.origin;
                if ( flipX ) _origin.X = subtexture.sourceRect.Width - subtexture.origin.X;
            }*/
     
        }


        /// <summary>
        /// returns a new drawable with the tint color specified
        /// </summary>
        /// <returns>The tinted drawable.</returns>
        /// <param name="tint">Tint.</param>
        public SubtextureDrawable newTintedDrawable( Color tint )
        {
            return new SubtextureDrawable( subtextures[currentFrame] )
            {
                leftWidth = leftWidth,
                rightWidth = rightWidth,
                topHeight = topHeight,
                bottomHeight = bottomHeight,
                minWidth = minWidth,
                minHeight = minHeight,
                tintColor = tint
            };
        }

    }
}
