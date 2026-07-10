# How to get the code coverage:

## Steps for creating the coverage results (using terminal)
1. Configure & build the Coverage CMake target

## Steps for creating the coverage results (using CLion)
1. Configure the entire project (you cannot just configure the wanted one, which was the 'Build Ninja Clang (Coverage)' one)
2. Select from 'CMake Profiles' the 'Build Ninja Clang (Coverage)' option
3. Select from available 'targets' (those that you can run/build) and select 'GameWatch.Client.Cli'
4. Build the selected target
5. Select from available 'targets' (those that you can run/build) and select 'GameWatch.Client.Cli.Tests'
6. Build the selected target

## Steps for viewing coverage results in CLion
1. Select from available 'targets' (those that you can run/build) and select 'GameWatch.Client.Cli'
2. Click the button which says "Run 'GameWatch.Client.Cli' with Coverage"