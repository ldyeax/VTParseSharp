# VTParse for .NET

A pure C# implementation of Paul Williams' DEC compatible state machine parser for VT terminal emulation.

Original implementation: http://www.vt100.net/emu/dec_ansi_parser

## Target Framework

- .NET Standard 2.1

## Installation

Add a reference to the VTParse library in your project.

## Usage

```csharp
using VTParse;

// Create a parser with a callback
var parser = new VTParser((p, action, ch) =>
{
    Console.WriteLine($"Action: {VTParser.GetActionName(action)}");

    if (ch != 0)
        Console.WriteLine($"Char: 0x{ch:X2} ('{(char)ch}')");

    if (p.NumIntermediateChars > 0)
    {
        Console.WriteLine($"{p.NumIntermediateChars} Intermediate chars:");
        foreach (var c in p.IntermediateChars)
            Console.WriteLine($"  0x{c:X2} ('{(char)c}')");
    }

    if (p.NumParams > 0)
    {
        Console.WriteLine($"{p.NumParams} Parameters:");
        foreach (var param in p.Parameters)
            Console.WriteLine($"  {param}");
    }

    Console.WriteLine();
});

// Parse some data
byte[] data = /* your terminal data */;
parser.Parse(data);

// Or parse byte-by-byte
parser.Parse(0x1B); // ESC
parser.Parse(0x5B); // [
parser.Parse(0x31); // 1
parser.Parse(0x6D); // m
```

## Actions

The parser generates the following actions via the callback:

| Action | Description |
|--------|-------------|
| `Print` | Print a character to the screen |
| `Execute` | Execute a C0 or C1 control function |
| `CsiDispatch` | Dispatch a CSI sequence |
| `EscDispatch` | Dispatch an ESC sequence |
| `Hook` | Start of a DCS sequence |
| `Put` | Pass a character in a DCS sequence |
| `Unhook` | End of a DCS sequence |
| `OscStart` | Start of an OSC sequence |
| `OscPut` | Pass a character in an OSC sequence |
| `OscEnd` | End of an OSC sequence |
| `Error` | An error occurred |

## API Reference

### VTParser Class

#### Constructor
- `VTParser(VTParseCallback? callback = null)` - Creates a new parser instance

#### Properties
- `State` - Current parser state
- `IntermediateChars` - Collected intermediate characters (as `ReadOnlySpan<byte>`)
- `NumIntermediateChars` - Count of intermediate characters
- `Parameters` - Collected parameters (as `ReadOnlySpan<int>`)
- `NumParams` - Count of parameters
- `IgnoreFlagged` - Whether the ignore flag is set
- `UserData` - User-defined data object

#### Methods
- `Parse(ReadOnlySpan<byte> data)` - Parse a buffer of data
- `Parse(byte ch)` - Parse a single byte
- `Reset()` - Reset the parser to initial state
- `GetActionName(VTParseAction action)` - Get action name for debugging
- `GetStateName(VTParseState state)` - Get state name for debugging

## License

This code is in the public domain.

## Credits

- [Original C implementation](https://github.com/haberman/vtparse): Joshua Haberman <joshua@reverberate.org>
- [State machine specification](http://www.vt100.net/emu/dec_ansi_parser): Paul Williams
- Thanks to Julian Scheid for bugfixes and enhancements to the original
