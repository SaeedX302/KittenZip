# KittenZip.Project

Some MSBuild configurations shared by whole KittenZip project.

The precompiled binaries in this folder is necessary for implementing the
out-of-box KittenZip building experience. Open the `KittenZip.MaintainerTools.sln`
at the root folder of the KittenZip source code repository, find the
`KittenZip.Build.Tasks` project, you will see the source for the precompiled
binaries which contained in this folder. Also, these precompiled binaries will
be updated automatically via the GitHub Actions workflow if the implementation
of the `KittenZip.Build.Tasks` project has some changes.
