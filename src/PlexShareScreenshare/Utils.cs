﻿///<author>Satyam Mishra</author>
///<summary>
/// This file has Frame and the related structures which are used for storing
/// the difference in the pixel and the resolution of image. It also has some other
/// general utilities.
///</summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PlexShareScreenshare
{
    /// <summary>
    /// struct for storing x and y coordinates of a pixel
    /// </summary>
    public struct Coordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    /// <summary>
    /// struct for storing RGB value of a pixel
    /// </summary>
    public struct RGB
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }

    /// <summary>
    /// struct for storing both the coordinates and the RGB values
    /// </summary>
    public struct Pixel
    {
        public Coordinates Coordinates { get; set; }
        public RGB RGB { get; set; }
    }

    /// <summary>
    /// struct for storing the resolution of a image
    /// </summary>
    public struct Resolution
    {
        public int Height { get; set; }
        public int Width { get; set; }

        public bool Equals(Resolution p) => Height == p.Height && Width == p.Width;
        public static bool operator ==(Resolution lhs, Resolution rhs) => lhs.Equals(rhs);
        public static bool operator !=(Resolution lhs, Resolution rhs) => !(lhs == rhs);
    }

    /// <summary>
    /// frame struct storing resolution of the image and list of
    /// pixels which are different from the previous image
    /// </summary>
    public struct Frame
    {
        public Resolution Resolution { get; set; }
        public List<Pixel> Pixels { get; set; }
    }

    /// <summary>
    /// Defines various general utilities.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// The string representing the module identifier for screen share.
        /// </summary>
        public const string ModuleIdentifier = "ScreenShare";

        /// <summary>
        /// Static method to get a nice debug message wrapped with useful information.
        /// </summary>
        /// <param name="message">
        /// Message to wrap
        /// </param>
        /// <param name="withTimeStamp">
        /// Whether to prefix the wrapped message with time stamp or not
        /// </param>
        /// <returns>
        /// The message wrapped with class and method name and prefixed with time stamp if asked
        /// </returns>
        public static string GetDebugMessage(string message, bool withTimeStamp = false)
        {
            // Get the class name and the name of the caller function
            StackFrame? stackFrame = (new StackTrace()).GetFrame(1);
            string className = stackFrame?.GetMethod()?.DeclaringType?.Name ?? "SharedClientScreen";
            string methodName = stackFrame?.GetMethod()?.Name ?? "GetDebugMessage";

            string prefix = withTimeStamp ? $"{DateTimeOffset.Now:F} | " : "";

            return $"{prefix}[{className}::{methodName}] : {message}";
        }
    }
}
