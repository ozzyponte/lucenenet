﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Lucene.Net.Cli
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    public static class StringExtensions
    {
        public static string[] ToArgs(this string input)
        {
            return Regex.Replace(input.Trim(), @"\s+", " ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
        }

        public static string OptionValue(this IEnumerable<string> args, string option)
        {
            return args.SkipWhile(a => a != option).Skip(1).FirstOrDefault();
        }

        public static IList<string> OptionValues(this IEnumerable<string> args, string option)
        {
            var argsList = new List<string>(args);
            var result = new List<string>();
            for (int i = 0; i < argsList.Count; i++)
            {
                string current = argsList[i];
                if (current == option)
                {
                    if (i == argsList.Count - 1)
                    {
                        result.Add(null);
                    }
                    else
                    {
                        current = argsList[i + 1];
                        if (current != option)
                        {
                            result.Add(current);
                            i++;
                        }
                        else
                        {
                            result.Add(null);
                        }
                    }
                }
            }
            return result;
        }
    }
}
