# How to get the code coverage:

Definition: code coverage can mean what code has been used in the main app or how much code has been tested.
The meaning is chosen when doing coverage on certain presets/targets/configurations

## Code coverage through terminal

### Code coverage showing what code which is tested

#### Linux
```shell
# Be inside the Cli folder (which is the root folder of the current project)

# Configure the preset
cmake --preset "linux_code_coverage"

# Build the preset
cmake --build --preset "linux_code_coverage"

# Run the test binary (to generate the raw coverage file)
cd ./output/code_coverage/
./GameWatch.Client.Cli.Tests

# Index the raw coverage file (turn it into .profdata file)
llvm-profdata merge -sparse default.profraw -o coverage.profdata

# Print a summary report
llvm-cov report ./GameWatch.Client.Cli.Tests -instr-profile=coverage.profdata

# See line-by-line coverage
llvm-cov show ./GameWatch.Client.Cli.Tests -instr-profile=coverage.profdata
```

#### Windows
```shell

```

### Code coverage showing what code which is used in the main app

#### Linux
```shell
# Be inside the Cli folder (root of the current project)

# Configure the preset
cmake --preset "linux_code_coverage"

# Build the preset
cmake --build --preset "linux_code_coverage"

# Run the test binary (to generate the raw coverage file)
cd ./output/code_coverage/
./GameWatch.Client.Cli

# Index the raw coverage file (turn it into .profdata file)
llvm-profdata merge -sparse default.profraw -o coverage.profdata

# Print a summary report
llvm-cov report ./GameWatch.Client.Cli -instr-profile=coverage.profdata

# See line-by-line coverage
llvm-cov show ./GameWatch.Client.Cli -instr-profile=coverage.profdata
```

#### Windows
```shell

```

## Code coverage through IDE (CLion)

### Code coverage showing what code which is tested (steps)

1. Press CTRL+SHIFT+A and select the option called 'Load CMake Presets' to load all cmake presets, but then stop it from
starting to configure all the presets.
2. In the CMake tab of the IDE, click the cog and select option 'CMake Settings'
3. Disable all presets but keep the 'linux_coverage' preset enabled
4. Load CMakeLists.txt file from within the Cli folder (which is the root folder of the current folder)
and start to configure it (i usually just select it and hit 'Reload CMake Project'). DO NOT load the CMakeLists.txt file
from the Tests folder!
5. From the 'Run/Debug configurations' zone at the top right usually of the IDE, select the GameWatch.Client.Cli.Test
option which has the Google icon.
6. Click the dropdown arrow, then select the hit the 'More Actions' option (the 3 dots) to the right of the target
7. Select the option called 'Run GameWatch.Client.Cli.Tests with Coverage'

### Code coverage showing what code which is used in the main app (steps)

1. Press CTRL+SHIFT+A and select the option called 'Load CMake Presets' to load all cmake presets, but then stop it from
   starting to configure all the presets.
2. In the CMake tab of the IDE, click the cog and select option 'CMake Settings'
3. Disable all presets but keep the 'linux_coverage' preset enabled
4. Load CMakeLists.txt file from within the Cli folder (which is the root folder of the current folder)
   and start to configure it (i usually just select it and hit 'Reload CMake Project'). DO NOT load the CMakeLists.txt file
   from the Tests folder!
5. From the 'Run/Debug configurations' zone at the top right usually of the IDE, select the GameWatch.Client.Cli
   option which has the Google icon.
6. Click the dropdown arrow, then select the hit the 'More Actions' option (the 3 dots) to the right of the target
7. Select the option called 'Run GameWatch.Client.Cli with Coverage'