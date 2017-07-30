using System;
using System.Collections.Generic;
using System.Text;

namespace AvalonStudio.Projects.OmniSharp.MSBuild
{
    public partial class ProjectFileInfo
    {
        private static class MetadataNames
        {
            public const string FullPath = nameof(FullPath);
            public const string IsImplicitlyDefined = nameof(IsImplicitlyDefined);
            public const string Project = nameof(Project);
            public const string ReferenceSourceTarget = nameof(ReferenceSourceTarget);
            public const string Version = nameof(Version);
        }
    }
}
