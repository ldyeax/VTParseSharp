/*

#include <stdio.h>
#include <io.h>
#include <fcntl.h>
#include "vtparse.h"

using VTParse;

int codesOnly = 0;

void parser_callback(vtparse_t* parser, vtparse_action_t action, unsigned int ch)
{
    int i;

    printf("Received action %s\n", ACTION_NAMES[action]);
    if (ch != 0) {
        if (!codesOnly) {
            printf("Char: 0x%02x ('%c')\n", ch, ch);
        }
        else {
            printf("Char: 0x%02x\n", ch);
        }
    }
    if (parser->num_intermediate_chars > 0)
    {
        printf("%d Intermediate chars:\n", parser->num_intermediate_chars);
        for (i = 0; i < parser->num_intermediate_chars; i++) {
            if (!codesOnly) {
                printf("  0x%02x ('%c')\n", parser->intermediate_chars[i],
                    parser->intermediate_chars[i]);
            }
            else {
                printf("  0x%02x\n", parser->intermediate_chars[i]);
            }
        }

    }
    if (parser->num_params > 0)
    {
        printf("%d Parameters:\n", parser->num_params);
        for (i = 0; i < parser->num_params; i++)
            printf("\t%d\n", parser->params[i]);
    }

    printf("\n");
}

int main(int argc, char** argv)
{
	if (argc > 1 && strcmp(argv[1], "--codes-only") == 0)
        codesOnly = 1;

    unsigned char buf[1024];
    int bytes;
    vtparse_t parser;

    vtparse_init(&parser, parser_callback);

    _setmode(_fileno(stdin), _O_BINARY);

    do {
        bytes = _read(_fileno(stdin), buf, 1024);
        vtparse(&parser, buf, bytes);
    } while (bytes > 0);

    return 0;
}
*/
using VTParseSharp;

namespace VTParseSharp_Test;

public class Program
{
    private bool _codesOnly;

    public Program(bool codesOnly)
    {
        _codesOnly = codesOnly;
    }

    private void ParserCallback(VTParser parser, VTParseAction action, uint ch)
    {
        Console.WriteLine($"Received action {VTParser.GetActionName(action)}");

        if (ch != 0)
        {
            if (!_codesOnly)
            {
                Console.WriteLine($"Char: 0x{ch:x2} ('{(char)ch}')");
            }
            else
            {
                Console.WriteLine($"Char: 0x{ch:x2}");
            }
        }

        if (parser.NumIntermediateChars > 0)
        {
            Console.WriteLine($"{parser.NumIntermediateChars} Intermediate chars:");
            foreach (byte ic in parser.IntermediateChars)
            {
                if (!_codesOnly)
                {
                    Console.WriteLine($"  0x{ic:x2} ('{(char)ic}')");
                }
                else
                {
                    Console.WriteLine($"  0x{ic:x2}");
                }
            }
        }

        if (parser.NumParams > 0)
        {
            Console.WriteLine($"{parser.NumParams} Parameters:");
            foreach (int param in parser.Parameters)
            {
                Console.WriteLine($"\t{param}");
            }
        }

        Console.WriteLine();
    }

    public void Run()
    {
        var parser = new VTParser(ParserCallback);
        using var stdin = Console.OpenStandardInput();
        var buffer = new byte[1024];
        int bytesRead;

        do
        {
            bytesRead = stdin.Read(buffer, 0, buffer.Length);
            if (bytesRead > 0)
            {
                parser.Parse(buffer.AsSpan(0, bytesRead));
            }
        } while (bytesRead > 0);
    }

    public static void Main(string[] args)
    {
        bool codesOnly = args.Length > 0 && args[0] == "--codes-only";
        var program = new Program(codesOnly);
        program.Run();
    }
}
