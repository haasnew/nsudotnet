﻿using System;
using System.Collections.Generic;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            var files = Directory.EnumerateFiles(directoryName);
            var selected = new List<string>();

            foreach (var extension in extensions)
            {
                selected.AddRange(Directory.EnumerateFiles(directoryName, extension));
            }

            var count = 0;
            
            foreach (var file in selected)
            {
                var isMultilineComment = false;

                var multiStart = "/*";
                var multiEnd = "*/";
                var singleStart = "//";

                foreach (var line in File.ReadAllLines(file).Select(line => line.Replace("\t", string.Empty)))
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    if (isMultilineComment)
                    {
                        var endCommentPosition = line.IndexOf(multiEnd);

                        if (endCommentPosition != -1)
                        {
                            var notComment = line.Substring(endCommentPosition + multiEnd.Length);

                            isMultilineComment = false;

                            if (!string.IsNullOrWhiteSpace(notComment))
                            {
                                count++;
                            }
                        }
                    }
                    else
                    {
                        var singleCommentStart = line.IndexOf(singleStart);
                        var multiCommentStart = line.IndexOf(multiStart);
                        

                        if (singleCommentStart != -1 && (multiCommentStart == -1 || singleCommentStart < multiCommentStart))
                        {
                            var notComment = line.Remove(singleCommentStart);

                            if (!string.IsNullOrWhiteSpace(notComment))
                            {
                                count++;
                            }
                        }
                        else if (multiCommentStart != -1 && (singleCommentStart == -1 || multiCommentStart < singleCommentStart))
                        {
                            var multiCommentEnd = line.IndexOf(multiEnd, multiCommentStart + 2);
                            var removeRange = line.Length - multiCommentStart;

                            if (multiCommentEnd != -1)
                            {
                                removeRange = multiCommentEnd + multiEnd.Length - multiCommentStart;
                            }
                            else
                            {
                                isMultilineComment = true;
                            }
                            
                            var notComment = line.Remove(multiCommentStart, removeRange);

                            if (!string.IsNullOrWhiteSpace(notComment))
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
