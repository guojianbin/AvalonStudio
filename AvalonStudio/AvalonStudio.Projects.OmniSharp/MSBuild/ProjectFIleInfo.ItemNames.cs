﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AvalonStudio.Projects.OmniSharp.MSBuild
{
    public partial class ProjectFileInfo
    {
        private static class ItemNames
        {
            public const string Analyzer = nameof(Analyzer);
            public const string Compile = nameof(Compile);
            public const string PackageReference = nameof(PackageReference);
            public const string ProjectReference = nameof(ProjectReference);
            public const string ReferencePath = nameof(ReferencePath);
        }
    }
}
