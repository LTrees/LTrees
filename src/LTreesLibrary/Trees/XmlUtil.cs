using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace LTreesLibrary.Trees
{
    /// <summary>
    /// Contains static methods convenient for parsing XML documents.
    /// The invariant culture is used to parse numbers, meaning that dot is always the decimal separator.
    /// </summary>
    public static class XmlUtil
    {
        private static readonly NumberFormatInfo numberFormat = NumberFormatInfo.InvariantInfo;

        /// <summary>
        /// Parses an attribute as a floating-point value.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Floating-point value of the attribute.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a floating-point value.</exception>
        public static float GetFloat(XmlNode child, String attributeName)
        {
            String value = GetString(child, attributeName);
            try
            {
                return float.Parse(value, numberFormat);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a float; found in element '{2}'", attributeName, value, child.Name), e);
            }
        }

        /// <summary>
        /// Parses an attribute as a integer value.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Integer value of the attribute.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as an integer value.</exception>
        public static int GetInt(XmlNode child, String attributeName)
        {
            String value = GetString(child, attributeName);
            try
            {
                return int.Parse(value, numberFormat);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a int; found in element '{2}'", attributeName, value, child.Name), e);
            }
        }

        /// <summary>
        /// Parses an attribute as a integer value, it is exists.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <param name="defaultValue">Value of return if the attribute does not exist.</param>
        /// <returns>Integer value of the attribute, or <code>defaultValue</code> if the attribute does not exist.</returns>
        /// <exception cref="ArgumentException">If the attribute exists but cannot parse as an integer value.</exception>
        public static int GetInt(XmlNode child, String attributeName, int defaultValue)
        {
            String value = GetStringOrNull(child, attributeName);
            if (value == null)
                return defaultValue;
            try
            {
                return int.Parse(value, numberFormat);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a int; found in element '{2}'", attributeName, value, child.Name), e);
            }
        }

        /// <summary>
        /// Parses an attribute as a floating-point value, it is exists.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <param name="defaultValue">Value of return if the attribute does not exist.</param>
        /// <returns>Integer value of the attribute, or <code>defaultValue</code> if the attribute does not exist.</returns>
        /// <exception cref="ArgumentException">If the attribute exists but cannot parse as a floating-point value.</exception>
        public static float GetFloat(XmlNode child, String attributeName, float defaultValue)
        {
            String value = GetStringOrNull(child, attributeName);
            if (value == null)
                return defaultValue;
            try
            {
                return float.Parse(value, numberFormat);
            }
            catch (FormatException e)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a float; found in element '{2}'", attributeName, value, child.Name), e);
            }
        }

        /// <summary>
        /// Gets the value of an attribute as a string.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Value of the attribute, as a string.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        public static string GetString(XmlNode child, string attributeName)
        {
            XmlNode a = child.Attributes[attributeName];
            if (a == null)
                throw new ArgumentException(String.Format("Required attribute '{0}' not found in element '{1}'.", attributeName, child.Name));
            return a.Value;
        }

        /// <summary>
        /// Gets the value of an attribute as a string.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Value of the attribute, as a string, or null if the attribute does not exist.</returns>
        public static string GetStringOrNull(XmlNode child, string attributeName)
        {
            XmlNode a = child.Attributes[attributeName];
            if (a == null)
                return null;
            return a.Value;
        }

        /// <summary>
        /// Parses the value of an attribute as a <see cref="Color"/> in the format: <code>R,G,B</code>.
        /// Alpha value cannot be specified, and will be set to 255.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Value of the attribute, parsed as a color, with an alpha of 255.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a color.</exception>
        public static Color GetColor(XmlNode child, string attributeName)
        {
            string s = GetString(child, attributeName);
            string[] parts = s.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a color; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                byte r = byte.Parse(parts[0], numberFormat);
                byte g = byte.Parse(parts[1], numberFormat);
                byte b = byte.Parse(parts[2], numberFormat);
                return new Color(r, g, b);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a color; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }

        /// <summary>
        /// Parses the value of an attribute as a <see cref="Color"/> in the format: <code>R,G,B</code>.
        /// Alpha value cannot be specified, and will be set to 255.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <param name="defaultColor">Value of return if the attribute does not exist.</param>
        /// <returns>Value of the attribute, parsed as a color, with an alpha of 255.</returns>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a color.</exception>
        public static Color GetColor(XmlNode child, string attributeName, Color defaultColor)
        {
            string s = GetStringOrNull(child, attributeName);
            if (s == null)
                return defaultColor;
            string[] parts = s.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a color; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                byte r = byte.Parse(parts[0], numberFormat);
                byte g = byte.Parse(parts[1], numberFormat);
                byte b = byte.Parse(parts[2], numberFormat);
                byte a = parts.Length > 3 ? byte.Parse(parts[3], numberFormat) : defaultColor.A;
                return new Color(r, g, b, a);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a color; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }

        /// <summary>
        /// Parses the value of an attribute as a <see cref="Vector2"/> in the format: <code>X,Y</code>.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Value of the attribute, parsed as a 2D vector.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a vector.</exception>
        public static Vector2 GetVector2(XmlNode child, string attributeName)
        {
            string s = GetString(child, attributeName);
            string[] parts = s.Split(',');
            if (parts.Length < 2)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector2; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                float x = float.Parse(parts[0], numberFormat);
                float y = float.Parse(parts[1], numberFormat);
                return new Vector2(x, y);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector2; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }


        /// <summary>
        /// Parses the value of an attribute as a <see cref="Vector2"/> in the format: <code>X,Y</code>.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <param name="defaultValue">Value of return if the attribute does not exist.</param>
        /// <returns>Value of the attribute, parsed as a 2D vector.</returns>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a vector.</exception>
        public static Vector2 GetVector2(XmlNode child, string attributeName, Vector2 defaultValue)
        {
            string s = GetStringOrNull(child, attributeName);
            if (s == null)
                return defaultValue;
            string[] parts = s.Split(',');
            if (parts.Length < 2)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector2; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                float x = float.Parse(parts[0], numberFormat);
                float y = float.Parse(parts[1], numberFormat);
                return new Vector2(x, y);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector2; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }

        /// <summary>
        /// Parses the value of an attribute as a <see cref="Vector3"/> in the format: <code>X,Y,Z</code>.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <returns>Value of the attribute, parsed as a 3D vector.</returns>
        /// <exception cref="ArgumentException">If the node does not have an attribute by that name.</exception>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a vector.</exception>
        public static Vector3 GetVector3(XmlNode child, string attributeName)
        {
            string s = GetString(child, attributeName);
            string[] parts = s.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector3; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                float x = float.Parse(parts[0], numberFormat);
                float y = float.Parse(parts[1], numberFormat);
                float z = float.Parse(parts[2], numberFormat);
                return new Vector3(x, y, z);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector3; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }

        /// <summary>
        /// Parses the value of an attribute as a <see cref="Vector3"/> in the format: <code>X,Y,Z</code>.
        /// </summary>
        /// <param name="child">The XML element containing the attribute.</param>
        /// <param name="attributeName">Name of the attribute to parse.</param>
        /// <param name="defaultValue">Value of return if the attribute does not exist.</param>
        /// <returns>Value of the attribute, parsed as a 3D vector.</returns>
        /// <exception cref="ArgumentException">If the attribute's value cannot parse as a vector.</exception>
        public static Vector3 GetVector3(XmlNode child, string attributeName, Vector3 defaultValue)
        {
            string s = GetStringOrNull(child, attributeName);
            if (s == null)
                return defaultValue;
            string[] parts = s.Split(',');
            if (parts.Length != 3)
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector3; found in element '{2}'", attributeName, s, child.Name));
            try
            {
                float x = float.Parse(parts[0], numberFormat);
                float y = float.Parse(parts[1], numberFormat);
                float z = float.Parse(parts[2], numberFormat);
                return new Vector3(x, y, z);
            }
            catch (FormatException ex)
            {
                throw new ArgumentException(String.Format("Attribute '{0}' set to '{1}' could not parse as a Vector3; found in element '{2}'", attributeName, s, child.Name), ex);
            }
        }
    }
}
