/*
 * VTParse - an implementation of Paul Williams' DEC compatible state machine parser
 *
 * Original Author: Joshua Haberman <joshua@reverberate.org>
 * C# Port: Converted to .NET Standard 2.1
 *
 * This code is in the public domain.
 */

using System;

namespace VTParse
{
    /// <summary>
    /// Callback delegate for VT parse events.
    /// </summary>
    /// <param name="parser">The parser instance that triggered the event.</param>
    /// <param name="action">The action being performed.</param>
    /// <param name="ch">The character associated with the action (0 if none).</param>
    public delegate void VTParseCallback(VTParser parser, VTParseAction action, byte ch);

    /// <summary>
    /// VT terminal escape sequence parser implementing Paul Williams' DEC compatible state machine.
    /// See: http://www.vt100.net/emu/dec_ansi_parser
    /// </summary>
    public class VTParser
    {
        /// <summary>
        /// Maximum number of intermediate characters that can be collected.
        /// </summary>
        public const int MaxIntermediateChars = 2;

        /// <summary>
        /// Maximum number of parameters that can be collected.
        /// </summary>
        public const int MaxParams = 16;

        private const int ActionMask = 0x0F;
        private const int StateShift = 4;

        private VTParseState _state;
        private readonly VTParseCallback? _callback;
        private readonly byte[] _intermediateChars;
        private int _numIntermediateChars;
        private bool _ignoreFlagged;
        private readonly int[] _params;
        private int _numParams;

        /// <summary>
        /// Gets or sets user-defined data associated with this parser instance.
        /// </summary>
        public object? UserData { get; set; }

        /// <summary>
        /// Gets the current state of the parser.
        /// </summary>
        public VTParseState State => _state;

        /// <summary>
        /// Gets the intermediate characters collected during parsing.
        /// </summary>
        public ReadOnlySpan<byte> IntermediateChars => _intermediateChars.AsSpan(0, _numIntermediateChars);

        /// <summary>
        /// Gets the number of intermediate characters collected.
        /// </summary>
        public int NumIntermediateChars => _numIntermediateChars;

        /// <summary>
        /// Gets the parameters collected during parsing.
        /// </summary>
        public ReadOnlySpan<int> Parameters => _params.AsSpan(0, _numParams);

        /// <summary>
        /// Gets the number of parameters collected.
        /// </summary>
        public int NumParams => _numParams;

        /// <summary>
        /// Gets whether the ignore flag has been set (too many intermediate chars).
        /// </summary>
        public bool IgnoreFlagged => _ignoreFlagged;

        /// <summary>
        /// Creates a new VT parser with the specified callback.
        /// </summary>
        /// <param name="callback">The callback to invoke for parse events.</param>
        public VTParser(VTParseCallback? callback = null)
        {
            _callback = callback;
            _intermediateChars = new byte[MaxIntermediateChars + 1];
            _params = new int[MaxParams];
            Reset();
        }

        /// <summary>
        /// Resets the parser to its initial state.
        /// </summary>
        public void Reset()
        {
            _state = VTParseState.Ground;
            _numIntermediateChars = 0;
            _numParams = 0;
            _ignoreFlagged = false;
        }

        /// <summary>
        /// Parses the specified data buffer.
        /// </summary>
        /// <param name="data">The data to parse.</param>
        public void Parse(ReadOnlySpan<byte> data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte ch = data[i];
                byte change = VTParseTable.StateTable[(int)_state - 1][ch];
                DoStateChange(change, ch);
            }
        }

        /// <summary>
        /// Parses a single byte.
        /// </summary>
        /// <param name="ch">The byte to parse.</param>
        public void Parse(byte ch)
        {
            byte change = VTParseTable.StateTable[(int)_state - 1][ch];
            DoStateChange(change, ch);
        }

        /// <summary>
        /// Gets the name of an action for debugging purposes.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <returns>The action name.</returns>
        public static string GetActionName(VTParseAction action)
        {
            int index = (int)action;
            if (index >= 0 && index < VTParseTable.ActionNames.Length)
                return VTParseTable.ActionNames[index];
            return "<unknown>";
        }

        /// <summary>
        /// Gets the name of a state for debugging purposes.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <returns>The state name.</returns>
        public static string GetStateName(VTParseState state)
        {
            int index = (int)state;
            if (index >= 0 && index < VTParseTable.StateNames.Length)
                return VTParseTable.StateNames[index];
            return "<unknown>";
        }

        private void DoStateChange(byte change, byte ch)
        {
            var newState = (VTParseState)(change >> StateShift);
            var action = (VTParseAction)(change & ActionMask);

            if (newState != VTParseState.None)
            {
                // Perform up to three actions:
                // 1. The exit action of the old state
                // 2. The action associated with the transition
                // 3. The entry action of the new state

                var exitAction = VTParseTable.ExitActions[(int)_state - 1];
                var entryAction = VTParseTable.EntryActions[(int)newState - 1];

                if (exitAction != VTParseAction.None)
                    DoAction(exitAction, 0);

                if (action != VTParseAction.None)
                    DoAction(action, ch);

                if (entryAction != VTParseAction.None)
                    DoAction(entryAction, 0);

                _state = newState;
            }
            else
            {
                DoAction(action, ch);
            }
        }

        private void DoAction(VTParseAction action, byte ch)
        {
            // Some actions we handle internally (like parsing parameters),
            // others we hand to our client for processing

            switch (action)
            {
                case VTParseAction.Print:
                case VTParseAction.Execute:
                case VTParseAction.Hook:
                case VTParseAction.Put:
                case VTParseAction.OscStart:
                case VTParseAction.OscPut:
                case VTParseAction.OscEnd:
                case VTParseAction.Unhook:
                case VTParseAction.CsiDispatch:
                case VTParseAction.EscDispatch:
                    _callback?.Invoke(this, action, ch);
                    break;

                case VTParseAction.Ignore:
                    // Do nothing
                    break;

                case VTParseAction.Collect:
                    // Append the character to the intermediate params
                    if (_numIntermediateChars + 1 > MaxIntermediateChars)
                    {
                        _ignoreFlagged = true;
                    }
                    else
                    {
                        _intermediateChars[_numIntermediateChars++] = ch;
                    }
                    break;

                case VTParseAction.Param:
                    // Process the param character
                    if (ch == ';')
                    {
                        _numParams++;
                        if (_numParams <= MaxParams)
                        {
                            _params[_numParams - 1] = 0;
                        }
                    }
                    else
                    {
                        // The character is a digit
                        if (_numParams == 0)
                        {
                            _numParams = 1;
                            _params[0] = 0;
                        }

                        if (_numParams <= MaxParams)
                        {
                            int currentParam = _numParams - 1;
                            _params[currentParam] *= 10;
                            _params[currentParam] += (ch - '0');
                        }
                    }
                    break;

                case VTParseAction.Clear:
                    _numIntermediateChars = 0;
                    _numParams = 0;
                    _ignoreFlagged = false;
                    break;

                default:
                    _callback?.Invoke(this, VTParseAction.Error, 0);
                    break;
            }
        }
    }
}
