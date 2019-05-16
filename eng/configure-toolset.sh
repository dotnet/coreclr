# We depend on a local cli for a number of our buildtool
# commands like init-tools so for now we need to disable
# using the globally installed dotnet

use_installed_dotnet_cli=false

# Most of this repo does not yet go through the full Arcade
# scripts and system, so we need to always use the local repo
# packages directory such that both systems work together.
use_global_nuget_cache=false