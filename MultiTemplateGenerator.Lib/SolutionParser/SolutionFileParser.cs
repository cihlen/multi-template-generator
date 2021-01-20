using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MultiTemplateGenerator.Lib.SolutionParser
{
    public class SolutionFileParser
    {
        public static SolutionItemCollection ParseSolutionFile(string solutionFile)
        {
            var solutionItems = new List<SolutionProjectItem>();
            var nestedProjects = new Dictionary<string, string>();

            using (StreamReader sr = File.OpenText(solutionFile))
            {
                string line;

                while (sr.Peek() != -1)
                {
                    line = sr.ReadLine()?.Trim();
                    if (line.StartsWith("Project(\""))
                    {
                        var parts = line.Split(new[] { "\", \"" }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length != 3)
                        {
                            continue;
                        }

                        solutionItems.Add(new SolutionProjectItem(line));
                    }
                    else if (line.Equals("GlobalSection(NestedProjects) = preSolution"))
                    {
                        line = sr.ReadLine()?.Trim();
                        do
                        {
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                continue;
                            }

                            var parts = line.Split(new[] { " = " }, StringSplitOptions.RemoveEmptyEntries);
                            if (parts.Length != 2)
                            {
                                throw new ArgumentOutOfRangeException(nameof(solutionFile),
                                    "NestedProjects parser error: Should spliot in 2, actual " + parts.Length);
                            }

                            nestedProjects.Add(parts[0], parts[1]);

                            line = sr.ReadLine()?.Trim();
                        } while (line != "EndGlobalSection" && sr.Peek() != -1);
                    }
                }
            }

            if (!solutionItems.Any(x => x.IsProject))
            {
                throw new ArgumentException(@"Solution doesn't have any projects.", nameof(solutionFile));
            }

            return new SolutionItemCollection(solutionItems, nestedProjects);
        }
    }
}
