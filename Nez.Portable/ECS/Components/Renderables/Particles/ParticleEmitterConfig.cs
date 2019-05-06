﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Nez.Sprites;

namespace Nez.Particles
{
	[Serializable]
	public class ParticleEmitterConfig
	{
		public Subtexture subtexture;

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
		public Vector2 sourcePositionVariance;
		public float speed, speedVariance;
		public float particleLifespan, particleLifespanVariance;
		public float angle, angleVariance;
		public Vector2 gravity;
		public float radialAcceleration, radialAccelVariance;
		public float tangentialAcceleration, tangentialAccelVariance;
		public Color startColor = Color.White, startColorVariance = Color.White;
		public Color finishColor = Color.Black, finishColorVariance = Color.Black;
		public uint maxParticles;
		public float startParticleSize, startParticleSizeVariance;
		public float finishParticleSize, finishParticleSizeVariance;
		public float duration;
		public ParticleEmitterType emitterType;

		public float rotationStart, rotationStartVariance;
		public float rotationEnd, rotationEndVariance;
		public float emissionRate;
        public SpriteAnimation animation;

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
        public bool animationByLifetime;

        public ParticleEmitterConfig()
		{}

	}
}

