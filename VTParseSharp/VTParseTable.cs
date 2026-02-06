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
    /// Contains the state transition tables for the VT parser.
    /// Generated from Paul Williams' DEC compatible state machine specification.
    /// </summary>
    internal static class VTParseTable
    {
        /// <summary>
        /// Action names for debugging/display purposes.
        /// </summary>
        public static readonly string[] ActionNames = new string[]
        {
            "<no action>",
            "CLEAR",
            "COLLECT",
            "CSI_DISPATCH",
            "ESC_DISPATCH",
            "EXECUTE",
            "HOOK",
            "IGNORE",
            "OSC_END",
            "OSC_PUT",
            "OSC_START",
            "PARAM",
            "PRINT",
            "PUT",
            "UNHOOK",
            "ERROR"
        };

        /// <summary>
        /// State names for debugging/display purposes.
        /// </summary>
        public static readonly string[] StateNames = new string[]
        {
            "<no state>",
            "CSI_ENTRY",
            "CSI_IGNORE",
            "CSI_INTERMEDIATE",
            "CSI_PARAM",
            "DCS_ENTRY",
            "DCS_IGNORE",
            "DCS_INTERMEDIATE",
            "DCS_PARAM",
            "DCS_PASSTHROUGH",
            "ESCAPE",
            "ESCAPE_INTERMEDIATE",
            "GROUND",
            "OSC_STRING",
            "SOS_PM_APC_STRING"
        };

        /// <summary>
        /// Entry actions for each state. Indexed by (state - 1).
        /// </summary>
        public static readonly VTParseAction[] EntryActions = new VTParseAction[]
        {
            VTParseAction.Clear,    // CSI_ENTRY
            VTParseAction.None,     // CSI_IGNORE
            VTParseAction.None,     // CSI_INTERMEDIATE
            VTParseAction.None,     // CSI_PARAM
            VTParseAction.Clear,    // DCS_ENTRY
            VTParseAction.None,     // DCS_IGNORE
            VTParseAction.None,     // DCS_INTERMEDIATE
            VTParseAction.None,     // DCS_PARAM
            VTParseAction.Hook,     // DCS_PASSTHROUGH
            VTParseAction.Clear,    // ESCAPE
            VTParseAction.None,     // ESCAPE_INTERMEDIATE
            VTParseAction.None,     // GROUND
            VTParseAction.OscStart, // OSC_STRING
            VTParseAction.None      // SOS_PM_APC_STRING
        };

        /// <summary>
        /// Exit actions for each state. Indexed by (state - 1).
        /// </summary>
        public static readonly VTParseAction[] ExitActions = new VTParseAction[]
        {
            VTParseAction.None,     // CSI_ENTRY
            VTParseAction.None,     // CSI_IGNORE
            VTParseAction.None,     // CSI_INTERMEDIATE
            VTParseAction.None,     // CSI_PARAM
            VTParseAction.None,     // DCS_ENTRY
            VTParseAction.None,     // DCS_IGNORE
            VTParseAction.None,     // DCS_INTERMEDIATE
            VTParseAction.None,     // DCS_PARAM
            VTParseAction.Unhook,   // DCS_PASSTHROUGH
            VTParseAction.None,     // ESCAPE
            VTParseAction.None,     // ESCAPE_INTERMEDIATE
            VTParseAction.None,     // GROUND
            VTParseAction.OscEnd,   // OSC_STRING
            VTParseAction.None      // SOS_PM_APC_STRING
        };

        // Helper method to create state change byte (action in lower nibble, state in upper nibble)
        private static byte SC(VTParseAction action, VTParseState state) => (byte)((byte)action | ((byte)state << 4));
        private static byte SC(VTParseAction action) => (byte)action;
        private static byte SC(VTParseState state) => (byte)((byte)state << 4);

        /// <summary>
        /// The main state transition table. Indexed by [state-1][character].
        /// Each byte encodes: lower nibble = action, upper nibble = new state.
        /// </summary>
        public static readonly byte[][] StateTable = CreateStateTable();

        private static byte[][] CreateStateTable()
        {
            var table = new byte[14][];
            for (int i = 0; i < 14; i++)
            {
                table[i] = new byte[256];
            }

            // Initialize all states with anywhere transitions first
            for (int state = 0; state < 14; state++)
            {
                InitializeAnywhereTransitions(table[state]);
            }

            // CSI_ENTRY (state 1, index 0)
            InitializeCsiEntry(table[0]);

            // CSI_IGNORE (state 2, index 1)
            InitializeCsiIgnore(table[1]);

            // CSI_INTERMEDIATE (state 3, index 2)
            InitializeCsiIntermediate(table[2]);

            // CSI_PARAM (state 4, index 3)
            InitializeCsiParam(table[3]);

            // DCS_ENTRY (state 5, index 4)
            InitializeDcsEntry(table[4]);

            // DCS_IGNORE (state 6, index 5)
            InitializeDcsIgnore(table[5]);

            // DCS_INTERMEDIATE (state 7, index 6)
            InitializeDcsIntermediate(table[6]);

            // DCS_PARAM (state 8, index 7)
            InitializeDcsParam(table[7]);

            // DCS_PASSTHROUGH (state 9, index 8)
            InitializeDcsPassthrough(table[8]);

            // ESCAPE (state 10, index 9)
            InitializeEscape(table[9]);

            // ESCAPE_INTERMEDIATE (state 11, index 10)
            InitializeEscapeIntermediate(table[10]);

            // GROUND (state 12, index 11)
            InitializeGround(table[11]);

            // OSC_STRING (state 13, index 12)
            InitializeOscString(table[12]);

            // SOS_PM_APC_STRING (state 14, index 13)
            InitializeSosPmApcString(table[13]);

            return table;
        }

        private static void InitializeAnywhereTransitions(byte[] stateRow)
        {
            // Anywhere transitions - these apply to all states
            stateRow[0x18] = SC(VTParseAction.Execute, VTParseState.Ground);
            stateRow[0x1a] = SC(VTParseAction.Execute, VTParseState.Ground);

            for (int i = 0x80; i <= 0x8f; i++)
                stateRow[i] = SC(VTParseAction.Execute, VTParseState.Ground);

            for (int i = 0x91; i <= 0x97; i++)
                stateRow[i] = SC(VTParseAction.Execute, VTParseState.Ground);

            stateRow[0x99] = SC(VTParseAction.Execute, VTParseState.Ground);
            stateRow[0x9a] = SC(VTParseAction.Execute, VTParseState.Ground);
            stateRow[0x9c] = SC(VTParseState.Ground);
            stateRow[0x1b] = SC(VTParseState.Escape);
            stateRow[0x98] = SC(VTParseState.SosPmApcString);
            stateRow[0x9e] = SC(VTParseState.SosPmApcString);
            stateRow[0x9f] = SC(VTParseState.SosPmApcString);
            stateRow[0x90] = SC(VTParseState.DcsEntry);
            stateRow[0x9d] = SC(VTParseState.OscString);
            stateRow[0x9b] = SC(VTParseState.CsiEntry);
        }

        private static void InitializeGround(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            for (int i = 0x20; i <= 0x7f; i++)
                stateRow[i] = SC(VTParseAction.Print);
        }

        private static void InitializeEscape(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.EscapeIntermediate);
            for (int i = 0x30; i <= 0x4f; i++)
                stateRow[i] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            for (int i = 0x51; i <= 0x57; i++)
                stateRow[i] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            stateRow[0x59] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            stateRow[0x5a] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            stateRow[0x5c] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            for (int i = 0x60; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
            stateRow[0x5b] = SC(VTParseState.CsiEntry);
            stateRow[0x5d] = SC(VTParseState.OscString);
            stateRow[0x50] = SC(VTParseState.DcsEntry);
            stateRow[0x58] = SC(VTParseState.SosPmApcString);
            stateRow[0x5e] = SC(VTParseState.SosPmApcString);
            stateRow[0x5f] = SC(VTParseState.SosPmApcString);
        }

        private static void InitializeEscapeIntermediate(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x30; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.EscDispatch, VTParseState.Ground);
        }

        private static void InitializeCsiEntry(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.CsiIntermediate);
            stateRow[0x3a] = SC(VTParseState.CsiIgnore);
            for (int i = 0x30; i <= 0x39; i++)
                stateRow[i] = SC(VTParseAction.Param, VTParseState.CsiParam);
            stateRow[0x3b] = SC(VTParseAction.Param, VTParseState.CsiParam);
            for (int i = 0x3c; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.CsiParam);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.CsiDispatch, VTParseState.Ground);
        }

        private static void InitializeCsiIgnore(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            for (int i = 0x20; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseState.Ground);
        }

        private static void InitializeCsiParam(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            for (int i = 0x30; i <= 0x39; i++)
                stateRow[i] = SC(VTParseAction.Param);
            stateRow[0x3b] = SC(VTParseAction.Param);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            stateRow[0x3a] = SC(VTParseState.CsiIgnore);
            for (int i = 0x3c; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseState.CsiIgnore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.CsiIntermediate);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.CsiDispatch, VTParseState.Ground);
        }

        private static void InitializeCsiIntermediate(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            stateRow[0x19] = SC(VTParseAction.Execute);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Execute);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x30; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseState.CsiIgnore);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.CsiDispatch, VTParseState.Ground);
        }

        private static void InitializeDcsEntry(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            stateRow[0x3a] = SC(VTParseState.DcsIgnore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.DcsIntermediate);
            for (int i = 0x30; i <= 0x39; i++)
                stateRow[i] = SC(VTParseAction.Param, VTParseState.DcsParam);
            stateRow[0x3b] = SC(VTParseAction.Param, VTParseState.DcsParam);
            for (int i = 0x3c; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.DcsParam);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseState.DcsPassthrough);
        }

        private static void InitializeDcsIntermediate(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            for (int i = 0x30; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseState.DcsIgnore);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseState.DcsPassthrough);
        }

        private static void InitializeDcsIgnore(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x7f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
        }

        private static void InitializeDcsParam(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            for (int i = 0x30; i <= 0x39; i++)
                stateRow[i] = SC(VTParseAction.Param);
            stateRow[0x3b] = SC(VTParseAction.Param);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
            stateRow[0x3a] = SC(VTParseState.DcsIgnore);
            for (int i = 0x3c; i <= 0x3f; i++)
                stateRow[i] = SC(VTParseState.DcsIgnore);
            for (int i = 0x20; i <= 0x2f; i++)
                stateRow[i] = SC(VTParseAction.Collect, VTParseState.DcsIntermediate);
            for (int i = 0x40; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseState.DcsPassthrough);
        }

        private static void InitializeDcsPassthrough(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Put);
            stateRow[0x19] = SC(VTParseAction.Put);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Put);
            for (int i = 0x20; i <= 0x7e; i++)
                stateRow[i] = SC(VTParseAction.Put);
            stateRow[0x7f] = SC(VTParseAction.Ignore);
        }

        private static void InitializeOscString(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x7f; i++)
                stateRow[i] = SC(VTParseAction.OscPut);
        }

        private static void InitializeSosPmApcString(byte[] stateRow)
        {
            for (int i = 0x00; i <= 0x17; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            stateRow[0x19] = SC(VTParseAction.Ignore);
            for (int i = 0x1c; i <= 0x1f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
            for (int i = 0x20; i <= 0x7f; i++)
                stateRow[i] = SC(VTParseAction.Ignore);
        }
    }
}
