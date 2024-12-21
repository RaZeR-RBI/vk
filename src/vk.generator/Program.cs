using System;
using System.IO;
using System.Collections.Generic;
using CommandLine;

namespace Vk.Generator
{
    public class Options
    {
        [Option('o', "out", Required = false, HelpText = "The folder into which code is generated. Defaults to the application directory.")]
        public string OutputPath { get; set; }
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            string outputPath = AppContext.BaseDirectory;

            Parser.Default.ParseArguments<Options>(args)
                          .WithParsed<Options>(o =>
            {
                if (!string.IsNullOrEmpty(o.OutputPath)) outputPath = o.OutputPath;
            });

            Configuration.CodeOutputPath = outputPath;

            if (File.Exists(outputPath))
            {
                Console.Error.WriteLine("The given path is a file, not a folder.");
                return 1;
            }
            else if (!Directory.Exists(outputPath))
            {
                Directory.CreateDirectory(outputPath);
            }

            using (var fs = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "vk.xml")))
            {
                VulkanSpecification vs = VulkanSpecification.LoadFromXmlStream(fs);
                TypeNameMappings tnm = new TypeNameMappings();
                foreach (var typedef in vs.Typedefs)
                {
                    if (typedef.Requires != null)
                    {
                        tnm.AddMapping(typedef.Requires, typedef.Name);
                    }
                    else
                    {
                        tnm.AddMapping(typedef.Name, "uint");
                    }
                }

                HashSet<string> definedBaseTypes = new HashSet<string>
                {
                    "VkBool32"
                };

                if (Configuration.MapBaseTypes)
                {
                    foreach (var baseType in vs.BaseTypes)
                    {
                        if (!definedBaseTypes.Contains(baseType.Key))
                        {
                            tnm.AddMapping(baseType.Key, baseType.Value);
                        }
                    }
                }

                CodeGenerator.GenerateCodeFiles(vs, tnm, Configuration.CodeOutputPath);
            }

            return 0;
        }
    }
}
