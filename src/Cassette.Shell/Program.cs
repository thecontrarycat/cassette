// BIG FAT WARNING!
// This is just temporary spike code I've knocked together for my own immediate needs!
// Use at your own risk ;)

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Cassette.HtmlTemplates;
using Cassette.ModuleProcessing;
using Cassette.Scripts;
using Cassette.Stylesheets;
using NConsoler;
using System.Text;

namespace Cassette.Shell
{
    class Program
    {
        static void Main(string[] args)
        {
            Consolery.Run(typeof(Program), args);
        }

        [Action]
        public static void Go(
            [Required(Description = "Path to asset directory.")] string source,
            [Required(Description = "Output directory")] string destination,
            [Required(Description = "The type of asset module to create (Script, Stylesheet or HtmlTemplate).")] string type,
            [Optional(true)] bool single
        )
        {
            source = Path.GetFullPath(source);
            destination = Path.GetFullPath(destination);
            var app = new Application(source);
            
            var goMethod = typeof(Program).GetMethod("Go" + type, BindingFlags.Static | BindingFlags.Public);
            if (goMethod == null)
            {
                Console.Error.WriteLine("Invalid module type. Valid types are Script, Stylesheet and HtmlTemplate.");
                return;
            }

            goMethod.Invoke(null, new object[] { source, destination, app, single });
        }

        public static void GoScript(string sourceDirectory, string destinationDirectory, ICassetteApplication application, bool single)
        {
            FileSystemModuleSource<ScriptModule> source;
            if (single) 
                source = new DirectorySource<ScriptModule>(sourceDirectory);
            else 
                source = new PerSubDirectorySource<ScriptModule>(sourceDirectory);

            source.FilePattern = "*.js;*.coffee";

            var modules = source.GetModules(new ScriptModuleFactory(), application);
            if (single)
                OutputModules(destinationDirectory, Path.GetFileName(sourceDirectory) + ".js", application, modules);
            else
                OutputModules(destinationDirectory, "{0}.js", application, modules);
        }

        public static void GoStylesheet(string sourceDirectory, string destinationDirectory, ICassetteApplication application, bool single)
        {
            FileSystemModuleSource<StylesheetModule> source;
            if (single)
                source = new DirectorySource<StylesheetModule>(sourceDirectory);
            else
                source = new PerSubDirectorySource<StylesheetModule>(sourceDirectory);

            source.FilePattern = "*.css;*.less";
            source.CustomizeModule = m => m.Processor = new StylesheetPipeline().Remove<ExpandCssUrls>();

            var modules = source.GetModules(new StylesheetModuleFactory(), application);
            if (single)
                OutputModules(destinationDirectory, Path.GetFileName(sourceDirectory) + ".css", application, modules);
            else
                OutputModules(destinationDirectory, "{0}.css", application, modules);
        }

        public static void GoHtmlTemplate(string sourceDirectory, string destinationDirectory, ICassetteApplication application, bool single)
        {
            FileSystemModuleSource<HtmlTemplateModule> source;
            if (single)
                source = new DirectorySource<HtmlTemplateModule>("");
            else
                source = new PerSubDirectorySource<HtmlTemplateModule>("");

            source.FilePattern = "*.htm;*.html";
            source.CustomizeModule = m => m.Processor = new Pipeline<HtmlTemplateModule>(
                new AddTransformerToAssets(
                    new CompileAsset(new KnockoutJQueryTmplCompiler())
                ),
                new ConcatenateAssets()
            );

            var modules = source.GetModules(new HtmlTemplateModuleFactory(), application);
            if (single)
                OutputModules(destinationDirectory, Path.GetFileName(sourceDirectory) + ".js", application, modules);
            else
                OutputModules(destinationDirectory, "{0}.js", application, modules);
        }

        static void OutputModules(string destinationDirectory, string filenamePattern, ICassetteApplication application, IEnumerable<Module> modules)
        {
            foreach (var module in modules)
            {
                module.Process(application);

                using (var stream = module.Assets[0].OpenStream())
                using (var reader = new StreamReader(stream))
                {
                    var file = new FileInfo(Path.Combine(destinationDirectory, string.Format(filenamePattern, module.Path)));
                    if (!file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }
                    using (var fileStream = file.Open(FileMode.Create, FileAccess.Write))
                    using (var writer = new StreamWriter(fileStream, Encoding.UTF8))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            writer.WriteLine(line);
                        }
                        writer.Flush();
                    }
                }
            }
        }
    }

    class Application : ICassetteApplication
    {
        readonly FileSystem rootDirectory;

        public Application(string path)
        {
            rootDirectory = new FileSystem(path);
            IsOutputOptimized = true;
            UrlGenerator = new UrlGenerator();
        }

        public IFileSystem RootDirectory
        {
            get { return rootDirectory; }
        }

        public bool IsOutputOptimized { get; set; }

        public IUrlGenerator UrlGenerator { get; set; }

        public UI.IPageAssetManager GetPageAssetManager<T>() where T : Module
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }

    class UrlGenerator : IUrlGenerator
    {
        public string CreateModuleUrl(Module module)
        {
            throw new NotImplementedException();
        }

        public string CreateAssetUrl(Module module, IAsset asset)
        {
            throw new NotImplementedException();
        }

        public string CreateAssetCompileUrl(Module module, IAsset asset)
        {
            throw new NotImplementedException();
        }

        public string CreateImageUrl(string filename, string hash)
        {
            return filename.Replace('\\', '/');
        }
    }
}
