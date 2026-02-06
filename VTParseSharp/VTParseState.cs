/*
 * VTParse - an implementation of Paul Williams' DEC compatible state machine parser
 *
 * Original Author: Joshua Haberman <joshua@reverberate.org>
 * C# Port: Converted to .NET Standard 2.1
 *
 * This code is in the public domain.
 */

namespace VTParseSharp
{
    /// <summary>
    /// States of the VT parser state machine.
    /// </summary>
    public enum VTParseState : byte
    {
        /// <summary>No state (used as a sentinel value).</summary>
        None = 0,
        /// <summary>Entry point for a CSI (Control Sequence Introducer) sequence.</summary>
        CsiEntry = 1,
        /// <summary>Ignoring a malformed CSI sequence.</summary>
        CsiIgnore = 2,
        /// <summary>Collecting intermediate characters within a CSI sequence.</summary>
        CsiIntermediate = 3,
        /// <summary>Collecting parameters within a CSI sequence.</summary>
        CsiParam = 4,
        /// <summary>Entry point for a DCS (Device Control String) sequence.</summary>
        DcsEntry = 5,
        /// <summary>Ignoring a malformed DCS sequence.</summary>
        DcsIgnore = 6,
        /// <summary>Collecting intermediate characters within a DCS sequence.</summary>
        DcsIntermediate = 7,
        /// <summary>Collecting parameters within a DCS sequence.</summary>
        DcsParam = 8,
        /// <summary>Passthrough mode within a DCS sequence.</summary>
        DcsPassthrough = 9,
        /// <summary>Processing an ESC (Escape) sequence.</summary>
        Escape = 10,
        /// <summary>Collecting intermediate characters within an ESC sequence.</summary>
        EscapeIntermediate = 11,
        /// <summary>Ground state; normal character processing.</summary>
        Ground = 12,
        /// <summary>Processing an OSC (Operating System Command) string.</summary>
        OscString = 13,
        /// <summary>Processing a SOS, PM, or APC string.</summary>
        SosPmApcString = 14
    }
}
