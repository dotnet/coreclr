### Cross OS DAC Readme

This repo is a fork of the primary CoreCLR release/3.1 branch.

It exists to implement the Windows version of the DAC & DBI libraries
which we use for debugging. It enables examination of Linux core dumps
on a Windows machine.

It exists as a separate entity, because the original change was too late
to be included in the 3.1 release, and the change complexity was too
high to be considered for backport.

#### Servicing Releases

Periodically, Microsoft will release updates for .NET 3.1 servicing.
These occur most months, typically around the second Tuesday of each
month.

For each of these servicing releases we need to release a new cross OS
DAC/DBI set.

##### Release Process

Assumes the user has a clone of the dotnet/coreclr repo.

In the afternoon when a servicing release has occurred.
- Checkout release/3.1-crossdac branch of the repo.
- Run ./crossDacMergeLatestTag.sh <upstream>
  - For my use case, I have created a remote with `git remote add <remoteName> <url>`.
  In my case it is `git remote add sdmaclea git@github.com:sdmaclea\coreclr`.
  So I would this as `./crossDacMergeLatestTag.sh sdmaclea`.
  - You could choose to pass a URL instead.  So in my case it would be
  `./crossDacMergeLatestTag.sh git@github.com:sdmaclea\coreclr`
- Create a PR to merge from <upstream>:release/3.1-crossdac-<latestTag>
  to dotnet/coreclr:release/3.1-crossdac
  - Review PR looking for changes which could potentially break the Cross
    DAC/DBI. These would typically be layout changes to structures or the
    addition of HOST_* conditional code
  - In the absence of risky changes, merge the commit.
  - If there are riskier changes extra eyes might be useful to see if the
    changes are breaking. (Not really expected.)
- When the PR is merged an internal trial build will be triggered. See
  https://dev.azure.com/dnceng/internal/_build?definitionId=244&_a=summary.
  This will do a complete build, except publishing the cross DAC/DBI to
  the symbol server.
- When the trial build completes, we need to trrigger an official build.
  See https://dev.azure.com/dnceng/internal/_build?definitionId=244&_a=summary.
  - Click 'Run pipeline'
  - Select branch 'release/3.1-crossdac'
  - Enable 'SymStoreCrossDacIndex'. This enables uploading the DAC/DBI
    to the symbol server.

##### Release Testing

We do not expect much churn in the 3.1 or 3.1-crossdac branches.
Therefore the current plan is to do simple smoke testing only. This is
testing that the debugging libraries are properly available on the
Symbol servers and that at least some functionality works.

The plan is to test the `clrstack -i` command on Windows with a
`linux-x64` core dump.  This tests the DAC & DBI are both present and
at least partially functional. It also does a basic test of stack
walking.

The plan would be to do the testing the Wednesday after a servicing
release primarily to catch issues with symbol server upload and
forgetting to do the monthly build.
