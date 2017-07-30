﻿using AsyncRpc;
using AsyncRpc.Routing;
using AsyncRpc.Transport.Tcp;
using AvalonStudio.Extensibility.Utils;
using AvalonStudio.Languages.CSharp.OmniSharp;
using AvalonStudio.MSBuildHost;
using AvalonStudio.Platforms;
using AvalonStudio.Utils;
using Microsoft.Build.Construction;
using Microsoft.CodeAnalysis.Host.Mef;
using RoslynPad.Roslyn;
using RoslynPad.Roslyn.Diagnostics;
using System;
using System.Collections.ObjectModel;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace AvalonStudio.Projects.OmniSharp
{
    public class OmniSharpSolution : ISolution
    {
        private CompositionHost _compositionContext;
        private MefHostServices _host;

        static OmniSharpSolution()
        {
            MSBuild.MSBuildEnvironment.Initialize();
        }

        public static async Task<OmniSharpSolution> Create(string path)
        {
            OmniSharpSolution result = new OmniSharpSolution();

            await result.LoadSolution(path);

            return result;
        }

        public NuGetConfiguration NuGetConfiguration { get; }

        public RoslynWorkspace Workspace { get; private set; }

        private OmniSharpServer server;

        private OmniSharpSolution()
        {
            server = new OmniSharpServer(TcpUtils.FreeTcpPort());
            Projects = new ObservableCollection<IProject>();
        }

        private async Task LoadSolution(string path)
        {
            var currentDir = AvalonStudio.Platforms.Platform.ExecutionPath;

            var assemblies = new[]
            {
                Assembly.LoadFrom(Path.Combine(currentDir, "Roslyn", "Microsoft.CodeAnalysis.dll")),
                Assembly.LoadFrom(Path.Combine(currentDir, "Roslyn", "Microsoft.CodeAnalysis.CSharp.dll")),
                Assembly.LoadFrom(Path.Combine(currentDir, "Roslyn", "Microsoft.CodeAnalysis.Features.dll")),
                Assembly.LoadFrom(Path.Combine(currentDir, "Roslyn", "Microsoft.CodeAnalysis.CSharp.Features.dll")),
                typeof(DiagnosticsService).Assembly,
            };

            var partTypes = MefHostServices.DefaultAssemblies.Concat(assemblies)
                    .Distinct()
                    .SelectMany(x => x.GetTypes())
                    //.Concat(new[] { typeof(DocumentationProviderServiceFactory) })
                    .ToArray();

            _compositionContext = new ContainerConfiguration()
                .WithParts(partTypes)
                .CreateContainer();

            _host = MefHostServices.Create(_compositionContext);

            Workspace = new RoslynWorkspace(_host, NuGetConfiguration, _compositionContext);

            if (Path.GetExtension(path) == ".sln")
            {
                var sln = SolutionFile.Parse(path);

                var solutionDir = Path.GetDirectoryName(path);

                foreach (var project in sln.ProjectsInOrder.Where(p => Path.GetExtension(p.AbsolutePath) == ".csproj"))
                {
                    var roslynProject = await Workspace.AddProject(solutionDir, project.AbsolutePath);

                    var asProject = OmniSharpProject.Create(roslynProject.Item1, this, project.AbsolutePath, roslynProject.Item2);                    

                    AddProject(asProject);

                    asProject.LoadFiles();
                }

                foreach(var project in Projects)
                {
                    var asProject = (project as OmniSharpProject);

                    if(project.Location.Contains("Controls"))
                    {

                    }

                    foreach (var unresolvedReference in asProject.UnresolvedReferences)
                    {
                         Workspace.ResolveReference(project, unresolvedReference);
                    }
                }
            }
            else if(Path.GetExtension(path) == ".csproj")
            {
                var solutionDir = Path.GetDirectoryName(path);
                var roslynProject = await Workspace.AddProject(solutionDir, path);

                var asProject = OmniSharpProject.Create(roslynProject.Item1, this, path, roslynProject.Item2);

                AddProject(asProject);

                asProject.LoadFiles();
            }

            Location = path;

            Name = Path.GetFileNameWithoutExtension(path);

           

            CurrentDirectory = Path.GetDirectoryName(path);
        }

        public IProject AddProject(IProject project)
        {
            var currentProject = Projects.FirstOrDefault(p => p.Name == project.Name);

            if (currentProject != null) return currentProject;

            //ProjectReferences.Add(CurrentDirectory.MakeRelativePath(project.Location));
            Projects.InsertSorted(project);
            currentProject = project;

            project.FileAdded += Project_FileAdded;

            return currentProject;
        }

        private void Project_FileAdded(object sender, ISourceFile e)
        {
            switch (e.Extension)
            {
                case ".cs":
                    Workspace.AddDocument((sender as OmniSharpProject).RoslynProject, e);
                    break;
            }
        }

        public OmniSharpServer Server => server;

        public string CurrentDirectory { get; set; }

        public string Name { get; set; }

        public ObservableCollection<IProject> Projects { get; set; }

        public IProject StartupProject { get; set; }

        public string Location { get; private set; }

        public ISourceFile FindFile(string file)
        {
            ISourceFile result = null;

            foreach (var project in Projects)
            {
                result = project.FindFile(file);

                if (result != null)
                {
                    break;
                }
            }

            return result;
        }

        public void RemoveProject(IProject project)
        {
            project.FileAdded -= Project_FileAdded;
        }

        public void Save()
        {
            //throw new NotImplementedException();
        }
    }
}