using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Xsl;

namespace Ferrologic.Tools.Xsl
{
    internal class Program
    {
        private const int EXIT_CODE_OK = 0;
        private const int EXIT_CODE_INCORRECT_USAGE = 1;
        private const int EXIST_CODE_TRANSFORMATION_FAILURE = 2;
        private const int EXIT_CODE_REINDENT_FAILURE = 3;

        private static int Main(string[] args)
        {
            int exitCode = EXIT_CODE_INCORRECT_USAGE;

            try
            {
                if (!UsageOk(args))
                    return EXIT_CODE_INCORRECT_USAGE;

                var inputPath = args[0];
                var transformPath = args[1];
                var outputPath = args[2];

                exitCode = EXIST_CODE_TRANSFORMATION_FAILURE;

                var settings = new XsltSettings { EnableDocumentFunction = true, EnableScript = false };
                var transform = new XslCompiledTransform();
                var xmlResolver = new XmlUrlResolver();
                var xsltArgumentList = new XsltArgumentList();

                for (int i = 3; i < args.Length; i++)
                {
                    var keyValuePair = args[i].Split('=');
                    xsltArgumentList.AddParam(keyValuePair[0], "", keyValuePair[1]);
                }

                using (var outputStream = new StreamWriter(outputPath))
                {
                    transform.Load(transformPath, settings, xmlResolver);
                    transform.Transform(inputPath, xsltArgumentList, outputStream);
                }

                if (transform.OutputSettings.OutputMethod == XmlOutputMethod.Text) return EXIT_CODE_OK;

                exitCode = EXIT_CODE_REINDENT_FAILURE;
                Reindent(outputPath);
            }
            catch (Exception ex)
            {
                while (ex != null)
                {
                    Console.Error.WriteLine("Error: " + ex);
                    ex = ex.InnerException;
                }
                return exitCode;
            }
            return EXIT_CODE_OK;
        }

        private static bool UsageOk(string[] args)
        {
            if (args.Length >= 3)
                return true;

            Console.WriteLine("Usage: {0} inputPath transformPath outputPath [param1=value1 [param2=value2 [...]]]",
                              Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase));
            Console.WriteLine();
            return false;
        }

        private static void Reindent(string path)
        {
            var xml = new XmlDocument();
            xml.Load(path);

            var settings = new XmlWriterSettings { Indent = true, IndentChars = "\t" };
            var xmlWriter = XmlWriter.Create(path, settings);

            xml.PreserveWhitespace = false;
            xml.Save(xmlWriter);
        }

    }
}