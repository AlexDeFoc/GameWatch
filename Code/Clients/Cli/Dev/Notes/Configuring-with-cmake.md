# Notes

1. You might have to give the same command / do the same operation which u did to start the wished preset to get
configured, because VCPKG will not be found initially (probably, certainly it's the case with CLion IDE), and you will
have to retry, which it then will work. (THIS IS only needed though when you first fresh configure)

2. (CLion IDE related) If you are coding in Windows and using WSL for Linux, you probably have different cmake toolchains, one for your
windows setup and one for your wsl setup. Do not forget to change which is the default, depending on what OS are you
building/coding/doing the coverage!

3. (CLion IDE related) Don't forget to choose the CMake presets which have their name duplicated. They are the ones that use the
CMakePresets.json in the config stage AND build stage. Else you'd get the config stage and just default build stage
which is most likely wrong!