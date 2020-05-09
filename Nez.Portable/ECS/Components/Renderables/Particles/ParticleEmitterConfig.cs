using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Nez.Sprites;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nez.Tweens;

namespace Nez.Particles
{
    public enum PARTICLE_BOUNDS_BEHAVIOUR : int
    {
        DESTROY = 0,
        REFLECT = 1
    }

	[Serializable]
	public class ParticleEmitterConfig
	{
        public List<Subtexture> subTextures = new List<Subtexture>();
		public Subtexture subtexture
        {
            get
            {
                if ( subTextures == null || subTextures.Count == 0 ) return null;
                return subTextures[ Random.nextInt( subTextures.Count ) ];
            }
            set
            {
                if ( value == null ) return;
                if( subTextures == null ) subTextures = new List<Subtexture>();

                if ( subTextures.Count == 0)
                {
                    subTextures.Add( value );
                }
                else
                {
                    subTextures[ 0 ] = value;
                }
            }
        }

		/// <summary>
		/// If true, particles will simulate in world space. ie when the parent Transform moves it will have no effect on any already active Particles.
		/// </summary>
		public bool simulateInWorldSpace = true;

		public Blend blendFuncSource;
		public Blend blendFuncDestination;

		/// <summary>
		/// sourcePosition is read in but internally it is not used. The ParticleEmitter.localPosition is what the emitter will use for positioning
		/// </summary>
		public Vector2 sourcePosition;
        public Vector2 sourcePositionVariance = new Vector2(5, 5);
        public RectangleF bounds = RectangleF.empty;
        public PARTICLE_BOUNDS_BEHAVIOUR boundsBehaviour = PARTICLE_BOUNDS_BEHAVIOUR.REFLECT;

        public float speed = 5;
        public float speedVariance;
        public bool scaleBySpeed = false;
        public float minSpeedScale = 1;
        public float maxSpeedScale = 1;
        public float particleLifespan = 1;
        public float particleLifespanVariance;
        public float angle = 360;
        public float angleVariance;
		public Vector2 gravity;
		public float radialAcceleration, radialAccelVariance;
		public float tangentialAcceleration, tangentialAccelVariance;

		public Color startColor = Color.White, startColorVariance = Color.White;
        public Color finishColor = Color.Black, finishColorVariance = Color.Black;
        public LoopType colorLoopType = LoopType.None;

        public uint maxParticles = 10;
        public float startParticleSize = 1;
        public Vector2 scale = Vector2.One;
        public float startParticleSizeVariance;
        public float finishParticleSize = 1;
        public float finishParticleSizeVariance;
        public float duration = -1;
		public ParticleEmitterType emitterType;

		public float rotationStart, rotationStartVariance;
		public float rotationEnd, rotationEndVariance;
        public float emissionRate = 1;
        public List<SpriteAnimation> animations;

        public bool flipXWithVelocity = false;

		/////// Particle ivars only used when a maxRadius value is provided.  These values are used for
		/////// the special purpose of creating the spinning portal emitter
		// Max radius at which particles are drawn when rotating
		public float maxRadius;
		// Variance of the maxRadius
		public float maxRadiusVariance;
		// Radius from source below which a particle dies
		public float minRadius;
		// Variance of the minRadius
		public float minRadiusVariance;
		// Number of degress to rotate a particle around the source pos per second
		public float rotatePerSecond;
		// Variance in degrees for rotatePerSecond
		public float rotatePerSecondVariance;

        // How much a particle's position should be offset by the camera, to apply a parallax effect
        public Vector2 parallax = Vector2.Zero;
        public Vector2 parallaxVariance = Vector2.Zero;
        public bool scaleByParallax = false;
        public bool parallaxByScale = false;
        public bool alphaByParallax = false;
        public float parallaxScaleFactor = 1;
        public bool animationByLifetime;

        public ParticleEmitterConfig()
		{}

	}
}

