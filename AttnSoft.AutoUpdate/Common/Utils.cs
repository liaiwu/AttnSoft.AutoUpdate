﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace AttnSoft.AutoUpdate.Common
{
    internal static class Utils
    {
        /// <summary>
        /// Build the command line arguments.
        /// </summary>
        /// <param name="argumentList"></param>
        /// <returns></returns>
        public static string BuildArguments(Collection<string> argumentList)
        {
            var arguments = new StringBuilder();
            if (argumentList is not { Count: > 0 })
            {
                return string.Empty;
            }
            foreach (string argument in argumentList) PasteArguments.AppendArgument(ref arguments, argument);
            return arguments.ToString();
        }
    }
    internal static class PasteArguments
    {
        private const char Quote = '\"';
        private const char Backslash = '\\';

        internal static void AppendArgument(ref StringBuilder stringBuilder, string argument)
        {
            if (stringBuilder.Length != 0)
            {
                stringBuilder.Append(' ');
            }

            // Parsing rules for non-argv[0] arguments:
            //   - Backslash is a normal character except followed by a quote.
            //   - 2N backslashes followed by a quote ==> N literal backslashes followed by unescaped quote
            //   - 2N+1 backslashes followed by a quote ==> N literal backslashes followed by a literal quote
            //   - Parsing stops at first whitespace outside of quoted region.
            //   - (post 2008 rule): A closing quote followed by another quote ==> literal quote, and parsing remains in quoting mode.
            if (argument.Length != 0 && ContainsNoWhitespaceOrQuotes(argument))
            {
                // Simple case - no quoting or changes needed.
                stringBuilder.Append(argument);
            }
            else
            {
                stringBuilder.Append(Quote);
                var idx = 0;
                while (idx < argument.Length)
                {
                    char c = argument[idx++];
                    switch (c)
                    {
                        case Backslash:
                            {
                                var numBackSlash = 1;
                                while (idx < argument.Length && argument[idx] == Backslash)
                                {
                                    idx++;
                                    numBackSlash++;
                                }

                                if (idx == argument.Length)
                                {
                                    // We'll emit an end quote after this so must double the number of backslashes.
                                    stringBuilder.Append(Backslash, numBackSlash * 2);
                                }
                                else if (argument[idx] == Quote)
                                {
                                    // Backslashes will be followed by a quote. Must double the number of backslashes.
                                    stringBuilder.Append(Backslash, numBackSlash * 2 + 1);
                                    stringBuilder.Append(Quote);
                                    idx++;
                                }
                                else
                                {
                                    // Backslash will not be followed by a quote, so emit as normal characters.
                                    stringBuilder.Append(Backslash, numBackSlash);
                                }

                                continue;
                            }
                        case Quote:
                            // Escape the quote so it appears as a literal. This also guarantees that we won't end up generating a closing quote followed
                            // by another quote (which parses differently pre-2008 vs. post-2008.)
                            stringBuilder.Append(Backslash);
                            stringBuilder.Append(Quote);
                            continue;
                        default:
                            stringBuilder.Append(c);
                            break;
                    }
                }

                stringBuilder.Append(Quote);
            }
        }

        private static bool ContainsNoWhitespaceOrQuotes(string s)
        {
            return s.All(c => !char.IsWhiteSpace(c) && c != Quote);
        }
    }
}
