# .NET Core Runtime Shared Directory

This directory contains the shared sources for System.Private.CoreLib. These are shared between dotnet/corert and dotnet/coreclr.

The sources are synchronized with a mirroring tool that watches for new commits on either side and creates new pull requests (as @dotnet-bot) in the other repository.

## Getting clean CI and merging the mirror PRs

Once the mirror PR is created there is a high chance that the new code will require changes to get a clean CI. Any changes can be added to the PR by checking out the PR branch and adding new commits. Please follow the following guidelines for modifying these PRs.

 - **DO NOT** modify the commits made by @dotnet-bot in any way.
 - **TRY** to only make changes outside of shared.
   - Changes made in the shared folder in additional commits will get mirrored properly if the mirror PR is merged with a **REBASE**
 - **ALWAYS** Merge the mirror PR with the **REBASE** option.
   - Using one of the other options will cause the mirror to miss commits
