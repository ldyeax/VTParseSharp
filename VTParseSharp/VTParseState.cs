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
        None = 0,
        CsiEntry = 1,
        CsiIgnore = 2,
        CsiIntermediate = 3,
        CsiParam = 4,
        DcsEntry = 5,
        DcsIgnore = 6,
        DcsIntermediate = 7,
        DcsParam = 8,
        DcsPassthrough = 9,
        Escape = 10,
        EscapeIntermediate = 11,
        Ground = 12,
        OscString = 13,
        SosPmApcString = 14
    }
}
