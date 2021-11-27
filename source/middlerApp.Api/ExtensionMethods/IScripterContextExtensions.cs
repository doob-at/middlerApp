//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;
//using doob.Scripter.Shared;
//using middlerApp.Api.Helper;
//using Serilog;

//namespace middlerApp.API.ExtensionMethods
//{
//    public static class IScripterContextExtensions
//    {

//        internal static List<Assembly> assembliesForJint { get; } = new List<Assembly>();
//        internal static List<string> assemblyLocations { get; } = new List<string>();

//        //public static IScripterContext AddModulePlugins(this IScripterContext context)
//        //{
//        //    var dir = PathHelper.GetFullPath("Scripter/M");

            
//        //    var folderPluginCatalog = new FolderPluginCatalog(@dir, type =>
//        //    {
//        //        type.Implements<IScripterModule>();
//        //    });


//        //    folderPluginCatalog.Initialize().GetAwaiter().GetResult();

//        //    var assemplyPlugins = folderPluginCatalog.GetPlugins();

//        //    foreach (var plugin in assemplyPlugins)
//        //    {
//        //        assembliesForJint.Add(plugin.Assembly);
//        //        context.AddScripterModule(plugin);
//        //    }

//        //    return context;
//        //}
//        public static IScripterContext AddModulePlugins(this IScripterContext context)
//        {


//            var dir = PathHelper.GetFullPath("Scripter/Modules");
//            var sharedDllsDir = PathHelper.GetFullPath("Scripter/Modules/SharedDLLs");
            

//            Log.Information($"Scriper Modules Path: {dir}");
//            Log.Information($"Scriper SharedDlls Path: {sharedDllsDir}");

//            var assembliesToLoad = new List<string>();
//            var tsDefinitionAssemblies = new List<string>();

//            if (!Directory.Exists(dir))
//            {
//                Directory.CreateDirectory(dir);
//            }

//            if (!Directory.Exists(sharedDllsDir))
//            {
//                Directory.CreateDirectory(sharedDllsDir);
//            }

//            foreach (var file in Directory.GetFiles(sharedDllsDir, "*.dll"))
//            {
//                var ass = Assembly.LoadFrom(file);
//                assembliesForJint.Add(ass);
//            }

//            foreach (var directory in Directory.GetDirectories(dir))
//            {

//                foreach (var file in Directory.GetFiles(directory, "Scripter.Module.*.dll"))
//                {
//                    assembliesToLoad.Add(file);

//                }

//                foreach (var file in Directory.GetFiles(directory, "*.ScripterModule.dll"))
//                {
//                    assembliesToLoad.Add(file);
//                }

//                foreach (var file in Directory.GetFiles(directory, "*.Scripter.Module.dll"))
//                {
//                    assembliesToLoad.Add(file);
//                }

//                //foreach (var file in Directory.GetFiles(directory, "*.Definition.dll"))
//                //{
//                //    assembliesToLoad.Add(file);
//                //}

//                //foreach (var file in Directory.GetFiles(directory, "*.dll"))
//                //{
//                //    assemblyLocations.Add(file);
//                //    //assembliesForJint.Add(ass);
//                //}

//                //foreach (var file in Directory.GetFiles(directory, "*.SharedModels.dll"))
//                //{
//                //    var ass = Assembly.LoadFrom(file);
//                //    //assembliesForJint.Add(ass);
//                //}

//                //foreach (var file in Directory.GetFiles(directory, "*.Shared.Interfaces.dll"))
//                //{
//                //    //var ass = Assembly.LoadFrom(file);
//                //    //assembliesToLoad.Add(file);
//                //    //foreach (var referencedAssembly in ass.GetReferencedAssemblies())
//                //    //{
//                //    //    if (referencedAssembly.Name.Contains("scsm", StringComparison.CurrentCultureIgnoreCase))
//                //    //    {
//                //    //        //Assembly.LoadFrom(referencedAssembly.Name + ".dll");
//                //    //    }

//                //    //}

//                //    //assembliesForJint.Add(ass);
//                //}

//            }

//            if (!assembliesToLoad.Any())
//                return context;

//            var loaders = new List<PluginLoader>();

//            foreach (var assembly in assembliesToLoad)
//            {

//                //Assembly.LoadFrom(assembly);

//                var loader = PluginLoader.CreateFromAssemblyFile(
//                    assembly,
//                    config =>
//                    {
//                        config.PreferSharedTypes = true;
//                        config.LoadInMemory = false;
//                    });
//                loaders.Add(loader);
//            }

//            foreach (var loader in loaders)
//            {
//                using (loader.EnterContextualReflection())
//                {
                    
//                    var defAss = loader.LoadDefaultAssembly();


                    
//                    //foreach (var referencedAssembly in defAss.GetReferencedAssemblies())
//                    //{
//                    //    //Assembly.Load(referencedAssembly);
//                    //    loader.LoadAssembly(referencedAssembly);
//                    //}

//                    var assDir = Path.GetDirectoryName(defAss.Location);
//                    //assemblyLocations.Add(assDir);
//                    foreach (var enumerateFile in Directory.EnumerateFiles(assDir, "*.dll", SearchOption.AllDirectories))
//                    {
//                        if (!enumerateFile.Equals(defAss.Location, StringComparison.CurrentCultureIgnoreCase))
//                        {
//                            var addas = loader.LoadAssemblyFromPath(enumerateFile);
//                            assembliesForJint.Add(addas);
//                            assemblyLocations.Add(enumerateFile);
//                        }

//                    }

//                    foreach (var pluginType in defAss
//                        .GetTypes()
//                        .Where(t => typeof(IScripterModule).IsAssignableFrom(t) && !t.IsAbstract))
//                    {

                        
//                        assembliesForJint.Add(pluginType.Assembly);
//                        context.AddScripterModule(pluginType);
//                    }
//                }


//            }

//            return context;
//        }
//    }
//}
