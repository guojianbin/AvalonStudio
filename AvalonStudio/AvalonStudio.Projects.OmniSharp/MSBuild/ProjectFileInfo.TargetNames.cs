using System;
using System.Collections.Generic;
using System.Text;

namespace AvalonStudio.Projects.OmniSharp.MSBuild
{
    public partial class ProjectFileInfo
    {
        private static class TargetNames
        {
            public const string Compile = nameof(Compile);
            public const string ResolveReferences = nameof(ResolveReferences);
        }
    }
}
