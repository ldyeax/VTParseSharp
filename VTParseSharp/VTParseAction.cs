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
    /// Actions that can be performed during VT parsing.
    /// </summary>
    public enum VTParseAction : byte
    {
        /// <summary>No action.</summary>
        None = 0,
        /// <summary>Clear all parameters, intermediate characters, and the ignore flag.</summary>
        Clear = 1,
        /// <summary>Collect an intermediate character.</summary>
        Collect = 2,
        /// <summary>Dispatch a CSI (Control Sequence Introducer) sequence.</summary>
        CsiDispatch = 3,
        /// <summary>Dispatch an ESC (Escape) sequence.</summary>
        EscDispatch = 4,
        /// <summary>Execute a C0 or C1 control function.</summary>
        Execute = 5,
        /// <summary>Start of a DCS (Device Control String) sequence.</summary>
        Hook = 6,
        /// <summary>Ignore the current byte.</summary>
        Ignore = 7,
        /// <summary>End of an OSC (Operating System Command) sequence.</summary>
        OscEnd = 8,
        /// <summary>Pass a character within an OSC sequence.</summary>
        OscPut = 9,
        /// <summary>Start of an OSC (Operating System Command) sequence.</summary>
        OscStart = 10,
        /// <summary>Process a parameter character (digit or semicolon).</summary>
        Param = 11,
        /// <summary>Print a character or Unicode codepoint to the screen.</summary>
        Print = 12,
        /// <summary>Pass a character within a DCS sequence.</summary>
        Put = 13,
        /// <summary>End of a DCS (Device Control String) sequence.</summary>
        Unhook = 14,
        /// <summary>An error occurred during parsing.</summary>
        Error = 15
    }
}
