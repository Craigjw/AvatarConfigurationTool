using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace ACT
{
    /// <summary>
    /// A Serializable transform
    /// </summary>
    [Serializable]
    public class AvatarTransform
    {
        public Vector3 Position;
        public Vector3 LocalPosition;

        public Quaternion Rotation;
        public Quaternion LocalRotation;

        public Vector3 Scale;

        /// <summary>
        /// Constructor
        /// </summary>
        public AvatarTransform()
        {
            Position = Vector3.zero;
            LocalPosition = Vector3.zero;

            Rotation = Quaternion.identity;
            LocalRotation = Quaternion.identity;

            Scale = Vector3.one;
        }
        /// <summary>
        /// Constructor overload
        /// </summary>
        /// <param name="position">Vector3 Position</param>
        /// <param name="localPosition">Vector3 LocalPosition </param>
        /// <param name="rotation">Quaternion rotation</param>
        /// <param name="localRotation">Quaternion local rotation</param>
        /// <param name="scale">Vector3 Scale</param>
        public AvatarTransform(Vector3 position, Vector3 localPosition, Quaternion rotation, Quaternion localRotation, Vector3 scale) : this()
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;

            LocalPosition = localPosition;
            LocalRotation = localRotation;
        }
        /// <summary>
        /// Constructor overload
        /// </summary>
        /// <param name="other">Other transform to copy</param>
        public AvatarTransform(AvatarTransform other) : this(other.Position, other.LocalPosition, other.Rotation, other.LocalRotation, other.Scale)
        {

        }
        /// <summary>
        /// Compare method
        /// </summary>
        /// <param name="other">Other Transform to compare against</param>
        /// <returns>Whether Successful</returns>
        public bool Compare(Transform other)
        { 
            bool result = Position == other.position
                && Rotation == other.rotation
                && LocalPosition == other.localPosition
                && LocalRotation == other.localRotation
                && Scale == other.localScale;
            return result;
        }/// <summary>
         /// Compare method
         /// </summary>
         /// <param name="other">Other AvatarTransform to compare against</param>
         /// <returns>Whether Successful</returns>
        public bool Compare(AvatarTransform other)
        {
            bool result = Position == other.Position
                && Rotation == other.Rotation
                && LocalPosition == other.LocalPosition
                && LocalRotation == other.LocalRotation
                && Scale == other.Scale;
            return result;
        }
        /// <summary>
        /// Sets an AvatarTransform
        /// </summary>
        /// <param name="value">Source transform value</param>
        public void Set(Transform value)
        {
            Position = value.position;
            LocalPosition = value.localPosition;
            Rotation = value.rotation;
            LocalRotation = value.localRotation;
            Scale = value.localScale;
        }
        /// <summary>
        /// Sets and AvatarTransform
        /// </summary>
        /// <param name="other">Source AvatarTransform value</param>
        public void Set(AvatarTransform other)
        {
            Position = other.Position;
            LocalPosition = other.LocalPosition;
            Rotation = other.Rotation;
            LocalRotation = other.LocalRotation;
            Scale = other.Scale;
        }
    }
}