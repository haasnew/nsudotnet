using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LinesCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            var extensions = ParseParameters(args);

            if (!extensions.Any())
            {
                Console.WriteLine("Нет допустимых расширений");

                return;
            }

            var directoryName = Directory.GetCurrentDirectory();

            var result = CountLines(directoryName, extensions);

            Console.WriteLine(result);
        }

        private static List<string> ParseParameters(string[] args)
        {
            var extensions = new List<string>();

            foreach (var argument in args)
            {
                if (!argument.StartsWith("*."))
                {
                    Console.WriteLine("Неверный параметр: {0}.", argument);
                }
                else
                {
                    extensions.Add(argument);
                }
            }

            return extensions;
        }

        static int CountLines(string directoryName, List<string> extensions)
        {
            foreach (var subDirectory in Directory.EnumerateDirectories(directoryName))
            {
                CountLines(Path.Combine(directoryName, subDirectory), extensions);
            }

            var selected = new List<string>();

            foreach (var extension in extensions)
            {
                selected.AddRange(Directory.EnumerateFiles(directoryName, extension));
            }

            var count = 0;
            
            foreach (var file in selected)
            {
                var isMultilineComment = false;

                const string multiEnd = "*/";

                foreach (var line in File.ReadAllLines(file).Where(line => !string.IsNullOrWhiteSpace(line)))
                {
                    if (isMultilineComment)
                    {
                        var endCommentPosition = line.IndexOf(multiEnd, StringComparison.Ordinal);

                        if (endCommentPosition != -1)
                        {
                            for (var i = endCommentPosition + multiEnd.Length; i < line.Length; i++)
                            {
                                if (!char.IsWhiteSpace(line[i]))
                                {
                                    count++;

                                    break;
                                }
                            }

                            isMultilineComment = false;
                        }
                    }
                    else
                    {
                        var singleCommentStart = -1;
                        var multiCommentStart = -1;

                        for (var i = 0; i < line.Length - 1; i++)
                        {
                            if (line[i] == '/')
                            {
                                switch (line[i + 1])
                                {
                                    case '/':
                                        singleCommentStart = i;
                                        break;
                                    case '*':
                                        multiCommentStart = i;
                                        break;
                                }
                            }
                        }

                        if (singleCommentStart != -1 && (multiCommentStart == -1 || singleCommentStart < multiCommentStart))
                        {
                            for (var i = 0; i < singleCommentStart; i++)
                            {
                                if (!char.IsWhiteSpace(line, i))
                                {
                                    count++;

                                    break;
                                }
                            }
                        }
                        else if (multiCommentStart != -1 && (singleCommentStart == -1 || multiCommentStart < singleCommentStart))
                        {
                            var multiCommentEnd = line.IndexOf(multiEnd, multiCommentStart + 2, StringComparison.Ordinal);
                            var skipEnd = line.Length;

                            if (multiCommentEnd != -1)
                            {
                                skipEnd = multiCommentEnd + multiEnd.Length;
                            }
                            else
                            {
                                isMultilineComment = true;
                            }

                            if (line.Where((t, i) => (i < multiCommentStart || i >= skipEnd) && !char.IsWhiteSpace(line, i)).Any())
                            {
                                count++;
                            }
                        }
                        else
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }
    }
}
