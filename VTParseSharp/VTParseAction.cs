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
        None = 0,
        Clear = 1,
        Collect = 2,
        CsiDispatch = 3,
        EscDispatch = 4,
        Execute = 5,
        Hook = 6,
        Ignore = 7,
        OscEnd = 8,
        OscPut = 9,
        OscStart = 10,
        Param = 11,
        Print = 12,
        Put = 13,
        Unhook = 14,
        Error = 15
    }
}
